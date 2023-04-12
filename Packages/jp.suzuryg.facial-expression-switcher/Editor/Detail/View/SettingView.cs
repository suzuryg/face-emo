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

        private ISubWindowProvider _subWindowProvider;
        private ILocalizationSetting _localizationSetting;
        private UpdateMenuSubject _updateMenuSubject;
        private MainThumbnailDrawer _thumbnailDrawer;

        private Button _openGestureTableWindowButton;
        private Button _updateThumbnailButton;
        private SliderInt _thumbnailWidthSlider;
        private SliderInt _thumbnailHeightSlider;
        private Button _generateButton;
        private IMGUIContainer _defaultSelectionComboBoxArea;

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
            UpdateMenuSubject updateMenuSubject,
            MainThumbnailDrawer thumbnailDrawer)
        {
            // Usecases
            _modifyMenuPropertiesUseCase = modifyMenuPropertiesUseCase;
            _generateFxUseCase = generateFxUseCase;

            // Presenters
            _generateFxPresenter = generateFxPresenter;

            // Others
            _subWindowProvider = subWindowProvider;
            _localizationSetting = localizationSetting;
            _updateMenuSubject = updateMenuSubject;
            _thumbnailDrawer = thumbnailDrawer;

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(OnMenuUpdated).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Presenter event handlers
            _generateFxPresenter.Observable.Synchronize().Subscribe(OnGenerateFxPresenterCompleted).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _thumbnailWidthSlider.UnregisterValueChangedCallback(OnThumbnailWidthChanged);
            _thumbnailHeightSlider.UnregisterValueChangedCallback(OnThumbnailHeightChanged);
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
            NullChecker.Check(_openGestureTableWindowButton, _updateThumbnailButton, _thumbnailWidthSlider, _thumbnailHeightSlider, _generateButton, _defaultSelectionComboBoxArea);

            // Initialize fields
            _thumbnailWidthSlider.lowValue = DetailConstants.MinMainThumbnailWidth;
            _thumbnailWidthSlider.highValue = DetailConstants.MaxMainThumbnailWidth;
            _thumbnailWidthSlider.value = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailWidth) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailWidth) : DetailConstants.DefaultMainThumbnailWidth;

            _thumbnailHeightSlider.lowValue = DetailConstants.MinMainThumbnailHeight;
            _thumbnailHeightSlider.highValue = DetailConstants.MaxMainThumbnailHeight;
            _thumbnailHeightSlider.value = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailHeight) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailHeight) : DetailConstants.DefaultMainThumbnailHeight;

            // Add event handlers
            _thumbnailWidthSlider.RegisterValueChangedCallback(OnThumbnailWidthChanged);
            _thumbnailHeightSlider.RegisterValueChangedCallback(OnThumbnailHeightChanged);
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
            _openGestureTableWindowButton.text = "Open Gesuture Table Window";
            _updateThumbnailButton.text = localizationTable.MainView_UpdateThumbnails;
            _generateButton.text = "Generate";
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _flattendModes = AV3Utility.FlattenMenuItemList(menu.Registered);
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

        private void OnThumbnailWidthChanged(ChangeEvent<int> changeEvent)
        {
            EditorPrefs.SetInt(DetailConstants.KeyMainThumbnailWidth, changeEvent.newValue);
            _thumbnailDrawer.ClearCache();
        }

        private void OnThumbnailHeightChanged(ChangeEvent<int> changeEvent)
        {
            EditorPrefs.SetInt(DetailConstants.KeyMainThumbnailHeight, changeEvent.newValue);
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
