using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class SettingView : IDisposable
    {
        private IGenerateFxUseCase _generateFxUseCase;

        private IGenerateFxPresenter _generateFxPresenter;

        private ISubWindowProvider _subWindowProvider;
        private ILocalizationSetting _localizationSetting;
        private ThumbnailDrawer _thumbnailDrawer;

        private Button _openGestureTableWindowButton;
        private Button _updateThumbnailButton;
        private SliderInt _thumbnailSizeSlider;
        private Button _generateButton;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public SettingView(
            IGenerateFxUseCase generateFxUseCase,
            IGenerateFxPresenter generateFxPresenter,
            ISubWindowProvider subWindowProvider,
            ILocalizationSetting localizationSetting,
            ThumbnailDrawer thumbnailDrawer)
        {
            // Usecases
            _generateFxUseCase = generateFxUseCase;

            // Presenters
            _generateFxPresenter = generateFxPresenter;

            // Others
            _subWindowProvider = subWindowProvider;
            _localizationSetting = localizationSetting;
            _thumbnailDrawer = thumbnailDrawer;

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Presenter event handlers
            _generateFxPresenter.Observable.Synchronize().Subscribe(OnGenerateFxPresenterCompleted).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _thumbnailSizeSlider.UnregisterValueChangedCallback(OnThumbnailSizeChanged);
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
            _thumbnailSizeSlider = root.Q<SliderInt>("ThumbnailSizeSlider");
            _generateButton = root.Q<Button>("GenerateButton");
            NullChecker.Check(_openGestureTableWindowButton, _updateThumbnailButton, _thumbnailSizeSlider, _generateButton);

            // Initialize fields
            _thumbnailSizeSlider.lowValue = DetailConstants.MinMainThumbnailSize;
            _thumbnailSizeSlider.highValue = DetailConstants.MaxMainThumbnailSize;
            _thumbnailSizeSlider.value = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailSize) : DetailConstants.MinMainThumbnailSize;

            // Add event handlers
            _thumbnailSizeSlider.RegisterValueChangedCallback(OnThumbnailSizeChanged);
            Observable.FromEvent(x => _openGestureTableWindowButton.clicked += x, x => _openGestureTableWindowButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnOpenGestureTableWindowButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _updateThumbnailButton.clicked += x, x => _updateThumbnailButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnUpdateThumbnailButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _generateButton.clicked += x, x => _generateButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnGenerateButtonClicked()).AddTo(_disposables);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _openGestureTableWindowButton.text = "Open Gesuture Table Window";
            _updateThumbnailButton.text = localizationTable.MainView_UpdateThumbnails;
            _generateButton.text = "Generate";
        }

        private void OnThumbnailSizeChanged(ChangeEvent<int> changeEvent)
        {
            EditorPrefs.SetInt(DetailConstants.KeyMainThumbnailSize, changeEvent.newValue);
            _thumbnailDrawer.UpdateAll();
        }

        private void OnOpenGestureTableWindowButtonClicked()
        {
            _subWindowProvider.Open<GestureTableWindow>();
        }

        private void OnUpdateThumbnailButtonClicked()
        {
            _thumbnailDrawer.UpdateAll();
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
    }
}
