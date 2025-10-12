using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Models
{
    internal sealed class PreviewClipGenerator : IDisposable
    {
        public IObservable<AnimationClip> OnGenerated => _onGenerated.AsObservable();

        private readonly AV3Setting _av3Setting;
        private readonly IReadOnlyDictionary<BlendShape, float> _animatedBlendShapes;
        private readonly Subject<AnimationClip> _onGenerated = new();
        private readonly CompositeDisposable _subscriptions = new();

        [CanBeNull] private Animator _animator;
        [CanBeNull] private AnimationClip _targetClip;
        [CanBeNull] private AnimationClip _previewClip;
        [CanBeNull] private BlendShape _currentOverride;
        [CanBeNull] private BlendShape _overrideRequest;

        public PreviewClipGenerator(AV3Setting av3Setting, IReadOnlyDictionary<BlendShape, float> animatedBlendShapes)
        {
            _av3Setting = av3Setting;
            _animatedBlendShapes = animatedBlendShapes;
            Observable.Interval(TimeSpan.FromSeconds(0.1)).Subscribe(_ => ChangePreviewOverride())
                .AddTo(_subscriptions);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            _onGenerated.Dispose();
        }

        public void FetchPreviewAvatar()
        {
            var avatarDescriptor = _av3Setting.TargetAvatar as VRCAvatarDescriptor;
            _animator = avatarDescriptor?.gameObject.GetComponent<Animator>();
        }

        public void OpenTargetClip(AnimationClip targetClip)
        {
            _targetClip = targetClip;
            _previewClip = AV3Utility.SynthesizeAvatarPose(_targetClip, _av3Setting.TargetAvatar as VRCAvatarDescriptor);
            _onGenerated.OnNext(_previewClip);
        }

        public void SetBlendShapeValue(BlendShape blendShape, float value)
        {
            if (_previewClip == null) return;
            ExpressionEditorUtils.SetBlendShapeValue(_previewClip, blendShape, value);
            _onGenerated.OnNext(_previewClip);
        }

        public void SetBlendShapeValues(IEnumerable<KeyValuePair<BlendShape, float>> blendShapeValues)
        {
            if (_previewClip == null) return;
            foreach (var (blendShape, value) in blendShapeValues)
                ExpressionEditorUtils.SetBlendShapeValue(_previewClip, blendShape, value);
            _onGenerated.OnNext(_previewClip);
        }

        public void RemoveBlendShapeValue(BlendShape blendShape)
        {
            if (_previewClip == null) return;
            ExpressionEditorUtils.RemoveBlendShapeValue(_previewClip, blendShape);
            _onGenerated.OnNext(_previewClip);
        }

        public void SetToggleValue(GameObject target, bool value)
        {
            if (_previewClip == null) return;
            ExpressionEditorUtils.SetToggleValue(_previewClip, _animator, target, value);
            _onGenerated.OnNext(_previewClip);
        }

        public void RemoveToggleValue(GameObject target)
        {
            if (_previewClip == null) return;
            ExpressionEditorUtils.RemoveToggleValue(_previewClip, _animator, target);
            _onGenerated.OnNext(_previewClip);
        }

        public void SetTransformValue(TransformProxy value)
        {
            if (_previewClip == null) return;
            ExpressionEditorUtils.SetTransformValue(_previewClip, _animator, value);
            _onGenerated.OnNext(_previewClip);
        }

        public void RequestChangePreviewOverride(BlendShape blendShape)
        {
            _overrideRequest = blendShape;
        }

        private void ChangePreviewOverride()
        {
            if (_currentOverride == _overrideRequest || _previewClip == null) return;
            // revert
            if (_currentOverride != null && !_animatedBlendShapes.ContainsKey(_currentOverride))
            {
                ExpressionEditorUtils.RemoveBlendShapeValue(_previewClip, _currentOverride);
            }
            // override
            if (_overrideRequest != null && !_animatedBlendShapes.ContainsKey(_overrideRequest))
            {
                ExpressionEditorUtils.SetBlendShapeValue(_previewClip, _overrideRequest, 100f);
            }
            _currentOverride = _overrideRequest;
            _onGenerated.OnNext(_previewClip);
        }
    }
}
