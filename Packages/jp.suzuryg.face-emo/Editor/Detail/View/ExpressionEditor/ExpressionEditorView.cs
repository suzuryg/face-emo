using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Localization;
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

namespace Suzuryg.FaceEmo.Detail.View.ExpressionEditor
{
    public class ExpressionEditorView : IDisposable
    {
        private static readonly float IndentWidth = 20;
        private static readonly float ToggleWidth = 15;
        private static readonly string ClipNameFieldName = "ClipNameField";

        private ISubWindowProvider _subWindowProvider;
        private ILocalizationSetting _localizationSetting;
        private LocalizationTable _localizationTable;
        private AV3.ExpressionEditor _expressionEditor;
        private SerializedObject _expressionEditorSetting;

        private IMGUIContainer _rootContainer;

        private bool _isRenamingClip = false;
        private string _newClipName = string.Empty;

        private Dictionary<string, bool> _faceBlendShapeFoldoutStates = new Dictionary<string, bool>();
        private bool _toggleFoldoutState = false;
        private bool _transformFoldoutState = false;

        private Vector2 _leftScrollPosition = Vector2.zero;
        private Vector2 _rightScrollPosition = Vector2.zero;

        private Texture2D _redTexture; // Store to avoid destruction
        private Texture2D _emphasizedTexture; // Store to avoid destruction
        private GUIStyle _removeButtonStyle = new GUIStyle();
        private GUIStyle _addPropertyLabelStyle = new GUIStyle();
        private GUIStyle _addPropertyButtonStyle = new GUIStyle();
        private GUIStyle _addPropertyButtonMouseOverStyle = new GUIStyle();
        private GUIStyle _warningTextStyle = new GUIStyle();
        private GUIStyle _normalPropertyStyle = new GUIStyle();
        private GUIStyle _warnedPropertyStyle = new GUIStyle();

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
            // Texture
            _redTexture = ViewUtility.MakeTexture(Color.red);
            _emphasizedTexture = ViewUtility.MakeTexture(ViewUtility.GetEmphasizedBackgroundColor());

            // Remove button
            _removeButtonStyle = new GUIStyle(GUI.skin.button);
            //_removeButtonStyle.fontStyle = FontStyle.Bold;
            //_removeButtonStyle.normal.background = MakeTexture(Color.red);
            //_removeButtonStyle.normal.textColor = Color.white;
            //_removeButtonStyle.hover.background = MakeTexture(new Color(1, 0.7f, 0.7f));
            //_removeButtonStyle.hover.textColor = Color.white;
            //_removeButtonStyle.active.background = MakeTexture(new Color(0.8f, 0, 0));
            //_removeButtonStyle.active.textColor = Color.white;

            // Add property label / button
            _addPropertyLabelStyle = new GUIStyle(GUI.skin.label);
            _addPropertyLabelStyle.fixedHeight = EditorGUIUtility.singleLineHeight * 1.5f;

            _addPropertyButtonStyle = new GUIStyle(GUI.skin.button);
            _addPropertyButtonStyle.alignment = TextAnchor.MiddleLeft;
            _addPropertyButtonStyle.fixedHeight = EditorGUIUtility.singleLineHeight * 1.5f;

            _addPropertyButtonMouseOverStyle = new GUIStyle(GUI.skin.button);
            _addPropertyButtonMouseOverStyle.alignment = TextAnchor.MiddleLeft;
            _addPropertyButtonMouseOverStyle.normal.background = _emphasizedTexture;
            _addPropertyButtonMouseOverStyle.normal.scaledBackgrounds = new[] { _emphasizedTexture };
            _addPropertyButtonMouseOverStyle.normal.textColor = ViewUtility.GetEmphasizedTextColor();
            _addPropertyButtonMouseOverStyle.fixedHeight = EditorGUIUtility.singleLineHeight * 1.5f;

            // Warning text
            _warningTextStyle = new GUIStyle(GUI.skin.label);
            if (EditorGUIUtility.isProSkin)
            {
                _warningTextStyle.normal.textColor = Color.red;
            }
            else
            {
                _warningTextStyle.normal.background = _redTexture;
                _warningTextStyle.normal.scaledBackgrounds = new[] { _redTexture };
                _warningTextStyle.normal.textColor = Color.black;
            }

