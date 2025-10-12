using System;
using System.Collections.Generic;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class PropertyEditorViewFacade : IDisposable
    {
        public IObservable<AnimationClip> OnOpenClipRequested => _leftHeaderView.OnOpenClipRequested;

        public IObservable<(BlendShape blendShape, float value)> OnBlendShapeValueChanged =>
            _animatedBlendShapesView.OnValueChanged;
        public IObservable<BlendShape> OnBlendShapeAdded => _faceBlendShapesView.OnAdded;
        public IObservable<BlendShape> OnBlendShapeRemoved => _animatedBlendShapesView.OnRemoved;
        public IObservable<BlendShape> OnHoveredBlendShapeChanged => _faceBlendShapesView.OnHoveredBlendShapeChanged;
        public IObservable<Unit> OnAddAllBlendShapesRequested => _rightHeaderView.OnAddAllBlendShapesRequested;

        public IObservable<(int id, GameObject target, bool value)> OnToggleValueChanged =>
            _animatedTogglesView.OnValueChanged;
        public IObservable<(int id, GameObject target, bool value)> OnToggleAdded => _togglesView.OnAdded;
        public IObservable<(int id, GameObject target)> OnToggleRemoved => _animatedTogglesView.OnRemoved;

        public IObservable<(int id, TransformProxy value)> OnTransformValueChanged =>
            _animatedTransformsView.OnValueChanged;
        public IObservable<(int id, TransformProxy value)> OnTransformAdded => _transformsView.OnAdded;
        public IObservable<(int id, TransformProxy value)> OnTransformRemoved => _animatedTransformsView.OnRemoved;

        public bool IsRepaintRequested { get; set; }

        private readonly ContentSizeCalculator _contentSizeCalculator;

        private readonly LeftHeaderView _leftHeaderView;
        private readonly AnimatedBlendShapesView _animatedBlendShapesView;
        private readonly AnimatedTogglesView _animatedTogglesView;
        private readonly AnimatedTransformsView _animatedTransformsView;
        private readonly HintView _hintView;

        private readonly RightHeaderView _rightHeaderView;
        private readonly FaceBlendShapesView _faceBlendShapesView;
        private readonly TogglesView _togglesView;
        private readonly TransformsView _transformsView;

        private readonly SerializedObject _expressionEditorSetting;
        private readonly ExpressionEditorStyles _styles = new();

        private readonly CompositeDisposable _disposables = new();

        private Vector2 _leftScrollPosition;
        private Vector2 _rightScrollPosition;

        public PropertyEditorViewFacade(IReadOnlyDictionary<BlendShape, float> blinkBlendShapes,
            IReadOnlyDictionary<BlendShape, float> lipSyncBlendShapes,
            IReadOnlyDictionary<BlendShape, float> faceBlendShapes,
            IReadOnlyDictionary<int, (GameObject target, bool value)> toggles,
            IReadOnlyDictionary<int, TransformProxy> transforms,
            IReadOnlyDictionary<BlendShape, float> animatedBlendShapes,
            IReadOnlyDictionary<int, (GameObject target, bool value)> animatedToggles,
            IReadOnlyDictionary<int, TransformProxy> animatedTransforms,
            ExpressionEditorSetting expressionEditorSetting, IReadOnlyLocalizationSetting loc)
        {
            _expressionEditorSetting = new SerializedObject(expressionEditorSetting);

            _contentSizeCalculator = new ContentSizeCalculator(faceBlendShapes, toggles,
                transforms, animatedBlendShapes, loc).AddTo(_disposables);

            _leftHeaderView = new LeftHeaderView(loc).AddTo(_disposables);
            _animatedBlendShapesView = new AnimatedBlendShapesView(blinkBlendShapes, lipSyncBlendShapes, faceBlendShapes,
                animatedBlendShapes, _styles, loc).AddTo(_disposables);
            _animatedTogglesView = new AnimatedTogglesView(animatedToggles, _styles).AddTo(_disposables);
            _animatedTransformsView = new AnimatedTransformsView(animatedTransforms, _styles)
                .AddTo(_disposables);
            _hintView = new HintView(blinkBlendShapes, lipSyncBlendShapes, animatedBlendShapes, loc)
                .AddTo(_disposables);

            _rightHeaderView = new RightHeaderView(_expressionEditorSetting, _styles, loc).AddTo(_disposables);
            _faceBlendShapesView = new FaceBlendShapesView(blinkBlendShapes, lipSyncBlendShapes, faceBlendShapes,
                animatedBlendShapes, _expressionEditorSetting, _styles, loc).AddTo(_disposables);
            _togglesView =
                new TogglesView(toggles, animatedToggles, _styles, loc).AddTo(_disposables);
            _transformsView =
                new TransformsView(transforms, animatedTransforms, _styles, loc)
                    .AddTo(_disposables);

            _animatedBlendShapesView.OnRepaintRequested.Subscribe(_ => IsRepaintRequested = true).AddTo(_disposables);
            _rightHeaderView.OnDelimiterChanged.Subscribe(_ => _faceBlendShapesView.RebuildGUI()).AddTo(_disposables);
            _rightHeaderView.OnSearchChanged.Subscribe(_faceBlendShapesView.SetSearchKeyword).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void OpenTargetClip(AnimationClip clip)
        {
            _leftHeaderView.OpenTargetClip(clip);
        }

        public void RebuildAllViews()
        {
            _faceBlendShapesView.RebuildGUI();
            RebuildAnimatedPropertyViews();
        }

        public void RebuildAnimatedPropertyViews()
        {
            _contentSizeCalculator.RebuildGUI();
            _animatedBlendShapesView.RebuildGUI();
            _animatedTogglesView.RebuildGUI();
        }

        public Vector2 OnGUI(float windowWidth, float windowHeight)
        {
            _expressionEditorSetting.Update();

            _contentSizeCalculator.OnGUI(windowWidth, windowHeight);

            using (new EditorGUILayout.HorizontalScope())
            {
                // Left pane
                using (new EditorGUILayout.VerticalScope(
                           GUILayout.Width(_contentSizeCalculator.LeftPaneWidth),
                           GUILayout.Height(_contentSizeCalculator.ViewportHeight)))
                {
                    _leftHeaderView.OnGUI();

                    using (var scope = new EditorGUILayout.ScrollViewScope(_leftScrollPosition))
                    {
                        _animatedBlendShapesView.OnGUI(scope.scrollPosition, _contentSizeCalculator.ViewportHeight);
                        _animatedTogglesView.OnGUI();
                        _animatedTransformsView.OnGUI();
                        _hintView.OnGUI();
                        _leftScrollPosition = scope.scrollPosition;
                    }
                }

                EditorGUILayout.Space(ExpressionEditorViewConstants.LeftRightMargin);

                // Right pane
                using (new EditorGUILayout.VerticalScope(
                           GUILayout.Width(_contentSizeCalculator.RightPaneWidth),
                           GUILayout.Height(_contentSizeCalculator.ViewportHeight)))
                {
                    _rightHeaderView.OnGUI(_contentSizeCalculator.RightPaneWidth);

                    using (var scope = new EditorGUILayout.ScrollViewScope(_rightScrollPosition))
                    {
                        _faceBlendShapesView.OnGUI();
                        _togglesView.OnGUI();
                        _transformsView.OnGUI();
                        _rightScrollPosition = scope.scrollPosition;
                    }
                }
            }
            _expressionEditorSetting.ApplyModifiedProperties();

            return _contentSizeCalculator.MinWindowSize;
        }
    }
}
