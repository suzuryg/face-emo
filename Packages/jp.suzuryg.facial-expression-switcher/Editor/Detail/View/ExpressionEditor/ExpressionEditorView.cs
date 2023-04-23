using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View.ExpressionEditor
{
    public class ExpressionEditorView : IDisposable
    {
        private static readonly float IndentWidth = 20;

        private ISubWindowProvider _subWindowProvider;
        private ILocalizationSetting _localizationSetting;
        private LocalizationTable _localizationTable;
        private AV3.ExpressionEditor _expressionEditor;
        private SerializedObject _expressionEditorSetting;

        private IMGUIContainer _rootContainer;

        private Dictionary<string, bool> _faceBlendShapeFoldoutStates = new Dictionary<string, bool>();

        private Vector2 _leftScrollPosition = Vector2.zero;
        private Vector2 _rightScrollPosition = Vector2.zero;
        private GUIStyle _removeButtonStyle = new GUIStyle();
        private GUIStyle _addPropertyButtonStyle = new GUIStyle();

        private CompositeDisposable _disposables = new CompositeDisposable();

        public ExpressionEditorView(
            ISubWindowProvider subWindowProvider,
            ILocalizationSetting localizationSetting,
            AV3.ExpressionEditor expressionEditor,
            ExpressionEditorSetting expressionEditorSetting)
        {
            // Dependencies
            _subWindowProvider = subWindowProvider;
            _localizationSetting = localizationSetting;
            _expressionEditor = expressionEditor;
            _expressionEditorSetting = new SerializedObject(expressionEditorSetting);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Initialize(VisualElement root)
        {
            // Load UXML and style
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/ExpressionEditor/{nameof(ExpressionEditorView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/ExpressionEditor/{nameof(ExpressionEditorView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            // Query Elements
            _rootContainer = root.Q<IMGUIContainer>("RootContainer");
            NullChecker.Check(_rootContainer);

            // Add event handlers
            Observable.FromEvent(x => _rootContainer.onGUIHandler += x, x => _rootContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => OnGUI()).AddTo(_disposables);

            // Styles
            SetStyle();

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetStyle()
        {
            // Remove button
            _removeButtonStyle = new GUIStyle(GUI.skin.button);
            //_removeButtonStyle.fontStyle = FontStyle.Bold;
            //_removeButtonStyle.normal.background = MakeTexture(Color.red);
            //_removeButtonStyle.normal.textColor = Color.white;
            //_removeButtonStyle.hover.background = MakeTexture(new Color(1, 0.7f, 0.7f));
            //_removeButtonStyle.hover.textColor = Color.white;
            //_removeButtonStyle.active.background = MakeTexture(new Color(0.8f, 0, 0));
            //_removeButtonStyle.active.textColor = Color.white;

            // Add property button
            _addPropertyButtonStyle = new GUIStyle(GUI.skin.button);
            _addPropertyButtonStyle.alignment = TextAnchor.MiddleLeft;
        }

        /// <summary>
        /// Helper function to create a solid color texture
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private Texture2D MakeTexture(Color col)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, col);
            texture.Apply();
            return texture;
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        private void OnGUI()
        {
            _expressionEditorSetting.Update();

            // Calculate blend shape name width
            var labelWidth = _expressionEditor.FaceBlendShapes.Select(blendShape => GUI.skin.label.CalcSize(new GUIContent(blendShape.Key)).x).DefaultIfEmpty().Max();
            var buttonWidth = _expressionEditor.FaceBlendShapes.Select(blendShape => GUI.skin.button.CalcSize(new GUIContent(blendShape.Key)).x).DefaultIfEmpty().Max();
            var maxBlendShapeNameWidth = Math.Max(labelWidth, buttonWidth);

            // When contentRect is obtained from IMGUIContainer,
            // the height of Rect becomes a wrong value when multiple ScrollViewScope are lined up,
            // so acquire the size of EditorWindow.
            var window = _subWindowProvider.Provide<ExpressionEditorWindow>();
            const float leftRightMargin = 10;
            var verticalScrollbarWidth = GUI.skin.verticalScrollbar.fixedWidth;
            var padding = EditorStyles.inspectorDefaultMargins.padding;
            var contentHeight = window.position.height - padding.top - padding.bottom;
            var rightContentWidth = Math.Min(
                (window.position.width - padding.left - padding.right) / 2 - leftRightMargin,
                maxBlendShapeNameWidth + IndentWidth + padding.left + padding.right + verticalScrollbarWidth);
            var leftContentWidth = window.position.width - padding.left - padding.right - rightContentWidth - leftRightMargin;

            // Update property values when finished moving the Slider.
            if (Event.current.type == EventType.MouseUp)
            {
                _expressionEditor.CheckBuffer();
            }

            // Root scope
            using (new EditorGUILayout.HorizontalScope())
            {
                // Left scope
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(leftContentWidth), GUILayout.Height(contentHeight)))
                {
                    Field_AnimationClip();
                    Field_ShowOnlyDifferFromDefaultValue();

                    using (var scope = new EditorGUILayout.ScrollViewScope(_leftScrollPosition))
                    {
                        Field_AnimatedBlendShapes();
                        _leftScrollPosition = scope.scrollPosition;
                    }
                }

                EditorGUILayout.Space(leftRightMargin);

                // Right scope
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(rightContentWidth), GUILayout.Height(contentHeight)))
                {
                    Field_FaceBlendShapeDelimiter();
                    using (var scope = new EditorGUILayout.ScrollViewScope(_rightScrollPosition))
                    {
                        Field_FaceBlendShapes();
                        _rightScrollPosition = scope.scrollPosition;
                    }
                }
            }
        }

        private void Field_AnimationClip()
        {
            var ret = EditorGUILayout.ObjectField(_expressionEditor.Clip, typeof(AnimationClip), allowSceneObjects: false);
            if (ret is AnimationClip animationClip && !ReferenceEquals(animationClip, _expressionEditor.Clip))
            {
                _expressionEditor.Open(animationClip);
            }
        }

        private void Field_ShowOnlyDifferFromDefaultValue()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(_expressionEditorSetting.FindProperty(nameof(ExpressionEditorSetting.ShowOnlyDifferFromDefaultValue)),
                    new GUIContent("Show Only Differ From Default Value"));

                if (check.changed)
                {
                    _expressionEditorSetting.ApplyModifiedProperties();
                    _expressionEditor.FetchBlendShapeValues();
                }
            }
        }

        private void Field_FaceBlendShapeDelimiter()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.DelayedTextField(_expressionEditorSetting.FindProperty(nameof(ExpressionEditorSetting.FaceBlendShapeDelimiter)),
                    new GUIContent("Delimiter"));

                if (check.changed)
                {
                    _expressionEditorSetting.ApplyModifiedProperties();
                    _expressionEditor.FetchBlendShapeValues();
                }
            }
        }

        private void Field_AnimatedBlendShapes()
        {
            var fieldInputPerformed = false;
            var changed = new Dictionary<string, float>();
            var removed = new List<string>();

            var labelWidth = _expressionEditor.AnimatedBlendShapesBuffer
                .Select(blendShape => GUI.skin.label.CalcSize(new GUIContent(blendShape.Key)).x)
                .DefaultIfEmpty()
                .Max();
            labelWidth += 10;

            // If no space is inserted here, the horizontal line of the topmost Slider will disappear.
            EditorGUILayout.Space();

            // Draw controls
            foreach (var blendShape in _expressionEditor.AnimatedBlendShapesBuffer)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Label
                    GUIContent labelContent = new GUIContent(blendShape.Key);
                    Rect labelRect = GUILayoutUtility.GetRect(labelContent, GUI.skin.label, GUILayout.Width(labelWidth));
                    GUI.Label(labelRect, labelContent);

                    // Slider
                    const float increment = 0.1f;
                    Rect sliderRect = GUILayoutUtility.GetRect(labelRect.x, labelRect.y, GUILayout.ExpandWidth(true), GUILayout.MinWidth(100));
                    var sliderValue = GUI.HorizontalSlider(sliderRect, blendShape.Value, 0, 100);
                    sliderValue = Mathf.Round(sliderValue / increment) * increment;
                    if (!Mathf.Approximately(sliderValue, blendShape.Value))
                    {
                        changed[blendShape.Key] = sliderValue;
                    }

                    // DelayedFloatField
                    var fieldValue = EditorGUILayout.DelayedFloatField(blendShape.Value, GUILayout.Width(40));
                    if (!Mathf.Approximately(fieldValue, blendShape.Value))
                    {
                        changed[blendShape.Key] = fieldValue;
                        fieldInputPerformed = true;
                    }

                    // Remove button
                    if (GUILayout.Button("x", _removeButtonStyle, GUILayout.Width(20)))
                    {
                        removed.Add(blendShape.Key);
                    }
                }
            }

            // Set buffer
            foreach (var blendShape in changed)
            {
                _expressionEditor.SetBuffer(blendShape.Key, blendShape.Value);
            }
            foreach (var key in removed)
            {
                _expressionEditor.RemoveBuffer(key);
            }

            // Check buffer
            if (fieldInputPerformed || removed.Any())
            {
                _expressionEditor.CheckBuffer();
            }
        }

        private void Field_FaceBlendShapes()
        {
            const string text_uncategorized = "Uncategorized BlendShapes";

            // Categorize
            var delimiter = _expressionEditorSetting.FindProperty(nameof(ExpressionEditorSetting.FaceBlendShapeDelimiter)).stringValue;
            var categorized = new Dictionary<string, List<string>>();
            var categoryName = text_uncategorized;
            categorized[categoryName] = new List<string>();
            foreach (var key in _expressionEditor.FaceBlendShapes.Keys)
            {
                if (!string.IsNullOrEmpty(delimiter) && key.Contains(delimiter))
                {
                    categoryName = string.IsNullOrEmpty(delimiter) ? key : key.Replace(delimiter, string.Empty);
                    categorized[categoryName] = new List<string>();
                }
                else
                {
                    categorized[categoryName].Add(key);
                }
            }

            // Update foldout states
            var previousStates = new Dictionary<string, bool>(_faceBlendShapeFoldoutStates);
            _faceBlendShapeFoldoutStates.Clear();
            foreach (var key in categorized.Keys)
            {
                if (previousStates.ContainsKey(key)) { _faceBlendShapeFoldoutStates[key] = previousStates[key]; }
                else { _faceBlendShapeFoldoutStates[key] = false; }
            }

            // Draw buttons
            var added = new List<string>();
            foreach (var category in categorized)
            {
                _faceBlendShapeFoldoutStates[category.Key] = EditorGUILayout.Foldout(_faceBlendShapeFoldoutStates[category.Key], category.Key);
                if (_faceBlendShapeFoldoutStates[category.Key])
                {
                    foreach (var blendShapeKey in category.Value)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(IndentWidth);

                            if (_expressionEditor.AnimatedBlendShapesBuffer.ContainsKey(blendShapeKey))
                            {
                                GUILayout.Label(blendShapeKey);
                            }
                            else
                            {
                                if (GUILayout.Button(blendShapeKey, _addPropertyButtonStyle))
                                {
                                    added.Add(blendShapeKey);
                                }
                            }
                        }
                    }
                }
            }

            // Set buffer
            foreach (var key in added)
            {
                _expressionEditor.SetBuffer(key, 100);
            }

            // Check buffer
            if (added.Any())
            {
                _expressionEditor.CheckBuffer();
            }
        }
    }
}
