using System;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Detail.View;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class RightHeaderView : IDisposable
    {
        public IObservable<Unit> OnAddAllBlendShapesRequested => _onAddAllBlendShapesRequested.AsObservable();
        public IObservable<Unit> OnDelimiterChanged => _onDelimiterChanged.AsObservable();
        public IObservable<string> OnSearchChanged => _onSearchChanged.AsObservable();

        private readonly CompositeDisposable _disposable = new();
        private readonly Subject<Unit> _onAddAllBlendShapesRequested = new();
        private readonly Subject<Unit> _onDelimiterChanged = new();
        private readonly Subject<string> _onSearchChanged = new();

        private readonly SerializedObject _expressionEditorSetting;
        private readonly ExpressionEditorStyles _styles;

        [CanBeNull] private string _search;
        private LocalizationTable _loc;

        public RightHeaderView(SerializedObject expressionEditorSetting, ExpressionEditorStyles styles,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _expressionEditorSetting = expressionEditorSetting;
            _styles = styles;
            _loc = localizationSetting.Table;

            localizationSetting.OnTableChanged.Subscribe(table => _loc = table).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _onAddAllBlendShapesRequested.Dispose();
            _onDelimiterChanged.Dispose();
            _onSearchChanged.Dispose();
        }

        public void OnGUI(float contentWidth)
        {
            Field_AddAllBlendShapes();
            Field_FaceBlendShapeDelimiter(contentWidth);
            Field_FaceBlendShapeSearch(contentWidth);
            Field_ReflectInPreviewOnMouseOver();
        }

        private void Field_AddAllBlendShapes()
        {
            if (!GUILayout.Button(new GUIContent(_loc.ExpressionEditorView_AddAllBlendShapes,
                    _loc.ExpressionEditorView_Tooltip_AddAllBlendShapes))) return;

            // Delay the execution of the following code because ShowModalUtility() interferes with LayoutGroup processing.
            var centerPosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            EditorApplication.delayCall += () =>
            {
                if (OptoutableDialog.Show(DomainConstants.SystemName, _loc.ExpressionEditorView_Message_AddAllBlendShapes,
                        _loc.Common_Add, _loc.Common_Cancel,
                        centerPosition: centerPosition))
                {
                    EditorPrefsStore.ExpressionEditorSettings.ShowOnlyDifferFromDefaultValue = false;
                    _onAddAllBlendShapesRequested.OnNext(Unit.Default);
                }
            };
        }

        private void Field_FaceBlendShapeDelimiter(float contentWidth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var label = new GUIContent(_loc.ExpressionEditorView_Delimiter,
                    _loc.ExpressionEditorView_Tooltip_Delimiter);
                var labelWidth = GUI.skin.label.CalcSize(label).x;
                EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));

                var property =
                    _expressionEditorSetting.FindProperty(nameof(ExpressionEditorSetting.FaceBlendShapeDelimiter));
                var newValue =
                    EditorGUILayout.TextField(property.stringValue, GUILayout.Width(contentWidth - labelWidth));

                if (property.stringValue == newValue) return;

                property.stringValue = newValue;
                _expressionEditorSetting.ApplyModifiedProperties();
                _onDelimiterChanged.OnNext(Unit.Default);
            }
        }

        private void Field_FaceBlendShapeSearch(float contentWidth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var label = new GUIContent(_loc.ExpressionEditorView_Search, _loc.ExpressionEditorView_Tooltip_Search);
                var labelWidth = GUI.skin.label.CalcSize(label).x;
                var isHighlighted = !string.IsNullOrEmpty(_search);
                EditorGUILayout.LabelField(label,
                    isHighlighted ? _styles.HighlightedLabelStyle : _styles.NormalLabelStyle,
                    GUILayout.Width(labelWidth));
                var input = EditorGUILayout.TextField(_search, GUILayout.Width(contentWidth - labelWidth));

                if (_search == input) return;

                _search = input;
                _onSearchChanged.OnNext(_search);
            }
        }

        private void Field_ReflectInPreviewOnMouseOver()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var current = EditorPrefsStore.ExpressionEditorSettings.ReflectInPreviewOnMouseOver;
                if (EditorGUILayout.Toggle(current, GUILayout.Width(ExpressionEditorViewConstants.ToggleWidth)) != current)
                {
                    EditorPrefsStore.ExpressionEditorSettings.ReflectInPreviewOnMouseOver = !current;
                }
                GUILayout.Label(_loc.ExpressionEditorView_ReflectInPreviewOnMouseOver);
            }
        }
    }
}
