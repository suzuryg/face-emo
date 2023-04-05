using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.View.Element;
using System;
using System.Linq;
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
        private IModifyModePropertiesUseCase _modifyModePropertiesUseCase;
        private IModifyGroupPropertiesUseCase _modifyGroupPropertiesUseCase;
        private IMoveMenuItemUseCase _moveMenuItemUseCase;

        private IReadOnlyLocalizationSetting _localizationSetting;

        private UpdateMenuSubject _updateMenuSubject;
        private SelectionSynchronizer _selectionSynchronizer;
        private HierarchyViewState _hierarchyViewState;

        private Label _titleLabel;
        private Button _addModeButton;
        private Button _addGroupButton;
        private Button _removeGroupButton;
        private IMGUIContainer _treeViewContainer;

        private HierarchyTreeElement _hierarchyTreeElement;

        private CompositeDisposable _disposables = new CompositeDisposable();
        private CompositeDisposable _treeElementDisposables = new CompositeDisposable();

        public HierarchyView(
            IAddMenuItemUseCase addMenuItemUseCase,
            IRemoveMenuItemUseCase removeMenuItemUseCase,
            IModifyModePropertiesUseCase modifyModePropertiesUseCase,
            IModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase,
            IMoveMenuItemUseCase moveMenuItemUseCase,

            IReadOnlyLocalizationSetting localizationSetting,
            UpdateMenuSubject updateMenuSubject,
            SelectionSynchronizer selectionSynchronizer,
            HierarchyViewState hierarchyViewState)
        {
            // Usecases
            _addMenuItemUseCase = addMenuItemUseCase;
            _removeMenuItemUseCase = removeMenuItemUseCase;
            _modifyModePropertiesUseCase = modifyModePropertiesUseCase;
            _modifyGroupPropertiesUseCase = modifyGroupPropertiesUseCase;
            _moveMenuItemUseCase = moveMenuItemUseCase;

            // Others
            _localizationSetting = localizationSetting;
            _updateMenuSubject = updateMenuSubject;
            _selectionSynchronizer = selectionSynchronizer;
            _hierarchyViewState = hierarchyViewState;

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(OnMenuUpdated).AddTo(_disposables);

            // Synchronize selection event handler
            _selectionSynchronizer.OnSynchronizeSelection.Synchronize().Subscribe(OnSynchronizeSelection).AddTo(_disposables);

            // Initialize tree element
            InitializeTreeElement();
        }

        public void Dispose()
        {
            _treeViewContainer.UnregisterCallback<MouseLeaveEvent>(OnMouseLeft);
            _disposables.Dispose();
            _treeElementDisposables.Dispose();
        }

        public void Initialize(VisualElement root)
        {
            // Load UXML and style
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(HierarchyView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(HierarchyView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            // Query elements
            _titleLabel = root.Q<Label>("TitleLabel");
            _addModeButton = root.Q<Button>("AddModeButton");
            _addGroupButton = root.Q<Button>("AddGroupButton");
            _removeGroupButton = root.Q<Button>("RemoveGroupButton");
            _treeViewContainer = root.Q<IMGUIContainer>("TreeViewContainer");
            NullChecker.Check(_titleLabel, _addModeButton, _addGroupButton, _removeGroupButton, _treeViewContainer);

            // Add event handlers
            Observable.FromEvent(x => _addModeButton.clicked += x, x => _addModeButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnAddButtonClicked(AddMenuItemType.Mode)).AddTo(_disposables);
            Observable.FromEvent(x => _addGroupButton.clicked += x, x => _addGroupButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnAddButtonClicked(AddMenuItemType.Group)).AddTo(_disposables);
            Observable.FromEvent(x => _removeGroupButton.clicked += x, x => _removeGroupButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnRemoveButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _treeViewContainer.onGUIHandler += x, x => _treeViewContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => _hierarchyTreeElement?.OnGUI(_treeViewContainer.contentRect)).AddTo(_disposables);
            _treeViewContainer.RegisterCallback<MouseLeaveEvent>(OnMouseLeft);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void InitializeTreeElement()
        {
            var menu = _hierarchyTreeElement?.Menu;

            _treeElementDisposables.Dispose();
            _treeElementDisposables = new CompositeDisposable();

            // Initialize tree element
            if (_hierarchyViewState.TreeViewState is null)
            {
                _hierarchyViewState.TreeViewState = new TreeViewState();
            }
            _hierarchyTreeElement = new HierarchyTreeElement(_localizationSetting, _hierarchyViewState.TreeViewState).AddTo(_treeElementDisposables);

            // Tree element event handlers
            _hierarchyTreeElement.OnModeRenamed.Synchronize().Subscribe(x => _modifyModePropertiesUseCase.Handle("", x.menuItemId, x.displayName)).AddTo(_treeElementDisposables);
            _hierarchyTreeElement.OnGroupRenamed.Synchronize().Subscribe(x => _modifyGroupPropertiesUseCase.Handle("", x.menuItemId, x.displayName)).AddTo(_treeElementDisposables);
            _hierarchyTreeElement.OnSelectionChanged.Synchronize().Subscribe(x => OnSelectionChanged(x.menu, x.menuItemIds)).AddTo(_treeElementDisposables);
            _hierarchyTreeElement.OnDropped.Synchronize().Subscribe(OnDropped).AddTo(_treeElementDisposables);

            // Update display
            _hierarchyTreeElement.Setup(menu);
            UpdateDisplay();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _titleLabel.text = localizationTable.HierarchyView_Title;
        }

        private void UpdateDisplay()
        {
            _addModeButton?.SetEnabled(false);
            _addGroupButton?.SetEnabled(false);
            _removeGroupButton?.SetEnabled(false);

            var menu = _hierarchyTreeElement?.Menu;
            var selectedMenuItemIds = _hierarchyTreeElement?.GetSelectedMenuItemIds();

            if (menu is null || selectedMenuItemIds is null || selectedMenuItemIds.Count != 1 || selectedMenuItemIds[0] is null)
            {
                return;
            }

            var id = selectedMenuItemIds[0];
            var parentId = GetParentId(menu, id);

            // Add button
            if (parentId == Domain.Menu.RegisteredId)
            {
                _addModeButton?.SetEnabled(!menu.Registered.IsFull);
                _addGroupButton?.SetEnabled(!menu.Registered.IsFull);
            }
            else if (parentId == Domain.Menu.UnregisteredId)
            {
                _addModeButton?.SetEnabled(true);
                _addGroupButton?.SetEnabled(true);
            }
            else if (menu.ContainsGroup(parentId))
            {
                var group = menu.GetGroup(parentId);
                _addModeButton?.SetEnabled(!group.IsFull);
                _addGroupButton?.SetEnabled(!group.IsFull);
            }
             
            // Remove button
            if (id == Domain.Menu.RegisteredId)
            {
                _removeGroupButton?.SetEnabled(false);
            }
            else if (id == Domain.Menu.UnregisteredId)
            {
                _removeGroupButton?.SetEnabled(false);
            }
            else if (menu.ContainsGroup(id))
            {
                _removeGroupButton?.SetEnabled(true);
            }
            else if (menu.ContainsMode(id))
            {
                _removeGroupButton?.SetEnabled(true);
            }
        }

        private void OnMouseLeft(MouseLeaveEvent mouseLeaveEvent)
        {
            // Eliminate drag position display
            if (Event.current.type == EventType.DragUpdated)
            {
                InitializeTreeElement();
            }
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _hierarchyTreeElement?.Setup(menu);
            UpdateDisplay();
        }

        private void OnSelectionChanged(IMenu menu, IReadOnlyList<string> selectedMenuItemIds)
        {
            if (selectedMenuItemIds.Count == 1)
            {
                _selectionSynchronizer.ChangeHierarchyViewSelection(selectedMenuItemIds[0]);
            }
            else
            {
                UpdateDisplay();
            }
        }

        private void OnSynchronizeSelection(ViewSelection viewSelection)
        {
            _hierarchyTreeElement?.SelectMenuItems(new[] { viewSelection.HierarchyView });

            UpdateDisplay();
        }

        private void OnDropped((IReadOnlyList<string> source, string destination, int? index) args)
        {
            if (args.source is null || args.source.Count == 0 || args.destination is null)
            {
                return;
            }
            _moveMenuItemUseCase.Handle("", args.source, args.destination, args.index);
        }

        private void OnAddButtonClicked(AddMenuItemType addMenuItemType)
        {
            var ids = _hierarchyTreeElement.GetSelectedMenuItemIds();
            if (ids is IReadOnlyList<string> && ids.Count == 1)
            {
                var parentId = GetParentId(_hierarchyTreeElement.Menu, ids[0]);
                _addMenuItemUseCase.Handle("", parentId, addMenuItemType);
            }
        }

        private void OnRemoveButtonClicked()
        {
            var ids = _hierarchyTreeElement.GetSelectedMenuItemIds();
            if (ids is IReadOnlyList<string> && ids.Count == 1)
            {
                _removeMenuItemUseCase.Handle("", ids[0]);
            }
        }

        private static string GetParentId(IMenu menu, string menuItemId)
        {
            if (menuItemId == Domain.Menu.RegisteredId || menuItemId == Domain.Menu.UnregisteredId || menu.ContainsGroup(menuItemId))
            {
                return menuItemId;
            }
            else if (menu.ContainsMode(menuItemId))
            {
                var mode = menu.GetMode(menuItemId);
                return mode.Parent.GetId();
            }
            else
            {
                return null;
            }
        }
    }
}
