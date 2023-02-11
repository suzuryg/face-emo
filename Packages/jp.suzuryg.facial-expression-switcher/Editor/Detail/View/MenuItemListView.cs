using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
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
using UnityEditor.IMGUI.Controls;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class MenuItemListView : IDisposable
    {
        private IAddMenuItemUseCase _addMenuItemUseCase;
        private IRemoveMenuItemUseCase _removeMenuItemUseCase;
        private IModifyModePropertiesUseCase _modifyModePropertiesUseCase;
        private IModifyGroupPropertiesUseCase _modifyGroupPropertiesUseCase;
        private IMoveMenuItemUseCase _moveMenuItemUseCase;
        private ISetExistingAnimationUseCase _setExistingAnimationUseCase;

        private IAddMenuItemPresenter _addMenuItemPresenter;
        private IRemoveMenuItemPresenter _removeMenuItemPresenter;
        private IModifyModePropertiesPresenter _modifyModePropertiesPresenter;
        private IModifyGroupPropertiesPresenter _modifyGroupPropertiesPresenter;
        private IMoveMenuItemPresenter _moveMenuItemPresenter;
        private ISetExistingAnimationPresenter _setExistingAnimationPresenter;

        private IReadOnlyLocalizationSetting _localizationSetting;

        private UpdateMenuSubject _updateMenuSubject;
        private ChangeHierarchySelectionSubject _changeHierarchySelectionSubject;
        private ChangeMenuItemListRootSubject _changeMenuItemListRootSubject;
        private ChangeMenuItemListSelectionSubject _changeMenuItemListSelectionSubject;
        private ThumbnailDrawer _thumbnailDrawer;
        private MenuItemListViewState _menuItemListViewState;

        private Label _titleLabel;
        private Button _addModeButton;
        private Button _addGroupButton;
        private Button _removeButton;
        private IMGUIContainer _addressBarContainer;
        private IMGUIContainer _treeViewContainer;

        private AddressBarElement _addressBarElement;
        private MenuItemTreeElement _menuItemTreeElement;

        private CompositeDisposable _disposables = new CompositeDisposable();
        private CompositeDisposable _treeElementDisposables = new CompositeDisposable();

        public MenuItemListView(
            IAddMenuItemUseCase addMenuItemUseCase,
            IRemoveMenuItemUseCase removeMenuItemUseCase,
            IModifyModePropertiesUseCase modifyModePropertiesUseCase,
            IModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase,
            IMoveMenuItemUseCase moveMenuItemUseCase,
            ISetExistingAnimationUseCase setExistingAnimationUseCase,

            IAddMenuItemPresenter addMenuItemPresenter,
            IRemoveMenuItemPresenter removeMenuItemPresenter,
            IModifyModePropertiesPresenter modifyModePropertiesPresenter,
            IModifyGroupPropertiesPresenter modifyGroupPropertiesPresenter,
            IMoveMenuItemPresenter moveMenuItemPresenter,
            ISetExistingAnimationPresenter setExistingAnimationPresenter,

            IReadOnlyLocalizationSetting localizationSetting,
            UpdateMenuSubject updateMenuSubject,
            ChangeHierarchySelectionSubject changeHierarchySelectionSubject,
            ChangeMenuItemListRootSubject changeMenuItemListRootSubject,
            ChangeMenuItemListSelectionSubject changeMenuItemListSelectionSubject,
            ThumbnailDrawer thumbnailDrawer,
            MenuItemListViewState menuItemListViewState)
        {
            // Usecases
            _addMenuItemUseCase = addMenuItemUseCase;
            _removeMenuItemUseCase = removeMenuItemUseCase;
            _modifyModePropertiesUseCase = modifyModePropertiesUseCase;
            _modifyGroupPropertiesUseCase = modifyGroupPropertiesUseCase;
            _moveMenuItemUseCase = moveMenuItemUseCase;
            _setExistingAnimationUseCase = setExistingAnimationUseCase;

            // Presenters
            _addMenuItemPresenter = addMenuItemPresenter;
            _removeMenuItemPresenter = removeMenuItemPresenter;
            _modifyModePropertiesPresenter = modifyModePropertiesPresenter;
            _modifyGroupPropertiesPresenter = modifyGroupPropertiesPresenter;
            _moveMenuItemPresenter = moveMenuItemPresenter;
            _setExistingAnimationPresenter = setExistingAnimationPresenter;

            // Others
            _localizationSetting = localizationSetting;
            _updateMenuSubject = updateMenuSubject;
            _changeHierarchySelectionSubject = changeHierarchySelectionSubject;
            _changeMenuItemListRootSubject = changeMenuItemListRootSubject;
            _changeMenuItemListSelectionSubject = changeMenuItemListSelectionSubject;
            _thumbnailDrawer = thumbnailDrawer;
            _menuItemListViewState = menuItemListViewState;

            // Address bar element
            _addressBarElement = new AddressBarElement(_localizationSetting).AddTo(_disposables);
            _addressBarElement.OnAddressClicked.Synchronize().Subscribe(OnAddressClicked).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(OnMenuUpdated).AddTo(_disposables);

            // Change hierarchy selection event handler
            _changeHierarchySelectionSubject.Observable.Synchronize().Subscribe(OnHierarchySelectionChanged).AddTo(_disposables);

            // Presenter event handlers
            _addMenuItemPresenter.Observable.Synchronize().Subscribe(OnAddMenuItemPresenterCompleted).AddTo(_disposables);
            _removeMenuItemPresenter.Observable.Synchronize().Subscribe(OnRemoveMenuItemPresenterCompleted).AddTo(_disposables);
            _modifyModePropertiesPresenter.Observable.Synchronize().Subscribe(OnModifyModePropertiesPresenterCompleted).AddTo(_disposables);
            _modifyGroupPropertiesPresenter.Observable.Synchronize().Subscribe(OnModifyGroupPropertiesPresenterCompleted).AddTo(_disposables);
            _moveMenuItemPresenter.Observable.Synchronize().Subscribe(OnMoveMenuItemPresenterCompleted).AddTo(_disposables);
            _setExistingAnimationPresenter.Observable.Synchronize().Subscribe(OnSetExistingAnimationPresenterCompleted).AddTo(_disposables);

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
            // UXML and style
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(MenuItemListView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(MenuItemListView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            // Query elements
            _titleLabel = root.Q<Label>("TitleLabel");
            _addModeButton = root.Q<Button>("AddModeButton");
            _addGroupButton = root.Q<Button>("AddGroupButton");
            _removeButton = root.Q<Button>("RemoveButton");
            _addressBarContainer = root.Q<IMGUIContainer>("AddressBarContainer");
            _treeViewContainer = root.Q<IMGUIContainer>("TreeViewContainer");
            NullChecker.Check(_titleLabel, _addModeButton, _addGroupButton, _removeButton, _addressBarContainer, _treeViewContainer);

            // Add event handlers
            Observable.FromEvent(x => _addModeButton.clicked += x, x => _addModeButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnAddButtonClicked(AddMenuItemType.Mode)).AddTo(_disposables);
            Observable.FromEvent(x => _addGroupButton.clicked += x, x => _addGroupButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnAddButtonClicked(AddMenuItemType.Group)).AddTo(_disposables);
            Observable.FromEvent(x => _removeButton.clicked += x, x => _removeButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnRemoveButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _addressBarContainer.onGUIHandler += x, x => _addressBarContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => _addressBarElement?.OnGUI(_addressBarContainer.contentRect)).AddTo(_disposables);
            Observable.FromEvent(x => _treeViewContainer.onGUIHandler += x, x => _treeViewContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => _menuItemTreeElement?.OnGUI(_treeViewContainer.contentRect)).AddTo(_disposables);
            _treeViewContainer.RegisterCallback<MouseLeaveEvent>(OnMouseLeft);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void InitializeTreeElement()
        {
            var menu = _menuItemTreeElement?.Menu;

            _treeElementDisposables.Dispose();
            _treeElementDisposables = new CompositeDisposable();

            // Initialize tree element
            if (_menuItemListViewState.TreeViewState is null)
            {
                _menuItemListViewState.TreeViewState = new TreeViewState();
            }
            _menuItemTreeElement = new MenuItemTreeElement(_localizationSetting, _thumbnailDrawer, _menuItemListViewState).AddTo(_treeElementDisposables);

            // Tree element event handlers
            _menuItemTreeElement.OnModePropertiesModified.Synchronize().Subscribe(OnModePropertiesModified).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnGroupPropertiesModified.Synchronize().Subscribe(OnGroupPropertiesModified).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnSelectionChanged.Synchronize().Subscribe(x => OnSelectionChanged(x.menu, x.menuItemIds)).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnDropped.Synchronize().Subscribe(OnDropped).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnRootChanged.Synchronize().Subscribe(OnRootChanged).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnAnimationChanged.Synchronize().Subscribe(OnAnimationChanged).AddTo(_treeElementDisposables);

            // Setup
            _menuItemTreeElement.Setup(menu);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _titleLabel.text = localizationTable.MenuItemListView_Title;
        }

        private void UpdateButtonState(IMenu menu, IReadOnlyList<string> selectedMenuItemIds)
        {
            if (menu is null)
            {
                _addModeButton?.SetEnabled(false);
                _addGroupButton?.SetEnabled(false);
                _removeButton?.SetEnabled(false);
                return;
            }

            // Add button
            if (_menuItemListViewState.RootGroupId is null)
            {
                _addModeButton?.SetEnabled(false);
                _addGroupButton?.SetEnabled(false);
            }
            else if (_menuItemListViewState.RootGroupId == Domain.Menu.RegisteredId && !menu.Registered.IsFull)
            {
                _addModeButton?.SetEnabled(true);
                _addGroupButton?.SetEnabled(true);
            }
            else if (_menuItemListViewState.RootGroupId == Domain.Menu.UnregisteredId)
            {
                _addModeButton?.SetEnabled(true);
                _addGroupButton?.SetEnabled(true);
            }
            else if (menu.ContainsGroup(_menuItemListViewState.RootGroupId) && !menu.GetGroup(_menuItemListViewState.RootGroupId).IsFull)
            {
                _addModeButton?.SetEnabled(true);
                _addGroupButton?.SetEnabled(true);
            }
            else
            {
                _addModeButton?.SetEnabled(false);
                _addGroupButton?.SetEnabled(false);
            }

            // Remove button
            if (selectedMenuItemIds is null || selectedMenuItemIds.Count != 1 || selectedMenuItemIds[0] is null)
            {
                _removeButton?.SetEnabled(false);
            }
            else if (selectedMenuItemIds[0] == Domain.Menu.RegisteredId || selectedMenuItemIds[0] == Domain.Menu.UnregisteredId)
            {
                _removeButton?.SetEnabled(false);
            }
            else if (menu.ContainsMode(selectedMenuItemIds[0]) || menu.ContainsGroup(selectedMenuItemIds[0]))
            {
                _removeButton?.SetEnabled(true);
            }
            else
            {
                _removeButton?.SetEnabled(false);
            }
        }

        private void UpdateTree(IMenu menu)
        {
            // Do not call this method before setting up the tree element because the selection will be null.
            var selection = _menuItemTreeElement.GetSelectedMenuItemIds();
            var nearest = selection.Select(x => _menuItemTreeElement.GetNearestMenuItemId(x)).ToList();

            _menuItemTreeElement.Setup(menu);

            if (!_menuItemTreeElement.SelectMenuItems(selection))
            {
                _menuItemTreeElement.SelectMenuItems(nearest);
            }

            _addressBarElement.SetPath(menu, _menuItemListViewState.RootGroupId);

            UpdateButtonState(menu, _menuItemTreeElement.GetSelectedMenuItemIds());
        }

        private void OnMenuUpdated(IMenu menu)
        {
            UpdateTree(menu);

            // Send event
            _changeMenuItemListSelectionSubject.OnNext(_menuItemTreeElement.GetSelectedMenuItemIds());
        }

        private void OnHierarchySelectionChanged(IReadOnlyList<string> selectedMenuItemIds)
        {
            if (selectedMenuItemIds.Count == 1)
            {
                _menuItemTreeElement.ChangeRootGroup(selectedMenuItemIds[0]);
                UpdateTree(_menuItemTreeElement.Menu);
            }
        }

        private void OnAddressClicked(string rootGroupId)
        {
            _menuItemTreeElement.ChangeRootGroup(rootGroupId);
            UpdateTree(_menuItemTreeElement.Menu);
        }

        private void OnMouseLeft(MouseLeaveEvent mouseLeaveEvent)
        {
            // Eliminate drag position display
            if (Event.current.type == EventType.DragUpdated)
            {
                InitializeTreeElement();
            }
        }

        private void OnModePropertiesModified(
            (string modeId,
            string displayName,
            bool? useAnimationNameAsDisplayName,
            EyeTrackingControl? eyeTrackingControl,
            MouthTrackingControl? mouthTrackingControl,
            bool? blinkEnabled,
            bool? mouthMorphCancelerEnabled) args)
        {
            _modifyModePropertiesUseCase.Handle("",
                args.modeId,
                args.displayName,
                args.useAnimationNameAsDisplayName,
                args.eyeTrackingControl,
                args.mouthTrackingControl,
                args.blinkEnabled,
                args.mouthMorphCancelerEnabled);
        }

        private void OnGroupPropertiesModified((string groupId, string displayName) args)
        {
            _modifyGroupPropertiesUseCase.Handle("", args.groupId, args.displayName);
        }

        private void OnRootChanged(string rootGroupId)
        {
            _changeMenuItemListRootSubject.OnNext(rootGroupId);
            _addressBarElement.SetPath(_menuItemTreeElement.Menu, rootGroupId);
            _thumbnailDrawer.ResetPriority();
        }

        private void OnAnimationChanged((string modeId, string clipGUID) args)
        {
            _setExistingAnimationUseCase.Handle("", new Domain.Animation(args.clipGUID), args.modeId);
        }

        private void OnSelectionChanged(IMenu menu, IReadOnlyList<string> selectedMenuItemIds)
        {
            _changeMenuItemListSelectionSubject.OnNext(selectedMenuItemIds);
            UpdateTree(menu);
        }

        private void OnDropped((IReadOnlyList<string> source, string destination, int? index) args)
        {
            if (args.source is null || args.source.Count == 0)
            {
                return;
            }

            var destination = args.destination ?? _menuItemListViewState.RootGroupId;
            if (destination is string)
            {
                _moveMenuItemUseCase.Handle("", args.source, destination, args.index);
            }
        }

        private void OnAddButtonClicked(AddMenuItemType addMenuItemType)
        {
            if (_menuItemListViewState.RootGroupId is string)
            {
                _addMenuItemUseCase.Handle("", _menuItemListViewState.RootGroupId, addMenuItemType);
            }
        }

        private void OnRemoveButtonClicked()
        {
            var ids = _menuItemTreeElement.GetSelectedMenuItemIds();
            if (ids is IReadOnlyList<string> && ids.Count == 1)
            {
                _removeMenuItemUseCase.Handle("", ids[0]);
            }
        }

        private void OnAddMenuItemPresenterCompleted(
            (AddMenuItemResult addMenuItemResult, string addedItemId, IMenu menu, string errorMessage) args)
        {
            if (args.addMenuItemResult == AddMenuItemResult.Succeeded)
            {
                UpdateTree(args.menu);
            }
        }

        private void OnRemoveMenuItemPresenterCompleted(
            (RemoveMenuItemResult removeMenuItemResult, string removedItemId, IMenu menu, string errorMessage) args)
        {
            if (args.removeMenuItemResult == RemoveMenuItemResult.Succeeded)
            {
                UpdateTree(args.menu);
            }
        }

        private void OnModifyModePropertiesPresenterCompleted(
            (ModifyModePropertiesResult modifyModePropertiesResult, string modifiedModeId, IMenu menu, string errorMessage) args)
        {
            if (args.modifyModePropertiesResult == ModifyModePropertiesResult.Succeeded)
            {
                UpdateTree(args.menu);
            }
        }

        private void OnModifyGroupPropertiesPresenterCompleted(
            (ModifyGroupPropertiesResult modifyGroupPropertiesResult, string modifiedGroupId, IMenu menu, string errorMessage) args)
        {
            if (args.modifyGroupPropertiesResult == ModifyGroupPropertiesResult.Succeeded)
            {
                UpdateTree(args.menu);
            }
        }

        private void OnMoveMenuItemPresenterCompleted(
            (MoveMenuItemResult moveMenuItemResult, IMenu menu, string errorMessage) args)
        {
            if (args.moveMenuItemResult == MoveMenuItemResult.Succeeded)
            {
                UpdateTree(args.menu);
            }
        }

        private void OnSetExistingAnimationPresenterCompleted(
            (SetExistingAnimationResult setExistingAnimationResult, IMenu menu, string errorMessage) args)
        {
            if (args.setExistingAnimationResult == SetExistingAnimationResult.Succeeded)
            {
                UpdateTree(args.menu);
            }
        }
    }
}
