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
    public class BranchListView
    {
        public BranchListView()
        {
        }

        public void Initialize(VisualElement root)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(BranchListView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(BranchListView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);
        }
    }
}
