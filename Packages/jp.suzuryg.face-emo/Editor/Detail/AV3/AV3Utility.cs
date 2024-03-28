using AnimatorAsCode.V0;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using VRC.SDKBase;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Components.Settings;
using VRC.SDK3.Avatars.Components;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public class AV3Utility
    {
        public static void SetLayerWeight(AnimatorController animatorController, string layerName, float weight)
        {
            var layers = animatorController.layers;
            var layerCount = layers.Where(x => x.name == layerName).Count();
            if (layerCount == 1)
            {
                var layerIndex = layers.ToList().FindIndex(x => x.name == layerName);
                layers[layerIndex].defaultWeight = weight;
                animatorController.layers = layers;
            }
            else
            {
                throw new FaceEmoException($"Num of layers which has name {layerName} was not 1: {layerCount}");
            }
        }

        public static void SetMotion(AnimatorController animatorController, string layerName, string stateName, Motion motion)
        {
            var layers = animatorController.layers;
            var layerCount = layers.Where(x => x.name == layerName).Count();
            if (layerCount == 1)
            {
                var layerIndex = layers.ToList().FindIndex(x => x.name == layerName);
                var states = layers[layerIndex].stateMachine.states;
                var stateCount = states.Where(x => x.state.name == stateName).Count();
                if (stateCount == 1)
                {
                    var stateIndex = layers[layerIndex].stateMachine.states.ToList().FindIndex(x => x.state.name == stateName);
                    layers[layerIndex].stateMachine.states[stateIndex].state.motion = motion;
                    animatorController.layers = layers;
                }
                else
                {
                    throw new FaceEmoException($"Num of states which has name {stateName} was not 1: {stateCount}");
                }
            }
            else
            {
                throw new FaceEmoException($"Num of layers which has name {layerName} was not 1: {layerCount}");
            }
        }

        public static void RemoveLayer(AnimatorController animatorController, string layerName)
        {
            var layers = animatorController.layers;
            var layerCount = layers.Where(x => x.name == layerName).Count();
            if (layerCount == 1)
            {
                var removed = layers.Where(x => x.name != layerName);
                animatorController.layers = removed.ToArray();
            }
            else
            {
                throw new FaceEmoException($"Num of layers which has name {layerName} was not 1: {layerCount}");
            }
        }

        public static void MoveLayer(AnimatorController animatorController, string layerName, int newIndex)
        {
            var layers = animatorController.layers;
            var layerCount = layers.Where(x => x.name == layerName).Count();
            if (layerCount == 1)
            {
                var target = layers.Where(x => x.name == layerName).First();
                var removed = layers.Where(x => x.name != layerName).ToList();
                removed.Insert(newIndex, target);
                animatorController.layers = removed.ToArray();
            }
            else
            {
                throw new FaceEmoException($"Num of layers which has name {layerName} was not 1: {layerCount}");
            }
        }

        public static VRC_AnimatorTrackingControl.TrackingType ConvertEyeTrackingType(EyeTrackingControl eyeTrackingControl)
        {
            switch (eyeTrackingControl)
            {
                case EyeTrackingControl.Tracking:
                    return VRC_AnimatorTrackingControl.TrackingType.Tracking;
                case EyeTrackingControl.Animation:
                    return VRC_AnimatorTrackingControl.TrackingType.Animation;
                default:
                    throw new FaceEmoException("Unknown tracking type.");
            }
        }

        public static VRC_AnimatorTrackingControl.TrackingType ConvertMouthTrackingType(MouthTrackingControl mouthTrackingControl)
        {
            switch (mouthTrackingControl)
            {
                case MouthTrackingControl.Tracking:
                    return VRC_AnimatorTrackingControl.TrackingType.Tracking;
                case MouthTrackingControl.Animation:
                    return VRC_AnimatorTrackingControl.TrackingType.Animation;
                default:
                    throw new FaceEmoException("Unknown tracking type.");
            }
        }

        public static (AnimationClip clip, string name) GetAnimationClipWithName(Domain.Animation animation)
        {
            if (animation is Domain.Animation && animation.GUID is string && AssetDatabase.GUIDToAssetPath(animation.GUID) is string path)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                var name = Path.GetFileName(path).Replace(".anim", "");
                return (clip, name);
            }
            else
            {
                return (null, null);
            }
        }

        public static int GetPreselectEmoteIndex(HandGesture left, HandGesture right)
        {
            // Left is row index, right is column index, return row-major index
            var numOfGesture = AV3Constants.EmoteSelectToGesture.Count;
            return (int)left * numOfGesture + (int)right;
        }

        public static SkinnedMeshRenderer GetFaceMesh(VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor == null) { return null; }

            SkinnedMeshRenderer faceMesh = null;
            // Get from lipSync
            if (avatarDescriptor.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape &&
                avatarDescriptor.VisemeSkinnedMesh != null)
            {
                faceMesh = avatarDescriptor.VisemeSkinnedMesh;
            }
            // Get from eyelids
            else if (avatarDescriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh != null)
            {
                faceMesh = avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh;
            }
            // Get from body object
            else if (avatarDescriptor.gameObject != null && avatarDescriptor.gameObject.transform != null)
            {
                var avatarRoot = avatarDescriptor.gameObject.transform;
                for (int i = 0; i < avatarRoot.childCount; i++)
                {
                    var child = avatarRoot.GetChild(i);
                    if (child != null && child.name == "Body")
                    {
                        faceMesh = child.GetComponent<SkinnedMeshRenderer>();
                        if (faceMesh != null) { break; }
                    }
                }
            }
            return faceMesh;
        }

        public static HashSet<BlendShape> GetBlendShapesToBeExcluded(VRCAvatarDescriptor avatarDescriptor, bool excludeBlink, bool excludeLipSync)
        {
            HashSet<BlendShape> toBeExcluded = new HashSet<BlendShape>();

            // Exclude shape key for blinking when using AvatarDescriptor's blink feature
            if (excludeBlink)
            {
                toBeExcluded = new HashSet<BlendShape>(GetEyeLidsBlendShapes(avatarDescriptor));
            }

            // Exclude shape key for lip-sync
            if (excludeLipSync)
            {
                foreach (var blendShape in GetLipSyncBlendShapes(avatarDescriptor))
                {
                    toBeExcluded.Add(blendShape);
                }
            }

            return toBeExcluded;
        }

        // FIXME: This method should be merged with GetEyeLidsBlendShapes().
        public static BlendShape GetBlinkBlendShape(VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor != null &&
                avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes != null &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh != null &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.sharedMesh != null)
            {
                var skinnedMesh = avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh;
                var transformPath = GetPathFromAvatarRoot(skinnedMesh.transform, avatarDescriptor);
                if (transformPath == null) { return null; }

                if (avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes.Length > 0)
                {
                    var index = avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes[0];
                    if (0 <= index && index < skinnedMesh.sharedMesh.blendShapeCount)
                    {
                        var name = skinnedMesh.sharedMesh.GetBlendShapeName(index);
                        return new BlendShape(path: transformPath, name: name);
                    }
                }
            }
            return null;
        }

        // FIXME: Refactoring is needed because of confusion between "blink" and "eyelid" blend shapes in this method calls.
        public static List<BlendShape> GetEyeLidsBlendShapes(VRCAvatarDescriptor avatarDescriptor)
        {
            var ret = new List<BlendShape>();
            if (avatarDescriptor != null &&
                avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes != null &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh != null &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.sharedMesh != null)
            {
                var skinnedMesh = avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh;
                var transformPath = GetPathFromAvatarRoot(skinnedMesh.transform, avatarDescriptor);
                if (transformPath == null) { return ret; }

                foreach (var index in avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes)
                {
                    if (0 <= index && index < skinnedMesh.sharedMesh.blendShapeCount)
                    {
                        var name = skinnedMesh.sharedMesh.GetBlendShapeName(index);
                        ret.Add(new BlendShape(path: transformPath, name: name));
                    }
                }
            }
            return ret;
        }

        public static List<BlendShape> GetLipSyncBlendShapes(VRCAvatarDescriptor avatarDescriptor)
        {
            var ret = new List<BlendShape>();

            if (avatarDescriptor != null &&
                avatarDescriptor.VisemeSkinnedMesh != null &&
                avatarDescriptor.VisemeBlendShapes is string[])
            {
                var skinnedMesh = avatarDescriptor.VisemeSkinnedMesh;
                var transformPath = GetPathFromAvatarRoot(skinnedMesh.transform, avatarDescriptor);
                if (transformPath == null) { return ret; }

                foreach (var name in avatarDescriptor.VisemeBlendShapes)
                {
                    ret.Add(new BlendShape(path: transformPath, name: name));
                }
            }

            return ret;
        }

        public static string GetPathFromAvatarRoot(Transform transform, VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor == null || avatarDescriptor.gameObject == null) { return null; }
            var animator = avatarDescriptor.gameObject.GetComponent<Animator>();
            if (animator == null) { return null; }
            return AnimationUtility.CalculateTransformPath(transform, animator.transform);
        }

        public static SkinnedMeshRenderer GetMeshByPath(string pathFromAvatarRoot, VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor == null || avatarDescriptor.gameObject == null) { return null; }

            var animator = avatarDescriptor.gameObject.GetComponent<Animator>();
            if (animator == null) { return null; }

            var transform = animator.transform.Find(pathFromAvatarRoot);
            if (transform == null || transform.gameObject == null) { return null; }

            return transform.gameObject.GetComponent<SkinnedMeshRenderer>();
        }

        public static Dictionary<BlendShape, float> GetBlendShapeValues(SkinnedMeshRenderer skinnedMeshRenderer, VRCAvatarDescriptor avatarDescriptor, bool excludeBlink, bool excludeLipSync)
        {
            var values = new Dictionary<BlendShape, float>();

            if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null) { return values; }
            var transformPath = GetPathFromAvatarRoot(skinnedMeshRenderer.transform, avatarDescriptor);
            if (transformPath == null) { return values; }

            var toBeExcluded = GetBlendShapesToBeExcluded(avatarDescriptor, excludeBlink, excludeLipSync);

            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
            {
                var name = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
                var blendShape = new BlendShape(path: transformPath, name: name);

                var excluded = toBeExcluded.Contains(blendShape);
                if (excludeLipSync && IsLipSyncBlendShapeName(blendShape.Name)) { excluded = true; }

                if (!excluded)
                {
                    var weight = skinnedMeshRenderer.GetBlendShapeWeight(i);
                    values[blendShape] = weight;
                }
            }

            return values;
        }

        public static Dictionary<BlendShape, float> GetFaceMeshBlendShapeValues(VRCAvatarDescriptor avatarDescriptor, bool excludeBlink, bool excludeLipSync)
        {
            return GetBlendShapeValues(GetFaceMesh(avatarDescriptor), avatarDescriptor, excludeBlink, excludeLipSync);
        }

        public static string ConvertNameToSafePath(string name)
        {
            StringBuilder sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (char.IsControl(c) || char.IsWhiteSpace(c) ||
                    c == '/' || c == '\\' || c == ':' || c == '*' || c == '?' || c == '"' || c == '<' || c == '>' || c == '|' ||
                    c == '%' || c == '#' || c == '{' || c == '}' || c == '[' || c == ']' || c == '`' || c == '^')
                {
                    sb.Append('_'); // Replace invalid characters with an underscore or any other suitable character
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        // https://hacchi-man.hatenablog.com/entry/2020/08/23/220000
        public static void CreateFolderRecursively(string path)
        {
            // If it doesn't start with Assets, it can't be processed.
            if (!path.StartsWith("Assets/"))
            {
                return;
            }
         
            // AssetDatabase, so the delimiter is /.
            var dirs = path.Split('/');
            var combinePath = dirs[0];

            // Skip the Assets part.
            foreach (var dir in dirs.Skip(1))
            {
                // Check existence of directory
                if (!AssetDatabase.IsValidFolder(combinePath + '/' + dir))
                {
                    AssetDatabase.CreateFolder(combinePath, dir);
                }
                combinePath += '/' + dir;
            }
        }

        public static Animator GetAnimator(AV3Setting aV3Setting)
        {
            // Error occurs when returning from Play mode to Edit mode if Null conditional operator is used to determine Null
            if (aV3Setting == null || aV3Setting.TargetAvatar == null)
            {
                return null;
            }
            return aV3Setting.TargetAvatar.GetComponent<Animator>();
        }

        private static HumanPose _poseCache;
        private static AnimationClip _poseClipCache;
        public static AnimationClip GetAvatarPoseClip(VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor == null) { return null; }
            var clonedAvatar = UnityEngine.Object.Instantiate(avatarDescriptor.gameObject);
            try
            {
                clonedAvatar.transform.localPosition = Vector3.zero;
                clonedAvatar.transform.localRotation = Quaternion.identity;
                // FIXME: Unable to support the case that avatar's body shape balance is tuned by root object's scale. (Is it necessary to assume this case...?)
                clonedAvatar.transform.localScale = Vector3.one;

                var animator = clonedAvatar.GetComponent<Animator>();
                if (animator == null || !animator.isHuman) { return null; }

                var humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
                var humanPose = new HumanPose();
                humanPoseHandler.GetHumanPose(ref humanPose);

                if (_poseClipCache == null ||
                    !ArePosesSimilar(humanPose, _poseCache, float.Epsilon, float.Epsilon, float.Epsilon))
                {
                    _poseCache = humanPose;
                    _poseClipCache = new AnimationClip() { legacy = false };
                    SetDefaultTransformToClip(_poseClipCache);
                    SetHumanPoseToClip(ref humanPose, _poseClipCache);
                }

                return _poseClipCache;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clonedAvatar);
            }
        }

        private static bool ArePosesSimilar(HumanPose pose1, HumanPose pose2, float positionThreshold, float rotationThreshold, float muscleThreshold)
        {
            // Compare body position
            if (Vector3.Distance(pose1.bodyPosition, pose2.bodyPosition) > positionThreshold)
            {
                return false;
            }

            // Compare body rotation
            if (Quaternion.Angle(pose1.bodyRotation, pose2.bodyRotation) > rotationThreshold)
            {
                return false;
            }

            // Compare muscle values
            for (int i = 0; i < pose1.muscles.Length; i++)
            {
                if (Mathf.Abs(pose1.muscles[i] - pose2.muscles[i]) > muscleThreshold)
                {
                    return false;
                }
            }

            // If none of the values are beyond the thresholds, the poses are considered similar
            return true;
        }

        private static void SetDefaultTransformToClip(AnimationClip clip)
        {
            // FIXME: Scale change is reflected correctly in AnimationWindow preview, but not in AnimationMode sampling, so avatar scale must be changed directly.
            // This scale change is useless because it is not reflected, but leave it for now.
            clip.SetCurve("", typeof(Transform), "m_LocalScale.x", new AnimationCurve(new Keyframe(time: 0, value: 1)));
            clip.SetCurve("", typeof(Transform), "m_LocalScale.y", new AnimationCurve(new Keyframe(time: 0, value: 1)));
            clip.SetCurve("", typeof(Transform), "m_LocalScale.z", new AnimationCurve(new Keyframe(time: 0, value: 1)));

            clip.SetCurve("", typeof(Animator), "MotionT.x", new AnimationCurve(new Keyframe(time: 0, value: 0)));
            clip.SetCurve("", typeof(Animator), "MotionT.y", new AnimationCurve(new Keyframe(time: 0, value: 0)));
            clip.SetCurve("", typeof(Animator), "MotionT.z", new AnimationCurve(new Keyframe(time: 0, value: 0)));
        }

        private static void SetHumanPoseToClip(ref HumanPose humanPose, AnimationClip clip)
        {
            // Set body position and rotation
#if UNITY_2022_1_OR_NEWER
            clip.SetCurve("", typeof(Animator), "RootT.x", new AnimationCurve(new Keyframe(0, humanPose.bodyPosition.x)));
            clip.SetCurve("", typeof(Animator), "RootT.y", new AnimationCurve(new Keyframe(0, humanPose.bodyPosition.y)));
            clip.SetCurve("", typeof(Animator), "RootT.z", new AnimationCurve(new Keyframe(0, humanPose.bodyPosition.z)));
#else
            clip.SetCurve("", typeof(Animator), "RootT.x", new AnimationCurve(new Keyframe(0, humanPose.bodyPosition.normalized.x)));
            clip.SetCurve("", typeof(Animator), "RootT.y", new AnimationCurve(new Keyframe(0, humanPose.bodyPosition.normalized.y)));
            clip.SetCurve("", typeof(Animator), "RootT.z", new AnimationCurve(new Keyframe(0, humanPose.bodyPosition.normalized.z)));
#endif

            clip.SetCurve("", typeof(Animator), "RootQ.x", new AnimationCurve(new Keyframe(0, humanPose.bodyRotation.x)));
            clip.SetCurve("", typeof(Animator), "RootQ.y", new AnimationCurve(new Keyframe(0, humanPose.bodyRotation.y)));
            clip.SetCurve("", typeof(Animator), "RootQ.z", new AnimationCurve(new Keyframe(0, humanPose.bodyRotation.z)));
            clip.SetCurve("", typeof(Animator), "RootQ.w", new AnimationCurve(new Keyframe(0, humanPose.bodyRotation.w)));

            // Set muscle values
            for (int i = 0; i < humanPose.muscles.Length; i++)
            {
                string muscleName = HumanTrait.MuscleName[i];
                float muscleValue = humanPose.muscles[i];
                clip.SetCurve("", typeof(Animator), muscleName, new AnimationCurve(new Keyframe(0, muscleValue)));
            }
        }

        public static AnimationClip SynthesizeClip(AnimationClip baseClip, AnimationClip additionalClip)
        {
            var synthesized = new AnimationClip();

            if (baseClip != null) { EditorUtility.CopySerialized(baseClip, synthesized); }

            if (additionalClip != null)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(additionalClip))
                {
                    var curve = AnimationUtility.GetEditorCurve(additionalClip, binding);
                    AnimationUtility.SetEditorCurve(synthesized, binding, curve);
                }
            }

            return synthesized;
        }

        public static AnimationClip SynthesizeAvatarPose(AnimationClip animationClip, VRCAvatarDescriptor avatarDescriptor) => SynthesizeClip(GetAvatarPoseClip(avatarDescriptor), animationClip);

        public static void CombineExpressions(AnimationClip leftHand, AnimationClip rightHand, AnimationClip destination)
        {
            var leftTransforms = new Dictionary<string, List<(EditorCurveBinding binding, float value)>>();
            var rightTransforms = new Dictionary<string, List<(EditorCurveBinding binding, float value)>>();

            if (leftHand != null)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(leftHand))
                {
                    var leftCurve = AnimationUtility.GetEditorCurve(leftHand, binding);

                    var leftValue = leftCurve != null && leftCurve.keys.Length > 0 ? leftCurve.keys.Last().value : 0;

                    if (binding.type == typeof(Transform))
                    {
                        if (!leftTransforms.ContainsKey(binding.path)) { leftTransforms[binding.path] = new List<(EditorCurveBinding binding, float value)>(); }
                        leftTransforms[binding.path].Add((binding, leftValue));
                        continue;
                    }

                    var newCurve = new AnimationCurve(new Keyframe(time: 0, value: leftValue));
                    AnimationUtility.SetEditorCurve(destination, binding, newCurve);
                }
            }

            if (rightHand != null)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(rightHand)) 
                {
                    var rightCurve = AnimationUtility.GetEditorCurve(rightHand, binding);
                    var oldCurve = AnimationUtility.GetEditorCurve(destination, binding);

                    var rightValue = rightCurve != null && rightCurve.keys.Length > 0 ? rightCurve.keys.Last().value : 0;
                    var oldValue = oldCurve != null && oldCurve.keys.Length > 0 ? oldCurve.keys.Last().value : 0;

                    if (binding.type == typeof(Transform))
                    {
                        if (!rightTransforms.ContainsKey(binding.path)) { rightTransforms[binding.path] = new List<(EditorCurveBinding binding, float value)>(); }
                        rightTransforms[binding.path].Add((binding, rightValue));
                        continue;
                    }

                    var newCurve = new AnimationCurve(new Keyframe(time: 0, value: Math.Max(rightValue, oldValue)));
                    AnimationUtility.SetEditorCurve(destination, binding, newCurve);
                }
            }

            // TODO: Priority should be determined based on the difference from the default value.
            // For the time being, priority is given to the one selected later.
            // (When called from the gesture table, the right hand is the second argument, and in many cases, the right hand has priority for expression control.)
            var combinedTransforms = leftTransforms;
            foreach (var key in rightTransforms.Keys)
            {
                combinedTransforms[key] = rightTransforms[key];
            }

            foreach (var list in combinedTransforms.Values)
            {
                foreach (var pair in list)
                {
                    var newCurve = new AnimationCurve(new Keyframe(time: 0, value: pair.value));
                    AnimationUtility.SetEditorCurve(destination, pair.binding, newCurve);
                }
            }
        }

        public static List<ModeEx> FlattenMenuItemList(IMenuItemList menuItemList, ModeNameProvider modeNameProvider)
        {
            var ret = new List<ModeEx>();
            FlattenMenuItemListSub(menuItemList, ret, string.Empty, modeNameProvider);

            var branchCount = 0;
            foreach (var mode in ret)
            {
                mode.DefaultEmoteIndex = branchCount;
                branchCount += mode.Mode.Branches.Count + 1;
            }
            return ret;
        }

        private static void FlattenMenuItemListSub(IMenuItemList menuItemList, List<ModeEx> flattened, string pathToParent, ModeNameProvider modeNameProvider)
        {
            foreach (var id in menuItemList.Order)
            {
                if (menuItemList.GetType(id) == MenuItemType.Mode)
                {
                    var mode = menuItemList.GetMode(id);
                    flattened.Add(new ModeEx() { PathToMode = pathToParent + modeNameProvider.Provide(mode), Mode = new ModeExInner(mode) });
                }
                else
                {
                    var group = menuItemList.GetGroup(id);
                    FlattenMenuItemListSub(group, flattened, pathToParent + group.DisplayName + "/", modeNameProvider);
                }
            }
        }

        public static int? GetActualRegisteredListFreeSpace(IMenu menu, AV3Setting av3Setting)
        {
            var freeSpace = menu?.Registered?.FreeSpace;
            if (!freeSpace.HasValue) { return null; }

            if (av3Setting.AddConfig_EmoteSelect) { freeSpace--; }
            return freeSpace;
        }

        public static int GetActualRegisteredListCapacity(AV3Setting av3Setting)
        {
            var capacity = Domain.RegisteredMenuItemList.Capacity;
            if (av3Setting.AddConfig_EmoteSelect) { capacity--; }
            return capacity;
        }

        public static bool IsLipSyncBlendShapeName(string blendShapeName)
        {
            var lowerCase = blendShapeName.ToLower();

            return lowerCase.Contains("vrc.v_sil") ||
                lowerCase.Contains("vrc.v_pp") ||
                lowerCase.Contains("vrc.v_ff") ||
                lowerCase.Contains("vrc.v_th") ||
                lowerCase.Contains("vrc.v_dd") ||
                lowerCase.Contains("vrc.v_kk") ||
                lowerCase.Contains("vrc.v_ch") ||
                lowerCase.Contains("vrc.v_ss") ||
                lowerCase.Contains("vrc.v_nn") ||
                lowerCase.Contains("vrc.v_rr") ||
                lowerCase.Contains("vrc.v_aa") ||
                lowerCase.Contains("vrc.v_e") ||
                lowerCase.Contains("vrc.v_ih") ||
                lowerCase.Contains("vrc.v_oh") ||
                lowerCase.Contains("vrc.v_ou");
        }
    }
}
