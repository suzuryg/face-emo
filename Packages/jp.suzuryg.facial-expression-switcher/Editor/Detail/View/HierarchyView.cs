using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.Subject;
using Suzuryg.FacialExpressionSwitcher.Detail.View.Element;
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
    public class HierarchyView : IDisposable
    {
        HierarchyTreeElement _hierarchyTreeView;

        SettingRepository _settingRepository;

        UpdateMenuSubject _updateMenuSubject;

        HierarchyViewState _hierarchyViewState;

        Label _titleLabel;
        Button _addGroupButton;
        IMGUIContainer _treeViewContainer;

        public HierarchyView(SettingRepository settingRepository, UpdateMenuSubject updateMenuSubject, HierarchyViewState hierarchyViewState)
        {
            NullChecker.Check(settingRepository, updateMenuSubject, hierarchyViewState);
            _settingRepository = settingRepository;
            _updateMenuSubject = updateMenuSubject;
            _updateMenuSubject.OnMenuUpdated += OnMenuUpdated;
            _hierarchyViewState = hierarchyViewState;
        }

        public void Initialize(VisualElement root)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{CommonSetting.ViewDirectory}/{nameof(HierarchyView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{CommonSetting.ViewDirectory}/{nameof(HierarchyView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            _titleLabel = root.Q<Label>("TitleLabel");
            _addGroupButton = root.Q<Button>("AddGroupButton");
            _treeViewContainer= root.Q<IMGUIContainer>("TreeViewContainer");
            NullChecker.Check(_titleLabel, _addGroupButton, _treeViewContainer);
            //treeViewContainer.onGUIHandler += TreeViewOnGUI;

            //_hierarchyTreeView = new HierarchyTreeView();

            _addGroupButton.clicked += TestAction;

            SetText();
        }

        // TODO: Error handling
        private void SetText()
        {
            var setting = _settingRepository.Load();
            var loc = setting.LocalizationDictionary;

            _titleLabel.text = loc.HierarchyView_Title;
            //_addGroupButton.text = loc.HierarchyControl_AddButton;
            _addGroupButton.text = _hierarchyViewState.TestState;
        }

        private void TestAction()
        {
            _addGroupButton.text = _hierarchyViewState.TestState;

            _addGroupButton.text += "*";

            _hierarchyViewState.TestState = _addGroupButton.text;
            EditorUtility.SetDirty(_hierarchyViewState);
        }

        private void TreeViewOnGUI()
        {
            //_hierarchyTreeView.OnGUI(new Rect());
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _titleLabel.text += "*";
        }

        public void Dispose()
        {
            _updateMenuSubject.OnMenuUpdated -= OnMenuUpdated;
            //treeViewContainer.onGUIHandler -= TreeViewOnGUI;
            _addGroupButton.clicked -= TestAction;
        }

        // Presenterに対応するデリゲートを定義する
        // Presenterごとに設定するのではなく、引数が同じPresenterごとに定義する
    }
}