            // Normal property
            _normalPropertyStyle = new GUIStyle(GUI.skin.label);

            // Warned prperty
            _warnedPropertyStyle = new GUIStyle(GUI.skin.label);
            _warnedPropertyStyle.normal.background = _redTexture;
            _warnedPropertyStyle.normal.scaledBackgrounds = new[] { _redTexture };
            _warnedPropertyStyle.normal.textColor = Color.black;
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        private void OnGUI()
        {
            _expressionEditorSetting.Update();

            // Calculate property name width
            var propertyNames = _expressionEditor.FaceBlendShapes.Select(blendShape => blendShape.Key.Name)
                .Concat(_expressionEditor.AdditionalToggles.Select(toggle => toggle.Value.gameObject?.name))
                .Concat(_expressionEditor.AdditionalTransforms.Select(transform => transform.Value?.GameObject?.name))
                .Concat(new[] {
                    _localizationTable.ExpressionEditorView_UncategorizedBlendShapes,
                    _localizationTable.ExpressionEditorView_AddtionalToggleObjects,
                    _localizationTable.ExpressionEditorView_AddtionalTransformObjects});
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

            // Set minimum window size
            var minLeftContentWidth = GUI.skin.label.CalcSize(new GUIContent(_localizationTable.ExpressionEditorView_ShowOnlyDifferFromDefaultValue)).x + ToggleWidth;
            var minWindowWidth = padding.left + minLeftContentWidth + leftRightMargin + rightContentWidth + padding.right;
            const float minWindowHeight = 300;
            window.minSize = new Vector2(minWindowWidth, minWindowHeight);

            // Repaint other window when finished moving the Slider.
            if (Event.current.type == EventType.MouseUp)
            {
                _expressionEditor.RepaintOtherWindows();
            }

            // Root scope
            using (new EditorGUILayout.HorizontalScope())
            {
                // Left scope
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(leftContentWidth), GUILayout.Height(contentHeight)))
                {
                    Field_AnimationClip();
                    Field_UseMouseWheel();
                    Field_ShowOnlyDifferFromDefaultValue();

                    using (var scope = new EditorGUILayout.ScrollViewScope(_leftScrollPosition))
                    {
                        Field_AnimatedBlendShapes();
                        Field_AnimatedToggles();
                        Field_AnimatedTransforms();

                        ShowHints();

                        _leftScrollPosition = scope.scrollPosition;
                    }
                }

                EditorGUILayout.Space(leftRightMargin);

