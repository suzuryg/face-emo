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

        private IModifyMenuPropertiesPresenter _modifyMenuPropertiesPresenter;

        private ILocalizationSetting _localizationSetting;
        private UpdateMenuSubject _updateMenuSubject;
        private ThumbnailDrawer _thumbnailDrawer;

        private EnumField _localeField;
        private ObjectField _avatarField;
        private Button _updateThumbnailButton;
        private SliderInt _thumbnailSizeSlider;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public SettingView(
            IModifyMenuPropertiesUseCase modifyMenuPropertiesUseCase,

            IModifyMenuPropertiesPresenter modifyMenuPropertiesPresenter,

            ILocalizationSetting localizationSetting,
            UpdateMenuSubject updateMenuSubject,
            ThumbnailDrawer thumbnailDrawer)
        {
            // Usecases
            _modifyMenuPropertiesUseCase = modifyMenuPropertiesUseCase;

            // Presenters
            _modifyMenuPropertiesPresenter = modifyMenuPropertiesPresenter;

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
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _localeField.UnregisterValueChangedCallback(OnLocaleChanged);
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
            _localeField = root.Q<EnumField>("LocaleField");
            _avatarField = root.Q<ObjectField>("AvatarField");
            _updateThumbnailButton = root.Q<Button>("UpdateThumbnailButton");
            _thumbnailSizeSlider = root.Q<SliderInt>("ThumbnailSizeSlider");
            NullChecker.Check(_localeField, _avatarField, _updateThumbnailButton, _thumbnailSizeSlider);

            // Initialize fields
            _localeField.Init(_localizationSetting.Locale);
            _avatarField.objectType = typeof(Animator);
            _thumbnailSizeSlider.lowValue = DetailConstants.MinMainThumbnailSize;
            _thumbnailSizeSlider.highValue = DetailConstants.MaxMainThumbnailSize;
            _thumbnailSizeSlider.value = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailSize) : DetailConstants.MinMainThumbnailSize;

            // Add event handlers
            _localeField.RegisterValueChangedCallback(OnLocaleChanged);
            _avatarField.RegisterValueChangedCallback(OnAvatarChanged);
            _thumbnailSizeSlider.RegisterValueChangedCallback(OnThumbnailSizeChanged);
            Observable.FromEvent(x => _updateThumbnailButton.clicked += x, x => _updateThumbnailButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnUpdateThumbnailButtonClicked()).AddTo(_disposables);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localeField.Init(_localizationSetting.Locale);

            _localeField.label = localizationTable.MainView_Language;
            _updateThumbnailButton.text = localizationTable.MainView_UpdateThumbnails;
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

        private void OnLocaleChanged(ChangeEvent<Enum> changeEvent)
        {
            var locale = (Locale)changeEvent.newValue;
            _localizationSetting.SetLocale(locale);
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

        private void OnModifyMenuPropertiesPresenterCompleted(
            (ModifyMenuPropertiesResult modifyMenuPropertiesResult, IMenu menu, string errorMessage) args)
        {
            if (args.modifyMenuPropertiesResult == ModifyMenuPropertiesResult.Succeeded)
            {
                // NOP
            }
        }
    }
}
