using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Checker;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.View.Element;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UniRx;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class InspectorView : IDisposable
    {
        private static readonly float ToggleWidth = 15;

        public IObservable<Unit> OnLaunchButtonClicked => _onLaunchButtonClicked.AsObservable();
        public IObservable<Locale> OnLocaleChanged => _onLocaleChanged.AsObservable();

        private Subject<Unit> _onLaunchButtonClicked = new Subject<Unit>();
        private Subject<Locale> _onLocaleChanged = new Subject<Locale>();

        private ILocalizationSetting _localizationSetting;
        private ThumbnailDrawerBase _thumbnailDrawer;
        private SerializedObject _av3Setting;
        private SerializedObject _thumbnailSetting;

        private ReorderableList _mouthMorphBlendShapes;
        private ReorderableList _additionalToggleObjects;
        private ReorderableList _additionalTransformObjects;

        private bool _isMouthMorphBlendShapesOpened = false;
        private bool _isAddtionalToggleOpened = false;
        private bool _isAddtionalTransformOpened = false;
        private bool _isAFKOpened = false;
        private bool _isThumbnailOpened = false;
        private bool _isExpressionsMenuItemsOpened = false;
        private bool _isAvatarApplicationOpened = false;
        private bool _isEditorSettingOpened = false;
        private bool _isHelpOpened = false;

        private LocalizationTable _localizationTable;

        private GUIStyle _warningLabelStyle = new GUIStyle();
        private GUIStyle _helpBoxStyle = new GUIStyle();

        private CompositeDisposable _disposables = new CompositeDisposable();

        public InspectorView(
            ILocalizationSetting localizationSetting,
            InspectorThumbnailDrawer exMenuThumbnailDrawer,
            AV3Setting av3Setting,
            ThumbnailSetting thumbnailSetting)
        {
            // Dependencies
            _localizationSetting = localizationSetting;
            _thumbnailDrawer = exMenuThumbnailDrawer;
            _av3Setting = new SerializedObject(av3Setting);
            _thumbnailSetting = new SerializedObject(thumbnailSetting);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Mouth morph blendshapes
            _mouthMorphBlendShapes = new ReorderableList(null, typeof(string));
            _mouthMorphBlendShapes.onAddCallback = AddMouthMorphBlendShape;
            _mouthMorphBlendShapes.onRemoveCallback = RemoveMouthMorphBlendShape;
            _mouthMorphBlendShapes.draggable = false;
            _mouthMorphBlendShapes.headerHeight = 0;

            // Additional expression objects
            _additionalToggleObjects = new ReorderableList(_av3Setting, _av3Setting.FindProperty(nameof(AV3Setting.AdditionalToggleObjects)));
            _additionalToggleObjects.headerHeight = 0;
            _additionalToggleObjects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _additionalToggleObjects.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };

            _additionalTransformObjects = new ReorderableList(_av3Setting, _av3Setting.FindProperty(nameof(AV3Setting.AdditionalTransformObjects)));
            _additionalTransformObjects.headerHeight = 0;
            _additionalTransformObjects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _additionalTransformObjects.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };

            // Styles
            try
            {
                _warningLabelStyle = new GUIStyle(EditorStyles.label);
                _helpBoxStyle = new GUIStyle(EditorStyles.helpBox);
                _helpBoxStyle.fontSize = EditorStyles.label.fontSize;
            }
            catch (NullReferenceException)
            {
                // Workaround for play mode
                _warningLabelStyle = new GUIStyle();
                _helpBoxStyle = new GUIStyle();
            }
            _warningLabelStyle.normal.textColor = Color.red;

            // Set text
            SetText(_localizationSetting.Table);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void OnGUI()
        {
            _av3Setting.Update();
            _thumbnailSetting.Update();

            Field_CheckVersion();

            // Launch button
            var avatarDescriptor = _av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar)).objectReferenceValue as VRCAvatarDescriptor;
            var canLaunch = avatarDescriptor != null;
            using (new EditorGUI.DisabledScope(!canLaunch))
            {
                var buttonText = canLaunch ? _localizationTable.InspectorView_Launch : _localizationTable.InspectorView_TargetAvatarIsNotSpecified;
                if (GUILayout.Button(buttonText, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3)))
                {
                    _onLaunchButtonClicked.OnNext(Unit.Default);
                }
            }
            EditorGUILayout.Space(10);

            // Target avatar
            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar)), new GUIContent(_localizationTable.InspectorView_TargetAvatar));

            EditorGUILayout.Space(10);

            // Locale
            Field_Locale();

            EditorGUILayout.Space(10);

            // Mouth Morph Blend Shapes
            _isMouthMorphBlendShapesOpened = EditorGUILayout.Foldout(_isMouthMorphBlendShapesOpened, _localizationTable.InspectorView_MouthMorphBlendShapes);
            if (_isMouthMorphBlendShapesOpened)
            {
                Field_MouthMorphBlendShape();
            }

            EditorGUILayout.Space(10);

            // Additional Toggle Objects
            _isAddtionalToggleOpened = EditorGUILayout.Foldout(_isAddtionalToggleOpened, _localizationTable.Common_AddtionalToggleObjects);
            if (_isAddtionalToggleOpened)
            {
                Field_AdditionalToggleObjects();
            }

            EditorGUILayout.Space(10);

            // Additional Transform Objects
            _isAddtionalTransformOpened = EditorGUILayout.Foldout(_isAddtionalTransformOpened, _localizationTable.Common_AddtionalTransformObjects);
            if (_isAddtionalTransformOpened)
            {
                Field_AdditionalTransformObjects();
            }

            EditorGUILayout.Space(10);

            // AFK face setting
            _isAFKOpened = EditorGUILayout.Foldout(_isAFKOpened, _localizationTable.InspectorView_AFK);
            if (_isAFKOpened)
            {
                Field_AFKFace();
            }

            EditorGUILayout.Space(10);

            // Thumbnail Setting
            _isThumbnailOpened = EditorGUILayout.Foldout(_isThumbnailOpened, _localizationTable.InspectorView_Thumbnail);
            if (_isThumbnailOpened)
            {
                Field_ThumbnailSetting();
            }

            EditorGUILayout.Space(10);

            // Expressions Menu Setting Items
            _isExpressionsMenuItemsOpened = EditorGUILayout.Foldout(_isExpressionsMenuItemsOpened, _localizationTable.InspectorView_ExpressionsMenuSettingItems);
            if (_isExpressionsMenuItemsOpened)
            {
                Field_ExpressionsMenuSettingItems();
            }

            EditorGUILayout.Space(10);

            // Avatar application setting
            _isAvatarApplicationOpened = EditorGUILayout.Foldout(_isAvatarApplicationOpened, _localizationTable.InspectorView_AvatarApplicationSetting);
            if (_isAvatarApplicationOpened)
            {
                Field_AvatarApplicationSetting();
            }

            EditorGUILayout.Space(10);

            // Editor setting
            _isEditorSettingOpened = EditorGUILayout.Foldout(_isEditorSettingOpened, _localizationTable.InspectorView_EditorSetting);
            if (_isEditorSettingOpened)
            {
                Field_EditorSetting();
            }

            EditorGUILayout.Space(10);

            // Help
            _isHelpOpened = EditorGUILayout.Foldout(_isHelpOpened, _localizationTable.InspectorView_Help);
            if (_isHelpOpened)
            {
                Field_Help();
            }

            _av3Setting.ApplyModifiedProperties();
            _thumbnailSetting.ApplyModifiedProperties();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        private void Field_CheckVersion()
        {
            if (!PackageVersionChecker.IsCompleted) { return; }

            if (string.IsNullOrEmpty(PackageVersionChecker.ModularAvatar))
            {
                HelpBoxWithErrorIcon(_localizationTable.InspectorView_Message_MAVersionError_NotFound);
            }
            else if (PackageVersionChecker.ModularAvatar == "1.5.0-beta-4" || PackageVersionChecker.ModularAvatar == "1.5.0")
            {
                HelpBoxWithErrorIcon(_localizationTable.InspectorView_Message_MAVersionError_1_5_0);
            }
        }

        private void HelpBoxWithErrorIcon(string message)
        {
            GUILayout.Label(new GUIContent(message, EditorGUIUtility.IconContent("console.erroricon").image), _helpBoxStyle);
        }

        private void Field_Locale()
        {
            using (new EditorGUILayout.HorizontalScope())
            { 
                var oldLocale = _localizationSetting.Locale;
                var newLocale = (Locale)EditorGUILayout.EnumPopup(new GUIContent("言語設定 (Language Setting)"), oldLocale);
                if (newLocale != oldLocale)
                {
                    _localizationSetting.SetLocale(newLocale);
                    _onLocaleChanged.OnNext(newLocale);
                    SetText(_localizationSetting.Table);
                }
            }
        }

        private List<string> GetMouthMorphBlendShapes()
        {
            _av3Setting.Update();
            var property = _av3Setting.FindProperty(nameof(AV3Setting.MouthMorphBlendShapes));

            var list = new List<string>();
            for (int i = 0; i < property.arraySize; i++)
            {
                list.Add(property.GetArrayElementAtIndex(i).stringValue);
            }

            return list;
        }

        private void AddMouthMorphBlendShape(ReorderableList reorderableList)
        {
            var faceBlendShapes = new List<string>();
            _av3Setting.Update();
            var avatarDescriptor = _av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar)).objectReferenceValue as VRCAvatarDescriptor;
            if (avatarDescriptor != null)
            {
                var replaceBlink = _av3Setting.FindProperty(nameof(AV3Setting.ReplaceBlink)).boolValue;
                var excludeBlink = !replaceBlink; // If blinking is not replaced by animation, do not reset the shape key for blinking
                var excludeLipSync = true;
                faceBlendShapes = AV3Utility.GetFaceMeshBlendShapes(avatarDescriptor, excludeBlink, excludeLipSync).Select(x => x.Key).ToList();
            }

            UnityEditor.PopupWindow.Show(
                new Rect(Event.current.mousePosition, Vector2.one),
                new ListSelectPopupContent<string>(faceBlendShapes, _localizationTable.Common_Add, _localizationTable.Common_Cancel,
                new Action<IReadOnlyList<string>>(blendShapes =>
                {
                    var existing = new HashSet<string>(GetMouthMorphBlendShapes());
                    var toAdd = blendShapes.Distinct().Where(x => !existing.Contains(x)).ToList();

                    _av3Setting.Update();

                    var property = _av3Setting.FindProperty(nameof(AV3Setting.MouthMorphBlendShapes));
                    var start = property.arraySize;
                    property.arraySize += toAdd.Count;
                    for (int i = 0; i < toAdd.Count; i++)
                    {
                        property.GetArrayElementAtIndex(i + start).stringValue = toAdd[i];
                    }

                    _av3Setting.ApplyModifiedProperties();
                })));
        }

        private void RemoveMouthMorphBlendShape(ReorderableList reorderableList)
        {
            // Check for abnormal selections.
            if (_mouthMorphBlendShapes.index >= _mouthMorphBlendShapes.list.Count)
            {
                return;
            }

            var selected = _mouthMorphBlendShapes.list[_mouthMorphBlendShapes.index] as string;

            _av3Setting.Update();

            var property = _av3Setting.FindProperty(nameof(AV3Setting.MouthMorphBlendShapes));
            for (int i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                if (element.stringValue == selected)
                {
                    // Remove all duplicate elements without breaking to be sure.
                    property.DeleteArrayElementAtIndex(i);
                }
            }

            _av3Setting.ApplyModifiedProperties();
        }

        private void Field_MouthMorphBlendShape()
        {
            _mouthMorphBlendShapes.list = GetMouthMorphBlendShapes(); // Is it necessary to get every frame?
            _mouthMorphBlendShapes.DoLayoutList();

            EditorGUILayout.Space(10);

            if (GUILayout.Button(_localizationTable.Common_Clear) &&
                EditorUtility.DisplayDialog(DomainConstants.SystemName,
                    _localizationTable.Common_Message_ClearMouthMorphBlendShapes,
                    _localizationTable.Common_Yes, _localizationTable.Common_No))
            {
                var property = _av3Setting.FindProperty(nameof(AV3Setting.MouthMorphBlendShapes));
                property.ClearArray();
            }
        }

        private void Field_AdditionalToggleObjects()
        {
            var avatarPath = (_av3Setting?.FindProperty(nameof(AV3Setting.TargetAvatar))?.objectReferenceValue as VRCAvatarDescriptor)?.gameObject?.GetFullPath();

            _additionalToggleObjects.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, _localizationTable.Common_AddtionalToggleObjects);
            };
            _additionalToggleObjects.DoLayoutList();

            var toggleProperty = _av3Setting?.FindProperty(nameof(AV3Setting.AdditionalToggleObjects));
            for (int i = 0; i < toggleProperty?.arraySize; i++)
            {
                var gameObject = toggleProperty?.GetArrayElementAtIndex(i)?.objectReferenceValue as GameObject;
                if (gameObject == null) { continue; }
                if (string.IsNullOrEmpty(avatarPath) || !gameObject.GetFullPath().StartsWith(avatarPath))
                {
                    EditorGUILayout.LabelField($"{gameObject.name}{_localizationTable.InspectorView_Message_NotInAvatar}", _warningLabelStyle);
                }
            }
        }

        private void Field_AdditionalTransformObjects()
        {
            var avatarPath = (_av3Setting?.FindProperty(nameof(AV3Setting.TargetAvatar))?.objectReferenceValue as VRCAvatarDescriptor)?.gameObject?.GetFullPath();

            _additionalTransformObjects.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, _localizationTable.Common_AddtionalTransformObjects);
            };
            _additionalTransformObjects.DoLayoutList();

            var transformProperty = _av3Setting?.FindProperty(nameof(AV3Setting.AdditionalTransformObjects));
            for (int i = 0; i < transformProperty?.arraySize; i++)
            {
                var gameObject = transformProperty?.GetArrayElementAtIndex(i)?.objectReferenceValue as GameObject;
                if (gameObject == null) { continue; }
                if (string.IsNullOrEmpty(avatarPath) || !gameObject.GetFullPath().StartsWith(avatarPath))
                {
                    EditorGUILayout.LabelField($"{gameObject.name}{_localizationTable.InspectorView_Message_NotInAvatar}", _warningLabelStyle);
                }
            }
        }

        private void Field_AFKFace()
        {
            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AfkEnterFace)), new GUIContent(_localizationTable.InspectorView_AFK_EnterFace));
            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AfkFace)), new GUIContent(_localizationTable.InspectorView_AFK_Face));
            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AfkExitFace)), new GUIContent(_localizationTable.InspectorView_AFK_ExitFace));
        }

        private void Field_ThumbnailSetting()
        {
            // Draw thumbnail
            float aspectRatio = 1;
            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            float newHeight = inspectorWidth * aspectRatio;
            var textureWidth = (int)inspectorWidth;
            var textureHeight = (int)newHeight;
            Rect textureRect = GUILayoutUtility.GetRect(textureWidth, textureHeight, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            var width = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Inspector_Width));
            var height = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Inspector_Height));
            if (width.intValue != textureWidth ||
                height.intValue != textureHeight)
            {
                width.intValue = textureWidth;
                height.intValue = textureHeight;
                _thumbnailDrawer.ClearCache();
            }
            _thumbnailDrawer.Update();
            var texture = _thumbnailDrawer.GetThumbnail(new Domain.Animation(string.Empty));
            GUI.DrawTexture(textureRect, texture);

            EditorGUILayout.Space(10);

            // Draw sliders
            var labelTexts = new []
            {
                _localizationTable.InspectorView_Thumbnail_Distance,
                _localizationTable.InspectorView_Thumbnail_HorizontalPosition,
                _localizationTable.InspectorView_Thumbnail_VerticalPosition,
                _localizationTable.InspectorView_Thumbnail_HorizontalAngle,
                _localizationTable.InspectorView_Thumbnail_VerticalAngle,
            };
            var labelWidth = labelTexts
                .Select(text => GUI.skin.label.CalcSize(new GUIContent(text)).x)
                .DefaultIfEmpty()
                .Max();
            labelWidth += 10;

            var distance = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_OrthoSize));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_Distance, distance.floatValue, ThumbnailSetting.MinOrthoSize, ThumbnailSetting.MaxOrthoSize,
                value => { distance.floatValue = value; _thumbnailDrawer.ClearCache(); }, labelWidth);

            var hPosition = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_CameraPosX));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_HorizontalPosition, hPosition.floatValue, 0, 1,
                value => { hPosition.floatValue = value; _thumbnailDrawer.ClearCache(); }, labelWidth);

            var vPosition = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_CameraPosY));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_VerticalPosition, vPosition.floatValue, 0, 1,
                value => { vPosition.floatValue = value; _thumbnailDrawer.ClearCache(); }, labelWidth);

            var hAngle = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_CameraAngleH));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_HorizontalAngle, hAngle.floatValue, ThumbnailSetting.MaxCameraAngleH * -1, ThumbnailSetting.MaxCameraAngleH,
                value => { hAngle.floatValue = value; _thumbnailDrawer.ClearCache(); }, labelWidth);

            var vAngle = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_CameraAngleV));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_VerticalAngle, vAngle.floatValue, ThumbnailSetting.MaxCameraAngleV * -1, ThumbnailSetting.MaxCameraAngleV,
                value => { vAngle.floatValue = value; _thumbnailDrawer.ClearCache(); }, labelWidth);

            EditorGUILayout.Space(10);

            // Draw reset button
            if (GUILayout.Button(_localizationTable.InspectorView_Thumbnail_Reset))
            {
                distance.floatValue = 0.1f;
                hPosition.floatValue = 0.5f;
                vPosition.floatValue = 0.5f;
                hAngle.floatValue = 0;
                vAngle.floatValue = 0;
                _thumbnailDrawer.ClearCache();
            }
        }

        private void Field_Slider(string labelText, float value, float minValue, float maxValue, Action<float> onValueChanged, float labelWidth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Label
                GUIContent labelContent = new GUIContent(labelText);
                Rect labelRect = GUILayoutUtility.GetRect(labelContent, GUI.skin.label, GUILayout.Width(labelWidth));
                GUI.Label(labelRect, labelContent);

                // Slider
                Rect sliderRect = GUILayoutUtility.GetRect(labelRect.x, labelRect.y, GUILayout.ExpandWidth(true));
                var sliderValue = GUI.HorizontalSlider(sliderRect, value, minValue, maxValue);
                if (!Mathf.Approximately(sliderValue, value))
                {
                    onValueChanged(sliderValue);
                }

                // DelayedFloatField
                var fieldValue = EditorGUILayout.DelayedFloatField(value, GUILayout.Width(80));
                if (!Mathf.Approximately(fieldValue, value))
                {
                    onValueChanged(fieldValue);
                }
            }
        }

        private void Field_ExpressionsMenuSettingItems()
        {
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_EmoteLock)),       _localizationTable.InspectorView_AddConfig_EmoteLock);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_BlinkOff)),        _localizationTable.InspectorView_AddConfig_BlinkOff);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_DanceGimmick)),    _localizationTable.InspectorView_AddConfig_DanceGimmick);
            if (_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_EmoteLock)).boolValue)
            {
                TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_ContactLock)), _localizationTable.InspectorView_AddConfig_ContactLock);
            }
            else
            {
                ViewUtility.LayoutDummyToggle(_localizationTable.InspectorView_AddConfig_ContactLock);
            }
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_Override)),        _localizationTable.InspectorView_AddConfig_Override);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_Voice)),        _localizationTable.InspectorView_AddConfig_Voice);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_HandPattern)),    _localizationTable.InspectorView_AddConfig_HandPattern);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_Controller)),      _localizationTable.InspectorView_AddConfig_Controller);
        }

        private void Field_AvatarApplicationSetting()
        {
            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.TransitionDurationSeconds)), new GUIContent(_localizationTable.InspectorView_TransitionDuration));
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.GenerateModeThumbnails)), _localizationTable.InspectorView_GenerateModeThumbnails);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.SmoothAnalogFist)), _localizationTable.InspectorView_SmoothAnalogFist);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddParameterPrefix)), _localizationTable.InspectorView_AddExpressionParameterPrefix);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ReplaceBlink)), _localizationTable.InspectorView_ReplaceBlink);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.DisableTrackingControls)), _localizationTable.InspectorView_DisableTrackingControls);
        }

        private void Field_EditorSetting()
        {
            ToggleEditorPrefsField(DetailConstants.KeyGroupDeleteConfirmation, DetailConstants.DefaultGroupDeleteConfirmation, _localizationTable.InspectorView_GroupDeleteConfirmation);
            ToggleEditorPrefsField(DetailConstants.KeyModeDeleteConfirmation, DetailConstants.DefaultModeDeleteConfirmation, _localizationTable.InspectorView_ModeDeleteConfirmation);
            ToggleEditorPrefsField(DetailConstants.KeyBranchDeleteConfirmation, DetailConstants.DefaultBranchDeleteConfirmation, _localizationTable.InspectorView_BranchDeleteConfirmation);
        }

        private void Field_Help()
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(rect, _localizationTable.InspectorView_Help_Manual, EditorStyles.linkLabel);

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                var url = "https://suzuryg.github.io/facial-expression-switcher/";
                if (_localizationSetting.Locale == Locale.ja_JP)
                {
                    url += "jp/";
                }
                Application.OpenURL(url);
            }
        }

        private static void TogglePropertyField(SerializedProperty serializedProperty, string label)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var value = EditorGUILayout.Toggle(string.Empty, serializedProperty.boolValue, GUILayout.Width(ToggleWidth));
                if (value != serializedProperty.boolValue)
                {
                    serializedProperty.boolValue = value;
                }
                GUILayout.Label(label);
            }
        }

        private static void ToggleEditorPrefsField(string key, bool defaultValue, string label)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var oldValue = EditorPrefs.HasKey(key) ? EditorPrefs.GetBool(key) : defaultValue;
                var newValue = EditorGUILayout.Toggle(string.Empty, oldValue, GUILayout.Width(ToggleWidth));
                if (newValue != oldValue)
                {
                    EditorPrefs.SetBool(key, newValue);
                }
                GUILayout.Label(label);
            }
        }
    }
}
