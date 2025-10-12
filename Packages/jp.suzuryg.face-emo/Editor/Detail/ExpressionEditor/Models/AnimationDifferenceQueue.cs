using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Domain;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Models
{
    internal sealed class AnimationDifferenceQueue
    {
        private readonly Queue<AnimationDifference> _squashedDifferences = new();

        [CanBeNull] private AnimationDifference _latestDifference;

        public void SetBlendShapeValue(BlendShape blendShape, float value)
        {
            // squash
            if (_latestDifference is AnimationDifference.BlendShapeDiff blendShapeDiff &&
                blendShapeDiff.Key.Equals(blendShape))
            {
                _latestDifference.Operation = AnimationDifference.OperationType.Set;
                blendShapeDiff.Value = value;
                return;
            }

            // enqueue
            if (_latestDifference != null)
            {
                _squashedDifferences.Enqueue(_latestDifference);
            }

            _latestDifference =
                AnimationDifference.BlendShape(AnimationDifference.OperationType.Set, blendShape, value);
        }

        public void RemoveBlendShapeValue(BlendShape blendShape)
        {
            // squash
            if (_latestDifference is AnimationDifference.BlendShapeDiff blendShapeDiff &&
                blendShapeDiff.Key.Equals(blendShape))
            {
                _latestDifference.Operation = AnimationDifference.OperationType.Remove;
                return;
            }

            // enqueue
            if (_latestDifference != null)
            {
                _squashedDifferences.Enqueue(_latestDifference);
            }

            _latestDifference = AnimationDifference.BlendShape(AnimationDifference.OperationType.Remove, blendShape, 0);
        }

        public void SetToggleValue(int id, GameObject target, bool value)
        {
            // squash
            if (_latestDifference is AnimationDifference.ToggleDiff toggleDiff &&
                toggleDiff.Key.Equals(id))
            {
                _latestDifference.Operation = AnimationDifference.OperationType.Set;
                toggleDiff.Value = (target, value);
                return;
            }

            // enqueue
            if (_latestDifference != null)
            {
                _squashedDifferences.Enqueue(_latestDifference);
            }

            _latestDifference =
                AnimationDifference.Toggle(AnimationDifference.OperationType.Set, id, (target, value));
        }

        public void RemoveToggleValue(int id, GameObject target)
        {
            // squash
            if (_latestDifference is AnimationDifference.ToggleDiff toggleDiff &&
                toggleDiff.Key.Equals(id))
            {
                _latestDifference.Operation = AnimationDifference.OperationType.Remove;
                return;
            }

            // enqueue
            if (_latestDifference != null)
            {
                _squashedDifferences.Enqueue(_latestDifference);
            }

            _latestDifference = AnimationDifference.Toggle(AnimationDifference.OperationType.Remove, id, (target, false));
        }

        public void SetTransformValue(int id, TransformProxy value)
        {
            // squash
            if (_latestDifference is AnimationDifference.TransformDiff transformDiff &&
                transformDiff.Key.Equals(id))
            {
                _latestDifference.Operation = AnimationDifference.OperationType.Set;
                transformDiff.Value = value;
                return;
            }

            // enqueue
            if (_latestDifference != null)
            {
                _squashedDifferences.Enqueue(_latestDifference);
            }

            _latestDifference =
                AnimationDifference.Transform(AnimationDifference.OperationType.Set, id, value);
        }

        public void RemoveTransformValue(int id, TransformProxy value)
        {
            // squash
            if (_latestDifference is AnimationDifference.TransformDiff transformDiff &&
                transformDiff.Key.Equals(id))
            {
                _latestDifference.Operation = AnimationDifference.OperationType.Remove;
                return;
            }

            // enqueue
            if (_latestDifference != null)
            {
                _squashedDifferences.Enqueue(_latestDifference);
            }

            _latestDifference = AnimationDifference.Transform(AnimationDifference.OperationType.Remove, id, value);
        }

        public bool TryDequeue(out AnimationDifference difference)
        {
            if (_squashedDifferences.Any())
            {
                difference = _squashedDifferences.Dequeue();
                return true;
            }

            if (_latestDifference != null)
            {
                difference = _latestDifference;
                _latestDifference = null;
                return true;
            }

            difference = null;
            return false;
        }
    }
}
