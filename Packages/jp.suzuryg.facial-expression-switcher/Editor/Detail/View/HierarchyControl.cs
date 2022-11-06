using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class HierarchyControl
    {
        HierarchyTreeView _hierarchyTreeView;

        public HierarchyControl()
        {
        }

        public void Apply(VisualElement root)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(CommonSetting.ViewDirectory, nameof(HierarchyControl) + ".uxml"));
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(CommonSetting.ViewDirectory, nameof(HierarchyControl) + ".uss"));
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            var titleLabel = root.Q<Label>("TitleLabel");
            var addGroupButton = root.Q<Button>("AddGroupButton");
            var treeViewContainer= root.Q<IMGUIContainer>("TreeViewContainer");
            NullChecker.Check(titleLabel, addGroupButton, treeViewContainer);

            titleLabel.text = "階層ビュー"; // TODO: ローカリゼーション

            addGroupButton.text = "add";
            treeViewContainer.onGUIHandler += TreeViewOnGUI; // 購読解除が必要？

            //_hierarchyTreeView = new HierarchyTreeView();
        }

        private void TreeViewOnGUI()
        {
            //_hierarchyTreeView.OnGUI(new Rect());
        }

        // Presenterに対応するデリゲートを定義する
        // Presenterごとに設定するのではなく、引数が同じPresenterごとに定義する
    }
}
