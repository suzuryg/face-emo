using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.ExpressionEditor.Models;
using Suzuryg.FaceEmo.Detail.ExpressionEditor.Views;
using Suzuryg.FaceEmo.Detail.Localization;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Presenters
{
    internal sealed class ExpressionEditorPresenter : IExpressionEditor
    {
        private readonly MainThumbnailDrawer _mainThumbnailDrawer;
        private readonly GestureTableThumbnailDrawer _gestureTableThumbnailDrawer;
        private readonly ExpressionEditorModelFacade _modelFacade;
        private readonly PropertyEditorViewFacade _viewFacade;

        private readonly HashSet<AnimationClip> _thumbnailUpdateRequests = new();

        private readonly CompositeDisposable _disposables = new();

        [CanBeNull] private PropertyEditorWindow _propertyEditorWindow;
        [CanBeNull] private PreviewWindow _previewWindow;
        [CanBeNull] private AnimationClip _currentClip;

        public ExpressionEditorPresenter(MainThumbnailDrawer mainThumbnailDrawer,
            GestureTableThumbnailDrawer gestureTableThumbnailDrawer,
            AV3Setting av3Setting, ExpressionEditorSetting expressionEditorSetting,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _mainThumbnailDrawer = mainThumbnailDrawer;
            _gestureTableThumbnailDrawer = gestureTableThumbnailDrawer;
            _modelFacade = new ExpressionEditorModelFacade(av3Setting).AddTo(_disposables);

            _modelFacade.OnThumbnailUpdateRequested.Subscribe(x => _thumbnailUpdateRequests.Add(x)).AddTo(_disposables);

            _viewFacade = new PropertyEditorViewFacade(_modelFacade.BlinkBlendShapes, _modelFacade.LipSyncBlendShapes,
                _modelFacade.FaceBlendShapes, _modelFacade.Toggles, _modelFacade.Transforms,
                _modelFacade.AnimatedBlendShapes, _modelFacade.AnimatedToggles, _modelFacade.AnimatedTransforms,
                expressionEditorSetting, localizationSetting).AddTo(_disposables);

            _viewFacade.OnOpenClipRequested.Subscribe(Open).AddTo(_disposables);

            _viewFacade.OnBlendShapeValueChanged.Subscribe(x => _modelFacade.SetBlendShapeValue(x.blendShape, x.value))
                .AddTo(_disposables);

            _viewFacade.OnBlendShapeAdded.Subscribe(x =>
            {
                _modelFacade.SetBlendShapeValue(x, 100);
                _viewFacade.RebuildAnimatedPropertyViews();
            }).AddTo(_disposables);

            _viewFacade.OnBlendShapeRemoved.Subscribe(x =>
            {
                _modelFacade.RemoveBlendShapeValue(x);
                _viewFacade.RebuildAnimatedPropertyViews();
            }).AddTo(_disposables);

            _viewFacade.OnHoveredBlendShapeChanged.Subscribe(_modelFacade.RequestChangePreviewOverride)
                .AddTo(_disposables);

            _viewFacade.OnAddAllBlendShapesRequested.Subscribe(_ =>
            {
                _modelFacade.AddAllBlendShapes();
                _viewFacade.RebuildAnimatedPropertyViews();
            }).AddTo(_disposables);

            _viewFacade.OnToggleValueChanged.Subscribe(x => _modelFacade.SetToggleValue(x.id, x.target, x.value))
                .AddTo(_disposables);

            _viewFacade.OnToggleAdded.Subscribe(x =>
            {
                _modelFacade.SetToggleValue(x.id, x.target, x.value);
                _viewFacade.RebuildAnimatedPropertyViews();
            }).AddTo(_disposables);

            _viewFacade.OnToggleRemoved.Subscribe(x =>
            {
                _modelFacade.RemoveToggleValue(x.id, x.target);
                _viewFacade.RebuildAnimatedPropertyViews();
            }).AddTo(_disposables);

            _viewFacade.OnTransformValueChanged.Subscribe(x => _modelFacade.SetTransformValue(x.id, x.value))
                .AddTo(_disposables);

            _viewFacade.OnTransformAdded.Subscribe(x =>
            {
                _modelFacade.SetTransformValue(x.id, x.value);
                _viewFacade.RebuildAnimatedPropertyViews();
            }).AddTo(_disposables);

            _viewFacade.OnTransformRemoved.Subscribe(x =>
            {
                _modelFacade.RemoveTransformValue(x.id, x.value);
                _viewFacade.RebuildAnimatedPropertyViews();
            }).AddTo(_disposables);

            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnChangedPlayModeState;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public void Dispose()
        {
            EditorApplication.update -= Update;
            EditorApplication.playModeStateChanged -= OnChangedPlayModeState;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            _propertyEditorWindow?.CloseIfNotDocked();
            _previewWindow?.CloseIfNotDocked();

            _disposables.Dispose();
        }

        public void OpenIfOpenedAlready(AnimationClip clip)
        {
            if (EditorWindow.HasOpenInstances<PropertyEditorWindow>() ||
                EditorWindow.HasOpenInstances<PreviewWindow>()) Open(clip);
        }

        public void Open(AnimationClip clip)
        {
            _currentClip = clip;

            OpenWindows();
            _modelFacade.FetchPreviewAvatar();
            _modelFacade.OpenTargetClip(clip);
            _viewFacade.OpenTargetClip(clip);
            _viewFacade.RebuildAllViews();

            _previewWindow?.Initialize(_modelFacade.GetAvatarViewPosition());
        }

        private void OpenWindows()
        {
            _propertyEditorWindow = EditorWindow.GetWindow<PropertyEditorWindow>();
            _previewWindow = EditorWindow.GetWindow<PreviewWindow>();
            _propertyEditorWindow?.Initialize(_viewFacade);
        }

        private void Update()
        {
            CheckSampling();
            CheckThumbnailUpdateRequests();
        }

        private void CheckSampling()
        {
            if (AnimationMode.InAnimationMode())
            {
                if (_propertyEditorWindow?.InUse != true && _previewWindow?.InUse != true)
                {
                    _previewWindow?.UpdateRenderCache();
                    _modelFacade.StopSampling();
                }
            }
            else
            {
                if (_propertyEditorWindow?.InUse == true || _previewWindow?.InUse == true)
                    _modelFacade.StartSampling();
            }
        }

        private void CheckThumbnailUpdateRequests()
        {
            if (!_thumbnailUpdateRequests.Any()) return;
            if (_propertyEditorWindow?.InUse == true || _previewWindow?.InUse == true) return;
            UpdateThumbnails();
        }

        private void OnUndoRedoPerformed()
        {
            if (_currentClip == null) return;
            OpenIfOpenedAlready(_currentClip);
            _propertyEditorWindow?.Repaint();
            _previewWindow?.Repaint();
            _thumbnailUpdateRequests.Add(_currentClip);
        }

        private void UpdateThumbnails()
        {
            foreach (var clip in _thumbnailUpdateRequests)
            {
                _mainThumbnailDrawer.RequestUpdate(clip);
                _gestureTableThumbnailDrawer.RequestUpdate(clip);
            }
            _thumbnailUpdateRequests.Clear();
        }

        private void OnChangedPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) Dispose();
        }
    }
}
