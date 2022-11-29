using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class MenuItemListView
    {
        public MenuItemListView()
        {
        }

        public void Initialize(VisualElement root)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DomainConstants.ViewDirectory}/{nameof(MenuItemListView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DomainConstants.ViewDirectory}/{nameof(MenuItemListView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);
        }
    }
}
