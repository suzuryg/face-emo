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
    public class BranchListControl
    {
        public BranchListControl()
        {
        }

        public void Apply(VisualElement root)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(CommonSetting.ViewDirectory, nameof(BranchListControl) + ".uxml"));
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(CommonSetting.ViewDirectory, nameof(BranchListControl) + ".uss"));
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);
        }
    }
}
