using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.Subject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniRx;
using UnityEditorInternal;
using System.Linq;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using UnityEditor.IMGUI.Controls;
using Suzuryg.FacialExpressionSwitcher.Detail.View.Element;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class InspectorView : IDisposable
    {
        public IObservable<Unit> OnLaunchButtonClicked => _onLaunchButtonClicked.AsObservable();
        public IObservable<Locale> OnLocaleChanged => _onLocaleChanged.AsObservable();

        private Subject<Unit> _onLaunchButtonClicked = new Subject<Unit>();
        private Subject<Locale> _onLocaleChanged = new Subject<Locale>();

        private IMenuRepository _menuRepository; // TODO: Use use-case instead
        private ILocalizationSetting _localizationSetting;
        private SerializedObject _av3Setting;
        private UpdateMenuSubject _updateMenuSubject;

        private IMenu _menu;

        private bool _isMouthMorphBlendShapesOpened = false;
        private ReorderableList _mouthMorphBlendShapes;
        private string[] _faceBlendShapes = new string[0];

        private bool _isMenuPropertiesSettingOpened = false;

        private string _mouthMorphBlendShapesText;
        private string _addText;
        private string _cancelText;
        private string _menuPropertiesText;
        private string _smoothAnalogFistText;
        private string _transitionDurationSecondsText;
        private string _replaceBlinkText;
        private string _disableTrackingControlsText;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public InspectorView(
            IMenuRepository menuRepository,
            ILocalizationSetting localizationSetting,
            AV3Setting av3Setting,
            UpdateMenuSubject updateMenuSubject)
        {
            _menuRepository = menuRepository;
            _localizationSetting = localizationSetting;
            _av3Setting = new SerializedObject(av3Setting);
            _updateMenuSubject = updateMenuSubject;

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(OnMenuUpdated).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Mouth morph blendshapes
            _mouthMorphBlendShapes = new ReorderableList(null, typeof(string));
            _mouthMorphBlendShapes.onAddCallback = AddMouthMorphBlendShape;
            _mouthMorphBlendShapes.onRemoveCallback = RemoveMouthMorphBlendShape;
            _mouthMorphBlendShapes.draggable = false;
            _mouthMorphBlendShapes.headerHeight = 0;

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

            _isMouthMorphBlendShapesOpened = EditorGUILayout.Foldout(_isMouthMorphBlendShapesOpened, _mouthMorphBlendShapesText);
            if (_isMouthMorphBlendShapesOpened)
            {
                _mouthMorphBlendShapes.DoLayoutList();
            }

            _isMenuPropertiesSettingOpened = EditorGUILayout.Foldout(_isMenuPropertiesSettingOpened, _menuPropertiesText);
            if (_isMenuPropertiesSettingOpened)
            {
                MenuPropertiesGUI();
            }
        }

        private void SetText(LocalizationTable localizationTable)
        {
            if (localizationTable.HierarchyView_Title == "HierarchyView")
            {
                _mouthMorphBlendShapesText = "MouthMorphBlendShapes";
                _addText = "Add";
                _cancelText = "Cancel";
                _menuPropertiesText = "Menu Properties";
                _smoothAnalogFistText = "Smooth Analog Fist";
                _transitionDurationSecondsText = "Transition Duration (sec)";
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
                _replaceBlinkText = "ビルド時にまばたきをアニメーションに置き換える（推奨）";
                _disableTrackingControlsText = "ビルド時に目と口のVRCTrackingControlを無効にする（推奨）";
            }
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _menu = menu;
            _mouthMorphBlendShapes.list = _menu?.MouthMorphBlendShapes.ToList();
            if (_menu.Avatar is Domain.Avatar)
            {
                _faceBlendShapes = AV3Utility.GetFaceMeshBlendShapes(AV3Utility.GetAvatarDescriptor(_menu.Avatar)).Select(x => x.name).ToArray();
            }
        }

        private void AddMouthMorphBlendShape(ReorderableList reorderableList)
        {
            UnityEditor.PopupWindow.Show(
                new Rect(Event.current.mousePosition, Vector2.one),
                new ListSelectPopupContent<string>(_faceBlendShapes, _addText, _cancelText,
                new Action<IReadOnlyList<string>>(blendShapes =>
                {
                    // TODO: Add use-case
                    var menu = _menuRepository.Load(null);
                    foreach (var blendShape in blendShapes)
                    {
                        menu.AddMouthMorphBlendShape(blendShape);
                    }
                    _menuRepository.Save(string.Empty, menu, "AddMouthMorphBlendShapes");
                    _updateMenuSubject.OnNext(menu);
                })));
        }

        private void RemoveMouthMorphBlendShape(ReorderableList reorderableList)
        {
            if (_mouthMorphBlendShapes.index >= _mouthMorphBlendShapes.list.Count)
            {
                return;
            }

            var selected = _mouthMorphBlendShapes.list[_mouthMorphBlendShapes.index] as string;

            // TODO: Add use-case
            var menu = _menuRepository.Load(null);
            menu.RemoveMouthMorphBlendShape(selected);
            _menuRepository.Save(string.Empty, menu, "RemoveMouthMorphBlendShapes");
            _updateMenuSubject.OnNext(menu);
        }

        private void MenuPropertiesGUI()
        {
            _av3Setting.Update();
            using (new EditorGUILayout.VerticalScope())
            {
                TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.SmoothAnalogFist)), _smoothAnalogFistText);
                EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.TransitionDurationSeconds)), new GUIContent(_transitionDurationSecondsText));
                TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ReplaceBlink)), _replaceBlinkText);
                TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.DisableTrackingControls)), _disableTrackingControlsText);
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
