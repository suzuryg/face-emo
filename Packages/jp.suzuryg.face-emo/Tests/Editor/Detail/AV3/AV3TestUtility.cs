using NUnit.Framework;
using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public class AV3TestUtility
    {
        public static float? GetBlendShapeValue(AnimationClip clip, BlendShape blendShape)
        {
            var binding = new EditorCurveBinding { path = blendShape.Path, propertyName = $"blendShape.{blendShape.Name}", type = typeof(SkinnedMeshRenderer) };
            return GetCurveValue(clip, binding);
        }

        public static bool? GetToggleValue(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "m_IsActive", type = typeof(GameObject) };
            var value = GetCurveValue(clip, binding);
            if (value.HasValue)
            {
                return value.Value > 0;
            }
            else
            {
                return null;
            }
        }

        public static float? GetPositionX(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "m_LocalPosition.x", type = typeof(Transform) };
            return GetCurveValue(clip, binding);
        }

        public static float? GetPositionY(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "m_LocalPosition.y", type = typeof(Transform) };
            return GetCurveValue(clip, binding);
        }

        public static float? GetPositionZ(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "m_LocalPosition.z", type = typeof(Transform) };
            return GetCurveValue(clip, binding);
        }

        public static float? GetRotationX(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "localEulerAnglesRaw.x", type = typeof(Transform) };
            return GetCurveValue(clip, binding);
        }

        public static float? GetRotationY(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "localEulerAnglesRaw.y", type = typeof(Transform) };
            return GetCurveValue(clip, binding);
        }

        public static float? GetRotationZ(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "localEulerAnglesRaw.z", type = typeof(Transform) };
            return GetCurveValue(clip, binding);
        }

        public static float? GetScaleX(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "m_LocalScale.x", type = typeof(Transform) };
            return GetCurveValue(clip, binding);
        }

        public static float? GetScaleY(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "m_LocalScale.y", type = typeof(Transform) };
            return GetCurveValue(clip, binding);
        }

        public static float? GetScaleZ(AnimationClip clip, string path)
        {
            var binding = new EditorCurveBinding { path = path, propertyName = "m_LocalScale.z", type = typeof(Transform) };
            return GetCurveValue(clip, binding);
        }

        private static float? GetCurveValue(AnimationClip clip, EditorCurveBinding binding)
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve is null)
            {
                return null;
            }
            else if (curve.keys.Length == 1)
            {
                return curve.keys[0].value;
            }
            else if (curve.keys.Length == 2)
            {
                Assert.That(curve.keys[0].value == curve.keys[1].value, Is.True);
                return curve.keys[0].value;
            }
            else
            {
                return null;
            }
        }
    }
}
