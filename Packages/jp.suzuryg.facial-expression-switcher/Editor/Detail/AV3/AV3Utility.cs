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

        public static VRCAvatarDescriptor GetAvatarDescriptor(Domain.Avatar avatar)
        {
            if (avatar is Domain.Avatar &&
                GameObject.Find(avatar.Path) is GameObject gameObject &&
                gameObject.GetComponent<VRCAvatarDescriptor>() is VRCAvatarDescriptor avatarDescriptor)
            {
                return avatarDescriptor;
            }
            else
            {
                throw new FacialExpressionSwitcherException("AvatarDescriptor was not found.");
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

        public static HashSet<string> GetBlendShapeNamesToBeExcluded(VRCAvatarDescriptor avatarDescriptor)
        {
            HashSet<string> toBeExcluded = new HashSet<string>();
            if (avatarDescriptor.VisemeBlendShapes is string[])
            {
                foreach (var name in avatarDescriptor.VisemeBlendShapes)
                {
                    toBeExcluded.Add(name);
                }
            }
            if (avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes is int[] &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh is SkinnedMeshRenderer &&
                avatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh.sharedMesh is Mesh sharedMesh)
            {
                foreach (var index in avatarDescriptor.customEyeLookSettings.eyelidsBlendshapes)
                {
                    if (index < sharedMesh.blendShapeCount)
                    {
                        toBeExcluded.Add(sharedMesh.GetBlendShapeName(index));
                    }
                }
            }
            return toBeExcluded;
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

        public static List<(string name, float weight)> GetFaceMeshBlendShapes(VRCAvatarDescriptor avatarDescriptor)
        {
            var faceMesh = GetFaceMesh(avatarDescriptor);
            var toBeExcluded = GetBlendShapeNamesToBeExcluded(avatarDescriptor);

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


        public static List<(string name, float weight)> GetBlendShapesInRange(string start, string end, VRCAvatarDescriptor avatarDescriptor)
        {
            var faceMesh = GetFaceMesh(avatarDescriptor);
            var toBeExcluded = GetBlendShapeNamesToBeExcluded(avatarDescriptor);

            // Get blendshape names and weights
            var blendShapes = new List<(string name, float weight)>();
            bool startExists = false;
            bool endExists = false;
            if (faceMesh.sharedMesh is Mesh)
            {
                for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
                {
                    var name = faceMesh.sharedMesh.GetBlendShapeName(i);

                    if (name == start)
                    {
                        startExists = true;
                    }

                    if (startExists && !IsExcluded(name, toBeExcluded))
                    {
                        var weight = faceMesh.GetBlendShapeWeight(i);
                        blendShapes.Add((name, weight));
                    }

                    if (name == end)
                    {
                        endExists = true;
                        break;
                    }
                }
            }
            if (!endExists)
            {
                blendShapes.Clear();
            }

            return blendShapes;
        }
    }
}
