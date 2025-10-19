using System;
using System.Collections.Generic;
using System.Linq;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class ContentSizeCalculator : IDisposable
    {
        public float ViewportHeight { get; private set; }
        public float LeftPaneWidth { get; private set; }
        public float RightPaneWidth { get; private set; }
        public Vector2 MinWindowSize { get; private set; }

        private readonly CompositeDisposable _disposable = new();

        private readonly IReadOnlyDictionary<BlendShape, float> _faceBlendShapes;
        private readonly IReadOnlyDictionary<int, (GameObject target, bool value)> _additionalToggles;
        private readonly IReadOnlyDictionary<int, TransformProxy> _additionalTransforms;
        private readonly IReadOnlyDictionary<BlendShape, float> _animatedBlendShapes;

        private LocalizationTable _loc;
        private IEnumerable<string> _propertyNames = Array.Empty<string>();
        private IEnumerable<string> _animatedPropertyNames = Array.Empty<string>();
        private float? _labelMaxWidth;
        private float? _buttonMaxWidth;
        private float? _minLeftPaneWidth;

        public ContentSizeCalculator(IReadOnlyDictionary<BlendShape, float> faceBlendShapes,
            IReadOnlyDictionary<int, (GameObject target, bool value)> additionalToggles,
            IReadOnlyDictionary<int, TransformProxy> additionalTransforms,
            IReadOnlyDictionary<BlendShape, float> animatedBlendShapes,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _faceBlendShapes = faceBlendShapes;
            _additionalToggles = additionalToggles;
            _additionalTransforms = additionalTransforms;
            _animatedBlendShapes = animatedBlendShapes;
            _loc = localizationSetting.Table;

            localizationSetting.OnTableChanged.Subscribe(table => _loc = table).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        public void RebuildGUI()
        {
            _propertyNames = _faceBlendShapes.Select(blendShape => blendShape.Key.Name)
                .Concat(_additionalToggles.Select(toggle => toggle.Value.target.name))
                .Concat(_additionalTransforms.Select(transform => transform.Value?.GameObject?.name))
                .Concat(new[] {
                    _loc.ExpressionEditorView_UncategorizedBlendShapes,
                    _loc.ExpressionEditorView_AddtionalToggleObjects,
                    _loc.ExpressionEditorView_AddtionalTransformObjects});

            _animatedPropertyNames = _animatedBlendShapes.Select(blendShape => blendShape.Key.Name);

            _labelMaxWidth = null;
            _buttonMaxWidth = null;
            _minLeftPaneWidth = null;
        }

        public void OnGUI(float windowWidth, float windowHeight)
        {
            // Calculate property name width
            _labelMaxWidth ??= _propertyNames.Select(x => GUI.skin.label.CalcSize(new GUIContent(x)).x).DefaultIfEmpty()
                .Max();
            _buttonMaxWidth ??= _propertyNames.Select(x => GUI.skin.button.CalcSize(new GUIContent(x)).x).DefaultIfEmpty()
                .Max();
            var maxBlendShapeNameWidth = Math.Max(_labelMaxWidth.Value, _buttonMaxWidth.Value);

            var padding = EditorStyles.inspectorDefaultMargins.padding;
            if (!_minLeftPaneWidth.HasValue)
            {
                var leftOptionWidth =
                    GUI.skin.label.CalcSize(new GUIContent(_loc.ExpressionEditorView_ShowOnlyDifferFromDefaultValue)).x +
                    ExpressionEditorViewConstants.ToggleWidth;
                var animatedPropertyMaxWidth = _animatedPropertyNames
                    .Select(x => GUI.skin.label.CalcSize(new GUIContent(x)).x).DefaultIfEmpty().Max();
                var leftPropertyWidth = animatedPropertyMaxWidth + ExpressionEditorViewConstants.LabelPadding +
                                        ExpressionEditorViewConstants.MinSliderWidth +
                                        ExpressionEditorViewConstants.FloatFieldWidth +
                                        ExpressionEditorViewConstants.RemoveButtonWidth +
                                        30; // Extra margin
                _minLeftPaneWidth = Math.Max(leftOptionWidth, leftPropertyWidth);
            }

            // Calculate content size
            var verticalScrollbarWidth = GUI.skin.verticalScrollbar.fixedWidth;
            ViewportHeight = windowHeight - padding.top - padding.bottom;

            RightPaneWidth = Math.Min(
                (windowWidth - padding.left - padding.right) / 2f -  ExpressionEditorViewConstants.LeftRightMargin,
                maxBlendShapeNameWidth + ExpressionEditorViewConstants.IndentWidth + padding.left + padding.right +
                verticalScrollbarWidth);
            LeftPaneWidth = windowWidth - padding.left - padding.right - RightPaneWidth -
                               ExpressionEditorViewConstants.LeftRightMargin;

            // Set minimum window size
            var minWindowWidth = padding.left + _minLeftPaneWidth.Value +
                                 ExpressionEditorViewConstants.LeftRightMargin + RightPaneWidth + padding.right;
            const float minWindowHeight = 300;
            if (!Mathf.Approximately(MinWindowSize.x, minWindowWidth))
                MinWindowSize = new Vector2(minWindowWidth, minWindowHeight);
        }
    }
}
