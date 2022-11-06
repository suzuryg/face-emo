using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class MainWindow : IDisposable
    {
        private HierarchyControl _hierarchyControl;
        private MenuItemListControl _menuItemListControl;
        private BranchListControl _branchListControl;

        public MainWindow(HierarchyControl hierarchyControl, MenuItemListControl menuItemListControl, BranchListControl branchListControl)
        {
            _hierarchyControl = hierarchyControl;
            _menuItemListControl = menuItemListControl;
            _branchListControl = branchListControl;
        }

        public void Apply(VisualElement root)
        {
            var commonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(CommonSetting.ViewDirectory, "Common.uss"));
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(CommonSetting.ViewDirectory, nameof(MainWindow) + ".uxml"));
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(CommonSetting.ViewDirectory, nameof(MainWindow) + ".uss"));
            NullChecker.Check(commonStyle, uxml, style);

            root.styleSheets.Add(commonStyle);
            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            _hierarchyControl.Apply(root.Q<VisualElement>("HierarchyControl"));
            _menuItemListControl.Apply(root.Q<VisualElement>("MenuItemListControl"));
            _branchListControl.Apply(root.Q<VisualElement>("BranchListControl"));
        }

        public void Dispose()
        {
        }
    }
}
