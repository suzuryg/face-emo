using AnimatorAsCode.V0;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using System.IO;
using VRC.SDKBase;
using Suzuryg.FacialExpressionSwitcher.Domain;
using VRC.SDK3.Avatars.Components;
using System.Linq;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
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
                throw new FacialExpressionSwitcherException($"Num of layers which has name {layerName} was not 1: {layerCount}");
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
                    throw new FacialExpressionSwitcherException($"Num of states which has name {stateName} was not 1: {stateCount}");
                }
            }
            else
            {
                throw new FacialExpressionSwitcherException($"Num of layers which has name {layerName} was not 1: {layerCount}");
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
                throw new FacialExpressionSwitcherException($"Num of layers which has name {layerName} was not 1: {layerCount}");
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
                throw new FacialExpressionSwitcherException($"Num of layers which has name {layerName} was not 1: {layerCount}");
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
                    throw new FacialExpressionSwitcherException("Unknown tracking type.");
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
                    throw new FacialExpressionSwitcherException("Unknown tracking type.");
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
            SkinnedMeshRenderer faceMesh;
            if (avatarDescriptor.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape &&
                avatarDescriptor.VisemeSkinnedMesh is SkinnedMeshRenderer)
            {
                faceMesh = avatarDescriptor.VisemeSkinnedMesh;
            }
            else if (avatarDescriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh is SkinnedMeshRenderer)
            {
                faceMesh = avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh;
            }
            else
            {
                faceMesh = null;
            }
            return faceMesh;
        }

        public static HashSet<string> GetBlendShapeNamesToBeExcluded(VRCAvatarDescriptor avatarDescriptor, bool replaceBlink)
        {
            HashSet<string> toBeExcluded = new HashSet<string>();

            // Exclude shape key for blinking when using AvatarDescriptor's blink feature
            if (!replaceBlink)
            {
                toBeExcluded = new HashSet<string>(GetEyeLidsBlendShapes(avatarDescriptor));
            }

            // Exclude shape key for lip-sync
            if (avatarDescriptor.VisemeBlendShapes is string[])
            {
                foreach (var name in avatarDescriptor.VisemeBlendShapes)
                {
                    toBeExcluded.Add(name);
                }
            }
            return toBeExcluded;
        }

        public static List<string> GetEyeLidsBlendShapes(VRCAvatarDescriptor avatarDescriptor)
        {
            var ret = new List<string>();
            if (avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes is int[] &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh is SkinnedMeshRenderer &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.sharedMesh is Mesh sharedMesh)
            {
                foreach (var index in avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes)
                {
                    if (index < sharedMesh.blendShapeCount)
                    {
                        ret.Add(sharedMesh.GetBlendShapeName(index));
                    }
                }
            }
            return ret;
        }

        public static bool IsExcluded(string name, HashSet<string> toBeExcluded)
        {
            if (toBeExcluded.Contains(name) || name.StartsWith("vrc."))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<(string name, float weight)> GetFaceMeshBlendShapes(VRCAvatarDescriptor avatarDescriptor, bool replaceBlink)
        {
            var faceMesh = GetFaceMesh(avatarDescriptor);
            var toBeExcluded = GetBlendShapeNamesToBeExcluded(avatarDescriptor, replaceBlink);

            // Get blendshape names and weights
            var blendShapes = new List<(string name, float weight)>();
            if (faceMesh.sharedMesh is Mesh)
            {
                for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
                {
                    var name = faceMesh.sharedMesh.GetBlendShapeName(i);
                    if (!IsExcluded(name, toBeExcluded))
                    {
                        var weight = faceMesh.GetBlendShapeWeight(i);
                        blendShapes.Add((name, weight));
                    }
                }
            }

            return blendShapes;
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
                    flattened.Add(new ModeEx() { PathToMode = pathToParent + modeNameProvider.Provide(mode), Mode = mode });
                }
                else
                {
                    var group = menuItemList.GetGroup(id);
                    FlattenMenuItemListSub(group, flattened, pathToParent + group.DisplayName + "/", modeNameProvider);
                }
            }
        }
    }
}
