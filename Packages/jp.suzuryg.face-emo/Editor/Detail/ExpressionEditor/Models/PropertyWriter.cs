using System;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Models
{
    internal sealed class PropertyWriter : IDisposable
    {
        public IObservable<AnimationClip> OnThumbnailUpdateRequested => _onThumbnailUpdateRequested;

        private readonly CompositeDisposable _subscriptions = new();
        private readonly Subject<AnimationClip> _onThumbnailUpdateRequested = new();

        private readonly AV3Setting _av3Setting;
        private readonly AnimationDifferenceQueue _animationDifferenceQueue = new();

        [CanBeNull] private Animator _animator;
        [CanBeNull] private AnimationClip _targetClip;

        public PropertyWriter(AV3Setting av3Setting)
        {
            _av3Setting = av3Setting;
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => FlushAnimationDifferences())
                .AddTo(_subscriptions);
        }

        public void Dispose()
        {
            FlushAnimationDifferences();
            _subscriptions.Dispose();
            _onThumbnailUpdateRequested.Dispose();
        }

        public void FetchPreviewAvatar()
        {
            var avatarDescriptor = _av3Setting.TargetAvatar as VRCAvatarDescriptor;
            _animator = avatarDescriptor?.gameObject.GetComponent<Animator>();
        }

        public void OpenTargetClip(AnimationClip targetClip)
        {
            FlushAnimationDifferences();
            _targetClip = targetClip;
        }

        public void SetBlendShapeValue(BlendShape blendShape, float value) =>
            _animationDifferenceQueue.SetBlendShapeValue(blendShape, value);

        public void RemoveBlendShapeValue(BlendShape blendShape) =>
            _animationDifferenceQueue.RemoveBlendShapeValue(blendShape);

        public void SetToggleValue(int id, GameObject target, bool value) =>
            _animationDifferenceQueue.SetToggleValue(id, target, value);

        public void RemoveToggleValue(int id, GameObject target) =>
            _animationDifferenceQueue.RemoveToggleValue(id, target);

        public void SetTransformValue(int id, TransformProxy value) =>
            _animationDifferenceQueue.SetTransformValue(id, value);

        public void RemoveTransformValue(int id, TransformProxy value) =>
            _animationDifferenceQueue.RemoveTransformValue(id, value);

        private void FlushAnimationDifferences()
        {
            if (_targetClip == null) return;

            var isDirty = false;
            while (_animationDifferenceQueue.TryDequeue(out var diff))
            {
                switch (diff)
                {
                    case AnimationDifference.BlendShapeDiff blendShapeDiff:
                        SaveBlendShapeDiff(blendShapeDiff);
                        break;
                    case AnimationDifference.ToggleDiff toggleDiff:
                        SaveToggleDiff(toggleDiff);
                        break;
                    case AnimationDifference.TransformDiff transformDiff:
                        SaveTransformDiff(transformDiff);
                        break;
                }
                isDirty = true;
            }

            if (!isDirty) return;

            EditorUtility.SetDirty(_targetClip);
            _onThumbnailUpdateRequested.OnNext(_targetClip);
            return;

            void SaveBlendShapeDiff(AnimationDifference.BlendShapeDiff diff)
            {
                switch (diff.Operation)
                {
                    case AnimationDifference.OperationType.Set:
                        Undo.RecordObject(_targetClip, $"Set {diff.Key} to {diff.Value}");
                        ExpressionEditorUtils.SetBlendShapeValue(_targetClip, diff.Key, diff.Value);
                        return;
                    case AnimationDifference.OperationType.Remove:
                        Undo.RecordObject(_targetClip, $"Remove {diff.Key}");
                        ExpressionEditorUtils.RemoveBlendShapeValue(_targetClip, diff.Key);
                        return;
                }
            }

            void SaveToggleDiff(AnimationDifference.ToggleDiff diff)
            {
                var gameObject = diff.Value.target;
                if (gameObject == null || _animator == null) return;

                switch (diff.Operation)
                {
                    case AnimationDifference.OperationType.Set:
                        var text = diff.Value.value ? "enabled" : "disabled";
                        Undo.RecordObject(_targetClip, $"Set {gameObject.name} to {text}");
                        ExpressionEditorUtils.SetToggleValue(_targetClip, _animator, gameObject, diff.Value.value);
                        return;
                    case AnimationDifference.OperationType.Remove:
                        Undo.RecordObject(_targetClip, $"Remove Toggle of {gameObject.name}");
                        ExpressionEditorUtils.RemoveToggleValue(_targetClip, _animator, gameObject);
                        break;
                }
            }

            void SaveTransformDiff(AnimationDifference.TransformDiff diff)
            {
                var gameObject = diff.Value.GameObject;
                if (gameObject == null || _animator == null) return;

                switch (diff.Operation)
                {
                    case AnimationDifference.OperationType.Set:
                        Undo.RecordObject(_targetClip, $"Update Transform of {gameObject.name}");
                        ExpressionEditorUtils.SetTransformValue(_targetClip, _animator, diff.Value);
                        return;
                    case AnimationDifference.OperationType.Remove:
                        Undo.RecordObject(_targetClip, $"Remove Transform of {gameObject.name}");
                        ExpressionEditorUtils.RemoveTransformValue(_targetClip, _animator, gameObject);
                        break;
                }
            }
        }
    }
}
