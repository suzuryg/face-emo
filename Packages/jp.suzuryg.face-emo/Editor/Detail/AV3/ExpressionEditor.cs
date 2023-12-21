using Suzuryg.FaceEmo.Detail.View;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Components.Settings;
using System.Linq;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public class ExpressionEditor : IDisposable
    {
        public static readonly float PreviewAvatarPosX = 100;
        public static readonly float PreviewAvatarPosY = 100;
        public static readonly float PreviewAvatarPosZ = 100;

        public AnimationClip Clip { get; private set; }
        public string FaceMeshTransformPath { get; private set; }
        public HashSet<BlendShape> BlinkBlendShapes = new HashSet<BlendShape>();
        public HashSet<BlendShape> LipSyncBlendShapes = new HashSet<BlendShape>();
        public IReadOnlyDictionary<BlendShape, float> FaceBlendShapes => _faceBlendShapes;
        public IReadOnlyDictionary<BlendShape, float> AnimatedBlendShapesBuffer => _animatedBlendShapesBuffer;
        public IReadOnlyDictionary<int, (GameObject gameObject, bool isActive)> AdditionalToggles => _additionalToggles;
        public IReadOnlyDictionary<int, (GameObject gameObject, bool isActive)> AnimatedAdditionalTogglesBuffer => _animatedAdditionalTogglesBuffer;
        public IReadOnlyDictionary<int, TransformProxy> AdditionalTransforms => _additionalTransforms;
        public IReadOnlyDictionary<int, TransformProxy> AnimatedAdditionalTransformsBuffer => _animatedAdditionalTransformsBuffer;
        public IObservable<Unit> OnClipUpdated => _onClipUpdated.AsObservable();
        public bool IsDisposed => _disposables is null || _disposables.IsDisposed;

        private ISubWindowProvider _subWindowProvider;
        private ILocalizationSetting _localizationSetting;
        private MainThumbnailDrawer _mainThumbnailDrawer;
        private GestureTableThumbnailDrawer _gestureTableThumbnailDrawer;
        private AV3Setting _aV3Setting;
        private ExpressionEditorSetting _expressionEditorSetting;

        private GameObject _previewAvatar;
        private AnimationClip _previewClip;
        private BlendShape _previewBlendShape;
        private Dictionary<BlendShape, float> _faceBlendShapes = new Dictionary<BlendShape, float>();
        private Dictionary<BlendShape, float> _animatedBlendShapes = new Dictionary<BlendShape, float>();
        private Dictionary<BlendShape, float> _animatedBlendShapesBuffer = new Dictionary<BlendShape, float>();
        private Dictionary<int, (GameObject gameObject, bool isActive)> _additionalToggles = new Dictionary<int, (GameObject gameObject, bool isActive)>();
        private Dictionary<int, (GameObject gameObject, bool isActive)> _animatedAdditionalToggles = new Dictionary<int, (GameObject gameObject, bool isActive)>();
        private Dictionary<int, (GameObject gameObject, bool isActive)> _animatedAdditionalTogglesBuffer = new Dictionary<int, (GameObject gameObject, bool isActive)>();
        private Dictionary<int, TransformProxy> _additionalTransforms = new Dictionary<int, TransformProxy>();
        private Dictionary<int, TransformProxy> _animatedAdditionalTransforms = new Dictionary<int, TransformProxy>();
        private Dictionary<int, TransformProxy> _animatedAdditionalTransformsBuffer = new Dictionary<int, TransformProxy>();
        private Subject<Unit> _onClipUpdated = new Subject<Unit>();
        private bool _isRepaintOtherWindowsNeeded = false;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public ExpressionEditor(
            ISubWindowProvider subWindowProvider,
            ILocalizationSetting localizationSetting,
            MainThumbnailDrawer mainThumbnailDrawer,
            GestureTableThumbnailDrawer gestureTableThumbnailDrawer,
            AV3Setting aV3Setting,
            ExpressionEditorSetting expressionEditorSetting)
        {
            // Dependencies
            _subWindowProvider = subWindowProvider;
            _localizationSetting = localizationSetting;
            _mainThumbnailDrawer = mainThumbnailDrawer;
            _gestureTableThumbnailDrawer = gestureTableThumbnailDrawer;
            _aV3Setting = aV3Setting;
            _expressionEditorSetting = expressionEditorSetting;

            // Add event handlers
            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            // Periodic repaint
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(5)).ObserveOnMainThread().Subscribe(_ => RepaintOtherWindows()).AddTo(_disposables);
        }

        public void Dispose()
        {
            StopSampling();
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            _disposables.Dispose();
        }

        public void Open(AnimationClip animationClip)
        {
            RepaintOtherWindows();

            Clip = animationClip;

            // Focus the preview window last because the preview window is likely to be hidden
            try
            {
                _subWindowProvider.Provide<ExpressionEditorWindow>()?.Focus();
                _subWindowProvider.Provide<ExpressionPreviewWindow>()?.Focus();
            }
            catch (NullReferenceException)
            {
                // Somehow even if I do NULL checks with "window ! = null", NullReferenceException will be thrown.
            }

            FetchProperties();
            InitializePreviewClip();
            StartSampling(); // Because preview window is focused.
        }

        public void OpenIfOpenedAlready(AnimationClip animationClip)
        {
            if (_subWindowProvider.ProvideIfOpenedAlready<ExpressionEditorWindow>() is ExpressionEditorWindow ||
                _subWindowProvider.ProvideIfOpenedAlready<ExpressionPreviewWindow>() is ExpressionPreviewWindow)
            {
                Open(animationClip);
            }
        }

        public void SetPreviewBlendShape(BlendShape blendShape)
        {
            var previous = _previewBlendShape;
            _previewBlendShape = blendShape;
            if (previous != _previewBlendShape) { RenderPreviewClip(); }
        }

        public void StartSampling()
        {
            var avatarRoot = _aV3Setting?.TargetAvatar?.gameObject;
            if (avatarRoot == null) { return; }

            if (_previewAvatar != null) { UnityEngine.Object.DestroyImmediate(_previewAvatar); }
            _previewAvatar = UnityEngine.Object.Instantiate(avatarRoot);
            // FIXME: Unable to support the case that avatar's body shape balance is tuned by root object's scale. (Is it necessary to assume this case...?)
            _previewAvatar.transform.localScale = Vector3.one;
            _previewAvatar.SetActive(true);
            _previewAvatar.hideFlags = HideFlags.HideAndDontSave;

            // Synthesize preview blend shape
            AnimationClip synthesized;
            if (_previewBlendShape == null)
            {
                synthesized = _previewClip;
            }
            else
            {
                synthesized = new AnimationClip();
                if (_previewClip != null) { EditorUtility.CopySerialized(_previewClip, synthesized); }
                SetBlendShapeValue(synthesized, _previewBlendShape, 100);
            }

            // Sample
            if (synthesized != null)
            {
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_previewAvatar, synthesized, synthesized.length);
                AnimationMode.EndSampling();
            }

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
                if (_previewAvatar != null) { UnityEngine.Object.DestroyImmediate(_previewAvatar); }
                AnimationMode.StopAnimationMode();
            }
        }

        public Vector3 GetAvatarViewPosition()
        {
            // Returns view position if previewable in T-pose.
            if (AV3Utility.GetAvatarPoseClip(_aV3Setting?.TargetAvatar as VRCAvatarDescriptor) != null && (_aV3Setting?.TargetAvatar as VRCAvatarDescriptor)?.GetScaledViewPosition() != null)
            {
                return (_aV3Setting.TargetAvatar as VRCAvatarDescriptor).GetScaledViewPosition() + new Vector3(PreviewAvatarPosX, PreviewAvatarPosY, PreviewAvatarPosZ);
            }
            // Returns head position if not previewable in T-pose.
            else
            {
                return GetAvatarHeadPosition();
            }
        }

        public Vector3 GetAvatarHeadPosition()
        {
            var origin = new Vector3(PreviewAvatarPosX, PreviewAvatarPosY, PreviewAvatarPosZ);
            var avatarRoot = _aV3Setting?.TargetAvatar?.gameObject;
            if (avatarRoot != null)
            {
                var clonedAvatar = UnityEngine.Object.Instantiate(avatarRoot);
                // FIXME: Unable to support the case that avatar's body shape balance is tuned by root object's scale. (Is it necessary to assume this case...?)
                clonedAvatar.transform.localScale = Vector3.one;

                try
                {
                    var animator = clonedAvatar.GetComponent<Animator>();
                    if (animator != null && animator.isHuman)
                    {
                        var clip = AV3Utility.GetAvatarPoseClip(_aV3Setting?.TargetAvatar as VRCAvatarDescriptor);
                        if (clip == null) { clip = new AnimationClip(); }
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
                    if (clonedAvatar != null) { UnityEngine.Object.DestroyImmediate(clonedAvatar); }
                }
            }
            else
            {
                return origin;
            }
        }

        public void FetchProperties()
        {
            BlinkBlendShapes.Clear();
            LipSyncBlendShapes.Clear();

            _faceBlendShapes.Clear();
            _animatedBlendShapes.Clear();
            _animatedBlendShapesBuffer.Clear();

            _additionalTransforms.Clear();
            _animatedAdditionalTransforms.Clear();
            _animatedAdditionalTransformsBuffer.Clear();

            _additionalToggles.Clear();
            _animatedAdditionalToggles.Clear();
            _animatedAdditionalTogglesBuffer.Clear();

            // Get face mesh path
            var faceMesh = AV3Utility.GetFaceMesh(_aV3Setting.TargetAvatar as VRCAvatarDescriptor);
            if (faceMesh != null)
            {
                FaceMeshTransformPath = AV3Utility.GetPathFromAvatarRoot(faceMesh.transform, _aV3Setting.TargetAvatar as VRCAvatarDescriptor);
            }

            // Get face blendshapes
            _faceBlendShapes = AV3Utility.GetFaceMeshBlendShapeValues(_aV3Setting.TargetAvatar as VRCAvatarDescriptor, excludeBlink: false, excludeLipSync: false);
            foreach (var mesh in _aV3Setting.AdditionalSkinnedMeshes)
            {
                var blendShapes = AV3Utility.GetBlendShapeValues(mesh, _aV3Setting.TargetAvatar as VRCAvatarDescriptor, excludeBlink: false, excludeLipSync: false);
                foreach (var item in blendShapes) { _faceBlendShapes[item.Key] = item.Value; }
            }

            // TODO: Update the value when the blink setting is changed.
            // TODO: Change the UI display between the Eyelids setting in AvatarDescriptor and the blink clip.
            if (_aV3Setting.ReplaceBlink && _aV3Setting.UseBlinkClip)
            {
                if (_aV3Setting.BlinkClip != null)
                {
                    BlinkBlendShapes = new HashSet<BlendShape>(GetBlendShapeValues(_aV3Setting.BlinkClip, _faceBlendShapes.Keys).Keys);
                }
                else
                {
                    BlinkBlendShapes = new HashSet<BlendShape>();
                }
            }
            else
            {
                BlinkBlendShapes = new HashSet<BlendShape>(AV3Utility.GetEyeLidsBlendShapes(_aV3Setting.TargetAvatar as VRCAvatarDescriptor));
            }

            LipSyncBlendShapes = new HashSet<BlendShape>(AV3Utility.GetLipSyncBlendShapes(_aV3Setting.TargetAvatar as VRCAvatarDescriptor));

            var animatedBlendShapes = GetBlendShapeValues(Clip, _faceBlendShapes.Keys);
            var showOnlyDifference = EditorPrefs.GetBool(DetailConstants.Key_ExpressionEditor_ShowOnlyDifferFromDefaultValue, DetailConstants.Default_ExpressionEditor_ShowOnlyDifferFromDefaultValue);
            foreach (var blendShape in animatedBlendShapes)
            {
                if (!showOnlyDifference ||
                    blendShape.Value != _faceBlendShapes[blendShape.Key] ||
                    BlinkBlendShapes.Contains(blendShape.Key) || // Add for warning display
                    LipSyncBlendShapes.Contains(blendShape.Key))   // Add for warning display
                {
                    _animatedBlendShapes[blendShape.Key] = blendShape.Value;
                }
            }

            // Exclude blendshapes
            foreach (var excluded in _aV3Setting.ExcludedBlendShapes)
            {
                while (_faceBlendShapes.ContainsKey(excluded)) { _faceBlendShapes.Remove(excluded); }
            }

            // Get additional toggles
            foreach (var gameObject in _aV3Setting?.AdditionalToggleObjects)
            {
                if (gameObject != null)
                {
                    var id = gameObject.GetInstanceID();
                    _additionalToggles[id] = (gameObject, gameObject.activeSelf);

                    var animatedValue = GetToggleValue(Clip, gameObject);
                    if (animatedValue.HasValue) { _animatedAdditionalToggles[id] = (gameObject, animatedValue.Value); }
                }
            }

            // Get additional transforms
            foreach (var gameObject in _aV3Setting?.AdditionalTransformObjects)
            {
                if (gameObject != null) 
                {
                    var id = gameObject.GetInstanceID();
                    _additionalTransforms[id] = TransformProxy.FromGameObject(gameObject);
                    if (GetTransformValue(Clip, gameObject) is TransformProxy transform) { _animatedAdditionalTransforms[id] = transform; }
                }
            }

            // Initialize buffer
            _animatedBlendShapesBuffer = new Dictionary<BlendShape, float>(_animatedBlendShapes);
            _animatedAdditionalTogglesBuffer = new Dictionary<int, (GameObject gameObject, bool isActive)>(_animatedAdditionalToggles);
            _animatedAdditionalTransformsBuffer = new Dictionary<int, TransformProxy>(_animatedAdditionalTransforms);
        }

        public void AddAllFaceBlendShapes()
        {
            var animatedBlendShapes = GetBlendShapeValues(Clip, _faceBlendShapes.Keys);
            foreach (var blendShape in _faceBlendShapes)
            {
                if (!animatedBlendShapes.ContainsKey(blendShape.Key) &&
                    !BlinkBlendShapes.Contains(blendShape.Key) &&
                    !LipSyncBlendShapes.Contains(blendShape.Key))
                {
                    _animatedBlendShapesBuffer[blendShape.Key] = blendShape.Value;
                    SetBlendShapeValue(_previewClip, blendShape.Key, blendShape.Value);
                }
            }
            RenderPreviewClip();
            CheckBuffer();
        }

        public void SetBlendShapeBuffer(BlendShape blendShape, float newValue)
        {
            // Set buffer
            _animatedBlendShapesBuffer[blendShape] = newValue;

            // Update preview clip
            SetBlendShapeValue(_previewClip, blendShape, newValue);
            RenderPreviewClip();
        }

        public void SetToggleBuffer(int objectId, bool isActive) 
        {
            var gameObject = GetMatchedToggleObject(objectId);
            if (gameObject == null) { return; }

            // Set buffer
            _animatedAdditionalTogglesBuffer[objectId] = (gameObject, isActive);

            // Update preview clip
            SetToggleValue(_previewClip, gameObject, isActive);
            RenderPreviewClip();
        }

        public void SetTransformBuffer(int objectId, TransformProxy transform)
        {
            // Set buffer
            _animatedAdditionalTransformsBuffer[objectId] = transform;

            // Update preview clip
            SetTransformValue(_previewClip, transform);
            RenderPreviewClip();
        }

        public void RemoveBlendShapeBuffer(BlendShape blendShape)
        {
            // Remove buffer
            _animatedBlendShapesBuffer.Remove(blendShape);

            // Update preview clip
            RemoveBlendShapeValue(_previewClip, blendShape);
            RenderPreviewClip();
        }

        public void RemoveToggleBuffer(int objectId)
        {
            // Remove buffer
            var gameObject = _animatedAdditionalTogglesBuffer[objectId].gameObject;
            _animatedAdditionalTogglesBuffer.Remove(objectId);

            // Update preview clip
            RemoveToggleValue(_previewClip, gameObject);
            RenderPreviewClip();
        }

        public void RemoveTransformBuffer(int objectId)
        {
            // Remove buffer
            var transform = _animatedAdditionalTransformsBuffer[objectId];
            _animatedAdditionalTransformsBuffer.Remove(objectId);

            // Update preview clip
            RemoveTransformValue(_previewClip, transform?.GameObject);
            RenderPreviewClip();
        }

        public void CheckBuffer()
        {
            // Null check
            if (Clip == null)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationSetting.GetCurrentLocaleTable().ExpressionEditorView_Message_ClipIsNull, "OK");
                return;
            }

            // Check for updates
            // BlendShape
            var updatedBlendShapes = new List<BlendShape>();
            foreach (var blendShape in _animatedBlendShapesBuffer)
            {
                if (_animatedBlendShapes.ContainsKey(blendShape.Key))
                {
                    // If the value is changed, assume it updated.
                    if (!Mathf.Approximately(blendShape.Value, _animatedBlendShapes[blendShape.Key]))
                    {
                        updatedBlendShapes.Add(blendShape.Key);
                    }
                }
                else
                {
                    // If the key is added, assume it updated.
                    updatedBlendShapes.Add(blendShape.Key);
                }
            }
            // Toggle
            var updatedToggles = new List<int>();
            foreach (var toggle in _animatedAdditionalTogglesBuffer)
            {
                if (_animatedAdditionalToggles.ContainsKey(toggle.Key))
                {
                    // If the value is changed, assume it updated.
                    if (_animatedAdditionalToggles[toggle.Key] != toggle.Value)
                    {
                        updatedToggles.Add(toggle.Key);
                    }
                }
                else
                {
                    // If the key is added, assume it updated.
                    updatedToggles.Add(toggle.Key);
                }
            }
            // Transform
            var updatedTransforms = new List<int>();
            foreach (var transform in _animatedAdditionalTransformsBuffer)
            {
                if (_animatedAdditionalTransforms.ContainsKey(transform.Key))
                {
                    // If the value is changed, assume it updated.
                    if (TransformProxy.IsUpdated(_animatedAdditionalTransforms[transform.Key], transform.Value))
                    {
                        updatedTransforms.Add(transform.Key);
                    }
                }
                else
                {
                    // If the key is added, assume it updated.
                    updatedTransforms.Add(transform.Key);
                }
            }

            // Check for removes
            // BlendShape
            var removedBlendShapes = new List<BlendShape>();
            foreach (var key in _animatedBlendShapes.Keys)
            {
                if (!_animatedBlendShapesBuffer.ContainsKey(key)) { removedBlendShapes.Add(key); }
            }
            // Toggle
            var removedToggles = new List<int>();
            foreach (var key in _animatedAdditionalToggles.Keys)
            {
                if (!_animatedAdditionalTogglesBuffer.ContainsKey(key)) { removedToggles.Add(key); }
            }
            // Transform
            var removedTransforms = new List<int>();
            foreach (var key in _animatedAdditionalTransforms.Keys)
            {
                if (!_animatedAdditionalTransformsBuffer.ContainsKey(key)) { removedTransforms.Add(key); }
            }

            // Update clip
            // BlendShape
            foreach (var key in updatedBlendShapes)
            {
                var value = _animatedBlendShapesBuffer[key];
                _animatedBlendShapes[key] = value;
                Undo.RecordObject(Clip, $"Set {key} to {value}");
                SetBlendShapeValue(Clip, key, value);
            }
            foreach (var key in removedBlendShapes)
            {
                _animatedBlendShapes.Remove(key);
                Undo.RecordObject(Clip, $"Remove {key}");
                RemoveBlendShapeValue(Clip, key);
            }
            // Toggle
            foreach (var key in updatedToggles)
            {
                var value = _animatedAdditionalTogglesBuffer[key];
                var text = value.isActive ? "enabled" : "disabled";

                _animatedAdditionalToggles[key] = value;
                Undo.RecordObject(Clip, $"Set {value.gameObject} to {text}");
                SetToggleValue(Clip, value.gameObject, value.isActive);
            }
            foreach (var key in removedToggles)
            {
                var value = _animatedAdditionalToggles[key];
                _animatedAdditionalToggles.Remove(key);
                Undo.RecordObject(Clip, $"Remove Toggle of {value.gameObject.name}");
                RemoveToggleValue(Clip, value.gameObject);
            }
            // Transform
            foreach (var key in updatedTransforms)
            {
                var value = _animatedAdditionalTransformsBuffer[key];
                _animatedAdditionalTransforms[key] = value;
                Undo.RecordObject(Clip, $"Update Transform of {value?.GameObject?.name}");
                SetTransformValue(Clip, value);
            }
            foreach (var key in removedTransforms)
            {
                var gameObject = _animatedAdditionalTransforms[key]?.GameObject;
                _animatedAdditionalTransforms.Remove(key);
                Undo.RecordObject(Clip, $"Remove Transform of {gameObject?.name}");
                RemoveTransformValue(Clip, gameObject);
            }

            // Repaint
            if (updatedBlendShapes.Any() || removedBlendShapes.Any() || updatedToggles.Any() || removedToggles.Any() || updatedTransforms.Any() || removedTransforms.Any())
            {
                _isRepaintOtherWindowsNeeded = true;
            }

            // Initialize buffer
            _animatedBlendShapesBuffer = new Dictionary<BlendShape, float>(_animatedBlendShapes);
            _animatedAdditionalTransformsBuffer = new Dictionary<int, TransformProxy>(_animatedAdditionalTransforms);
        }

        public void RepaintOtherWindows()
        {
            if (_isRepaintOtherWindowsNeeded)
            {
                _isRepaintOtherWindowsNeeded = false;

                _mainThumbnailDrawer.RequestUpdate(Clip);
                _gestureTableThumbnailDrawer.RequestUpdate(Clip);
                _subWindowProvider.ProvideIfOpenedAlready<GestureTableWindow>()?.Repaint();
                _onClipUpdated.OnNext(Unit.Default);
            }
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
            _previewClip = AV3Utility.SynthesizeAvatarPose(Clip, _aV3Setting?.TargetAvatar as VRCAvatarDescriptor);
            RenderPreviewClip();
        }

        private void OnUndoRedoPerformed()
        {
            // Update ExpressionEditorWindow
            var expressionEditorWindow = _subWindowProvider.ProvideIfOpenedAlready<ExpressionEditorWindow>();
            if (expressionEditorWindow is ExpressionEditorWindow)
            {
                FetchProperties();
                InitializePreviewClip();
                expressionEditorWindow.Repaint();
            }

            // Update ExpressionPreviewWindow
            _subWindowProvider.ProvideIfOpenedAlready<ExpressionPreviewWindow>()?.Repaint();

            // Update other windows
            RepaintOtherWindows();
        }

        private Dictionary<BlendShape, EditorCurveBinding> GetBlendShapeBindings(IEnumerable<BlendShape> blendShapes)
        {
            var bindings = new Dictionary<BlendShape, EditorCurveBinding>();
            foreach (var blendShape in blendShapes)
            {
                bindings[blendShape] = new EditorCurveBinding { path = blendShape.Path, propertyName = $"blendShape.{blendShape.Name}", type = typeof(SkinnedMeshRenderer) };
            }
            return bindings;
        }

        private Dictionary<BlendShape, float> GetBlendShapeValues(AnimationClip animationClip, IEnumerable<BlendShape> blendShapes)
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

        private void SetBlendShapeValue(AnimationClip animationClip, BlendShape blendShape, float newValue)
        {
            var binding = GetBlendShapeBindings(new[] { blendShape }).FirstOrDefault();
            var curve = new AnimationCurve(new Keyframe(time: 0, value: newValue));
            AnimationUtility.SetEditorCurve(animationClip, binding.Value, curve);
        }

        private void RemoveBlendShapeValue(AnimationClip animationClip, BlendShape blendShape)
        {
            var binding = GetBlendShapeBindings(new[] { blendShape }).FirstOrDefault();
            AnimationUtility.SetEditorCurve(animationClip, binding.Value, null);
        }

        private bool? GetToggleValue(AnimationClip animationClip, GameObject gameObject)
        {
            var animator = _aV3Setting?.TargetAvatar?.gameObject?.GetComponent<Animator>();
            var transformPath = AnimationUtility.CalculateTransformPath(gameObject?.transform, animator?.transform);

            var curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_IsActive", type = typeof(GameObject) });
            if (curve != null && curve.keys.Length > 0)
            {
                var value = curve.keys[0].value;
                if (value > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return null;
            }
        }

        private GameObject GetMatchedToggleObject(int objectId)
        {
            GameObject targetObject = null;
            foreach (var gameObject in _aV3Setting?.AdditionalToggleObjects)
            {
                if (gameObject != null && gameObject?.GetInstanceID() == objectId)
                {
                    targetObject = gameObject;
                    break;
                }
            }
            return targetObject;
        }

        private void SetToggleValue(AnimationClip animationClip, GameObject gameObject, bool isActive)
        {
            var animator = _aV3Setting?.TargetAvatar?.gameObject?.GetComponent<Animator>();
            var transformPath = AnimationUtility.CalculateTransformPath(gameObject?.transform, animator?.transform);
            var curve = new AnimationCurve(new Keyframe(time: 0, value: isActive ? 1 : 0));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_IsActive", type = typeof(GameObject) }, curve);
        }

        private void RemoveToggleValue(AnimationClip animationClip, GameObject gameObject)
        {
            var animator = _aV3Setting?.TargetAvatar?.gameObject?.GetComponent<Animator>();
            var transformPath = AnimationUtility.CalculateTransformPath(gameObject?.transform, animator?.transform);
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_IsActive", type = typeof(GameObject) }, null);
        }

        private TransformProxy GetTransformValue(AnimationClip animationClip, GameObject gameObject)
        {
            var propertyExists = false;
            var transform = TransformProxy.FromGameObject(gameObject);

            var animator = _aV3Setting?.TargetAvatar?.gameObject?.GetComponent<Animator>();
            var transformPath = AnimationUtility.CalculateTransformPath(gameObject?.transform, animator?.transform);

            AnimationCurve curve;

            // Get local position
            curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.x", type = typeof(Transform) });
            if (curve != null && curve.keys.Length > 0) { transform.PositionX = curve.keys[0].value; propertyExists = true; }
            curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.y", type = typeof(Transform) });
            if (curve != null && curve.keys.Length > 0) { transform.PositionY = curve.keys[0].value; propertyExists = true; }
            curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.z", type = typeof(Transform) });
            if (curve != null && curve.keys.Length > 0) { transform.PositionZ = curve.keys[0].value; propertyExists = true; }

            // Get local rotation
            curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.x", type = typeof(Transform) });
            if (curve != null && curve.keys.Length > 0) { transform.RotationX = curve.keys[0].value; propertyExists = true; }
            curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.y", type = typeof(Transform) });
            if (curve != null && curve.keys.Length > 0) { transform.RotationY = curve.keys[0].value; propertyExists = true; }
            curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.z", type = typeof(Transform) });
            if (curve != null && curve.keys.Length > 0) { transform.RotationZ = curve.keys[0].value; propertyExists = true; }

            // Get local scale
            curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.x", type = typeof(Transform) });
            if (curve != null && curve.keys.Length > 0) { transform.ScaleX = curve.keys[0].value; propertyExists = true; }
            curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.y", type = typeof(Transform) });
            if (curve != null && curve.keys.Length > 0) { transform.ScaleY = curve.keys[0].value; propertyExists = true; }
            curve = AnimationUtility.GetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.z", type = typeof(Transform) });
            if (curve != null && curve.keys.Length > 0) { transform.ScaleZ = curve.keys[0].value; propertyExists = true; }

            if (propertyExists) { return transform; }
            else { return null; }
        }

        private void SetTransformValue(AnimationClip animationClip, TransformProxy transform)
        {
            var animator = _aV3Setting?.TargetAvatar?.gameObject?.GetComponent<Animator>();
            var transformPath = AnimationUtility.CalculateTransformPath(transform?.GameObject?.transform, animator?.transform);

            AnimationCurve curve;

            // Set local position
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.PositionX));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.x", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.PositionY));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.y", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.PositionZ));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalPosition.z", type = typeof(Transform) }, curve);

            // Set local rotation
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.RotationX));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.x", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.RotationY));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.y", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.RotationZ));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "localEulerAnglesRaw.z", type = typeof(Transform) }, curve);

            // Set local scale
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.ScaleX));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.x", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.ScaleY));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.y", type = typeof(Transform) }, curve);
            curve = new AnimationCurve(new Keyframe(time: 0, value: transform.ScaleZ));
            AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding { path = transformPath, propertyName = "m_LocalScale.z", type = typeof(Transform) }, curve);
        }

        private void RemoveTransformValue(AnimationClip animationClip, GameObject gameObject)
        {
            var animator = _aV3Setting?.TargetAvatar?.gameObject?.GetComponent<Animator>();
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
