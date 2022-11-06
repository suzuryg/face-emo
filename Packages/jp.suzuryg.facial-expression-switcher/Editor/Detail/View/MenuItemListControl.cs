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
    public class MenuItemListControl
    {
        public MenuItemListControl()
        {
        }

        public void Apply(VisualElement root)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(CommonSetting.ViewDirectory, nameof(MenuItemListControl) + ".uxml"));
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(CommonSetting.ViewDirectory, nameof(MenuItemListControl) + ".uss"));
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);
        }
    }
}
