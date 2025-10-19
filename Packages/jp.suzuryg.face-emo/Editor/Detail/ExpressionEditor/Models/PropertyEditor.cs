using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Models
{
    internal sealed class PropertyEditor : IDisposable
    {
        public IReadOnlyDictionary<BlendShape, float> BlinkBlendShapes => _blinkBlendShapes;
        public IReadOnlyDictionary<BlendShape, float> LipSyncBlendShapes => _lipSyncBlendShapes;
        public IReadOnlyDictionary<BlendShape, float> FaceBlendShapes => _faceBlendShapes;
        public IReadOnlyDictionary<int, (GameObject target, bool value)> Toggles => _toggles;
        public IReadOnlyDictionary<int, TransformProxy> Transforms => _transforms;
        public IReadOnlyDictionary<BlendShape, float> AnimatedBlendShapes => _animatedBlendShapes;
        public IReadOnlyDictionary<int, (GameObject target, bool value)> AnimatedToggles =>
            _animatedToggles;
        public IReadOnlyDictionary<int, TransformProxy> AnimatedTransforms => _animatedTransforms;

        private readonly AV3Setting _av3Setting;
        private readonly Dictionary<BlendShape, float> _blinkBlendShapes = new();
        private readonly Dictionary<BlendShape, float> _lipSyncBlendShapes = new();
        private readonly Dictionary<BlendShape, float> _faceBlendShapes = new();
        private readonly Dictionary<int, (GameObject target, bool value)> _toggles = new();
        private readonly Dictionary<int, TransformProxy> _transforms = new();
        private readonly Dictionary<BlendShape, float> _animatedBlendShapes = new();
        private readonly Dictionary<int, (GameObject target, bool value)> _animatedToggles = new();
        private readonly Dictionary<int, TransformProxy> _animatedTransforms = new();

        [CanBeNull] private Animator _animator;
        [CanBeNull] private AnimationClip _targetClip;

        public PropertyEditor(AV3Setting av3Setting)
        {
            _av3Setting = av3Setting;
        }

        public void Dispose()
        {
        }

        public void FetchPreviewAvatar()
        {
            var avatarDescriptor = _av3Setting.TargetAvatar as VRCAvatarDescriptor;
            _animator = avatarDescriptor?.gameObject.GetComponent<Animator>();

            _faceBlendShapes.Clear();
            _blinkBlendShapes.Clear();
            _lipSyncBlendShapes.Clear();
            _toggles.Clear();
            _transforms.Clear();

            // Face mesh blend shapes
            var faceBlendShapes = AV3Utility.GetFaceMeshBlendShapeValues(avatarDescriptor, false, false);
            if (faceBlendShapes != null)
                foreach (var item in faceBlendShapes)
                    _faceBlendShapes[item.Key] = item.Value;
            // Add blend shapes in additional skinned meshes
            foreach (var item in _av3Setting.AdditionalSkinnedMeshes
                         .Select(mesh => AV3Utility.GetBlendShapeValues(mesh, avatarDescriptor, false, false))
                         .SelectMany(blendShapes => blendShapes)) _faceBlendShapes[item.Key] = item.Value;
            // Exclude specified blend shapes
            foreach (var excluded in _av3Setting.ExcludedBlendShapes)
            {
                while (_faceBlendShapes.ContainsKey(excluded)) _faceBlendShapes.Remove(excluded);
            }

            // Blink blend shapes
            // TODO: Update the value when the blink setting is changed.
            // TODO: Change the UI display between the Eyelids setting in AvatarDescriptor and the blink clip.
            if (_av3Setting.ReplaceBlink && _av3Setting.UseBlinkClip)
            {
                if (_av3Setting.BlinkClip != null)
                {
                    var blinkBlendShapes =
                        ExpressionEditorUtils.GetBlendShapeValues(_av3Setting.BlinkClip, _faceBlendShapes.Keys);
                    if (blinkBlendShapes != null)
                        foreach (var item in blinkBlendShapes)
                            _blinkBlendShapes[item.Key] = item.Value;
                }
            }
            else
            {
                var eyeLidsBlendShapes = AV3Utility.GetEyeLidsBlendShapes(avatarDescriptor);
                if (eyeLidsBlendShapes != null)
                    foreach (var item in eyeLidsBlendShapes)
                        _blinkBlendShapes[item] = 0;
            }

            // LipSync blend shapes
            var lipSyncBlendShapes = AV3Utility.GetLipSyncBlendShapes(avatarDescriptor);
            if (lipSyncBlendShapes != null)
                foreach (var item in lipSyncBlendShapes)
                    _lipSyncBlendShapes[item] = 0;

            // Additional toggle objects
            foreach (var gameObject in _av3Setting.AdditionalToggleObjects)
            {
                if (gameObject == null) continue;
                var id = gameObject.GetInstanceID();
                _toggles[id] = (gameObject, gameObject.activeSelf);
            }

            // Additional transform objects
            foreach (var gameObject in _av3Setting.AdditionalTransformObjects)
            {
                if (gameObject == null) continue;
                var id = gameObject.GetInstanceID();
                _transforms[id] = TransformProxy.FromGameObject(gameObject);
            }
        }

        public void OpenTargetClip(AnimationClip targetClip)
        {
            _targetClip = targetClip;

            _animatedBlendShapes.Clear();
            _animatedToggles.Clear();
            _animatedTransforms.Clear();

            // Face mesh blend shapes
            var animatedBlendShapes = ExpressionEditorUtils.GetBlendShapeValues(_targetClip, _faceBlendShapes.Keys);
            foreach (var item in animatedBlendShapes.Where(item =>
                         !EditorPrefsStore.ExpressionEditorSettings.ShowOnlyDifferFromDefaultValue  ||
                         !Mathf.Approximately(item.Value, _faceBlendShapes[item.Key]) ||
                         BlinkBlendShapes.ContainsKey(item.Key) || // Add for warning display
                         LipSyncBlendShapes.ContainsKey(item.Key))) // Add for warning display
            {
                _animatedBlendShapes[item.Key] = item.Value;
            }

            // Additional toggle objects
            foreach (var kvp in _toggles)
            {
                var animatedValue = ExpressionEditorUtils.GetToggleValue(_targetClip, _animator, kvp.Value.target);
                if (animatedValue.HasValue)
                    _animatedToggles[kvp.Key] = (kvp.Value.target, animatedValue.Value);
            }

            // Additional transform objects
            foreach (var kvp in _transforms)
            {
                if (ExpressionEditorUtils.GetTransformValue(_targetClip, _animator, kvp.Value.GameObject) is
                    { } transform) _animatedTransforms[kvp.Key] = transform;
            }
        }

        public List<KeyValuePair<BlendShape, float>> AddAllFaceBlendShapes()
        {
            var added = new List<KeyValuePair<BlendShape, float>>();
            foreach (var kvp in _faceBlendShapes.Where(kvp =>
                         !_animatedBlendShapes.ContainsKey(kvp.Key) &&
                         !BlinkBlendShapes.ContainsKey(kvp.Key) &&
                         !LipSyncBlendShapes.ContainsKey(kvp.Key)))
            {
                _animatedBlendShapes[kvp.Key] = kvp.Value;
                added.Add(kvp);
            }

            return added;
        }

        public void SetBlendShapeValue(BlendShape blendShape, float value) => _animatedBlendShapes[blendShape] = value;
        public void RemoveBlendShapeValue(BlendShape blendShape) => _animatedBlendShapes.Remove(blendShape);
        public void SetToggleValue(int id, bool value) =>
            _animatedToggles[id] = (_toggles[id].target, value);
        public void RemoveToggleValue(int id) => _animatedToggles.Remove(id);
        public void SetTransformValue(int id, TransformProxy val) => _animatedTransforms[id] = val;
        public void RemoveTransformValue(int id) => _animatedTransforms.Remove(id);
    }
}
