﻿using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    public class ImportUtility
    {
        public static Dictionary<BlendShape, float> GetAllFaceBlendShapeValues(VRCAvatarDescriptor avatarDescriptor, AV3Setting av3Setting, bool excludeBlink, bool excludeLipSync)
        {
            var faceBlendShapeValues = AV3Utility.GetFaceMeshBlendShapeValues(avatarDescriptor, excludeBlink, excludeLipSync);

            foreach (var mesh in av3Setting.AdditionalSkinnedMeshes)
            {
                var blendShapes = AV3Utility.GetBlendShapeValues(mesh, avatarDescriptor, excludeBlink, excludeLipSync);
                foreach (var item in blendShapes) { faceBlendShapeValues[item.Key] = item.Value; }
            }
            foreach (var excluded in av3Setting.ExcludedBlendShapes)
            {
                while (faceBlendShapeValues.ContainsKey(excluded)) { faceBlendShapeValues.Remove(excluded); }
            }

            return faceBlendShapeValues;
        }

        public static bool IsFaceMotion(Motion motion, IEnumerable<BlendShape> faceBlendShapes)
        {
            if (motion == null) { return false; }
            else if (motion is BlendTree blendTree && blendTree.children.Length > 0)
            {
                var first = blendTree.children.First().motion;
                var last = blendTree.children.Last().motion;
                return IsFaceMotion(first as AnimationClip, faceBlendShapes) || IsFaceMotion(last as AnimationClip, faceBlendShapes);
            }
            else if (motion is AnimationClip animationClip)
            {
                var bindings = new HashSet<EditorCurveBinding>(faceBlendShapes.Select(x => new EditorCurveBinding { path = x.Path, propertyName = $"blendShape.{x.Name}", type = typeof(SkinnedMeshRenderer) }));
                return AnimationUtility.GetCurveBindings(animationClip).Any(x => bindings.Contains(x));
            }
            else { return false; }
        }

        public static bool AreAllValuesZero(AnimationClip animationClip, IEnumerable<BlendShape> blendShapes)
        {
            foreach (var binding in blendShapes.Select(x => new EditorCurveBinding { path = x.Path, propertyName = $"blendShape.{x.Name}", type = typeof(SkinnedMeshRenderer) }))
            {
                var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                if (curve != null && curve.keys.Any(x => Math.Abs(x.value) > 0))
                {
                    return false;
                }
            }
            return true;
        }

        public static AnimatorController GetFxLayer(VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor == null || avatarDescriptor.baseAnimationLayers == null)
            {
                return null;
            }

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

        public static string GetNewAssetDir()
        {
            return "Assets/Suzuryg/FaceEmo/Imported/" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }
    }
}
