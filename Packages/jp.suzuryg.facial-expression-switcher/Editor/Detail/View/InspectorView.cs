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

        private bool _isMouthMorphBlendShapesOpened = false;
        private ReorderableList _mouthMorphBlendShapes;
        private ReorderableList _additionalToggleObjects;
        private ReorderableList _additionalTransformObjects;

        private bool _isMenuPropertiesSettingOpened = false;

        private string _mouthMorphBlendShapesText;
        private string _addText;
        private string _cancelText;
        private string _menuPropertiesText;
        private string _smoothAnalogFistText;
        private string _transitionDurationSecondsText;
        private string _replaceBlinkText;
        private string _disableTrackingControlsText;
        private string _doNotTransitionWhenSpeakingText;

        private LocalizationTable _localizationTable;

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
            _additionalToggleObjects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _additionalToggleObjects.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };

            _additionalTransformObjects = new ReorderableList(_av3Setting, _av3Setting.FindProperty(nameof(AV3Setting.AdditionalTransformObjects)));
            _additionalTransformObjects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _additionalTransformObjects.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };

            // Set text
            SetText(_localizationSetting.Table);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void OnGUI()
        {
            if (GUILayout.Button($"Launch {DomainConstants.SystemName}"))
            {
                _onLaunchButtonClicked.OnNext(Unit.Default);
            }

            var oldLocale = _localizationSetting.Locale;
            var newLocale = (Locale)EditorGUILayout.EnumPopup(oldLocale);
            if (newLocale != oldLocale)
            {
                _localizationSetting.SetLocale(newLocale);
                _onLocaleChanged.OnNext(newLocale);
                SetText(_localizationSetting.Table);
            }

            _isMenuPropertiesSettingOpened = EditorGUILayout.Foldout(_isMenuPropertiesSettingOpened, _menuPropertiesText);
            if (_isMenuPropertiesSettingOpened)
            {
                MenuPropertiesGUI();
            }

            _isMouthMorphBlendShapesOpened = EditorGUILayout.Foldout(_isMouthMorphBlendShapesOpened, _mouthMorphBlendShapesText);
            if (_isMouthMorphBlendShapesOpened)
            {
                _mouthMorphBlendShapes.list = GetMouthMorphBlendShapes(); // Is it necessary to get every frame?
                _mouthMorphBlendShapes.DoLayoutList();
            }
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;

            if (localizationTable.HierarchyView_Title == "HierarchyView")
            {
                _mouthMorphBlendShapesText = "MouthMorphBlendShapes";
                _addText = "Add";
                _cancelText = "Cancel";
                _menuPropertiesText = "Menu Properties";
                _smoothAnalogFistText = "Smooth Analog Fist";
                _transitionDurationSecondsText = "Transition Duration (sec)";
                _doNotTransitionWhenSpeakingText = "Do Not Transition When Speaking.";
                _replaceBlinkText = "Replace blink with animation at build time (recommended)";
                _disableTrackingControlsText = "Disable VRCTrackingControls for eyes and mouth at build time (recommended)";
            }
            else
            {
                _mouthMorphBlendShapesText = "口変形キャンセル用シェイプキー";
                _addText = "追加";
                _cancelText = "キャンセル";
                _menuPropertiesText = "メニュー設定";
                _smoothAnalogFistText = "アナログ値のスムージング";
                _transitionDurationSecondsText = "遷移時間（秒）";
                _doNotTransitionWhenSpeakingText = "発話中は表情遷移しない";
                _replaceBlinkText = "ビルド時にまばたきをアニメーションに置き換える（推奨）";
                _disableTrackingControlsText = "ビルド時に目と口のVRCTrackingControlを無効にする（推奨）";
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
                new ListSelectPopupContent<string>(faceBlendShapes, _addText, _cancelText,
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

        private void MenuPropertiesGUI()
        {
            _av3Setting.Update();
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar)), new GUIContent(_localizationTable.InspectorView_TargetAvatar));
                EditorGUILayout.Space();

                TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.SmoothAnalogFist)), _smoothAnalogFistText);
                EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.TransitionDurationSeconds)), new GUIContent(_transitionDurationSecondsText));
                TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.DoNotTransitionWhenSpeaking)), _doNotTransitionWhenSpeakingText);
                TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.GenerateModeThumbnails)), _localizationTable.InspectorView_GenerateModeThumbnails);
                TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ReplaceBlink)), _replaceBlinkText);
                TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.DisableTrackingControls)), _disableTrackingControlsText);

                EditorGUILayout.Space();
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

                EditorGUILayout.Space();

                _additionalToggleObjects.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, _localizationTable.Common_AddtionalToggleObjects);
                };
                _additionalToggleObjects.DoLayoutList();

                _additionalTransformObjects.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, _localizationTable.Common_AddtionalTransformObjects);
                };
                _additionalTransformObjects.DoLayoutList();
            }
            _av3Setting.ApplyModifiedProperties();
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
