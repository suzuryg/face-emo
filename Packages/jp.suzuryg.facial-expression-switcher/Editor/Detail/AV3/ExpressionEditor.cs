using Suzuryg.FacialExpressionSwitcher.Detail.View;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using Suzuryg.FacialExpressionSwitcher.Domain;
using System.Linq;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class ExpressionEditor : IDisposable
    {
        public static readonly float PreviewAvatarPosX = 100;
        public static readonly float PreviewAvatarPosY = 100;
        public static readonly float PreviewAvatarPosZ = 100;

        public AnimationClip Clip { get; private set; }
        public IReadOnlyDictionary<string, float> FaceBlendShapes => _faceBlendShapes;
        public IReadOnlyDictionary<string, float> AnimatedBlendShapesBuffer => _animatedBlendShapesBuffer;

        private ISubWindowProvider _subWindowProvider;
        private MainThumbnailDrawer _mainThumbnailDrawer;
        private GestureTableThumbnailDrawer _gestureTableThumbnailDrawer;
        private AV3Setting _aV3Setting;
        private ExpressionEditorSetting _expressionEditorSetting;

        private GameObject _previewAvatar;
        private AnimationClip _previewClip;
        private Dictionary<string, float> _faceBlendShapes = new Dictionary<string, float>();
        private Dictionary<string, float> _animatedBlendShapes = new Dictionary<string, float>();
        private Dictionary<string, float> _animatedBlendShapesBuffer = new Dictionary<string, float>();

        private CompositeDisposable _disposables = new CompositeDisposable();

        public ExpressionEditor(
            ISubWindowProvider subWindowProvider,
            MainThumbnailDrawer mainThumbnailDrawer,
            GestureTableThumbnailDrawer gestureTableThumbnailDrawer,
            AV3Setting aV3Setting,
            ExpressionEditorSetting expressionEditorSetting)
        {
            // Dependencies
            _subWindowProvider = subWindowProvider;
            _mainThumbnailDrawer = mainThumbnailDrawer;
            _gestureTableThumbnailDrawer = gestureTableThumbnailDrawer;
            _aV3Setting = aV3Setting;
            _expressionEditorSetting = expressionEditorSetting;

            // Add event handlers
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public void Dispose()
        {
            StopSampling();
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            _disposables.Dispose();
        }

        public void Open(AnimationClip animationClip)
        {
            Clip = animationClip;

            // Focus the preview window last because the preview window is likely to be hidden
            _subWindowProvider.Provide<ExpressionEditorWindow>()?.Focus();
            _subWindowProvider.Provide<ExpressionPreviewWindow>()?.Focus();

            FetchBlendShapeValues();
            InitializePreviewClip();
            StartSampling(); // Because preview window is focused.
        }

        public void OpenIfOpenedAlready(AnimationClip animationClip)
        {
            if (_subWindowProvider.ProvideIfOpenedAlready<ExpressionEditorWindow>() is ExpressionEditorWindow &&
                _subWindowProvider.ProvideIfOpenedAlready<ExpressionPreviewWindow>() is ExpressionPreviewWindow)
            {
                Open(animationClip);
            }
        }

        public void StartSampling()
        {
            var avatarRoot = _aV3Setting?.TargetAvatar?.gameObject;
            if (avatarRoot is null) { return; }

            if (_previewAvatar is GameObject) { UnityEngine.Object.DestroyImmediate(_previewAvatar); }
            _previewAvatar = UnityEngine.Object.Instantiate(avatarRoot);

            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(_previewAvatar, _previewClip, _previewClip.length);
            AnimationMode.EndSampling();

            _previewAvatar.transform.position = new Vector3(PreviewAvatarPosX, PreviewAvatarPosY, PreviewAvatarPosZ);
            _previewAvatar.transform.rotation = Quaternion.identity;
        }

        public void StopSampling()
        {
            try
            {
                // Something
            }
            finally
            {
                if (_previewAvatar is GameObject) { UnityEngine.Object.DestroyImmediate(_previewAvatar); }
                AnimationMode.StopAnimationMode();
            }
        }

        public Vector3 GetAvatarViewPosition()
        {
            var origin = new Vector3(PreviewAvatarPosX, PreviewAvatarPosY, PreviewAvatarPosZ);
            var avatarRoot = _aV3Setting?.TargetAvatar?.gameObject;
            if (avatarRoot is GameObject)
            {
                var clonedAvatar = UnityEngine.Object.Instantiate(avatarRoot);
                try
                {
                    if (clonedAvatar.GetComponent<Animator>() is Animator animator && animator.isHuman)
                    {
                        var clip = new AnimationClip();
                        AnimationMode.StartAnimationMode();
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(clonedAvatar, clip, clip.length);
                        AnimationMode.EndSampling();

                        clonedAvatar.transform.position = new Vector3(PreviewAvatarPosX, PreviewAvatarPosY, PreviewAvatarPosZ);
                        clonedAvatar.transform.rotation = Quaternion.identity;

                        return animator.GetBoneTransform(HumanBodyBones.Head).position;
                    }
                    else
                    {
                        return origin;
                    }
                }
                finally
                {
                    AnimationMode.StopAnimationMode();
                    UnityEngine.Object.DestroyImmediate(clonedAvatar);
                }
            }
            else
            {
                return origin;
            }
        }

        public void FetchBlendShapeValues()
        {
            _faceBlendShapes.Clear();
            _animatedBlendShapes.Clear();
            _animatedBlendShapesBuffer.Clear();

            // Get face blendshapes
            _faceBlendShapes = AV3Utility.GetFaceMeshBlendShapes(_aV3Setting?.TargetAvatar, excludeBlink: false, excludeLipSync: true);

            // Get animated blendshapes
            var animatedBlendShapes = GetBlendShapeValues(Clip, _faceBlendShapes.Keys);
            foreach (var blendShape in animatedBlendShapes)
            {
                if (!_expressionEditorSetting.ShowOnlyDifferFromDefaultValue || blendShape.Value != _faceBlendShapes[blendShape.Key])
                {
                    _animatedBlendShapes[blendShape.Key] = blendShape.Value;
                }
            }

            // Initialize buffer
            _animatedBlendShapesBuffer = new Dictionary<string, float>(_animatedBlendShapes);
        }

        public void SetBuffer(string blendShapeName, float newValue)
        {
            // Set buffer
            _animatedBlendShapesBuffer[blendShapeName] = newValue;

            // Update preview clip
            SetBlendShapeValue(_previewClip, blendShapeName, newValue);
            RenderPreviewClip();
        }

        public void RemoveBuffer(string blendShapeName)
        {
            // Remove buffer
            _animatedBlendShapesBuffer.Remove(blendShapeName);

            // Update preview clip
            RemoveBlendShapeValue(_previewClip, blendShapeName);
            RenderPreviewClip();
        }

        public void CheckBuffer()
        {
            // Check for updates
            var updated = new List<string>();
            foreach (var blendShape in _animatedBlendShapesBuffer)
            {
                if (_animatedBlendShapes.ContainsKey(blendShape.Key))
                {
                    // If the value is changed, assume it updated.
                    if (!Mathf.Approximately(blendShape.Value, _animatedBlendShapes[blendShape.Key]))
                    {
                        updated.Add(blendShape.Key);
                    }
                }
                else
                {
                    // If the key is added, assume it updated.
                    updated.Add(blendShape.Key);
                }
            }

            // Check for removes
            var removed = new List<string>();
            foreach (var key in _animatedBlendShapes.Keys)
            {
                if (!_animatedBlendShapesBuffer.ContainsKey(key)) { removed.Add(key); }
            }

            // Update clip
            foreach (var key in updated)
            {
                var value = _animatedBlendShapesBuffer[key];
                _animatedBlendShapes[key] = value;
                Undo.RecordObject(Clip, $"Set {key} to {value}");
                SetBlendShapeValue(Clip, key, value);
            }
            foreach (var key in removed)
            {
                _animatedBlendShapes.Remove(key);
                Undo.RecordObject(Clip, $"Remove {key}");
                RemoveBlendShapeValue(Clip, key);
            }

            // Repaint
            if (updated.Any() || removed.Any())
            {
                _mainThumbnailDrawer.RequestUpdate(Clip);
                _gestureTableThumbnailDrawer.RequestUpdate(Clip);
                // TODO: Repaint MainWindow
                _subWindowProvider.ProvideIfOpenedAlready<GestureTableWindow>()?.Repaint();
            }

            // Initialize buffer
            _animatedBlendShapesBuffer = new Dictionary<string, float>(_animatedBlendShapes);
        }

        private void RenderPreviewClip()
        {
            try
            {
                StartSampling();
                _subWindowProvider.Provide<ExpressionPreviewWindow>()?.UpdateRenderCache();
            }
            finally
            {
                StopSampling();
            }
        }

        private void InitializePreviewClip()
        {
            _previewClip = new AnimationClip();
            EditorUtility.CopySerialized(Clip, _previewClip);
            RenderPreviewClip();
        }

        private void OnUndoRedoPerformed()
        {
            FetchBlendShapeValues();
            InitializePreviewClip();

            _subWindowProvider.ProvideIfOpenedAlready<ExpressionEditorWindow>()?.Repaint();
            _subWindowProvider.ProvideIfOpenedAlready<ExpressionPreviewWindow>()?.Repaint();
        }

        private Dictionary<string, EditorCurveBinding> GetBlendShapeBindings(IEnumerable<string> blendShapeNames)
        {
            var animator = _aV3Setting?.TargetAvatar?.gameObject?.GetComponent<Animator>();
            var faceMesh = AV3Utility.GetFaceMesh(_aV3Setting?.TargetAvatar);
            var transformPath = AnimationUtility.CalculateTransformPath(faceMesh?.transform, animator?.transform);
            var bindings = new Dictionary<string, EditorCurveBinding>();
            foreach (var blendShapeName in blendShapeNames)
            {
                bindings[blendShapeName] = new EditorCurveBinding { path = transformPath, propertyName = $"blendShape.{blendShapeName}", type = typeof(SkinnedMeshRenderer) };
            }
            return bindings;
        }

        private Dictionary<string, float> GetBlendShapeValues(AnimationClip animationClip, IEnumerable<string> blendShapeNames)
        {
            var blendShapes = new Dictionary<string, float>();
            if (animationClip is null) { return blendShapes; }

            var bindings = GetBlendShapeBindings(blendShapeNames);
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(animationClip, binding.Value);
                if (curve is AnimationCurve && curve.keys.Length > 0)
                {
                    var value = curve.keys[0].value;
                    blendShapes[binding.Key] = value;
                }
            }
            return blendShapes;
        }

        private void SetBlendShapeValue(AnimationClip animationClip, string blendShapeName, float newValue)
        {
            var binding = GetBlendShapeBindings(new[] { blendShapeName }).FirstOrDefault();
            var curve = AnimationUtility.GetEditorCurve(animationClip, binding.Value);
            if (curve is AnimationCurve && curve.keys.Length > 0)
            {
                // Modify
                curve.keys = curve.keys.Select(keyframe => { keyframe.value = newValue; return keyframe; }).ToArray();
            }
            else
            {
                // Add
                curve = new AnimationCurve(new Keyframe(time: 0, value: newValue));
            }
            AnimationUtility.SetEditorCurve(animationClip, binding.Value, curve);
        }

        private void RemoveBlendShapeValue(AnimationClip animationClip, string blendShapeName)
        {
            var binding = GetBlendShapeBindings(new[] { blendShapeName }).FirstOrDefault();
            AnimationUtility.SetEditorCurve(animationClip, binding.Value, null);
        }
    }
}
