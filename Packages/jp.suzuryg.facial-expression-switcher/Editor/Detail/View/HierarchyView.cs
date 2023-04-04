using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.Subject;
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
        private ChangeHierarchySelectionSubject _changeHierarchySelectionSubject;
        private ChangeMenuItemListRootSubject _changeMenuItemListRootSubject;
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
            ChangeHierarchySelectionSubject changeHierarchySelectionSubject,
            ChangeMenuItemListRootSubject changeMenuItemListRootSubject,
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
            _changeHierarchySelectionSubject = changeHierarchySelectionSubject;
            _changeMenuItemListRootSubject = changeMenuItemListRootSubject;
            _hierarchyViewState = hierarchyViewState;

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(OnMenuUpdated).AddTo(_disposables);

            // Change menu item list root event handler
            _changeMenuItemListRootSubject.Observable.Synchronize().Subscribe(OnMenuItemListRootChanged).AddTo(_disposables);

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

            // Setup
            _hierarchyTreeElement.Setup(menu);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _titleLabel.text = localizationTable.HierarchyView_Title;
        }

        private void UpdateButtonState(IMenu menu, IReadOnlyList<string> selectedMenuItemIds)
        {
            if (menu is null || selectedMenuItemIds is null || selectedMenuItemIds.Count != 1 || selectedMenuItemIds[0] is null)
            {
                _addModeButton?.SetEnabled(false);
                _addGroupButton?.SetEnabled(false);
                _removeGroupButton?.SetEnabled(false);
            }
            else if (selectedMenuItemIds[0] == Domain.Menu.RegisteredId)
            {
                _addModeButton?.SetEnabled(!menu.Registered.IsFull);
                _addGroupButton?.SetEnabled(!menu.Registered.IsFull);
                _removeGroupButton?.SetEnabled(false);
            }
            else if (selectedMenuItemIds[0] == Domain.Menu.UnregisteredId)
            {
                _addModeButton?.SetEnabled(true);
                _addGroupButton?.SetEnabled(true);
                _removeGroupButton?.SetEnabled(false);
            }
            else if (menu.ContainsMode(selectedMenuItemIds[0]))
            {
                _addModeButton?.SetEnabled(false);
                _addGroupButton?.SetEnabled(false);
                _removeGroupButton?.SetEnabled(true);
            }
            else if (menu.ContainsGroup(selectedMenuItemIds[0]))
            {
                var group = menu.GetGroup(selectedMenuItemIds[0]);
                _addModeButton?.SetEnabled(!group.IsFull);
                _addGroupButton?.SetEnabled(!group.IsFull);
                _removeGroupButton?.SetEnabled(true);
            }
            else
            {
                _addModeButton?.SetEnabled(false);
                _addGroupButton?.SetEnabled(false);
                _removeGroupButton?.SetEnabled(false);
            }
        }

        private void UpdateTree(IMenu menu)
        {
            // Do not call this method before setting up the tree element because the selection will be null.
            var selection = _hierarchyTreeElement.GetSelectedMenuItemIds();
            var nearest = selection.Select(x => _hierarchyTreeElement.GetNearestMenuItemId(x)).ToList();

            _hierarchyTreeElement.Setup(menu);

            if (!_hierarchyTreeElement.SelectMenuItems(selection))
            {
                _hierarchyTreeElement.SelectMenuItems(nearest);
            }

            selection = _hierarchyTreeElement.GetSelectedMenuItemIds();
            _changeHierarchySelectionSubject.OnNext(selection);
            UpdateButtonState(menu, selection);
        }

        private void OnMouseLeft(MouseLeaveEvent mouseLeaveEvent)
        {
            // Eliminate drag position display
            if (Event.current.type == EventType.DragUpdated)
            {
                InitializeTreeElement();
            }
        }

        private void OnMenuUpdated(IMenu menu) => UpdateTree(menu);

        private void OnMenuItemListRootChanged(string rootGroupId)
        {
            if (rootGroupId is string && !_hierarchyTreeElement.GetSelectedMenuItemIds().Contains(rootGroupId))
            {
                _hierarchyTreeElement.RevealMenuItem(rootGroupId);
                _hierarchyTreeElement.SelectMenuItems(new[] { rootGroupId });
                UpdateTree(_hierarchyTreeElement.Menu);
            }
        }

        private void OnSelectionChanged(IMenu menu, IReadOnlyList<string> selectedMenuItemIds) => UpdateTree(menu);

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
                _addMenuItemUseCase.Handle("", ids[0], addMenuItemType);
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
    }
}