                // Right scope
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(rightContentWidth), GUILayout.Height(contentHeight)))
                {
                    Field_AddAllBlendShapes();
                    Field_FaceBlendShapeDelimiter();
                    Field_ReflectInPreviewOnMouseOver();
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
            using (new EditorGUILayout.HorizontalScope())
            {
                const float buttonWidth = 60;
                var clipExists = _expressionEditor.Clip != null;

                if (_isRenamingClip && clipExists)
                {
                    // Enter key event
                    var e = Event.current;
                    if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
                    {
                        RenameClip();
                    }

                    // Text field
                    GUI.SetNextControlName(ClipNameFieldName);
                    _newClipName = EditorGUILayout.TextField(_newClipName);

                    // Confirm button
                    if (GUILayout.Button(_localizationTable.ExpressionEditorView_Confirm, GUILayout.Width(buttonWidth)))
                    {
                        RenameClip();
                    }
                }
                else
                {
                    // Update clip name
                    if (clipExists)
                    {
                        var path = AssetDatabase.GetAssetPath(_expressionEditor.Clip);
                        _newClipName = Path.GetFileName(path).Replace(".anim", "");
                    }

                    // Object field
                    var ret = EditorGUILayout.ObjectField(_expressionEditor.Clip, typeof(AnimationClip), allowSceneObjects: false);
                    if (ret is AnimationClip animationClip && animationClip != null && !ReferenceEquals(animationClip, _expressionEditor.Clip))
                    {
                        _expressionEditor.Open(animationClip);
                    }

                    // Rename button
                    using (new EditorGUI.DisabledScope(!clipExists))
                    {
                        if (GUILayout.Button(_localizationTable.ExpressionEditorView_Rename, GUILayout.Width(buttonWidth)))
                        {
                            _isRenamingClip = true;
                            EditorGUI.FocusTextInControl(ClipNameFieldName);
                        }
                    }
                }

                // Place a dummy because the input value of TextField will be entered into other fields when the rename is completed.
                _ = EditorGUILayout.TextField(string.Empty, GUILayout.Width(0));
            }
        }

        private void RenameClip()
        {
            var path = AssetDatabase.GetAssetPath(_expressionEditor.Clip);
            var oldName = Path.GetFileName(path).Replace(".anim", "");
            if (_newClipName != oldName)
            {
                var ret = AssetDatabase.RenameAsset(path, _newClipName);
                if (string.IsNullOrEmpty(ret))
                {
                    _isRenamingClip = false;
                }
                else
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.ExpressionEditorView_Message_FailedToRename, "OK");
                    Debug.LogError(_localizationTable.ExpressionEditorView_Message_FailedToRename + "\n" + ret);
                    // EditorGUI.FocusTextInControl(ClipNameFieldName); // not working
                }
            }
            else
            {
                _isRenamingClip = false;
            }
        }

        private void Field_UseMouseWheel()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var useMouseWheel = EditorPrefs.GetBool(DetailConstants.Key_ExpressionEditor_UseMouseWheel, DetailConstants.Default_ExpressionEditor_UseMouseWheel);
                if (EditorGUILayout.Toggle(useMouseWheel, GUILayout.Width(ToggleWidth)) != useMouseWheel)
                {
                    EditorPrefs.SetBool(DetailConstants.Key_ExpressionEditor_UseMouseWheel, !useMouseWheel);
                }
                GUILayout.Label(_localizationTable.ExpressionEditorView_UseMouseWheel);
            }
        }

        private void Field_ShowOnlyDifferFromDefaultValue()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var showOnlyDifference = EditorPrefs.GetBool(DetailConstants.Key_ExpressionEditor_ShowOnlyDifferFromDefaultValue, DetailConstants.Default_ExpressionEditor_ShowOnlyDifferFromDefaultValue);
                if (EditorGUILayout.Toggle(showOnlyDifference, GUILayout.Width(ToggleWidth)) != showOnlyDifference)
                {
                    EditorPrefs.SetBool(DetailConstants.Key_ExpressionEditor_ShowOnlyDifferFromDefaultValue, !showOnlyDifference);
                    _expressionEditorSetting.ApplyModifiedProperties();
                    _expressionEditor.FetchProperties();
                }
                GUILayout.Label(_localizationTable.ExpressionEditorView_ShowOnlyDifferFromDefaultValue);
            }
        }

        private void Field_AddAllBlendShapes()
        {
            if (GUILayout.Button(new GUIContent(_localizationTable.ExpressionEditorView_AddAllBlendShapes, _localizationTable.ExpressionEditorView_Tooltip_AddAllBlendShapes)))
            {
                // Delay the execution of the following code because ShowModalUtility() interferes with LayoutGroup processing.
                var centerPosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                EditorApplication.delayCall += () =>
                {
                    if (OptoutableDialog.Show(DomainConstants.SystemName, _localizationTable.ExpressionEditorView_Message_AddAllBlendShapes,
                        _localizationTable.Common_Add, _localizationTable.Common_Cancel,
                        centerPosition: centerPosition))
                    {
                        EditorPrefs.SetBool(DetailConstants.Key_ExpressionEditor_ShowOnlyDifferFromDefaultValue, false);
                        _expressionEditorSetting.ApplyModifiedProperties();
                        _expressionEditor.FetchProperties();
                        _expressionEditor.AddAllFaceBlendShapes();
                    }
                };
            }
        }

        private void Field_ReflectInPreviewOnMouseOver()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var reflect = EditorPrefs.GetBool(DetailConstants.Key_ExpressionEditor_ReflectInPreviewOnMouseOver, DetailConstants.Default_ExpressionEditor_ReflectInPreviewOnMouseOver);
                if (EditorGUILayout.Toggle(reflect, GUILayout.Width(ToggleWidth)) != reflect)
                {
                    EditorPrefs.SetBool(DetailConstants.Key_ExpressionEditor_ReflectInPreviewOnMouseOver, !reflect);
                }
                GUILayout.Label(_localizationTable.ExpressionEditorView_ReflectInPreviewOnMouseOver);
            }
        }

        private void Field_FaceBlendShapeDelimiter()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(_expressionEditorSetting.FindProperty(nameof(ExpressionEditorSetting.FaceBlendShapeDelimiter)),
                    new GUIContent(_localizationTable.ExpressionEditorView_Delimiter, _localizationTable.ExpressionEditorView_Tooltip_Delimiter));

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
            var changed = new Dictionary<BlendShape, float>();
            var removed = new List<BlendShape>();

            var labelWidth = _expressionEditor.AnimatedBlendShapesBuffer
                .Select(blendShape => GUI.skin.label.CalcSize(new GUIContent(blendShape.Key.Name)).x)
                .DefaultIfEmpty()
                .Max();
            labelWidth += 10;

            // If no space is inserted here, the horizontal line of the topmost Slider will disappear.
            EditorGUILayout.Space();

            // Draw warnings
            if (_expressionEditor.AnimatedBlendShapesBuffer.Any(blendShape => _expressionEditor.BlinkBlendShapes.Contains(blendShape.Key)))
            {
                GUILayout.Label(_localizationTable.ExpressionEditorView_Message_BlinkBlendShapeExists, _warningTextStyle);
            }
            if (_expressionEditor.AnimatedBlendShapesBuffer.Any(blendShape => _expressionEditor.LipSyncBlendShapes.Contains(blendShape.Key)))
            {
                GUILayout.Label(_localizationTable.ExpressionEditorView_Message_LipSyncBlendShapeExists, _warningTextStyle);
            }

            // Draw controls
            foreach (var blendShape in _expressionEditor.AnimatedBlendShapesBuffer)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    const float minValue = 0;
                    const float maxValue = 100;

                    // Label
                    GUIContent labelContent = new GUIContent(blendShape.Key.Name);
                    Rect labelRect = GUILayoutUtility.GetRect(labelContent, GUI.skin.label, GUILayout.Width(labelWidth));
                    var warned = _expressionEditor.BlinkBlendShapes.Contains(blendShape.Key) || _expressionEditor.LipSyncBlendShapes.Contains(blendShape.Key);
                    GUI.Label(labelRect, labelContent, warned ? _warnedPropertyStyle : _normalPropertyStyle);

                    // Slider
                    const float increment = 0.1f;
                    Rect sliderRect = GUILayoutUtility.GetRect(labelRect.x, labelRect.y, GUILayout.ExpandWidth(true), GUILayout.MinWidth(100));
                    var sliderValue = GUI.HorizontalSlider(sliderRect, blendShape.Value, minValue, maxValue);

                    var useMouseWheel = EditorPrefs.GetBool(DetailConstants.Key_ExpressionEditor_UseMouseWheel, DetailConstants.Default_ExpressionEditor_UseMouseWheel);
                    if (useMouseWheel)
                    {
                        var scrollWheelSensitivity = (maxValue - minValue) / 100;
                        var wheelRect = new Rect(sliderRect.x, labelRect.y, sliderRect.width, labelRect.height); // sliderRect.height is 0
                        if (wheelRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
                        {
                            sliderValue -= Event.current.delta.y * scrollWheelSensitivity;
                            sliderValue = Mathf.Clamp(sliderValue, minValue, maxValue);
                        }
                    }

                    sliderValue = Mathf.Round(sliderValue / increment) * increment;

                    if (!Mathf.Approximately(sliderValue, blendShape.Value))
                    {
                        changed[blendShape.Key] = sliderValue;
                    }

                    // FloatField
                    var fieldValue = EditorGUILayout.FloatField(blendShape.Value, GUILayout.Width(40));
                    fieldValue = Math.Max(Math.Min(fieldValue, maxValue), minValue);
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
            if (fieldInputPerformed || changed.Any() || removed.Any())
            {
                // CheckBuffer processing load is small (confirmed by profiler).
                _expressionEditor.CheckBuffer();
            }

            // Repaint
            if (fieldInputPerformed || removed.Any())
            {
                _expressionEditor.RepaintOtherWindows();
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

            // Repaint
            if (changed.Any() || removed.Any())
            {
                _expressionEditor.RepaintOtherWindows();
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
            if (fieldInputPerformed || changed.Any() || removed.Any())
            {
                _expressionEditor.CheckBuffer();
            }

            // Repaint
            if (fieldInputPerformed || removed.Any())
            {
                _expressionEditor.RepaintOtherWindows();
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


                var useMouseWheel = EditorPrefs.GetBool(DetailConstants.Key_ExpressionEditor_UseMouseWheel, DetailConstants.Default_ExpressionEditor_UseMouseWheel);
                if (useMouseWheel)
                {
                    var scrollWheelSensitivity = (maxValue - minValue) / 100;
                    var wheelRect = new Rect(sliderRect.x, labelRect.y, sliderRect.width, labelRect.height); // sliderRect.height is 0
                    if (wheelRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
                    {
                        sliderValue -= Event.current.delta.y * scrollWheelSensitivity;
                        sliderValue = Mathf.Clamp(sliderValue, minValue, maxValue);
                    }
                }

                sliderValue = Mathf.Round(sliderValue / increment) * increment;

                if (!Mathf.Approximately(sliderValue, value))
                {
                    changed(sliderValue);
                }

                // FloatField
                var fieldValue = EditorGUILayout.FloatField(value, GUILayout.Width(60));
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
            // Get setting values
            var showBlink = EditorPrefs.GetBool(DetailConstants.Key_ExpressionEditor_ShowBlinkBlendShapes, DetailConstants.Default_ExpressionEditor_ShowBlinkBlendShapes);
            var showLipSync  = EditorPrefs.GetBool(DetailConstants.Key_ExpressionEditor_ShowLipSyncBlendShapes, DetailConstants.Default_ExpressionEditor_ShowLipSyncBlendShapes);

            // Categorize
            var delimiter = _expressionEditorSetting.FindProperty(nameof(ExpressionEditorSetting.FaceBlendShapeDelimiter)).stringValue;
            var categorized = new Dictionary<string, List<BlendShape>>();
            var categoryName = _localizationTable.ExpressionEditorView_UncategorizedBlendShapes;
            categorized[categoryName] = new List<BlendShape>();
            foreach (var blendShape in _expressionEditor.FaceBlendShapes.Keys)
            {
                // face mesh
                if (blendShape.Path == _expressionEditor.FaceMeshTransformPath)
                {
                    if (!string.IsNullOrEmpty(delimiter) && blendShape.Name.Contains(delimiter))
                    {
                        categoryName = string.IsNullOrEmpty(delimiter) ? blendShape.Name : blendShape.Name.Replace(delimiter, string.Empty);
                        categorized[categoryName] = new List<BlendShape>();
                    }
                    else
                    {
                        categorized[categoryName].Add(blendShape);
                    }
                }
                // additional mesh
                else
                {
                    if (categorized.ContainsKey(blendShape.Path))
                    {
                        categorized[blendShape.Path].Add(blendShape);
                    }
                    else
                    {
                        categorized[blendShape.Path] = new List<BlendShape>() { blendShape };
                    }
                }
            }

            // Immediately after opening ExpressionEditor and if it has not been categorized, open the first category.
            var previousStates = new Dictionary<string, bool>(_faceBlendShapeFoldoutStates);
            if (!previousStates.Any() && categorized.Count == 1)
            {
                previousStates[categorized.First().Key] = true;
            }

            // Update foldout states
            _faceBlendShapeFoldoutStates.Clear();
            foreach (var key in categorized.Keys)
            {
                if (previousStates.ContainsKey(key)) { _faceBlendShapeFoldoutStates[key] = previousStates[key]; }
                else { _faceBlendShapeFoldoutStates[key] = false; }
            }

            // Draw buttons
            var added = new List<BlendShape>();
            BlendShape blendShapeMouseOver = null;
            foreach (var category in categorized)
            {
                _faceBlendShapeFoldoutStates[category.Key] = EditorGUILayout.Foldout(_faceBlendShapeFoldoutStates[category.Key], category.Key);
                if (_faceBlendShapeFoldoutStates[category.Key])
                {
                    // If there is no blend shape in the category, display so.
                    if (!category.Value.Any())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(IndentWidth);
                            GUILayout.Label(_localizationTable.ExpressionEditorView_NoBlendShapes);
                        }
                        continue;
                    }

                    // If there are any blend shapes in the category, display them.
                    foreach (var blendShapeKey in category.Value)
                    {
                        if (!showBlink && _expressionEditor.BlinkBlendShapes.Contains(blendShapeKey)) { continue; }
                        if (!showLipSync && _expressionEditor.LipSyncBlendShapes.Contains(blendShapeKey)) { continue; }

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(IndentWidth);

                            if (_expressionEditor.AnimatedBlendShapesBuffer.ContainsKey(blendShapeKey))
                            {
                                GUILayout.Label(blendShapeKey.Name, _addPropertyLabelStyle);
                            }
                            else
                            {
                                var buttonRect = GUILayoutUtility.GetRect(new GUIContent(blendShapeKey.Name), _addPropertyButtonStyle);
                                var buttonStyle = _addPropertyButtonStyle;

                                var reflect = EditorPrefs.GetBool(DetailConstants.Key_ExpressionEditor_ReflectInPreviewOnMouseOver, DetailConstants.Default_ExpressionEditor_ReflectInPreviewOnMouseOver);
                                if (reflect && buttonRect.Contains(Event.current.mousePosition))
                                {
                                    buttonStyle = _addPropertyButtonMouseOverStyle;
                                    blendShapeMouseOver = blendShapeKey;
                                }

                                if (GUI.Button(buttonRect, blendShapeKey.Name, buttonStyle))
                                {
                                    added.Add(blendShapeKey);
                                }
                            }
                        }
                    }
                }
            }

            // Preview mouse over blend shape
            // Since it is always judged not to be mouse-over at the Layout event, the preview process is called only at the Repaint event.
            if (Event.current.type == EventType.Repaint)
            {
                _expressionEditor.SetPreviewBlendShape(blendShapeMouseOver);
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

            // Repaint
            if (added.Any())
            {
                _expressionEditor.RepaintOtherWindows();
            }
        }

        private void Field_Toggles()
        {
            // Draw buttons
            var added = new List<int>();
            _toggleFoldoutState = EditorGUILayout.Foldout(_toggleFoldoutState, _localizationTable.ExpressionEditorView_AddtionalToggleObjects);
            if (_toggleFoldoutState)
            {
                // If there is no object, display so.
                if (!_expressionEditor.AdditionalToggles.Any())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(IndentWidth);
                        GUILayout.Label(_localizationTable.ExpressionEditorView_NoObjects);
                    }
                }

                // If there are any objects, display them.
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

            // Repaint
            if (added.Any())
            {
                _expressionEditor.RepaintOtherWindows();
            }
        }

        private void Field_Transforms()
        {
            // Draw buttons
            var added = new List<int>();
            _transformFoldoutState = EditorGUILayout.Foldout(_transformFoldoutState, _localizationTable.ExpressionEditorView_AddtionalTransformObjects);
            if (_transformFoldoutState)
            {
                // If there is no object, display so.
                if (!_expressionEditor.AdditionalTransforms.Any())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(IndentWidth);
                        GUILayout.Label(_localizationTable.ExpressionEditorView_NoObjects);
                    }
                }

                // If there are any objects, display them.
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

            // Repaint
            if (added.Any())
            {
                _expressionEditor.RepaintOtherWindows();
            }
        }

        private void ShowHints()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (!showHints) { return; }

            GUILayout.FlexibleSpace();

            if (_expressionEditor.AnimatedBlendShapesBuffer.Any(blendShape => _expressionEditor.BlinkBlendShapes.Contains(blendShape.Key)))
            {
                HelpBoxDrawer.WarnLayout(_localizationTable.Hints_BlinkBlendShapeIncluded);
            }
            if (_expressionEditor.AnimatedBlendShapesBuffer.Any(blendShape => _expressionEditor.LipSyncBlendShapes.Contains(blendShape.Key)))
            {
                HelpBoxDrawer.WarnLayout(_localizationTable.Hints_LipSyncBlendShapeIncluded);
            }

            HelpBoxDrawer.InfoLayout(_localizationTable.Hints_ExpressionEditorLayout);
            HelpBoxDrawer.InfoLayout(_localizationTable.Hints_ExpressionPreview);
        }
    }
}
