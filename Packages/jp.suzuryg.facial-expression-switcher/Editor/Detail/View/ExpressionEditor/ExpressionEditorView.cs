﻿using Suzuryg.FacialExpressionSwitcher.Domain;
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
        private bool _toggleFoldoutState = false;
        private bool _transformFoldoutState = false;

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

            // Calculate property name width
            var propertyNames = _expressionEditor.FaceBlendShapes.Select(blendShape => blendShape.Key)
                .Concat(_expressionEditor.AdditionalToggles.Select(toggle => toggle.Value.gameObject?.name))
                .Concat(_expressionEditor.AdditionalTransforms.Select(transform => transform.Value?.GameObject?.name));
            var labelWidth = propertyNames.Select(x => GUI.skin.label.CalcSize(new GUIContent(x)).x).DefaultIfEmpty().Max();
            var buttonWidth = propertyNames.Select(x => GUI.skin.button.CalcSize(new GUIContent(x)).x).DefaultIfEmpty().Max();
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
                        Field_AnimatedToggles();
                        Field_AnimatedTransforms();
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
                        Field_Toggles();
                        Field_Transforms();
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
                    _expressionEditor.FetchProperties();
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
                    _expressionEditor.FetchProperties();
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
                _expressionEditor.SetBlendShapeBuffer(blendShape.Key, blendShape.Value);
            }
            foreach (var key in removed)
            {
                _expressionEditor.RemoveBlendShapeBuffer(key);
            }

            // Check buffer
            if (fieldInputPerformed || removed.Any())
            {
                _expressionEditor.CheckBuffer();
            }
        }

        private void Field_AnimatedToggles()
        {
            var changed = new Dictionary<int, bool>();
            var removed = new List<int>();

            var labelWidth = _expressionEditor.AnimatedAdditionalTogglesBuffer
                .Select(toggle => GUI.skin.label.CalcSize(new GUIContent(toggle.Value.gameObject?.name)).x)
                .DefaultIfEmpty()
                .Max();
            labelWidth += 10;

            EditorGUILayout.Space();

            // Draw controls
            foreach (var toggle in _expressionEditor.AnimatedAdditionalTogglesBuffer)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Label
                    GUIContent labelContent = new GUIContent(toggle.Value.gameObject?.name);
                    Rect labelRect = GUILayoutUtility.GetRect(labelContent, GUI.skin.label, GUILayout.Width(labelWidth));
                    GUI.Label(labelRect, labelContent);

                    // Toggle
                    var isActive = GUILayout.Toggle(toggle.Value.isActive, string.Empty);
                    if (isActive != toggle.Value.isActive)
                    {
                        changed[toggle.Key] = isActive;
                    }

                    GUILayout.FlexibleSpace();

                    // Remove button
                    if (GUILayout.Button("x", _removeButtonStyle, GUILayout.Width(20)))
                    {
                        removed.Add(toggle.Key);
                    }
                }
            }

            // Set buffer
            foreach (var toggle in changed)
            {
                _expressionEditor.SetToggleBuffer(toggle.Key, toggle.Value);
            }
            foreach (var key in removed)
            {
                _expressionEditor.RemoveToggleBuffer(key);
            }

            // Check buffer
            if (changed.Any() || removed.Any())
            {
                _expressionEditor.CheckBuffer();
            }
        }

        private void Field_AnimatedTransforms()
        {
            var fieldInputPerformed = false;
            var changed = new Dictionary<int, TransformProxy>();
            var removed = new List<int>();

            var labelWidth = GUI.skin.label.CalcSize(new GUIContent("ScaleX")).x;
            labelWidth += 10;

            // If no space is inserted here, the horizontal line of the topmost Slider will disappear.
            EditorGUILayout.Space();

            // Draw controls
            foreach (var transform in _expressionEditor.AnimatedAdditionalTransformsBuffer)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(transform.Value?.GameObject?.name);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("x", _removeButtonStyle, GUILayout.Width(20)))
                    {
                        removed.Add(transform.Key);
                    }
                }

                var copied = transform.Value.Copy();

                // Position
                fieldInputPerformed |= TransformSlider(labelWidth, "PosX", transform.Value.PositionX, -0.2f, 0.2f, 0.0001f,
                    value => { copied.PositionX = value; changed[transform.Key] = copied; });
                fieldInputPerformed |= TransformSlider(labelWidth, "PosY", transform.Value.PositionY, -0.2f, 0.2f, 0.0001f,
                    value => { copied.PositionY = value; changed[transform.Key] = copied; });
                fieldInputPerformed |= TransformSlider(labelWidth, "PosZ", transform.Value.PositionZ, -0.2f, 0.2f, 0.0001f,
                    value => { copied.PositionZ = value; changed[transform.Key] = copied; });

                // Rotation
                fieldInputPerformed |= TransformSlider(labelWidth, "RotX", transform.Value.RotationX, -180, 180, 0.0001f,
                    value => { copied.RotationX = value; changed[transform.Key] = copied; });
                fieldInputPerformed |= TransformSlider(labelWidth, "RotY", transform.Value.RotationY, -180, 180, 0.0001f,
                    value => { copied.RotationY = value; changed[transform.Key] = copied; });
                fieldInputPerformed |= TransformSlider(labelWidth, "RotZ", transform.Value.RotationZ, -180, 180, 0.0001f,
                    value => { copied.RotationZ = value; changed[transform.Key] = copied; });

                // Scale
                fieldInputPerformed |= TransformSlider(labelWidth, "ScaleX", transform.Value.ScaleX, 0, 2, 0.0001f,
                    value => { copied.ScaleX = value; changed[transform.Key] = copied; });
                fieldInputPerformed |= TransformSlider(labelWidth, "ScaleY", transform.Value.ScaleY, 0, 2, 0.0001f,
                    value => { copied.ScaleY = value; changed[transform.Key] = copied; });
                fieldInputPerformed |= TransformSlider(labelWidth, "ScaleZ", transform.Value.ScaleZ, 0, 2, 0.0001f,
                    value => { copied.ScaleZ = value; changed[transform.Key] = copied; });
            }

            // Set buffer
            foreach (var transform in changed)
            {
                _expressionEditor.SetTransformBuffer(transform.Key, transform.Value);
            }
            foreach (var key in removed)
            {
                _expressionEditor.RemoveTransformBuffer(key);
            }

            // Check buffer
            if (fieldInputPerformed || removed.Any())
            {
                _expressionEditor.CheckBuffer();
            }
        }

        private static bool TransformSlider(float labelWidth, string labelText, float value, float minValue, float maxValue, float increment, Action<float> changed)
        {
            value = Mathf.Round(value / increment) * increment;
            var fieldInputPerformed = false;

            using (new EditorGUILayout.HorizontalScope())
            {
                // Label
                GUIContent labelContent = new GUIContent(labelText);
                Rect labelRect = GUILayoutUtility.GetRect(labelContent, GUI.skin.label, GUILayout.Width(labelWidth));
                GUI.Label(labelRect, labelContent);

                // Slider
                Rect sliderRect = GUILayoutUtility.GetRect(labelRect.x, labelRect.y, GUILayout.ExpandWidth(true), GUILayout.MinWidth(100));
                var sliderValue = GUI.HorizontalSlider(sliderRect, value, minValue, maxValue);
                sliderValue = Mathf.Round(sliderValue / increment) * increment;
                if (!Mathf.Approximately(sliderValue, value))
                {
                    changed(sliderValue);
                }

                // DelayedFloatField
                var fieldValue = EditorGUILayout.DelayedFloatField(value, GUILayout.Width(60));
                if (!Mathf.Approximately(fieldValue, value))
                {
                    changed(fieldValue);
                    fieldInputPerformed = true;
                }
            }

            return fieldInputPerformed;
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
                _expressionEditor.SetBlendShapeBuffer(key, 100);
            }

            // Check buffer
            if (added.Any())
            {
                _expressionEditor.CheckBuffer();
            }
        }

        private void Field_Toggles()
        {
            // Draw buttons
            var added = new List<int>();
            _toggleFoldoutState = EditorGUILayout.Foldout(_toggleFoldoutState, "Toggles");
            if (_toggleFoldoutState)
            {
                foreach (var toggle in _expressionEditor.AdditionalToggles)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(IndentWidth);

                        if (_expressionEditor.AnimatedAdditionalTogglesBuffer.ContainsKey(toggle.Key))
                        {
                            GUILayout.Label(toggle.Value.gameObject?.name);
                        }
                        else
                        {
                            if (GUILayout.Button(toggle.Value.gameObject?.name, _addPropertyButtonStyle))
                            {
                                added.Add(toggle.Key);
                            }
                        }
                    }
                }
            }

            // Set buffer
            foreach (var key in added)
            {
                _expressionEditor.SetToggleBuffer(key, _expressionEditor.AdditionalToggles[key].isActive);
            }

            // Check buffer
            if (added.Any())
            {
                _expressionEditor.CheckBuffer();
            }
        }

        private void Field_Transforms()
        {
            // Draw buttons
            var added = new List<int>();
            _transformFoldoutState = EditorGUILayout.Foldout(_transformFoldoutState, "Transforms");
            if (_transformFoldoutState)
            {
                foreach (var transform in _expressionEditor.AdditionalTransforms)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(IndentWidth);

                        if (_expressionEditor.AnimatedAdditionalTransformsBuffer.ContainsKey(transform.Key))
                        {
                            GUILayout.Label(transform.Value.GameObject?.name);
                        }
                        else
                        {
                            if (GUILayout.Button(transform.Value.GameObject?.name, _addPropertyButtonStyle))
                            {
                                added.Add(transform.Key);
                            }
                        }
                    }
                }
            }

            // Set buffer
            foreach (var key in added)
            {
                var transform = TransformProxy.FromGameObject(_expressionEditor.AdditionalTransforms[key]?.GameObject);
                _expressionEditor.SetTransformBuffer(key, transform);
            }

            // Check buffer
            if (added.Any())
            {
                _expressionEditor.CheckBuffer();
            }
        }
    }
}