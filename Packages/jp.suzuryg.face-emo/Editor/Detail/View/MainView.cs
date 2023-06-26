using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniRx;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class MainView : IDisposable
    {
        private static readonly float Padding = 20;
        private static readonly float MinHeight = 330;

        private HierarchyView _hierarchyView;
        private MenuItemListView _menuItemListView;
        private BranchListView _branchListView;
        private SettingView _settingView;

        private VisualElement _hierarchyArea;
        private VisualElement _menuItemListArea;
        private VisualElement _branchListArea;
        private VisualElement _settingArea;

        private UseCaseErrorHandler _useCaseErrorHandler;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public MainView(
            HierarchyView hierarchyView,
            MenuItemListView menuItemListView,
            BranchListView branchListView,
            SettingView settingView,
            MainWindowProvider mainWindowProvider,
            UseCaseErrorHandler useCaseErrorHandler)
        {
            // Views
            _hierarchyView = hierarchyView.AddTo(_disposables);
            _menuItemListView = menuItemListView.AddTo(_disposables);
            _branchListView = branchListView.AddTo(_disposables);
            _settingView = settingView.AddTo(_disposables);

            // Others
            _useCaseErrorHandler = useCaseErrorHandler.AddTo(_disposables);

            // MainWindow OnGUI event handler
            mainWindowProvider.OnGUI.Synchronize().ObserveOnMainThread().Subscribe(OnGUI).AddTo(_disposables);
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

            // Set class
            if (EditorGUIUtility.isProSkin)
            {
                root.AddToClassList("dark-theme");
            }
            else
            {
                root.AddToClassList("light-theme");
            }

            // Query Elements
            _hierarchyArea = root.Q<VisualElement>("HierarchyView");
            _menuItemListArea = root.Q<VisualElement>("MenuItemListView");
            _branchListArea =  root.Q<VisualElement>("BranchListView");
            _settingArea =  root.Q<VisualElement>("SettingView");
            NullChecker.Check(_hierarchyArea, _menuItemListArea, _branchListArea, _settingArea);

            // Initialize Views
            _hierarchyView.Initialize(_hierarchyArea);
            _menuItemListView.Initialize(_menuItemListArea);
            _branchListView.Initialize(_branchListArea);
            _settingView.Initialize(_settingArea);
        }

        private void OnGUI(EditorWindow mainWindow)
        {
            var hierarchyViewWidth = _hierarchyView != null ? _hierarchyView.GetWidth() : 100;
            var menuItemListViewWidth = _menuItemListView != null ? _menuItemListView.GetWidth() : 100;
            var branchListViewWidth = _branchListView != null ? _branchListView.GetWidth() : 100;

            if (_hierarchyArea != null)
            {
                _hierarchyArea.style.minWidth = hierarchyViewWidth;
            }
            if (_menuItemListArea != null)
            {
                _menuItemListArea.style.minWidth = menuItemListViewWidth;
                _menuItemListArea.style.maxWidth = menuItemListViewWidth;
            }
            if (_branchListArea != null)
            {
                _branchListArea.style.minWidth = branchListViewWidth;
                _branchListArea.style.maxWidth = branchListViewWidth;
            }

            var width = Padding + hierarchyViewWidth + menuItemListViewWidth + branchListViewWidth + Padding;
            mainWindow.minSize = new Vector2(width, MinHeight);
        }
    }
}
