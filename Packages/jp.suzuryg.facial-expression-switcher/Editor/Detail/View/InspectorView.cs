using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
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
        public IObservable<Unit> OnLaunchButtonClicked => _onLaunchButtonClicked.AsObservable();
        public IObservable<Locale> OnLocaleChanged => _onLocaleChanged.AsObservable();

        private Subject<Unit> _onLaunchButtonClicked = new Subject<Unit>();
        private Subject<Locale> _onLocaleChanged = new Subject<Locale>();

        private ILocalizationSetting _localizationSetting;
        private SerializedObject _av3Setting;

        private ReorderableList _mouthMorphBlendShapes;
        private ReorderableList _additionalToggleObjects;
        private ReorderableList _additionalTransformObjects;

        private bool _isMouthMorphBlendShapesOpened = false;
        private bool _isAddtionalToggleOpened = false;
        private bool _isAddtionalTransformOpened = false;
        private bool _isExpressionsMenuItemsOpened = false;
        private bool _isPreferencesOpened = false;

        private LocalizationTable _localizationTable;

        private GUIStyle _warningLabelStyle = new GUIStyle();

        private CompositeDisposable _disposables = new CompositeDisposable();

        public InspectorView(
            ILocalizationSetting localizationSetting,
            AV3Setting av3Setting)
        {
            _localizationSetting = localizationSetting;
            _av3Setting = new SerializedObject(av3Setting);

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
            _warningLabelStyle = new GUIStyle(GUI.skin.label);
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

            // Launch button
            if (GUILayout.Button(_localizationTable.InspectorView_Launch, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3)))
            {
                _onLaunchButtonClicked.OnNext(Unit.Default);
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
                _mouthMorphBlendShapes.list = GetMouthMorphBlendShapes(); // Is it necessary to get every frame?
                _mouthMorphBlendShapes.DoLayoutList();
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

            // Expressions Menu Setting Items
            _isExpressionsMenuItemsOpened = EditorGUILayout.Foldout(_isExpressionsMenuItemsOpened, _localizationTable.InspectorView_ExpressionsMenuSettingItems);
            if (_isExpressionsMenuItemsOpened)
            {
                Field_ExpressionsMenuSettingItems();
            }

            EditorGUILayout.Space(10);

            // Preferences
            _isPreferencesOpened = EditorGUILayout.Foldout(_isPreferencesOpened, _localizationTable.InspectorView_Preferences);
            if (_isPreferencesOpened)
            {
                Field_Preferences();
            }

            _av3Setting.ApplyModifiedProperties();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
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
            if (_av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar)).objectReferenceValue is VRCAvatarDescriptor avatarDescriptor)
            {
                var replaceBlink = _av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar)).boolValue;
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
                if (gameObject is null) { continue; }
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
                if (gameObject is null) { continue; }
                if (string.IsNullOrEmpty(avatarPath) || !gameObject.GetFullPath().StartsWith(avatarPath))
                {
                    EditorGUILayout.LabelField($"{gameObject.name}{_localizationTable.InspectorView_Message_NotInAvatar}", _warningLabelStyle);
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
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_HandPriority)),    _localizationTable.InspectorView_AddConfig_HandPriority);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_Controller)),      _localizationTable.InspectorView_AddConfig_Controller);
        }

        private void Field_Preferences()
        {
            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.TransitionDurationSeconds)), new GUIContent(_localizationTable.InspectorView_TransitionDuration));
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.DoNotTransitionWhenSpeaking)), _localizationTable.InspectorView_DoNotTransitionWhenSpeaking);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.GenerateModeThumbnails)), _localizationTable.InspectorView_GenerateModeThumbnails);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.SmoothAnalogFist)), _localizationTable.InspectorView_SmoothAnalogFist);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ReplaceBlink)), _localizationTable.InspectorView_ReplaceBlink);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.DisableTrackingControls)), _localizationTable.InspectorView_DisableTrackingControls);
        }

        private static void TogglePropertyField(SerializedProperty serializedProperty, string label)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var value = EditorGUILayout.Toggle(string.Empty, serializedProperty.boolValue, GUILayout.Width(15));
                if (value != serializedProperty.boolValue)
                {
                    serializedProperty.boolValue = value;
                }
                GUILayout.Label(label);
            }
        }
    }
}
