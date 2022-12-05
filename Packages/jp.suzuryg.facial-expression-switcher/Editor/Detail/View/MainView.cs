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
    public class MainView : IDisposable
    {
        private HierarchyView _hierarchyView;
        private MenuItemListView _menuItemListView;
        private BranchListView _branchListView;
        private SettingView _settingView;

        private UseCaseErrorHandler _useCaseErrorHandler;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public MainView(
            HierarchyView hierarchyView,
            MenuItemListView menuItemListView,
            BranchListView branchListView,
            SettingView settingView,

            UseCaseErrorHandler useCaseErrorHandler)
        {
            // Views
            _hierarchyView = hierarchyView.AddTo(_disposables);
            _menuItemListView = menuItemListView.AddTo(_disposables);
            _branchListView = branchListView.AddTo(_disposables);
            _settingView = settingView.AddTo(_disposables);

            // Others
            _useCaseErrorHandler = useCaseErrorHandler.AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Initialize(VisualElement root)
        {
            // Load UXML and style
            var commonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/Common.uss");
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(MainView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(MainView)}.uss");
            NullChecker.Check(commonStyle, uxml, style);

            root.styleSheets.Add(commonStyle);
            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            // Query Elements
            var hierarchyArea = root.Q<VisualElement>("HierarchyView");
            var menuItemListArea = root.Q<VisualElement>("MenuItemListView");
            var branchListArea =  root.Q<VisualElement>("BranchListView");
            var settingArea =  root.Q<VisualElement>("SettingView");
            NullChecker.Check(hierarchyArea, menuItemListArea, branchListArea, settingArea);

            // Initialize Views
            _hierarchyView.Initialize(hierarchyArea);
            _menuItemListView.Initialize(menuItemListArea);
            _branchListView.Initialize(branchListArea);
            _settingView.Initialize(settingArea);
        }
    }
}
