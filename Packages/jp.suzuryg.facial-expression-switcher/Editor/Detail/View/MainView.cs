using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class MainView : IDisposable
    {
        private HierarchyView _hierarchyControl;
        private MenuItemListView _menuItemListControl;
        private BranchListView _branchListControl;

        private ICreateMenuUseCase _createMenuUseCase;

        public MainView(HierarchyView hierarchyControl, MenuItemListView menuItemListControl, BranchListView branchListControl,
            ICreateMenuUseCase createMenuUseCase)
        {
            _hierarchyControl = hierarchyControl;
            _menuItemListControl = menuItemListControl;
            _branchListControl = branchListControl;

            _createMenuUseCase = createMenuUseCase;
        }

        public void Initialize(VisualElement root)
        {
            var commonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{CommonSetting.ViewDirectory}/Common.uss");
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{CommonSetting.ViewDirectory}/{nameof(MainView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{CommonSetting.ViewDirectory}/{nameof(MainView)}.uss");
            NullChecker.Check(commonStyle, uxml, style);

            root.styleSheets.Add(commonStyle);
            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            _hierarchyControl.Initialize(root.Q<VisualElement>("HierarchyControl"));
            _menuItemListControl.Initialize(root.Q<VisualElement>("MenuItemListControl"));
            _branchListControl.Initialize(root.Q<VisualElement>("BranchListControl"));
        }

        public void Dispose()
        {
            _hierarchyControl.Dispose();
        }
    }
}
