using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Domain;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Models
{
    internal abstract class AnimationDifference
    {
        internal enum OperationType
        {
            Set,
            Remove,
        }

        public OperationType Operation { get; set; }

        private AnimationDifference(OperationType operation)
        {
            Operation = operation;
        }

        public static BlendShapeDiff BlendShape(OperationType operation, BlendShape key, float value)
        {
            return new BlendShapeDiff(operation, key, value);
        }

        public static ToggleDiff Toggle(OperationType operation, int key, (GameObject target, bool value) value)
        {
            return new ToggleDiff(operation, key, value);
        }

        public static TransformDiff Transform(OperationType operation, int key, TransformProxy value)
        {
            return new TransformDiff(operation, key, value);
        }

        internal sealed class BlendShapeDiff : AnimationDifference
        {
            public BlendShape Key { get; }
            public float Value { get; set; }

            internal BlendShapeDiff(OperationType operation, BlendShape key, float value)
                : base(operation)
            {
                Key = key;
                Value = value;
            }
        }

        internal sealed class ToggleDiff : AnimationDifference
        {
            public int Key { get; }
            public (GameObject target, bool value) Value { get; set; }

            internal ToggleDiff(OperationType operation, int key, (GameObject target, bool value) value)
                : base(operation)
            {
                Key = key;
                Value = value;
            }
        }

        internal sealed class TransformDiff : AnimationDifference
        {
            public int Key { get; }
            public TransformProxy Value { get; set; }

            internal TransformDiff(OperationType operation, int key, TransformProxy value)
                : base(operation)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
