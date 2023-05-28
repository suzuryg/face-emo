using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class SettingView : IDisposable
    {
        private IModifyMenuPropertiesUseCase _modifyMenuPropertiesUseCase;
        private IGenerateFxUseCase _generateFxUseCase;

        private IGenerateFxPresenter _generateFxPresenter;

        private ILocalizationSetting _localizationSetting;
        private LocalizationTable _localizationTable;

        private ModeNameProvider _modeNameProvider;
        private UpdateMenuSubject _updateMenuSubject;
        private MainThumbnailDrawer _thumbnailDrawer;
        private GestureTableThumbnailDrawer _gestureTableThumbnailDrawer;
        private SerializedObject _thumbnailSetting;

        private Label _thumbnailWidthLabel;
        private Label _thumbnailHeightLabel;
        private SliderInt _thumbnailWidthSlider;
        private SliderInt _thumbnailHeightSlider;
        private Button _updateThumbnailButton;
        private Label _defaultSelectionLabel;
        private IMGUIContainer _defaultSelectionComboBoxArea;
        private Toggle _showHintsToggle;
        private Button _applyButton;

        private List<ModeEx> _flattendModes = new List<ModeEx>();
        private string[] _modePaths = new string[0];
        private int _defaultSelection = 0;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public SettingView(
            IModifyMenuPropertiesUseCase modifyMenuPropertiesUseCase,
            IGenerateFxUseCase generateFxUseCase,
            IGenerateFxPresenter generateFxPresenter,
            ILocalizationSetting localizationSetting,
            ModeNameProvider modeNameProvider,
            UpdateMenuSubject updateMenuSubject,
            MainThumbnailDrawer thumbnailDrawer,
            GestureTableThumbnailDrawer gestureTableThumbnailDrawer,
            ThumbnailSetting thumbnailSetting)
        {
            // Usecases
            _modifyMenuPropertiesUseCase = modifyMenuPropertiesUseCase;
            _generateFxUseCase = generateFxUseCase;

            // Presenters
            _generateFxPresenter = generateFxPresenter;

            // Others
            _localizationSetting = localizationSetting;
            _modeNameProvider = modeNameProvider;
            _updateMenuSubject = updateMenuSubject;
            _thumbnailDrawer = thumbnailDrawer;
            _gestureTableThumbnailDrawer = gestureTableThumbnailDrawer;
            _thumbnailSetting = new SerializedObject(thumbnailSetting);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(OnMenuUpdated).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Presenter event handlers
            _generateFxPresenter.Observable.Synchronize().Subscribe(OnGenerateFxPresenterCompleted).AddTo(_disposables);
        }

        public void Dispose()
        {
            _thumbnailWidthSlider.UnregisterValueChangedCallback(OnThumbnailSettingChanged);
            _thumbnailHeightSlider.UnregisterValueChangedCallback(OnThumbnailSettingChanged);
            _showHintsToggle.UnregisterValueChangedCallback(OnShowHintsValueChanged);

            _disposables.Dispose();
        }

        public void Initialize(VisualElement root)
        {
            // Load UXML and style
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(SettingView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(SettingView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            // Query Elements
            _thumbnailWidthLabel = root.Q<Label>("ThumbnailWidthLabel");
            _thumbnailHeightLabel = root.Q<Label>("ThumbnailHeightLabel");
            _thumbnailWidthSlider = root.Q<SliderInt>("ThumbnailWidthSlider");
            _thumbnailHeightSlider = root.Q<SliderInt>("ThumbnailHeightSlider");
            _updateThumbnailButton = root.Q<Button>("UpdateThumbnailButton");
            _defaultSelectionLabel = root.Q<Label>("DefaultSelectionLabel");
            _defaultSelectionComboBoxArea = root.Q<IMGUIContainer>("DefaultSelectionComboBox");
            _showHintsToggle = root.Q<Toggle>("ShowHintsToggle");
            _applyButton = root.Q<Button>("ApplyButton");
            NullChecker.Check(_thumbnailWidthLabel, _thumbnailHeightLabel, _thumbnailWidthSlider, _thumbnailHeightSlider, _updateThumbnailButton,
               _defaultSelectionLabel, _defaultSelectionComboBoxArea, _showHintsToggle, _applyButton);

            // Initialize fields
            _thumbnailSetting.Update();

            _thumbnailWidthSlider.bindingPath = nameof(ThumbnailSetting.Main_Width);
            _thumbnailWidthSlider.BindProperty(_thumbnailSetting);
            _thumbnailWidthSlider.lowValue = ThumbnailSetting.Main_MinWidth;
            _thumbnailWidthSlider.highValue = ThumbnailSetting.Main_MaxWidth;
            _thumbnailWidthSlider.value = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_Width)).intValue;

            _thumbnailHeightSlider.bindingPath = nameof(ThumbnailSetting.Main_Height);
            _thumbnailHeightSlider.BindProperty(_thumbnailSetting);
            _thumbnailHeightSlider.lowValue = ThumbnailSetting.Main_MinHeight;
            _thumbnailHeightSlider.highValue = ThumbnailSetting.Main_MaxHeight;
            _thumbnailHeightSlider.value = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_Height)).intValue;

            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            _showHintsToggle.value = showHints;

            // Add event handlers
            _thumbnailWidthSlider.RegisterValueChangedCallback(OnThumbnailSettingChanged);
            _thumbnailHeightSlider.RegisterValueChangedCallback(OnThumbnailSettingChanged);
            _showHintsToggle.RegisterValueChangedCallback(OnShowHintsValueChanged);

            Observable.FromEvent(x => _updateThumbnailButton.clicked += x, x => _updateThumbnailButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnUpdateThumbnailButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _applyButton.clicked += x, x => _applyButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnApplyButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _defaultSelectionComboBoxArea.onGUIHandler += x, x => _defaultSelectionComboBoxArea.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => DefaultSelectionComboBoxOnGUI()).AddTo(_disposables);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;

            if (_thumbnailWidthLabel != null) { _thumbnailWidthLabel.text = localizationTable.Common_ThumbnailWidth; }
            if (_thumbnailHeightLabel != null) { _thumbnailHeightLabel.text = localizationTable.Common_ThumbnailHeight; }
            if (_updateThumbnailButton != null) { _updateThumbnailButton.text = localizationTable.SettingView_UpdateThumbnails; }

            if (_defaultSelectionLabel != null) { _defaultSelectionLabel.text = localizationTable.SettingView_DefaultSelectedMode; }

            if (_showHintsToggle != null) { _showHintsToggle.text = localizationTable.SettingView_ShowHints; }

            if (_applyButton != null) { _applyButton.text = localizationTable.SettingView_ApplyToAvatar; }
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _flattendModes = AV3Utility.FlattenMenuItemList(menu.Registered, _modeNameProvider);
            _modePaths = _flattendModes.Select(x => x.PathToMode).ToArray();

            // Rename to avoid name duplication (for Popup)
            var used = new HashSet<string>();
            for (int i = 0; i < _modePaths.Length; i++)
            {
                while (used.Contains(_modePaths[i])) { _modePaths[i] = AddNumberToName(_modePaths[i]); }
                used.Add(_modePaths[i]);
            }

            // Update default selection
            _defaultSelection = 0;
            for (int i = 0; i < _flattendModes.Count; i++)
            {
                if (menu.DefaultSelection == _flattendModes[i].Mode.GetId())
                {
                    _defaultSelection = i;
                    break;
                }
            }
        }   

        private string AddNumberToName(string input)
        {
            const string pattern = @"\((\d+)\)$";
            var match = Regex.Match(input, pattern);
            if (match.Success)
            {
                int currentNumber = int.Parse(match.Groups[1].Value);
                int nextNumber = currentNumber + 1;
                return Regex.Replace(input, pattern, $"({nextNumber})");
            }
            else
            {
                return input + "(1)";
            }
        }

        private void OnThumbnailSettingChanged<T>(ChangeEvent<T> changeEvent)
        {
            // TODO: Reduce unnecessary redrawing
            _thumbnailDrawer.ClearCache();
        }

        private void OnUpdateThumbnailButtonClicked()
        {
            _thumbnailDrawer.ClearCache();
            _gestureTableThumbnailDrawer.ClearCache();
        }

        private void OnShowHintsValueChanged(ChangeEvent<bool> changeEvent)
        {
            EditorPrefs.SetBool(DetailConstants.KeyShowHints, changeEvent.newValue);
        }

        private void OnApplyButtonClicked()
        {
            if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_NotPossibleInPlayMode, "OK"); return; }

            if (EditorUtility.DisplayDialog(DomainConstants.SystemName,
                _localizationTable.SettingView_Message_ConfirmApplyToAvatar,
                _localizationTable.Common_Yes, _localizationTable.Common_No))
            {
                _generateFxUseCase.Handle("");
            }
        }

        private void OnGenerateFxPresenterCompleted(
            (GenerateFxResult generateFxResult, string errorMessage) args)
        {
            if (args.generateFxResult == GenerateFxResult.Succeeded)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, LocalizationSetting.InsertLineBreak(_localizationTable.SettingView_Message_Succeeded), "OK");
            }
        }

        private void DefaultSelectionComboBoxOnGUI()
        {
            var selected = EditorGUILayout.Popup(_defaultSelection, _modePaths);
            if (selected != _defaultSelection &&
                selected < _flattendModes.Count)
            {
                _modifyMenuPropertiesUseCase.Handle(string.Empty, _flattendModes[selected].Mode.GetId());
            }
        }
    }
}
