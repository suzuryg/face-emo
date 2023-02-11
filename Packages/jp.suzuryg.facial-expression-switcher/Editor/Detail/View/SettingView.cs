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
using Suzuryg.FacialExpressionSwitcher.Detail.Subject;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class SettingView : IDisposable
    {
        private IModifyMenuPropertiesUseCase _modifyMenuPropertiesUseCase;
        private IGenerateFxUseCase _generateFxUseCase;

        private IModifyMenuPropertiesPresenter _modifyMenuPropertiesPresenter;
        private IGenerateFxPresenter _generateFxPresenter;

        private ILocalizationSetting _localizationSetting;
        private UpdateMenuSubject _updateMenuSubject;
        private ThumbnailDrawer _thumbnailDrawer;

        private ObjectField _avatarField;
        private Button _updateThumbnailButton;
        private SliderInt _thumbnailSizeSlider;
        private Button _generateButton;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public SettingView(
            IModifyMenuPropertiesUseCase modifyMenuPropertiesUseCase,
            IGenerateFxUseCase generateFxUseCase,

            IModifyMenuPropertiesPresenter modifyMenuPropertiesPresenter,
            IGenerateFxPresenter generateFxPresenter,

            ILocalizationSetting localizationSetting,
            UpdateMenuSubject updateMenuSubject,
            ThumbnailDrawer thumbnailDrawer)
        {
            // Usecases
            _modifyMenuPropertiesUseCase = modifyMenuPropertiesUseCase;
            _generateFxUseCase = generateFxUseCase;

            // Presenters
            _modifyMenuPropertiesPresenter = modifyMenuPropertiesPresenter;
            _generateFxPresenter = generateFxPresenter;

            // Others
            _localizationSetting = localizationSetting;
            _updateMenuSubject = updateMenuSubject;
            _thumbnailDrawer = thumbnailDrawer;

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(OnMenuUpdated).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Presenter event handlers
            _modifyMenuPropertiesPresenter.Observable.Synchronize().Subscribe(OnModifyMenuPropertiesPresenterCompleted).AddTo(_disposables);
            _generateFxPresenter.Observable.Synchronize().Subscribe(OnGenerateFxPresenterCompleted).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _avatarField.UnregisterValueChangedCallback(OnAvatarChanged);
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
            _avatarField = root.Q<ObjectField>("AvatarField");
            _updateThumbnailButton = root.Q<Button>("UpdateThumbnailButton");
            _thumbnailSizeSlider = root.Q<SliderInt>("ThumbnailSizeSlider");
            _generateButton = root.Q<Button>("GenerateButton");
            NullChecker.Check(_avatarField, _updateThumbnailButton, _thumbnailSizeSlider, _generateButton);

            // Initialize fields
            _avatarField.objectType = typeof(Animator);
            _thumbnailSizeSlider.lowValue = DetailConstants.MinMainThumbnailSize;
            _thumbnailSizeSlider.highValue = DetailConstants.MaxMainThumbnailSize;
            _thumbnailSizeSlider.value = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailSize) : DetailConstants.MinMainThumbnailSize;

            // Add event handlers
            _avatarField.RegisterValueChangedCallback(OnAvatarChanged);
            _thumbnailSizeSlider.RegisterValueChangedCallback(OnThumbnailSizeChanged);
            Observable.FromEvent(x => _updateThumbnailButton.clicked += x, x => _updateThumbnailButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnUpdateThumbnailButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _generateButton.clicked += x, x => _generateButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnGenerateButtonClicked()).AddTo(_disposables);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _updateThumbnailButton.text = localizationTable.MainView_UpdateThumbnails;
            _generateButton.text = "Generate";
        }

        private void SetAvatar(Domain.Avatar avatar)
        {
            if (avatar is Domain.Avatar &&
                GameObject.Find(avatar.Path) is GameObject gameObject &&
                gameObject.GetComponent<Animator>() is Animator animator)
            {
                _avatarField.value = animator;
                // FIX: Is manual notification needed?
                _thumbnailDrawer.SetAvatar(animator);
            }
        }

        private Domain.Avatar GetAvatar()
        {
            if (_avatarField is ObjectField && _avatarField.value is Animator animator)
            {
                var fullPath = animator.gameObject.GetFullPath();
                if (fullPath.StartsWith("/"))
                {
                    return new Domain.Avatar(fullPath);
                }
            }

            return null;
        }

        private void OnMenuUpdated(IMenu menu)
        {
            SetAvatar(menu.Avatar);
        }

        private void OnAvatarChanged(ChangeEvent<UnityEngine.Object> changeEvent)
        {
            if (changeEvent.newValue is Animator animator)
            {
                _thumbnailDrawer.SetAvatar(animator);
                var avatar = GetAvatar();
                if (avatar is Domain.Avatar)
                {
                    _modifyMenuPropertiesUseCase.Handle("", avatar);
                }
            }
        }

        private void OnThumbnailSizeChanged(ChangeEvent<int> changeEvent)
        {
            EditorPrefs.SetInt(DetailConstants.KeyMainThumbnailSize, changeEvent.newValue);
            _thumbnailDrawer.UpdateAll();
        }

        private void OnUpdateThumbnailButtonClicked()
        {
            _thumbnailDrawer.UpdateAll();
        }

        private void OnGenerateButtonClicked()
        {
            _generateFxUseCase.Handle("");
        }

        private void OnModifyMenuPropertiesPresenterCompleted(
            (ModifyMenuPropertiesResult modifyMenuPropertiesResult, IMenu menu, string errorMessage) args)
        {
            if (args.modifyMenuPropertiesResult == ModifyMenuPropertiesResult.Succeeded)
            {
                // NOP
            }
        }

        private void OnGenerateFxPresenterCompleted(
            (GenerateFxResult generateFxResult, IMenu menu, string errorMessage) args)
        {
            if (args.generateFxResult == GenerateFxResult.Succeeded)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"Generation succeeded!", "OK");
            }
        }
    }
}
