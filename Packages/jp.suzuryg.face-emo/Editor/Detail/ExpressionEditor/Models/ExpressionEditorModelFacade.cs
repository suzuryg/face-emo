using System;
using System.Collections.Generic;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Models
{
    internal sealed class ExpressionEditorModelFacade : IDisposable
    {
        public IReadOnlyDictionary<BlendShape, float> BlinkBlendShapes => _propertyEditor.BlinkBlendShapes;
        public IReadOnlyDictionary<BlendShape, float> LipSyncBlendShapes => _propertyEditor.LipSyncBlendShapes;
        public IReadOnlyDictionary<BlendShape, float> FaceBlendShapes => _propertyEditor.FaceBlendShapes;
        public IReadOnlyDictionary<int, (GameObject target, bool value)> Toggles => _propertyEditor.Toggles;
        public IReadOnlyDictionary<int, TransformProxy> Transforms => _propertyEditor.Transforms;
        public IReadOnlyDictionary<BlendShape, float> AnimatedBlendShapes => _propertyEditor.AnimatedBlendShapes;
        public IReadOnlyDictionary<int, (GameObject target, bool value)> AnimatedToggles => _propertyEditor.AnimatedToggles;
        public IReadOnlyDictionary<int, TransformProxy> AnimatedTransforms => _propertyEditor.AnimatedTransforms;
        public IObservable<AnimationClip> OnThumbnailUpdateRequested => _propertyWriter.OnThumbnailUpdateRequested;

        private readonly PropertyEditor _propertyEditor;
        private readonly PropertyWriter _propertyWriter;
        private readonly PreviewClipGenerator _previewClipGenerator;
        private readonly PreviewClipSampler _previewClipSampler;

        private readonly CompositeDisposable _disposables = new();

        public ExpressionEditorModelFacade(AV3Setting av3Setting)
        {
            _propertyEditor = new PropertyEditor(av3Setting).AddTo(_disposables);
            _propertyWriter = new PropertyWriter(av3Setting).AddTo(_disposables);
            _previewClipGenerator =
                new PreviewClipGenerator(av3Setting, _propertyEditor.AnimatedBlendShapes).AddTo(_disposables);
            _previewClipSampler = new PreviewClipSampler(av3Setting).AddTo(_disposables);

            _previewClipGenerator.OnGenerated.Subscribe(_previewClipSampler.SetPreviewClip).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void FetchPreviewAvatar()
        {
            _propertyEditor.FetchPreviewAvatar();
            _propertyWriter.FetchPreviewAvatar();
            _previewClipGenerator.FetchPreviewAvatar();
            _previewClipSampler.FetchPreviewAvatar();
        }

        public void OpenTargetClip(AnimationClip clip)
        {
            _propertyEditor.OpenTargetClip(clip);
            _propertyWriter.OpenTargetClip(clip);
            _previewClipGenerator.OpenTargetClip(clip);
        }

        public Vector3 GetAvatarViewPosition() => _previewClipSampler.AvatarViewPosition;

        public void SetBlendShapeValue(BlendShape blendShape, float value)
        {
            _propertyEditor.SetBlendShapeValue(blendShape, value);
            _propertyWriter.SetBlendShapeValue(blendShape, value);
            _previewClipGenerator.SetBlendShapeValue(blendShape, value);
        }

        public void RemoveBlendShapeValue(BlendShape blendShape)
        {
            _propertyEditor.RemoveBlendShapeValue(blendShape);
            _propertyWriter.RemoveBlendShapeValue(blendShape);
            _previewClipGenerator.RemoveBlendShapeValue(blendShape);
        }

        public void AddAllBlendShapes()
        {
            var added = _propertyEditor.AddAllFaceBlendShapes();
            foreach (var kvp in added)
                _propertyWriter.SetBlendShapeValue(kvp.Key, kvp.Value);
            _previewClipGenerator.SetBlendShapeValues(added);
        }

        public void RequestChangePreviewOverride(BlendShape blendShape)
        {
            _previewClipGenerator.RequestChangePreviewOverride(blendShape);
        }

        public void SetToggleValue(int id, GameObject target, bool value)
        {
            _propertyEditor.SetToggleValue(id, value);
            _propertyWriter.SetToggleValue(id, target, value);
            _previewClipGenerator.SetToggleValue(target, value);
        }

        public void RemoveToggleValue(int id, GameObject target)
        {
            _propertyEditor.RemoveToggleValue(id);
            _propertyWriter.RemoveToggleValue(id, target);
            _previewClipGenerator.RemoveToggleValue(target);
        }

        public void SetTransformValue(int id, TransformProxy value)
        {
            _propertyEditor.SetTransformValue(id, value);
            _propertyWriter.SetTransformValue(id, value);
            _previewClipGenerator.SetTransformValue(value);
        }

        public void RemoveTransformValue(int id, TransformProxy value)
        {
            _propertyEditor.RemoveTransformValue(id);
            _propertyWriter.RemoveTransformValue(id, value);
            // Revert to default transform
            _previewClipGenerator.SetTransformValue(TransformProxy.FromGameObject(value.GameObject));
        }

        public void StartSampling() => _previewClipSampler.StartSampling();
        public void StopSampling() => PreviewClipSampler.StopSampling();
    }
}
