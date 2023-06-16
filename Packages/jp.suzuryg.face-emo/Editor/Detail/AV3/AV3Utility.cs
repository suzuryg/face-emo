using AnimatorAsCode.V0;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using System.IO;
using VRC.SDKBase;
using Suzuryg.FaceEmo.Domain;
using VRC.SDK3.Avatars.Components;
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

        public static HashSet<string> GetBlendShapeNamesToBeExcluded(VRCAvatarDescriptor avatarDescriptor, bool excludeBlink, bool excludeLipSync)
        {
            HashSet<string> toBeExcluded = new HashSet<string>();

            // Exclude shape key for blinking when using AvatarDescriptor's blink feature
            if (excludeBlink)
            {
                toBeExcluded = new HashSet<string>(GetEyeLidsBlendShapes(avatarDescriptor));
            }

            // Exclude shape key for lip-sync
            if (excludeLipSync)
            {
                foreach (var name in GetLipSyncBlendShapes(avatarDescriptor))
                {
                    toBeExcluded.Add(name);
                }
            }

            return toBeExcluded;
        }

        public static List<string> GetEyeLidsBlendShapes(VRCAvatarDescriptor avatarDescriptor)
        {
            var ret = new List<string>();
            if (avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes != null &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh != null &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.sharedMesh != null)
            {
                var sharedMesh = avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.sharedMesh;
                foreach (var index in avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes)
                {
                    if (0 <= index && index < sharedMesh.blendShapeCount)
                    {
                        ret.Add(sharedMesh.GetBlendShapeName(index));
                    }
                }
            }
            return ret;
        }

        public static List<string> GetLipSyncBlendShapes(VRCAvatarDescriptor avatarDescriptor)
        {
            var ret = new List<string>();
            if (avatarDescriptor.VisemeBlendShapes is string[])
            {
                foreach (var name in avatarDescriptor.VisemeBlendShapes)
                {
                    ret.Add(name);
                }
            }
            return ret;
        }

        public static Dictionary<string, float> GetFaceMeshBlendShapes(VRCAvatarDescriptor avatarDescriptor, bool excludeBlink, bool excludeLipSync)
        {
            var faceMesh = GetFaceMesh(avatarDescriptor);
            var toBeExcluded = GetBlendShapeNamesToBeExcluded(avatarDescriptor, excludeBlink, excludeLipSync);

            // Get blendshape names and weights
            var blendShapes = new Dictionary<string, float>();
            if (faceMesh != null && faceMesh.sharedMesh != null)
            {
                for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
                {
                    var name = faceMesh.sharedMesh.GetBlendShapeName(i);

                    var excluded = toBeExcluded.Contains(name);
                    if (excludeLipSync && name.StartsWith("vrc.")) { excluded = true; }

                    if (!excluded)
                    {
                        var weight = faceMesh.GetBlendShapeWeight(i);
                        blendShapes[name] = weight;
                    }
                }
            }

            return blendShapes;
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

        public static AnimationClip GetAvatarPoseClip()
        {
            // TODO: Support for avatar posture other than T-pose
            var tPose = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(AV3Constants.GUID_TPoseClip));
            if (tPose == null)
            {
                Debug.LogError("T-pose animation clip not found.");
            }
            return tPose;
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

        public static AnimationClip SynthesizeAvatarPose(AnimationClip animationClip) => SynthesizeClip(GetAvatarPoseClip(), animationClip);

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

            freeSpace--; // Emote select or lock
            return freeSpace;
        }

        public static int GetActualRegisteredListCapacity(AV3Setting av3Setting)
        {
            var capacity = Domain.RegisteredMenuItemList.Capacity;
            capacity--; // Emote select or lock
            return capacity;
        }
    }
}
