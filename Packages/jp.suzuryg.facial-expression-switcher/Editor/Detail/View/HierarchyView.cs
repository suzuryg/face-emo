using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
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
using UnityEditor.IMGUI.Controls;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class HierarchyView : IDisposable
    {
        private IAddMenuItemUseCase _addMenuItemUseCase;
        private IRemoveMenuItemUseCase _removeMenuItemUseCase;
        private IModifyGroupPropertiesUseCase _modifyGroupPropertiesUseCase;
        private IMoveMenuItemUseCase _moveMenuItemUseCase;

        private IAddMenuItemPresenter _addMenuItemPresenter;
        private IRemoveMenuItemPresenter _removeMenuItemPresenter;
        private IModifyGroupPropertiesPresenter _modifyGroupPropertiesPresenter;
        private IMoveMenuItemPresenter _moveMenuItemPresenter;

        private IReadOnlyLocalizationSetting _localizationSetting;

        private UpdateMenuSubject _updateMenuSubject;
        private HierarchyViewState _hierarchyViewState;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public HierarchyView(
            IAddMenuItemUseCase addMenuItemUseCase,
            IRemoveMenuItemUseCase removeMenuItemUseCase,
            IModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase,
            IMoveMenuItemUseCase moveMenuItemUseCase,

            IAddMenuItemPresenter addMenuItemPresenter,
            IRemoveMenuItemPresenter removeMenuItemPresenter,
            IModifyGroupPropertiesPresenter modifyGroupPropertiesPresenter,
            IMoveMenuItemPresenter moveMenuItemPresenter,

            IReadOnlyLocalizationSetting localizationSetting,
            UpdateMenuSubject updateMenuSubject,
            HierarchyViewState hierarchyViewState)
        {
            // usecase
            _addMenuItemUseCase = addMenuItemUseCase;
            _removeMenuItemUseCase = removeMenuItemUseCase;
            _modifyGroupPropertiesUseCase = modifyGroupPropertiesUseCase;
            _moveMenuItemUseCase = moveMenuItemUseCase;

            // presenter
            _addMenuItemPresenter = addMenuItemPresenter;
            _removeMenuItemPresenter = removeMenuItemPresenter;
            _modifyGroupPropertiesPresenter = modifyGroupPropertiesPresenter;
            _moveMenuItemPresenter = moveMenuItemPresenter;

            // others
            _localizationSetting = localizationSetting;
            _updateMenuSubject = updateMenuSubject;
            _hierarchyViewState = hierarchyViewState;
        }

        public void Dispose() => _disposables.Dispose();

        // TODO: Error handling
        public void Initialize(VisualElement root)
        {
            // UXML and style
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(HierarchyView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(HierarchyView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            // Query elements
            var titleLabel = root.Q<Label>("TitleLabel");
            var addGroupButton = root.Q<Button>("AddGroupButton");
            var removeGroupButton = root.Q<Button>("RemoveGroupButton");
            var treeViewContainer = root.Q<IMGUIContainer>("TreeViewContainer");
            NullChecker.Check(titleLabel, addGroupButton, removeGroupButton, treeViewContainer);

            // Tree view
            if (_hierarchyViewState.TreeViewState is null)
            {
                _hierarchyViewState.TreeViewState = new TreeViewState();
            }
            var hierarchyTreeElement = new HierarchyTreeElement(_localizationSetting, _hierarchyViewState.TreeViewState);

            // Tree view onGUI
            Observable.FromEvent(
                x => treeViewContainer.onGUIHandler += x,
                x => treeViewContainer.onGUIHandler -= x).Synchronize().Subscribe(_ =>
                {
                    hierarchyTreeElement.OnGUI(treeViewContainer.contentRect);
                }).AddTo(_disposables);

            // Tree view onRename
            hierarchyTreeElement.OnGroupRenamed.Synchronize().Subscribe(x =>
            {
                _modifyGroupPropertiesUseCase.Handle("", x.menuItemId, x.displayName);

            }).AddTo(_disposables);

            // Tree view onSelectionChanged
            hierarchyTreeElement.OnSelectionChanged.Synchronize().Subscribe(x =>
            {
                if (x.menu is null || x.menuItemIds is null || x.menuItemIds.Count != 1)
                {
                    addGroupButton.SetEnabled(false);
                    removeGroupButton.SetEnabled(false);
                }
                else if (x.menuItemIds[0] == Domain.Menu.RegisteredId)
                {
                    addGroupButton.SetEnabled(!x.menu.Registered.IsFull);
                    removeGroupButton.SetEnabled(false);
                }
                else if (x.menuItemIds[0] == Domain.Menu.UnregisteredId)
                {
                    addGroupButton.SetEnabled(true);
                    removeGroupButton.SetEnabled(false);
                }
                else if (x.menu.ContainsGroup(x.menuItemIds[0]))
                {
                    var group = x.menu.GetGroup(x.menuItemIds[0]);
                    addGroupButton.SetEnabled(!group.IsFull);
                    removeGroupButton.SetEnabled(true);
                }
                else
                {
                    addGroupButton.SetEnabled(false);
                    removeGroupButton.SetEnabled(false);
                }
            }).AddTo(_disposables);

            // Tree view onDropped
            hierarchyTreeElement.OnDropped.Synchronize().Subscribe(x =>
            {
                _moveMenuItemUseCase.Handle("", x.source, x.destination, x.index);

            }).AddTo(_disposables);

            // Add button
            Observable.FromEvent(
                x => addGroupButton.clicked += x,
                x => addGroupButton.clicked -= x).Synchronize().Subscribe(_ =>
                {
                    var ids = hierarchyTreeElement.GetSelectedMenuItemIds();
                    if (ids is IReadOnlyList<string> && ids.Count == 1)
                    {
                        _addMenuItemUseCase.Handle("", ids[0], AddMenuItemType.Group);
                    }
                }).AddTo(_disposables);

            // Remove button
            Observable.FromEvent(
                x => removeGroupButton.clicked += x,
                x => removeGroupButton.clicked -= x).Synchronize().Subscribe(_ =>
                {
                    var ids = hierarchyTreeElement.GetSelectedMenuItemIds();
                    if (ids is IReadOnlyList<string> && ids.Count == 1)
                    {
                        _removeMenuItemUseCase.Handle("", ids[0]);
                    }
                }).AddTo(_disposables);

            // Update menu
            _updateMenuSubject.Observable.Synchronize().Subscribe(x => 
            {
                hierarchyTreeElement.Setup(x);
                hierarchyTreeElement.ChangeSelectionDummy(); // Update button state
            }).AddTo(_disposables);

            // Add menu item presenter
            _addMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.addMenuItemResult == AddMenuItemResult.Succeeded)
                {
                    hierarchyTreeElement.ExpandSelectedElement();
                    hierarchyTreeElement.Setup(x.menu);
                    hierarchyTreeElement.ChangeSelectionDummy(); // Update button state
                }
            }).AddTo(_disposables);

            // Remove menu item presenter
            _removeMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.removeMenuItemResult == RemoveMenuItemResult.Succeeded)
                {
                    hierarchyTreeElement.SelectNearest();
                    hierarchyTreeElement.Setup(x.menu);
                    hierarchyTreeElement.ChangeSelectionDummy(); // Update button state
                }
            }).AddTo(_disposables);

            // Modify group properties presenter
            _modifyGroupPropertiesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyGroupPropertiesResult == ModifyGroupPropertiesResult.Succeeded)
                {
                    hierarchyTreeElement.Setup(x.menu);
                }
            }).AddTo(_disposables);

            // Move menu item presenter
            _moveMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.moveMenuItemResult == MoveMenuItemResult.Succeeded)
                {
                    hierarchyTreeElement.Setup(x.menu);
                }
            }).AddTo(_disposables);

            // Set text
            Action<LocalizationTable> setText = loc =>
            {
                titleLabel.text = loc.HierarchyView_Title;
            };
            setText(_localizationSetting.Table);
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(setText).AddTo(_disposables);
        }
    }
}
