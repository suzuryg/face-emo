using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
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
    public class MainView : IDisposable
    {
        private HierarchyView _hierarchyControl;
        private MenuItemListView _menuItemListControl;
        private BranchListView _branchListControl;

        private ICreateMenuUseCase _createMenuUseCase;

        private ILocalizationSetting _localizationSetting;

        private EnumField _localeField;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public MainView(HierarchyView hierarchyControl, MenuItemListView menuItemListControl, BranchListView branchListControl,
            ICreateMenuUseCase createMenuUseCase, ILocalizationSetting localizationSetting, UseCaseErrorHandler useCaseErrorHandler)
        {
            _hierarchyControl = hierarchyControl.AddTo(_disposables);
            _menuItemListControl = menuItemListControl;
            _branchListControl = branchListControl;

            _createMenuUseCase = createMenuUseCase;

            _localizationSetting = localizationSetting;

            _disposables.Add(useCaseErrorHandler);
        }

        public void Initialize(VisualElement root)
        {
            var commonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/Common.uss");
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(MainView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(MainView)}.uss");
            NullChecker.Check(commonStyle, uxml, style);

            root.styleSheets.Add(commonStyle);
            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            _hierarchyControl.Initialize(root.Q<VisualElement>("HierarchyControl"));
            _menuItemListControl.Initialize(root.Q<VisualElement>("MenuItemListControl"));
            _branchListControl.Initialize(root.Q<VisualElement>("BranchListControl"));

            _localeField = root.Q<EnumField>("LocaleField");
            NullChecker.Check(_localeField);

            _localizationSetting.OnTableChanged.Synchronize().Subscribe(_ => {
                if (_localeField.value != _localizationSetting.Locale as Enum)
                {
                    _localeField.Init(_localizationSetting.Locale);
                }
            }).AddTo(_disposables);

            _localeField.Init(_localizationSetting.Locale);
            _localeField.label = "Locale"; // test
            _localeField.RegisterValueChangedCallback(LocaleChangedCallback);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _localeField.UnregisterValueChangedCallback(LocaleChangedCallback);
        }

        private void LocaleChangedCallback(ChangeEvent<Enum> changeEvent)
        {
            var locale = (Locale)changeEvent.newValue;
            _localizationSetting.SetLocale(locale);
        }
    }
}
