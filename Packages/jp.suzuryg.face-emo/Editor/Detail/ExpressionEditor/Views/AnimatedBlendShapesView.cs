using System;
using System.Collections.Generic;
using System.Linq;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class AnimatedBlendShapesView : IDisposable
    {
        public IObservable<(BlendShape blendShape, float value)> OnValueChanged => _onValueChanged.AsObservable();
        public IObservable<BlendShape> OnRemoved => _onRemoved.AsObservable();
        public IObservable<Unit> OnRepaintRequested => _onRepaintRequested.AsObservable();

        private readonly CompositeDisposable _disposable = new();
        private readonly Subject<(BlendShape blendShape, float value)> _onValueChanged = new();
        private readonly Subject<BlendShape> _onRemoved = new();
        private readonly Subject<Unit> _onRepaintRequested = new();

        private readonly IReadOnlyDictionary<BlendShape, float> _blinkBlendShapes;
        private readonly IReadOnlyDictionary<BlendShape, float> _lipSyncBlendShapes;
        private readonly IReadOnlyDictionary<BlendShape, float> _faceBlendShapes;
        private readonly IReadOnlyDictionary<BlendShape, float> _animatedBlendShapes;
        private readonly ExpressionEditorStyles _styles;

        private LocalizationTable _loc;
        private float? _labelWidth;
        private bool _blinkBlendShapeExists;
        private bool _lipSyncBlendShapeExists;
        private bool _excludedBlendShapeExists;

        public AnimatedBlendShapesView(IReadOnlyDictionary<BlendShape, float> blinkBlendShapes,
            IReadOnlyDictionary<BlendShape, float> lipSyncBlendShapes,
            IReadOnlyDictionary<BlendShape, float> faceBlendShapes,
            IReadOnlyDictionary<BlendShape, float> animatedBlendShapes,
            ExpressionEditorStyles styles, IReadOnlyLocalizationSetting localizationSetting)
        {
            _blinkBlendShapes = blinkBlendShapes;
            _lipSyncBlendShapes = lipSyncBlendShapes;
            _faceBlendShapes = faceBlendShapes;
            _animatedBlendShapes = animatedBlendShapes;

            _styles = styles;
            _loc = localizationSetting.Table;

            localizationSetting.OnTableChanged.Subscribe(table => _loc = table).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _onValueChanged.Dispose();
            _onRemoved.Dispose();
            _onRepaintRequested.Dispose();
        }

        public void RebuildGUI()
        {
            _blinkBlendShapeExists =
                _animatedBlendShapes.Any(blendShape => _blinkBlendShapes.ContainsKey(blendShape.Key));
            _lipSyncBlendShapeExists =
                _animatedBlendShapes.Any(blendShape => _lipSyncBlendShapes.ContainsKey(blendShape.Key));
            _excludedBlendShapeExists =
                _animatedBlendShapes.Any(blendShape => !_faceBlendShapes.ContainsKey(blendShape.Key));

            _labelWidth = null;
        }

        public void OnGUI(Vector2 scrollPosition, float contentHeight)
        {
            if (!_animatedBlendShapes.Any()) return;

            _labelWidth ??= _animatedBlendShapes
                .Select(blendShape => GUI.skin.label.CalcSize(new GUIContent(blendShape.Key.Name)).x).DefaultIfEmpty()
                .Max() + ExpressionEditorViewConstants.LabelPadding;

            // If no space is inserted here, the horizontal line of the topmost Slider will disappear.
            EditorGUILayout.Space();

            // Draw warnings
            if (_blinkBlendShapeExists)
                GUILayout.Label(_loc.ExpressionEditorView_Message_BlinkBlendShapeExists, _styles.WarningTextStyle);
            if (_lipSyncBlendShapeExists)
                GUILayout.Label(_loc.ExpressionEditorView_Message_LipSyncBlendShapeExists, _styles.WarningTextStyle);
            if (_excludedBlendShapeExists)
                GUILayout.Label(_loc.ExpressionEditorView_Message_ExcluededBlendShapeExists, _styles.WarningTextStyle);

            var existingLines = 1 + (_blinkBlendShapeExists ? 1 : 0) + (_lipSyncBlendShapeExists ? 1 : 0) +
                           (_excludedBlendShapeExists ? 1 : 0);

            // Calculate visible range
            var rowHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var total = _animatedBlendShapes.Count;
            var first = Mathf.Clamp(Mathf.FloorToInt(scrollPosition.y / rowHeight), 0, total);
            first = Math.Max(0, first - existingLines);
            var visibleCount = Mathf.CeilToInt(contentHeight / rowHeight) + 1;
            var last = Mathf.Clamp(first + visibleCount, 0, total);

            GUILayout.Space(first * rowHeight);

            // Draw controls
            var count = -1;
            foreach (var kvp in _animatedBlendShapes)
            {
                count++;
                if (count < first || count >= last) continue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    const float minValue = 0;
                    const float maxValue = 100;

                    // Label
                    var labelContent = new GUIContent(kvp.Key.Name);
                    var labelRect =
                        GUILayoutUtility.GetRect(labelContent, GUI.skin.label, GUILayout.Width(_labelWidth.Value));
                    var warned = _blinkBlendShapes.ContainsKey(kvp.Key) ||
                                 _lipSyncBlendShapes.ContainsKey(kvp.Key) ||
                                 !_faceBlendShapes.ContainsKey(kvp.Key);
                    GUI.Label(labelRect, labelContent, warned ? _styles.WarnedPropertyStyle : _styles.NormalPropertyStyle);

                    // Slider
                    const float increment = 0.1f;
                    var sliderRect = GUILayoutUtility.GetRect(ExpressionEditorViewConstants.MinSliderWidth,
                        EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true),
                        GUILayout.MinWidth(ExpressionEditorViewConstants.MinSliderWidth));
                    var sliderValue = GUI.HorizontalSlider(sliderRect, kvp.Value, minValue, maxValue);

                    if (EditorPrefsStore.ExpressionEditorSettings.UseMouseWheel)
                    {
                        var scrollWheelSensitivity = (maxValue - minValue) / 100;
                        if (sliderRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
                        {
                            sliderValue -= Event.current.delta.y * scrollWheelSensitivity;
                            sliderValue = Mathf.Clamp(sliderValue, minValue, maxValue);
                            _onRepaintRequested.OnNext(Unit.Default);
                        }
                    }
                    sliderValue = Mathf.Round(sliderValue / increment) * increment;
                    if (!Mathf.Approximately(sliderValue, kvp.Value))
                    {
                        _onValueChanged.OnNext((kvp.Key, sliderValue));
                        break;
                    }

                    // FloatField
                    var fieldValue = EditorGUILayout.FloatField(kvp.Value,
                        GUILayout.Width(ExpressionEditorViewConstants.FloatFieldWidth));
                    fieldValue = Math.Max(Math.Min(fieldValue, maxValue), minValue);
                    if (!Mathf.Approximately(fieldValue, kvp.Value))
                    {
                        _onValueChanged.OnNext((kvp.Key, fieldValue));
                        break;
                    }

                    // Remove button
                    if (GUILayout.Button("x", _styles.RemoveButtonStyle,
                            GUILayout.Width(ExpressionEditorViewConstants.RemoveButtonWidth)))
                    {
                        _onRemoved.OnNext(kvp.Key);
                        break;
                    }
                }
            }
            GUILayout.Space((total - last) * rowHeight);
        }
    }
}
