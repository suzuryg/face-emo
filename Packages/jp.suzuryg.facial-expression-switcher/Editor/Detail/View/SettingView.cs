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

        private ISubWindowProvider _subWindowProvider;
        private ModeNameProvider _modeNameProvider;
        private UpdateMenuSubject _updateMenuSubject;
        private MainThumbnailDrawer _thumbnailDrawer;
        private SerializedObject _thumbnailSetting;

        private Button _openGestureTableWindowButton;
        private Button _updateThumbnailButton;
        private SliderInt _thumbnailWidthSlider;
        private SliderInt _thumbnailHeightSlider;
        private Button _generateButton;
        private IMGUIContainer _defaultSelectionComboBoxArea;
        private Toggle _groupDeleteConfirmation;
        private Toggle _modeDeleteConfirmation;
        private Toggle _branchDeleteConfirmation;

        private List<ModeEx> _flattendModes = new List<ModeEx>();
        private string[] _modePaths = new string[0];
        private int _defaultSelection = 0;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public SettingView(
            IModifyMenuPropertiesUseCase modifyMenuPropertiesUseCase,
            IGenerateFxUseCase generateFxUseCase,
            IGenerateFxPresenter generateFxPresenter,
            ISubWindowProvider subWindowProvider,
            ILocalizationSetting localizationSetting,
            ModeNameProvider modeNameProvider,
            UpdateMenuSubject updateMenuSubject,
            MainThumbnailDrawer thumbnailDrawer,
            ThumbnailSetting thumbnailSetting)
        {
            // Usecases
            _modifyMenuPropertiesUseCase = modifyMenuPropertiesUseCase;
            _generateFxUseCase = generateFxUseCase;

            // Presenters
            _generateFxPresenter = generateFxPresenter;

            // Others
            _subWindowProvider = subWindowProvider;
            _localizationSetting = localizationSetting;
            _modeNameProvider = modeNameProvider;
            _updateMenuSubject = updateMenuSubject;
            _thumbnailDrawer = thumbnailDrawer;
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
            _thumbnailWidthSlider.UnregisterValueChangedCallback(OnThumbnailSizeChanged);
            _thumbnailHeightSlider.UnregisterValueChangedCallback(OnThumbnailSizeChanged);

            _groupDeleteConfirmation.UnregisterValueChangedCallback(OnGroupDeleteConfirmationChanged);
            _modeDeleteConfirmation.UnregisterValueChangedCallback(OnModeDeleteConfirmationChanged);
            _branchDeleteConfirmation.UnregisterValueChangedCallback(OnBranchDeleteConfirmationChanged);

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
            _openGestureTableWindowButton = root.Q<Button>("OpenGestureTableWindowButton");
            _updateThumbnailButton = root.Q<Button>("UpdateThumbnailButton");
            _thumbnailWidthSlider = root.Q<SliderInt>("ThumbnailWidthSlider");
            _thumbnailHeightSlider = root.Q<SliderInt>("ThumbnailHeightSlider");
            _generateButton = root.Q<Button>("GenerateButton");
            _defaultSelectionComboBoxArea = root.Q<IMGUIContainer>("DefaultSelectionComboBox");
            _groupDeleteConfirmation = root.Q<Toggle>("GroupDeleteConfirmation");
            _modeDeleteConfirmation = root.Q<Toggle>("ModeDeleteConfirmation");
            _branchDeleteConfirmation = root.Q<Toggle>("BranchDeleteConfirmation");
            NullChecker.Check(_openGestureTableWindowButton, _updateThumbnailButton, _thumbnailWidthSlider, _thumbnailHeightSlider, _generateButton, _defaultSelectionComboBoxArea,
                _groupDeleteConfirmation, _modeDeleteConfirmation, _branchDeleteConfirmation);

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

            _groupDeleteConfirmation.value = EditorPrefs.HasKey(DetailConstants.KeyGroupDeleteConfirmation) ? EditorPrefs.GetBool(DetailConstants.KeyGroupDeleteConfirmation) : DetailConstants.DefaultGroupDeleteConfirmation;
            _modeDeleteConfirmation.value = EditorPrefs.HasKey(DetailConstants.KeyModeDeleteConfirmation) ? EditorPrefs.GetBool(DetailConstants.KeyModeDeleteConfirmation) : DetailConstants.DefaultModeDeleteConfirmation;
            _branchDeleteConfirmation.value = EditorPrefs.HasKey(DetailConstants.KeyBranchDeleteConfirmation) ? EditorPrefs.GetBool(DetailConstants.KeyBranchDeleteConfirmation) : DetailConstants.DefaultBranchDeleteConfirmation;

            // Add event handlers
            _thumbnailWidthSlider.RegisterValueChangedCallback(OnThumbnailSizeChanged);
            _thumbnailHeightSlider.RegisterValueChangedCallback(OnThumbnailSizeChanged);

            _groupDeleteConfirmation.RegisterValueChangedCallback(OnGroupDeleteConfirmationChanged);
            _modeDeleteConfirmation.RegisterValueChangedCallback(OnModeDeleteConfirmationChanged);
            _branchDeleteConfirmation.RegisterValueChangedCallback(OnBranchDeleteConfirmationChanged);

            Observable.FromEvent(x => _openGestureTableWindowButton.clicked += x, x => _openGestureTableWindowButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnOpenGestureTableWindowButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _updateThumbnailButton.clicked += x, x => _updateThumbnailButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnUpdateThumbnailButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _generateButton.clicked += x, x => _generateButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnGenerateButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _defaultSelectionComboBoxArea.onGUIHandler += x, x => _defaultSelectionComboBoxArea.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => DefaultSelectionComboBoxOnGUI()).AddTo(_disposables);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;

            _openGestureTableWindowButton.text = "Open Gesuture Table Window";
            _updateThumbnailButton.text = localizationTable.MainView_UpdateThumbnails;
            _generateButton.text = "Generate";

            _groupDeleteConfirmation.text = _localizationTable.SettingView_GroupDeleteConfirmation;
            _modeDeleteConfirmation.text = _localizationTable.SettingView_ModeDeleteConfirmation;
            _branchDeleteConfirmation.text = _localizationTable.SettingView_BranchDeleteConfirmation;
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _flattendModes = AV3Utility.FlattenMenuItemList(menu.Registered, _modeNameProvider);
            _modePaths = _flattendModes.Select(x => x.PathToMode).ToArray();

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

        private void OnGroupDeleteConfirmationChanged(ChangeEvent<bool> changeEvent) => EditorPrefs.SetBool(DetailConstants.KeyGroupDeleteConfirmation, changeEvent.newValue);

        private void OnModeDeleteConfirmationChanged(ChangeEvent<bool> changeEvent) => EditorPrefs.SetBool(DetailConstants.KeyModeDeleteConfirmation, changeEvent.newValue);

        private void OnBranchDeleteConfirmationChanged(ChangeEvent<bool> changeEvent) => EditorPrefs.SetBool(DetailConstants.KeyBranchDeleteConfirmation, changeEvent.newValue);

        private void OnThumbnailSizeChanged(ChangeEvent<int> changeEvent)
        {
            // TODO: Reduce unnecessary redrawing
            _thumbnailDrawer.ClearCache();
        }

        private void OnUpdateThumbnailButtonClicked()
        {
            _thumbnailDrawer.ClearCache();
        }

        private void OnOpenGestureTableWindowButtonClicked()
        {
            _subWindowProvider.Open<GestureTableWindow>();
        }

        private void OnGenerateButtonClicked()
        {
            _generateFxUseCase.Handle("");
        }

        private void OnGenerateFxPresenterCompleted(
            (GenerateFxResult generateFxResult, string errorMessage) args)
        {
            if (args.generateFxResult == GenerateFxResult.Succeeded)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"Generation succeeded!", "OK");
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
