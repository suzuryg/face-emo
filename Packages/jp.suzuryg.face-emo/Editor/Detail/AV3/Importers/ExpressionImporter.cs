﻿using System;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Detail.View.Element;
using Suzuryg.FaceEmo.Domain;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    public class ExpressionImporter
    {
        private Domain.Menu _menu;
        private AV3Setting _av3Setting;
        private string _assetDir;
        private IReadOnlyLocalizationSetting _localizationSetting;

        private Dictionary<BlendShape, float> _faceBlendShapesValues = new Dictionary<BlendShape, float>();
        private Dictionary<Motion, AnimationClip> _firstFrameCache = new Dictionary<Motion, AnimationClip>();
        private Dictionary<Motion, AnimationClip> _lastFrameCache = new Dictionary<Motion, AnimationClip>();

        private HashSet<string> _contactReceiversParamsInFx = new HashSet<string>();
        private HashSet<string> _physBoneParamsInFx = new HashSet<string>();

        private readonly Dictionary<Motion, Domain.Animation> _duplicatedAnimations = new();

        public ExpressionImporter(Domain.Menu menu, AV3Setting av3Setting, string assetDir, IReadOnlyLocalizationSetting localizationSetting)
        {
            _menu = menu;
            _av3Setting = av3Setting;
            _assetDir = assetDir;
            _localizationSetting = localizationSetting;
        }

        public List<IMode> ImportExpressionPatterns(VRCAvatarDescriptor avatarDescriptor)
        {
            _faceBlendShapesValues = ImportUtility.GetAllFaceBlendShapeValues(avatarDescriptor, _av3Setting, excludeBlink: false, excludeLipSync: true);

            _contactReceiversParamsInFx.Clear();
            foreach (var item in avatarDescriptor.gameObject.GetComponentsInChildren(typeof(VRCContactReceiver), includeInactive: true))
            {
                if (item is VRCContactReceiver contactReceiver &&
                    contactReceiver != null &&
                    !string.IsNullOrEmpty(contactReceiver.parameter))
                {
                    _contactReceiversParamsInFx.Add(contactReceiver.parameter);
                }
            }

            _physBoneParamsInFx.Clear();
            foreach (var item in avatarDescriptor.gameObject.GetComponentsInChildren(typeof(VRCPhysBoneBase), includeInactive: true))
            {
                if (item is VRCPhysBoneBase physBone &&
                    physBone != null &&
                    !string.IsNullOrEmpty(physBone.parameter))
                {
                    _physBoneParamsInFx.Add(physBone.parameter + "_IsGrabbed");
                    _physBoneParamsInFx.Add(physBone.parameter + "_IsPosed");
                    _physBoneParamsInFx.Add(physBone.parameter + "_Angle");
                    _physBoneParamsInFx.Add(physBone.parameter + "_Stretch");
                    _physBoneParamsInFx.Add(physBone.parameter + "_Squish");
                }
            }

            var fx = ImportUtility.GetFxLayer(avatarDescriptor);
            if (fx == null) { return new List<IMode>(); }

            var cacLayer = GetCacLayer(fx);
            if (cacLayer != null)
            {
                return ImportCac(cacLayer);
            }
            else
            {
                return ImportNormal(fx);
            }
        }

        public (AnimationClip blink, AnimationClip mouthMorphCancel) ImportOptionalClips(VRCAvatarDescriptor avatarDescriptor)
        {
            _faceBlendShapesValues = ImportUtility.GetAllFaceBlendShapeValues(avatarDescriptor, _av3Setting, excludeBlink: false, excludeLipSync: true);

            var fx = ImportUtility.GetFxLayer(avatarDescriptor);
            if (fx == null) { return (null, null); }

            var blink = ImportBlinkClip(fx);
            var mouthMorphCancel = ImportMouthMorphCancelClip(fx);

            return (blink, mouthMorphCancel);
        }

        private List<IMode> ImportNormal(AnimatorController fx)
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
            _menu.ModifyModeProperties(modeId, displayName: _localizationSetting.Table.ExpressionImporter_ExpressionPattern + "1");

            var unusedBranches = new List<IBranch>();
            foreach (var layer in layers)
            {
                foreach (var branch in layer)
                {
                    if (branch.Conditions.Any())
                    {
                        AddBranch(modeId, branch);
                    }
                    else
                    {
                        unusedBranches.Add(branch);
                    }
                }
            }

            foreach (var branch in unusedBranches)
            {
                if (branch.BaseAnimation is Domain.Animation &&
                    !_menu.GetMode(modeId).Branches.Any(x =>
                        x.BaseAnimation?.GUID == branch.BaseAnimation.GUID ||
                        (x.IsLeftTriggerUsed && x.LeftHandAnimation?.GUID == branch.BaseAnimation.GUID) ||
                        (x.IsRightTriggerUsed && x.RightHandAnimation?.GUID == branch.BaseAnimation.GUID)))
                {
                    AddBranch(modeId, branch);
                }
            }

            var importedPatterns = new List<IMode>();
            var mode = _menu.GetMode(modeId);
            if (mode.Branches.Any())
            {
                importedPatterns.Add(mode);
            }
            else
            {
                _menu.RemoveMenuItem(modeId);
            }
            return importedPatterns;
        }

        private List<IBranch> GetBranches(AnimatorStateMachine stateMachine)
        {
            var branches = new List<IBranch>();
            if (stateMachine == null) { return branches; }

            var defaultTransition = new AnimatorTransition();
            defaultTransition.destinationState = stateMachine.defaultState;
            branches.Add(GetBranch(defaultTransition));

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
                transition.destinationState != null &&
                ImportUtility.IsFaceMotion(transition.destinationState.motion, _faceBlendShapesValues.Select(blendShape => blendShape.Key)))
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

                    // "NotEqual" is not normally used in expression transition conditions
                    ComparisonOperator comparisonOperator;
                    if (condition.mode == AnimatorConditionMode.Equals) { comparisonOperator = ComparisonOperator.Equals; }
                    else { continue; }

                    branch.AddCondition(new Condition(hand, handGesture, comparisonOperator));
                }

                if (!branch.Conditions.Any())
                {
                    // exclude face toggles
                    var boolConditions = transition.conditions.Where(x => x.mode == AnimatorConditionMode.If || x.mode == AnimatorConditionMode.IfNot);
                    if (boolConditions.Any() && transition.conditions.Count() == boolConditions.Count())
                    {
                        return null;
                    }

                    // exclude contact expressions
                    if (transition.conditions.Any(x => _contactReceiversParamsInFx.Contains(x.parameter)))
                    {
                        return null;
                    }

                    // exclude phys-bone expressions
                    if (transition.conditions.Any(x => _physBoneParamsInFx.Contains(x.parameter)) ||
                        (transition.destinationState.timeParameterActive && _physBoneParamsInFx.Contains(transition.destinationState.timeParameter)))
                    {
                        return null;
                    }
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
                    branch.SetAnimation(GetDuplicatedAnimation(transition.destinationState.motion), BranchAnimationType.Base);
                }

                if (branch.BaseAnimation is Domain.Animation ||
                    branch.LeftHandAnimation is Domain.Animation ||
                    branch.RightHandAnimation is Domain.Animation ||
                    branch.BothHandsAnimation is Domain.Animation)
                {
                    return branch;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
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
                    var accepted = new List<EditorCurveBinding>();
                    foreach (var binding in bindings)
                    {

                        var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                        if (curve != null && curve.keys.Length > 0)
                        {
                            var value = curve.keys.First().value;

                            var blendShape = new BlendShape(binding.path, binding.propertyName.Replace("blendShape.", string.Empty));
                            if (_faceBlendShapesValues.ContainsKey(blendShape) && Mathf.Approximately(_faceBlendShapesValues[blendShape], value)) { continue; }

                            AnimationUtility.SetEditorCurve(firstFrame, binding, new AnimationCurve(new Keyframe(time: 0, value: value)));
                            accepted.Add(binding);
                        }
                    }
                    if (!accepted.Any())
                    {
                        return null;
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
                    var accepted = new List<EditorCurveBinding>();
                    foreach (var binding in bindings)
                    {
                        var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                        if (curve != null && curve.keys.Length > 0)
                        {
                            var value = curve.keys.Last().value;

                            var blendShape = new BlendShape(binding.path, binding.propertyName.Replace("blendShape.", string.Empty));
                            if (_faceBlendShapesValues.ContainsKey(blendShape) && Mathf.Approximately(_faceBlendShapesValues[blendShape], value)) { continue; }

                            AnimationUtility.SetEditorCurve(lastFrame, binding, new AnimationCurve(new Keyframe(time: 0, value: value)));
                            accepted.Add(binding);
                        }
                    }
                    if (!accepted.Any())
                    {
                        return null;
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

        private Domain.Animation GetDuplicatedAnimation(Motion motion)
        {
            if (motion == null) return null;
            if (_duplicatedAnimations.TryGetValue(motion, out var cached)) return cached;

            if (motion is AnimationClip sourceClip)
            {
                if (sourceClip.name.Contains("blink", StringComparison.OrdinalIgnoreCase)) return null;
                if (!HasFaceDifferences(sourceClip)) return null;

                if (!AssetDatabase.IsValidFolder(_assetDir)) AV3Utility.CreateFolderRecursively(_assetDir);

                var baseName = string.IsNullOrEmpty(sourceClip.name) ? "NoName" : sourceClip.name;
                var fileName = AnimationElement.GetNewAnimationName(_assetDir, baseName);
                var destPath = $"{_assetDir}/{fileName}";

                var duplicatedClip = Object.Instantiate(sourceClip);
                duplicatedClip.name = Path.GetFileNameWithoutExtension(fileName);
                AssetDatabase.CreateAsset(duplicatedClip, destPath);

                var guid = AssetDatabase.AssetPathToGUID(destPath);
                if (!string.IsNullOrEmpty(guid))
                {
                    var duplicated = new Domain.Animation(guid);
                    _duplicatedAnimations[motion] = duplicated;
                    return duplicated;
                }
            }

            return GetLastFrame(motion);

            bool HasFaceDifferences(AnimationClip animationClip)
            {
                var bindings = AnimationUtility.GetCurveBindings(animationClip);
                foreach (var binding in bindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                    if (curve == null || curve.keys.Length <= 0) continue;

                    var blendShape = new BlendShape(binding.path,
                        binding.propertyName.Replace("blendShape.", string.Empty));
                    if (!_faceBlendShapesValues.TryGetValue(blendShape, out var faceValue)) continue;

                    if (curve.keys.Any(x => !Mathf.Approximately(x.value, faceValue))) return true;
                }
                return false;
            }
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
                    layer.stateMachine.stateMachines.SelectMany(x => x.stateMachine.entryTransitions).Any(y => y.conditions.Any() && y.conditions.First().parameter == "SYNC_EM_EMOTE"))
                {
                    return layer;
                }
            }
            return null;
        }

        private List<IMode> ImportCac(AnimatorControllerLayer cacLayer)
        {
            var transitions = cacLayer.stateMachine.stateMachines
                .SelectMany(x => x.stateMachine.entryTransitions)
                .Where(x => 
                    !x.mute &&
                    x.conditions.Length == 1 &&
                    x.conditions.First().parameter == "SYNC_EM_EMOTE" &&
                    x.destinationState != null &&
                    ImportUtility.IsFaceMotion(x.destinationState.motion, _faceBlendShapesValues.Select(blendShape => blendShape.Key)))
                .Select(x => (emoteIndex: (int)x.conditions.First().threshold - 1, transition: x))
                .Where(x => x.emoteIndex >= 0)
                .OrderBy(x => x.emoteIndex);
            if (!transitions.Any()) { return new List<IMode>(); }

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
                        branch.SetAnimation(GetDuplicatedAnimation(item.transition.destinationState.motion), BranchAnimationType.Base);
                    }

                    mode.Add(branch);
                }

                if (mode.Any())
                {
                    modes.Add(mode);
                }
            }

            var results = new List<IMode>();
            for (int i = 0; i < modes.Count; i++)
            {
                var modeId = _menu.AddMode(Domain.Menu.RegisteredId);

                _menu.ModifyModeProperties(modeId, displayName: $"{_localizationSetting.Table.ExpressionImporter_ExpressionPattern}{i + 1}");

                foreach (var branch in modes[i])
                {
                    AddBranch(modeId, branch);
                }

                results.Add(_menu.GetMode(modeId));
            }

            return results;
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

        private AnimationClip ImportBlinkClip(AnimatorController fx)
        {
            foreach (var layer in fx.layers)
            {
                if (Regex.IsMatch(layer.name, @"\bblink\b", RegexOptions.IgnoreCase))
                {
                    foreach (var state in layer.stateMachine.states)
                    {
                        if (state.state.motion is AnimationClip animationClip && IsBlinkClip(animationClip))
                        {
                            var clone = new AnimationClip();
                            EditorUtility.CopySerialized(animationClip, clone);
                            SaveClip(clone, clone.name);

                            _av3Setting.UseBlinkClip = true;
                            _av3Setting.BlinkClip = clone;
                            return clone;
                        }
                    }
                }
            }
            return null;
        }

        private bool IsBlinkClip(AnimationClip animationClip)
        {
            const float blendShapeValueThreshold = 10;

            return animationClip.isLooping &&
                ImportUtility.IsFaceMotion(animationClip, _faceBlendShapesValues.Select(blendShape => blendShape.Key)) &&
                AnimationUtility.GetCurveBindings(animationClip).Any(binding =>
                    binding.type == typeof(SkinnedMeshRenderer) &&
                    binding.propertyName.StartsWith("blendShape.") &&
                    AnimationUtility.GetEditorCurve(animationClip, binding) is AnimationCurve curve &&
                    curve.length >= 3 &&
                    curve.keys.Any(x => x.value >= blendShapeValueThreshold));
        }

        private AnimationClip ImportMouthMorphCancelClip(AnimatorController fx)
        {
            var blendShapes = _faceBlendShapesValues.Select(blendShape => blendShape.Key);

            var patterns = new List<(string layerName, string stateName)>()
            {
                (@"\bmouth\s*morph\s*canceller\b", @"^enable$"),
                (@"^lipsync\s*override$", @"^lipsyncing$"),
                (@"^lipSynk$", @"^mouse0$"),
                (@"^lipSync\s*control$", @"^lipsyncon.*$"),
            };

            foreach (var pattern in patterns)
            {
                foreach (var matched in fx.layers
                    .Where(layer => Regex.IsMatch(layer.name, pattern.layerName, RegexOptions.IgnoreCase))
                    .SelectMany(layer => layer.stateMachine.states)
                    .Where(state => Regex.IsMatch(state.state.name, pattern.stateName, RegexOptions.IgnoreCase)))
                {
                    if (matched.state.motion is AnimationClip animationClip &&
                        ImportUtility.IsFaceMotion(animationClip, blendShapes) &&
                        ImportUtility.AreAllValuesZero(animationClip, blendShapes))
                    {
                        var clone = new AnimationClip();
                        EditorUtility.CopySerialized(animationClip, clone);
                        SaveClip(clone, clone.name);

                        _av3Setting.UseMouthMorphCancelClip = true;
                        _av3Setting.MouthMorphCancelClip = clone;
                        return clone;
                    }
                }
            }
            return null;
        }
    }
}
