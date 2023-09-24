using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.View.Element;
using Suzuryg.FaceEmo.Domain;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    public class ExpressionImporter
    {
        private Domain.Menu _menu;
        private AV3Setting _av3Setting;
        private string _assetDir;

        private List<BlendShape> _faceBlendShapes = new List<BlendShape>();
        private Dictionary<Motion, AnimationClip> _firstFrameCache = new Dictionary<Motion, AnimationClip>();
        private Dictionary<Motion, AnimationClip> _lastFrameCache = new Dictionary<Motion, AnimationClip>();

        public ExpressionImporter(Domain.Menu menu, AV3Setting av3Setting, string assetDir)
        {
            _menu = menu;
            _av3Setting = av3Setting;
            _assetDir = assetDir;
        }

        public void Import(VRCAvatarDescriptor avatarDescriptor)
        {
            // TODO: use additional meshes
            _faceBlendShapes = AV3Utility.GetFaceMeshBlendShapeValues(avatarDescriptor, excludeBlink: false, excludeLipSync: false).Keys.ToList();

            var fx = GetFxLayer(avatarDescriptor);
            if (fx == null) { return; }

            var cacLayer = GetCacLayer(fx);
            if (cacLayer != null)
            {
                ImportCac(cacLayer);
            }
            else
            {
                ImportNormal(fx);
            }
        }

        private AnimatorController GetFxLayer(VRCAvatarDescriptor avatarDescriptor)
        {
            foreach (var baseLayer in avatarDescriptor.baseAnimationLayers)
            {
                if (baseLayer.type == VRCAvatarDescriptor.AnimLayerType.FX &&
                    baseLayer.animatorController != null &&
                    baseLayer.animatorController is AnimatorController animatorController)
                {
                    return animatorController;
                }
            }
            return null;
        }

        private void ImportNormal(AnimatorController fx)
        {
            var layers = new List<List<IBranch>>();
            foreach (var layer in fx.layers)
            {
                var branches = GetBranches(layer.stateMachine);
                if (branches.Any())
                {
                    layers.Add(branches);
                }
            }
            layers.Reverse();

            var modeId = _menu.AddMode(Domain.Menu.RegisteredId);
            // TODO: localization
            _menu.ModifyModeProperties(modeId, displayName: "Imported");

            foreach (var layer in layers)
            {
                foreach (var branch in layer)
                {
                    AddBranch(modeId, branch);
                }
            }
        }

        private List<IBranch> GetBranches(AnimatorStateMachine stateMachine)
        {
            var branches = new List<IBranch>();
            if (stateMachine == null) { return branches; }

            foreach (var transition in stateMachine.entryTransitions)
            {
                branches.Add(GetBranch(transition));
            }

            foreach (var transition in stateMachine.anyStateTransitions)
            {
                branches.Add(GetBranch(transition));
            }

            foreach (var state in stateMachine.states)
            {
                if (state.state == null) { continue; }

                foreach (var transition in state.state.transitions)
                {
                    branches.Add(GetBranch(transition));
                }
            }

            foreach (var subMachine in stateMachine.stateMachines)
            {
                branches = branches.Concat(GetBranches(subMachine.stateMachine)).ToList();
            }

            return branches
                .Where(x => x is IBranch)
                .OrderBy(x => x.Conditions.FirstOrDefault()?.Hand)
                .ThenBy(x => x.Conditions.FirstOrDefault()?.HandGesture)
                .ToList();
        }

        private IBranch GetBranch(AnimatorTransitionBase transition)
        {
            if (transition != null &&
                !transition.mute &&
                transition.conditions.Any(x => x.parameter == "GestureLeft" || x.parameter == "GestureRight") &&
                transition.destinationState != null &&
                IsFaceMotion(transition.destinationState.motion))
            {
                var branch = new Branch();

                // condition
                foreach (var condition in transition.conditions)
                {
                    Hand hand;
                    if (condition.parameter == "GestureLeft") { hand = Hand.Left; }
                    else if (condition.parameter == "GestureRight") { hand = Hand.Right; }
                    else { continue; }

                    HandGesture handGesture = (HandGesture)condition.threshold;

                    ComparisonOperator comparisonOperator;
                    if (condition.mode == AnimatorConditionMode.Equals) { comparisonOperator = ComparisonOperator.Equals; }
                    else if (condition.mode == AnimatorConditionMode.NotEqual) { comparisonOperator = ComparisonOperator.NotEqual; }
                    else { continue; }

                    branch.AddCondition(new Condition(hand, handGesture, comparisonOperator));
                }

                // tracking
                foreach (var behaviour in transition.destinationState.behaviours)
                {
                    if (behaviour is VRC_AnimatorTrackingControl trackingControl && trackingControl != null)
                    {
                        branch.BlinkEnabled = trackingControl.trackingEyes == VRC_AnimatorTrackingControl.TrackingType.Animation ? false : true;
                        branch.EyeTrackingControl = trackingControl.trackingEyes == VRC_AnimatorTrackingControl.TrackingType.Animation ? EyeTrackingControl.Animation : EyeTrackingControl.Tracking;
                        branch.MouthTrackingControl = trackingControl.trackingMouth == VRC_AnimatorTrackingControl.TrackingType.Animation ? MouthTrackingControl.Animation : MouthTrackingControl.Tracking;
                        break;
                    }
                }

                // motion
                if (branch.Conditions.Any(x => x == new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals)) && transition.destinationState.timeParameterActive)
                {
                    branch.SetAnimation(GetFirstFrame(transition.destinationState.motion), BranchAnimationType.Base);
                    branch.SetAnimation(GetLastFrame(transition.destinationState.motion), BranchAnimationType.Left);
                    branch.IsLeftTriggerUsed = true;
                }
                else if (branch.Conditions.Any(x =>  x == new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)) && transition.destinationState.timeParameterActive)
                {
                    branch.SetAnimation(GetFirstFrame(transition.destinationState.motion), BranchAnimationType.Base);
                    branch.SetAnimation(GetLastFrame(transition.destinationState.motion), BranchAnimationType.Right);
                    branch.IsRightTriggerUsed = true;
                }
                else
                {
                    branch.SetAnimation(GetLastFrame(transition.destinationState.motion), BranchAnimationType.Base);
                }

                return branch;
            }
            else
            {
                return null;
            }
        }

        private bool IsFaceMotion(Motion motion)
        {
            if (motion == null) { return false; }
            else if (motion is BlendTree blendTree && blendTree.children.Length > 0)
            {
                var first = blendTree.children.First().motion;
                var last = blendTree.children.Last().motion;
                return IsFaceMotion(first as AnimationClip) && IsFaceMotion(last as AnimationClip);
            }
            else if (motion is AnimationClip animationClip)
            {
                var bindings = new HashSet<EditorCurveBinding>(_faceBlendShapes.Select(x => new EditorCurveBinding { path = x.Path, propertyName = $"blendShape.{x.Name}", type = typeof(SkinnedMeshRenderer) }));
                return AnimationUtility.GetCurveBindings(animationClip).Any(x => bindings.Contains(x));
            }
            else { return false; }
        }

        private Domain.Animation GetFirstFrame(Motion motion)
        {
            var baseName = string.IsNullOrEmpty(motion.name) ? "NoName" : motion.name;
            AnimationClip firstFrame;
            if (_firstFrameCache.ContainsKey(motion))
            {
                firstFrame = _firstFrameCache[motion];
            }
            else
            {
                if (motion is BlendTree blendTree && blendTree != null && blendTree.children.Length > 0)
                {
                    return GetLastFrame(blendTree.children.First().motion);
                }
                else if (motion is AnimationClip animationClip && animationClip != null)
                {
                    firstFrame = new AnimationClip();

                    var bindings = AnimationUtility.GetCurveBindings(animationClip);
                    foreach (var binding in bindings)
                    {
                        var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                        if (curve != null && curve.keys.Length > 0)
                        {
                            var value = curve.keys.First().value;
                            AnimationUtility.SetEditorCurve(firstFrame, binding, new AnimationCurve(new Keyframe(time: 0, value: value)));
                        }
                    }
                }
                else { return null; }

                _firstFrameCache[motion] = firstFrame;
            }

            if (!AssetDatabase.IsMainAsset(firstFrame))
            {
                SaveClip(firstFrame, baseName + "_Base");
            }
            return new Domain.Animation(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(firstFrame)));
        }

        private Domain.Animation GetLastFrame(Motion motion)
        {
            var baseName = string.IsNullOrEmpty(motion.name) ? "NoName" : motion.name;
            AnimationClip lastFrame;
            if (_lastFrameCache.ContainsKey(motion))
            {
                lastFrame = _lastFrameCache[motion];
            }
            else
            {
                if (motion is BlendTree blendTree && blendTree != null && blendTree.children.Length > 0)
                {
                    return GetLastFrame(blendTree.children.Last().motion);
                }
                else if (motion is AnimationClip animationClip && animationClip != null)
                {
                    lastFrame = new AnimationClip();

                    var bindings = AnimationUtility.GetCurveBindings(animationClip);
                    foreach (var binding in bindings)
                    {
                        var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                        if (curve != null && curve.keys.Length > 0)
                        {
                            var value = curve.keys.Last().value;
                            AnimationUtility.SetEditorCurve(lastFrame, binding, new AnimationCurve(new Keyframe(time: 0, value: value)));
                        }
                    }
                }
                else { return null; }

                _lastFrameCache[motion] = lastFrame;
            }

            if (!AssetDatabase.IsMainAsset(lastFrame))
            {
                SaveClip(lastFrame, baseName);
            }
            return new Domain.Animation(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(lastFrame)));
        }

        private void SaveClip(AnimationClip clip, string name)
        {
            if (!AssetDatabase.IsValidFolder(_assetDir))
            {
                AV3Utility.CreateFolderRecursively(_assetDir);
            }

            AssetDatabase.CreateAsset(clip, _assetDir + "/" + AnimationElement.GetNewAnimationName(_assetDir, name));
        }

        private void AddBranch(string modeId, IBranch branch)
        {
            _menu.AddBranch(modeId, conditions: branch.Conditions);
            var branchIndex = _menu.GetMode(modeId).Branches.Count - 1;

            _menu.ModifyBranchProperties(modeId, branchIndex,
                eyeTrackingControl: branch.EyeTrackingControl,
                mouthTrackingControl: branch.MouthTrackingControl,
                blinkEnabled: branch.BlinkEnabled,
                mouthMorphCancelerEnabled: branch.MouthMorphCancelerEnabled,
                isLeftTriggerUsed: branch.IsLeftTriggerUsed,
                isRightTriggerUsed: branch.IsRightTriggerUsed);

            _menu.SetAnimation(branch.BaseAnimation, modeId, branchIndex, BranchAnimationType.Base);
            _menu.SetAnimation(branch.LeftHandAnimation, modeId, branchIndex, BranchAnimationType.Left);
            _menu.SetAnimation(branch.RightHandAnimation, modeId, branchIndex, BranchAnimationType.Right);
            _menu.SetAnimation(branch.BothHandsAnimation, modeId, branchIndex, BranchAnimationType.Both);
        }

        private AnimatorControllerLayer GetCacLayer(AnimatorController fx)
        {
            foreach (var layer in fx.layers)
            {
                if (layer != null &&
                    layer.stateMachine != null &&
                    layer.stateMachine.stateMachines != null &&
                    layer.stateMachine.stateMachines.SelectMany(x => x.stateMachine.entryTransitions).Any(y => y.conditions.First().parameter == "SYNC_EM_EMOTE"))
                {
                    return layer;
                }
            }
            return null;
        }

        private void ImportCac(AnimatorControllerLayer cacLayer)
        {
            var transitions = cacLayer.stateMachine.stateMachines
                .SelectMany(x => x.stateMachine.entryTransitions)
                .Where(x => 
                    !x.mute &&
                    x.conditions.Length == 1 &&
                    x.conditions.First().parameter == "SYNC_EM_EMOTE" &&
                    x.destinationState != null &&
                    IsFaceMotion(x.destinationState.motion))
                .Select(x => (emoteIndex: (int)x.conditions.First().threshold - 1, transition: x))
                .Where(x => x.emoteIndex >= 0)
                .OrderBy(x => x.emoteIndex);
            if (!transitions.Any()) { return; }

            var chunks = new Dictionary<int, List<(int emoteIndex, AnimatorTransition transition)>>();
            foreach (var item in transitions)
            {
                int modeIndex = item.emoteIndex / 14;
                if (!chunks.ContainsKey(modeIndex)) { chunks[modeIndex] = new List<(int emoteIndex, AnimatorTransition transition)>(); }
                chunks[modeIndex].Add(item);
            }

            var modes = new List<List<Branch>>();
            foreach (var modeIndex in chunks.Keys.OrderBy(x => x))
            {
                var mode = new List<Branch>();
                var chunk = chunks[modeIndex];
                foreach (var item in chunk)
                {
                    var branch = new Branch();

                    // condition
                    branch.AddCondition(EmoteIndexToCondition(item.emoteIndex));

                    // tracking
                    foreach (var behaviour in item.transition.destinationState.behaviours)
                    {
                        if (behaviour is VRC_AnimatorTrackingControl trackingControl && trackingControl != null)
                        {
                            branch.EyeTrackingControl = trackingControl.trackingEyes == VRC_AnimatorTrackingControl.TrackingType.Animation ? EyeTrackingControl.Animation : EyeTrackingControl.Tracking;
                            branch.MouthTrackingControl = trackingControl.trackingMouth == VRC_AnimatorTrackingControl.TrackingType.Animation ? MouthTrackingControl.Animation : MouthTrackingControl.Tracking;
                        }
                        else if (behaviour is VRC_AvatarParameterDriver parameterDriver && parameterDriver != null)
                        {
                            foreach (var parameter in parameterDriver.parameters)
                            {
                                if (parameter.name == "CN_BLINK_ENABLE")
                                {
                                    branch.BlinkEnabled = parameter.value > 0;
                                }
                                else if (parameter.name == "CN_MOUTH_MORPH_CANCEL_ENABLE")
                                {
                                    branch.MouthMorphCancelerEnabled = parameter.value > 0;
                                }
                            }
                        }
                    }

                    // motion
                    if (branch.Conditions.Any(x => x == new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals)) && item.transition.destinationState.timeParameterActive)
                    {
                        branch.SetAnimation(GetFirstFrame(item.transition.destinationState.motion), BranchAnimationType.Base);
                        branch.SetAnimation(GetLastFrame(item.transition.destinationState.motion), BranchAnimationType.Left);
                        branch.IsLeftTriggerUsed = true;
                    }
                    else if (branch.Conditions.Any(x =>  x == new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)) && item.transition.destinationState.timeParameterActive)
                    {
                        branch.SetAnimation(GetFirstFrame(item.transition.destinationState.motion), BranchAnimationType.Base);
                        branch.SetAnimation(GetLastFrame(item.transition.destinationState.motion), BranchAnimationType.Right);
                        branch.IsRightTriggerUsed = true;
                    }
                    else
                    {
                        branch.SetAnimation(GetLastFrame(item.transition.destinationState.motion), BranchAnimationType.Base);
                    }

                    mode.Add(branch);
                }

                if (mode.Any())
                {
                    modes.Add(mode);
                }
            }

            for (int i = 0; i < modes.Count; i++)
            {
                var modeId = _menu.AddMode(Domain.Menu.RegisteredId);

                // TODO: localization
                _menu.ModifyModeProperties(modeId, displayName: $"Imported_{i + 1}");

                foreach (var branch in modes[i])
                {
                    AddBranch(modeId, branch);
                }
            }
        }

        private Condition EmoteIndexToCondition(int emoteIndex)
        {
            switch (emoteIndex % 14)
            {
                case 0:
                    return new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals);
                case 1:
                    return new Condition(Hand.Right, HandGesture.HandOpen, ComparisonOperator.Equals);
                case 2:
                    return new Condition(Hand.Right, HandGesture.Fingerpoint, ComparisonOperator.Equals);
                case 3:
                    return new Condition(Hand.Right, HandGesture.Victory, ComparisonOperator.Equals);
                case 4:
                    return new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals);
                case 5:
                    return new Condition(Hand.Right, HandGesture.HandGun, ComparisonOperator.Equals);
                case 6:
                    return new Condition(Hand.Right, HandGesture.ThumbsUp, ComparisonOperator.Equals);
                case 7:
                    return new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals);
                case 8:
                    return new Condition(Hand.Left, HandGesture.HandOpen, ComparisonOperator.Equals);
                case 9:
                    return new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals);
                case 10:
                    return new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.Equals);
                case 11:
                    return new Condition(Hand.Left, HandGesture.RockNRoll, ComparisonOperator.Equals);
                case 12:
                    return new Condition(Hand.Left, HandGesture.HandGun, ComparisonOperator.Equals);
                case 13:
                    return new Condition(Hand.Left, HandGesture.ThumbsUp, ComparisonOperator.Equals);
                default:
                    return null;
            }
        }
    }
}
