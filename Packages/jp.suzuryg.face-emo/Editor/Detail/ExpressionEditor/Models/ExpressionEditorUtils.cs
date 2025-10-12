using System.Collections.Generic;
using System.Linq;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Models
{
    internal static class ExpressionEditorUtils
    {
        public static Dictionary<BlendShape, float> GetBlendShapeValues(AnimationClip animationClip, IEnumerable<BlendShape> blendShapes)
        {
            var values = new Dictionary<BlendShape, float>();
            if (animationClip == null) { return values; }

            var bindings = GetBlendShapeBindings(blendShapes);
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(animationClip, binding.Value);
                if (curve != null && curve.keys.Length > 0) { values[binding.Key] = curve.keys.Last().value; }
            }
            return values;
        }

        public static void SetBlendShapeValue(AnimationClip animationClip, BlendShape blendShape, float newValue)
        {
            var binding = GetBlendShapeBindings(new[] { blendShape }).FirstOrDefault();
            var curve = new AnimationCurve(new Keyframe(time: 0, value: newValue));
            AnimationUtility.SetEditorCurve(animationClip, binding.Value, curve);
        }

        public static void RemoveBlendShapeValue(AnimationClip animationClip, BlendShape blendShape)
        {
            var binding = GetBlendShapeBindings(new[] { blendShape }).FirstOrDefault();
            AnimationUtility.SetEditorCurve(animationClip, binding.Value, null);
        }

        private static Dictionary<BlendShape, EditorCurveBinding> GetBlendShapeBindings(IEnumerable<BlendShape> blendShapes)
        {
            var bindings = new Dictionary<BlendShape, EditorCurveBinding>();
            foreach (var blendShape in blendShapes)
            {
                bindings[blendShape] = new EditorCurveBinding
                {
                    path = blendShape.Path, propertyName = $"blendShape.{blendShape.Name}",
                    type = typeof(SkinnedMeshRenderer)
                };
            }
            return bindings;
        }

        public static bool? GetToggleValue(AnimationClip animationClip, Animator animator, GameObject gameObject)
        {
            var transformPath = AnimationUtility.CalculateTransformPath(gameObject?.transform, animator?.transform);
            var binding = new EditorCurveBinding
                { path = transformPath, propertyName = "m_IsActive", type = typeof(GameObject) };
            var curve = AnimationUtility.GetEditorCurve(animationClip, binding);

            if (curve == null || curve.keys.Length <= 0) return null;
            var value = curve.keys[0].value;
            return value > 0;
        }

        public static void SetToggleValue(AnimationClip animationClip, Animator animator, GameObject gameObject, bool isActive)
        {
            var transformPath = AnimationUtility.CalculateTransformPath(gameObject?.transform, animator?.transform);
            var curve = new AnimationCurve(new Keyframe(time: 0, value: isActive ? 1 : 0));
            var binding = new EditorCurveBinding
                { path = transformPath, propertyName = "m_IsActive", type = typeof(GameObject) };
            AnimationUtility.SetEditorCurve(animationClip, binding, curve);
        }

        public static void RemoveToggleValue(AnimationClip animationClip, Animator animator, GameObject gameObject)
        {
            var transformPath = AnimationUtility.CalculateTransformPath(gameObject?.transform, animator?.transform);
            var binding = new EditorCurveBinding
                { path = transformPath, propertyName = "m_IsActive", type = typeof(GameObject) };
            AnimationUtility.SetEditorCurve(animationClip, binding, null);
        }

        public static TransformProxy GetTransformValue(AnimationClip animationClip, Animator animator, GameObject gameObject)
        {
            var transformPath = AnimationUtility.CalculateTransformPath(gameObject?.transform, animator?.transform);

            // Get local position
            var posX = GetCurveValue(AnimationUtility.GetEditorCurve(animationClip,
                new EditorCurveBinding
                    { path = transformPath, propertyName = "m_LocalPosition.x", type = typeof(Transform) }));
            var posY = GetCurveValue(AnimationUtility.GetEditorCurve(animationClip,
                new EditorCurveBinding
                    { path = transformPath, propertyName = "m_LocalPosition.y", type = typeof(Transform) }));
            var posZ = GetCurveValue(AnimationUtility.GetEditorCurve(animationClip,
                new EditorCurveBinding
                    { path = transformPath, propertyName = "m_LocalPosition.z", type = typeof(Transform) }));

            // Get local rotation
            var rotX = GetCurveValue(AnimationUtility.GetEditorCurve(animationClip,
                new EditorCurveBinding
                    { path = transformPath, propertyName = "localEulerAnglesRaw.x", type = typeof(Transform) }));
            var rotY = GetCurveValue(AnimationUtility.GetEditorCurve(animationClip,
                new EditorCurveBinding
                    { path = transformPath, propertyName = "localEulerAnglesRaw.y", type = typeof(Transform) }));
            var rotZ = GetCurveValue(AnimationUtility.GetEditorCurve(animationClip,
                new EditorCurveBinding
                    { path = transformPath, propertyName = "localEulerAnglesRaw.z", type = typeof(Transform) }));

            // Get local scale
            var scaleX = GetCurveValue(AnimationUtility.GetEditorCurve(animationClip,
                new EditorCurveBinding
                    { path = transformPath, propertyName = "m_LocalScale.x", type = typeof(Transform) }));
            var scaleY = GetCurveValue(AnimationUtility.GetEditorCurve(animationClip,
                new EditorCurveBinding
                    { path = transformPath, propertyName = "m_LocalScale.y", type = typeof(Transform) }));
            var scaleZ = GetCurveValue(AnimationUtility.GetEditorCurve(animationClip,
                new EditorCurveBinding
                    { path = transformPath, propertyName = "m_LocalScale.z", type = typeof(Transform) }));

            if (posX == null && posY == null && posZ == null &&
                rotX == null && rotY == null && rotZ == null &&
                scaleX == null && scaleY == null && scaleZ == null)
            {
                return null;
            }

            var current = TransformProxy.FromGameObject(gameObject);
            return new TransformProxy(
                gameObject,
                new Vector3(posX ?? current.Position.x, posY ?? current.Position.y, posZ ?? current.Position.z),
                new Vector3(rotX ?? current.Rotation.x, rotY ?? current.Rotation.y, rotZ ?? current.Rotation.z),
                new Vector3(scaleX ?? current.Scale.x, scaleY ?? current.Scale.y, scaleZ ?? current.Scale.z)
            );

            float? GetCurveValue(AnimationCurve curve)
            {
                if (curve != null && curve.keys.Length > 0)
                {
                    return curve.keys.Last().value;
                }
                return null;
            }
        }

        public static void SetTransformValue(AnimationClip animationClip, Animator animator, TransformProxy transform)
        {
            var transformPath = AnimationUtility.CalculateTransformPath(transform?.GameObject?.transform, animator?.transform);

            AnimationCurve curve;

            // Set local position
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.Position.x));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.x", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.Position.y));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.y", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.Position.z));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.z", type = typeof(Transform) }, curve);

            // Set local rotation
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.Rotation.x));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.x", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.Rotation.y));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.y", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.Rotation.z));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.z", type = typeof(Transform) }, curve);

            // Set local scale
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.Scale.x));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.x", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.Scale.y));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.y", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.Scale.z));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.z", type = typeof(Transform) }, curve);
        }

        public static void RemoveTransformValue(AnimationClip animationClip, Animator animator, GameObject gameObject)
        {
            var transformPath = AnimationUtility.CalculateTransformPath(gameObject?.transform, animator?.transform);

            // Remove local position
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.x", type = typeof(Transform) }, null);
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.y", type = typeof(Transform) }, null);
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.z", type = typeof(Transform) }, null);

            // Remove local rotation
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.x", type = typeof(Transform) }, null);
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.y", type = typeof(Transform) }, null);
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.z", type = typeof(Transform) }, null);

            // Remove local scale
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.x", type = typeof(Transform) }, null);
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.y", type = typeof(Transform) }, null);
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.z", type = typeof(Transform) }, null);
        }
    }
}
