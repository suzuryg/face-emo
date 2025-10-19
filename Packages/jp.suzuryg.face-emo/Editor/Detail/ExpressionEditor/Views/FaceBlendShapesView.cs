using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class FaceBlendShapesView : IDisposable
    {
        public IObservable<BlendShape> OnAdded => _onAdded.AsObservable();
        public IObservable<BlendShape> OnHoveredBlendShapeChanged => _onHoveredBlendShapeChanged.AsObservable();

        private readonly CompositeDisposable _disposable = new();
        private readonly Subject<BlendShape> _onAdded = new();
        private readonly Subject<BlendShape> _onHoveredBlendShapeChanged = new();

        private readonly IReadOnlyDictionary<BlendShape, float> _blinkBlendShapes;
        private readonly IReadOnlyDictionary<BlendShape, float> _lipSyncBlendShapes;
        private readonly IReadOnlyDictionary<BlendShape, float> _faceBlendShapes;
        private readonly IReadOnlyDictionary<BlendShape, float> _animatedBlendShapes;
        private readonly Dictionary<string, List<BlendShape>> _categorizedFaceBlendShapes = new();
        private readonly Dictionary<string, bool> _foldoutStates = new();
        private readonly SerializedObject _expressionEditorSetting;
        private readonly ExpressionEditorStyles _styles;

        [CanBeNull] private string _search;
        [CanBeNull] private BlendShape _previousHoveredBlendShape;
        private LocalizationTable _loc;

        public FaceBlendShapesView(IReadOnlyDictionary<BlendShape, float> blinkBlendShapes,
            IReadOnlyDictionary<BlendShape, float> lipSyncBlendShapes,
            IReadOnlyDictionary<BlendShape, float> faceBlendShapes,
            IReadOnlyDictionary<BlendShape, float> animatedBlendShapes,
            SerializedObject expressionEditorSetting, ExpressionEditorStyles styles,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _blinkBlendShapes = blinkBlendShapes;
            _lipSyncBlendShapes = lipSyncBlendShapes;
            _faceBlendShapes = faceBlendShapes;
            _animatedBlendShapes = animatedBlendShapes;

            _expressionEditorSetting = expressionEditorSetting;
            _styles = styles;
            _loc = localizationSetting.Table;
            localizationSetting.OnTableChanged.Subscribe(table => _loc = table).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _onAdded.Dispose();
            _onHoveredBlendShapeChanged.Dispose();
        }

        public void SetSearchKeyword(string search)
        {
            _search = search;
            RebuildGUI();
        }

        public void RebuildGUI()
        {
            // Categorize
            _categorizedFaceBlendShapes.Clear();
            var delimiter = _expressionEditorSetting
                .FindProperty(nameof(ExpressionEditorSetting.FaceBlendShapeDelimiter)).stringValue;
            var categoryName = _loc.ExpressionEditorView_UncategorizedBlendShapes;
            _categorizedFaceBlendShapes[categoryName] = new List<BlendShape>();
            var primaryMeshPath = _faceBlendShapes.Keys.FirstOrDefault()?.Path;
            foreach (var blendShape in _faceBlendShapes.Keys)
            {
                // face mesh
                if (blendShape.Path == primaryMeshPath)
                {
                    if (!string.IsNullOrEmpty(delimiter) && blendShape.Name.Contains(delimiter))
                    {
                        if (!_categorizedFaceBlendShapes[categoryName].Any())
                            _categorizedFaceBlendShapes.Remove(categoryName);

                        categoryName = string.IsNullOrEmpty(delimiter)
                            ? blendShape.Name
                            : blendShape.Name.Replace(delimiter, string.Empty);
                        _categorizedFaceBlendShapes[categoryName] = new List<BlendShape>();
                    }
                    else
                        _categorizedFaceBlendShapes[categoryName].Add(blendShape);
                }
                // additional mesh
                else
                {
                    if (_categorizedFaceBlendShapes.TryGetValue(blendShape.Path, out var existing))
                        existing.Add(blendShape);
                    else
                        _categorizedFaceBlendShapes[blendShape.Path] = new List<BlendShape> { blendShape };
                }
            }

            // Apply search
            // this allows fast path (in MatchName) due to being able to compare references
            if (!string.IsNullOrEmpty(_search))
            {
                foreach (var kvp in _categorizedFaceBlendShapes)
                {
                    kvp.Value.RemoveAll(a => a.MatchName(_search) <= 0);
                    kvp.Value.Sort((a, b) =>
                    {
                        var compareTo = b.MatchName(_search).CompareTo(a.MatchName(_search));
                        return compareTo == 0 ? string.CompareOrdinal(a.Name, b.Name) : compareTo;
                    });
                }
            }

            // Immediately after opening ExpressionEditor and if it has not been categorized, open the first category.
            if (!_foldoutStates.Any() && _categorizedFaceBlendShapes.Count == 1)
                _foldoutStates[_categorizedFaceBlendShapes.First().Key] = true;

            // Update foldout states
            foreach (var key in _categorizedFaceBlendShapes.Keys)
                _foldoutStates.TryAdd(key, false);
        }

        public void OnGUI()
        {
            // Draw buttons
            BlendShape hoveredBlendShape = null;
            foreach (var category in _categorizedFaceBlendShapes)
            {
                var filtered = category.Value.Where(blendShapeKey =>
                    (EditorPrefsStore.ExpressionEditorSettings.ShowBlinkBlendShapes ||
                     !_blinkBlendShapes.ContainsKey(blendShapeKey)) &&
                    (EditorPrefsStore.ExpressionEditorSettings.ShowLipSyncBlendShapes ||
                     !_lipSyncBlendShapes.ContainsKey(blendShapeKey))).ToArray();
                if (!filtered.Any()) continue;

                _foldoutStates[category.Key] =
                    EditorGUILayout.Foldout(_foldoutStates[category.Key], category.Key);
                if (!_foldoutStates[category.Key]) continue;

                // If there are any blend shapes in the category, display them.
                foreach (var blendShapeKey in filtered)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(ExpressionEditorViewConstants.IndentWidth);

                        if (_animatedBlendShapes.ContainsKey(blendShapeKey))
                        {
                            GUILayout.Label(blendShapeKey.Name, _styles.AddPropertyLabelStyle);
                        }
                        else
                        {
                            var buttonRect = GUILayoutUtility.GetRect(new GUIContent(blendShapeKey.Name),
                                _styles.AddPropertyButtonStyle);
                            var buttonStyle = _styles.AddPropertyButtonStyle;

                            if (EditorPrefsStore.ExpressionEditorSettings.ReflectInPreviewOnMouseOver &&
                                buttonRect.Contains(Event.current.mousePosition))
                            {
                                buttonStyle = _styles.AddPropertyButtonMouseOverStyle;
                                hoveredBlendShape = blendShapeKey;
                            }

                            if (GUI.Button(buttonRect, blendShapeKey.Name, buttonStyle))
                            {
                                _onAdded.OnNext(blendShapeKey);
                            }
                        }
                    }
                }
            }

            // Preview mouse over blend shape
            // Since it is always judged not to be mouse-over at the Layout event, the preview process is called only at the Repaint event.
            if (Event.current.type != EventType.Repaint || hoveredBlendShape == _previousHoveredBlendShape) return;
            _onHoveredBlendShapeChanged.OnNext(hoveredBlendShape);
            _previousHoveredBlendShape = hoveredBlendShape;
        }
    }
}
