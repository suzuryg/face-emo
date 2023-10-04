using Suzuryg.FaceEmo.Domain;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    internal class ImportUtility
    {
        public static bool IsFaceMotion(Motion motion, IEnumerable<BlendShape> faceBlendShapes)
        {
            if (motion == null) { return false; }
            else if (motion is BlendTree blendTree && blendTree.children.Length > 0)
            {
                var first = blendTree.children.First().motion;
                var last = blendTree.children.Last().motion;
                return IsFaceMotion(first as AnimationClip, faceBlendShapes) && IsFaceMotion(last as AnimationClip, faceBlendShapes);
            }
            else if (motion is AnimationClip animationClip)
            {
                var bindings = new HashSet<EditorCurveBinding>(faceBlendShapes.Select(x => new EditorCurveBinding { path = x.Path, propertyName = $"blendShape.{x.Name}", type = typeof(SkinnedMeshRenderer) }));
                return AnimationUtility.GetCurveBindings(animationClip).Any(x => bindings.Contains(x));
            }
            else { return false; }
        }

        public static AnimatorController GetFxLayer(VRCAvatarDescriptor avatarDescriptor)
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
    }
}
