using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.UseCase.ModifyMenu;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Components.States;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Detail.View.Element;
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

namespace Suzuryg.FaceEmo.Detail.View
{
    public class MenuItemListView : IDisposable
    {
        private IAddMenuItemUseCase _addMenuItemUseCase;
        private ICopyMenuItemUseCase _copyMenuItemUseCase;
        private IRemoveMenuItemUseCase _removeMenuItemUseCase;
        private IModifyModePropertiesUseCase _modifyModePropertiesUseCase;
        private IModifyGroupPropertiesUseCase _modifyGroupPropertiesUseCase;
        private IMoveMenuItemUseCase _moveMenuItemUseCase;
        private ISetExistingAnimationUseCase _setExistingAnimationUseCase;

        private IAddMenuItemPresenter _addMenuItemPresenter;
        private ICopyMenuItemPresenter _copyMenuItemPresenter;
        private IRemoveMenuItemPresenter _removeMenuItemPresenter;

        private IReadOnlyLocalizationSetting _localizationSetting;
        private LocalizationTable _localizationTable;

        private UpdateMenuSubject _updateMenuSubject;
        private SelectionSynchronizer _selectionSynchronizer;
        private MainThumbnailDrawer _thumbnailDrawer;
        private AV3Setting _aV3Setting;
        private ThumbnailSetting _thumbnailSetting;
        private MenuItemListViewState _menuItemListViewState;

        private Label _titleLabel;
        private Toggle _hierarchyViewToggle;
        private Button _addModeButton;
        private Button _addGroupButton;
        private Button _copyButton;
        private Button _removeButton;
        private IMGUIContainer _addressBarContainer;
        private IMGUIContainer _treeViewContainer;

        private DefaultsProviderGenerator _defaultProviderGenerator;
        private ModeNameProvider _modeNameProvider;
        private AnimationElement _animationElement;
        private AddressBarElement _addressBarElement;
        private MenuItemTreeElement _menuItemTreeElement;

        private CompositeDisposable _disposables = new CompositeDisposable();
        private CompositeDisposable _treeElementDisposables = new CompositeDisposable();

        public MenuItemListView(
            IAddMenuItemUseCase addMenuItemUseCase,
            ICopyMenuItemUseCase copyMenuItemUseCase,
            IRemoveMenuItemUseCase removeMenuItemUseCase,
            IModifyModePropertiesUseCase modifyModePropertiesUseCase,
            IModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase,
            IMoveMenuItemUseCase moveMenuItemUseCase,
            ISetExistingAnimationUseCase setExistingAnimationUseCase,

            IAddMenuItemPresenter addMenuItemPresenter,
            ICopyMenuItemPresenter copyMenuItemPresenter,
            IRemoveMenuItemPresenter removeMenuItemPresenter,

            IReadOnlyLocalizationSetting localizationSetting,
            UpdateMenuSubject updateMenuSubject,
            SelectionSynchronizer selectionSynchronizer,
            MainThumbnailDrawer thumbnailDrawer,
            AV3Setting aV3Setting,
            ThumbnailSetting thumbnailSetting,
            MenuItemListViewState menuItemListViewState,
            DefaultsProviderGenerator defaultProviderGenerator,
            ModeNameProvider modeNameProvider,
            AnimationElement animationElement)
        {
            // Usecases
            _addMenuItemUseCase = addMenuItemUseCase;
            _copyMenuItemUseCase = copyMenuItemUseCase;
            _removeMenuItemUseCase = removeMenuItemUseCase;
            _modifyModePropertiesUseCase = modifyModePropertiesUseCase;
            _modifyGroupPropertiesUseCase = modifyGroupPropertiesUseCase;
            _moveMenuItemUseCase = moveMenuItemUseCase;
            _setExistingAnimationUseCase = setExistingAnimationUseCase;

            // Presenters
            _addMenuItemPresenter = addMenuItemPresenter;
            _copyMenuItemPresenter = copyMenuItemPresenter;
            _removeMenuItemPresenter = removeMenuItemPresenter;

            // Others
            _localizationSetting = localizationSetting;
            _updateMenuSubject = updateMenuSubject;
            _selectionSynchronizer = selectionSynchronizer;
            _thumbnailDrawer = thumbnailDrawer;
            _aV3Setting = aV3Setting;
            _thumbnailSetting = thumbnailSetting;
            _menuItemListViewState = menuItemListViewState;
            _defaultProviderGenerator = defaultProviderGenerator;
            _modeNameProvider = modeNameProvider;
            _animationElement = animationElement;

            // Address bar element
            _addressBarElement = new AddressBarElement(_localizationSetting).AddTo(_disposables);
            _addressBarElement.OnAddressClicked.Synchronize().Subscribe(OnEnteredIntoGroup).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(x => OnMenuUpdated(x.menu)).AddTo(_disposables);

            // Synchronize selection event handler
            _selectionSynchronizer.OnSynchronizeSelection.Synchronize().Subscribe(OnSynchronizeSelection).AddTo(_disposables);

            // Repaint thumbnail event handler
            _thumbnailDrawer.OnThumbnailUpdated.Synchronize().ObserveOnMainThread().Subscribe(_ => _treeViewContainer?.MarkDirtyRepaint()).AddTo(_disposables);

            // Presenter event handlers
            _addMenuItemPresenter.Observable.Synchronize().Subscribe(OnAddMenuItemPresenterCompleted).AddTo(_disposables);
            _copyMenuItemPresenter.Observable.Synchronize().Subscribe(OnCopyMenuItemPresenterCompleted).AddTo(_disposables);
            _removeMenuItemPresenter.Observable.Synchronize().Subscribe(OnRemoveMenuItemPresenterCompleted).AddTo(_disposables);

            // Initialize tree element
            InitializeTreeElement();

        }

        public void Dispose()
        {
            _treeViewContainer?.UnregisterCallback<MouseLeaveEvent>(OnMouseLeft);
            _hierarchyViewToggle?.UnregisterValueChangedCallback(OnHierarchyViewToggleValueChanged);
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
            _hierarchyViewToggle = root.Q<Toggle>("HierarchyViewToggle");
            _addModeButton = root.Q<Button>("AddModeButton");
            _addGroupButton = root.Q<Button>("AddGroupButton");
            _copyButton = root.Q<Button>("CopyButton");
            _removeButton = root.Q<Button>("RemoveButton");
            _addressBarContainer = root.Q<IMGUIContainer>("AddressBarContainer");
            _treeViewContainer = root.Q<IMGUIContainer>("TreeViewContainer");
            NullChecker.Check(_titleLabel, _addModeButton, _addGroupButton, _copyButton, _removeButton, _addressBarContainer, _treeViewContainer);

            // Add event handlers
            Observable.FromEvent(x => _addModeButton.clicked += x, x => _addModeButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnAddButtonClicked(AddMenuItemType.Mode)).AddTo(_disposables);
            Observable.FromEvent(x => _addGroupButton.clicked += x, x => _addGroupButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnAddButtonClicked(AddMenuItemType.Group)).AddTo(_disposables);
            Observable.FromEvent(x => _copyButton.clicked += x, x => _copyButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnCopyButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _removeButton.clicked += x, x => _removeButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnRemoveButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _addressBarContainer.onGUIHandler += x, x => _addressBarContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => _addressBarElement?.OnGUI(_addressBarContainer.contentRect)).AddTo(_disposables);
            Observable.FromEvent(x => _treeViewContainer.onGUIHandler += x, x => _treeViewContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => _menuItemTreeElement?.OnGUI(_treeViewContainer.contentRect)).AddTo(_disposables);
            _treeViewContainer.RegisterCallback<MouseLeaveEvent>(OnMouseLeft);

            _hierarchyViewToggle.value = EditorPrefsStore.HierarchyViewVisible;
            _hierarchyViewToggle.RegisterValueChangedCallback(OnHierarchyViewToggleValueChanged);

            // Set icon
            SetIcon();

            // Set text
            SetText(_localizationSetting.Table);
        }

        public float GetWidth()
        {
            if (_menuItemTreeElement != null) { return _menuItemTreeElement.GetMinWidth(); }
            else { return 100; }
        }

        private void InitializeTreeElement()
        {
            var menu = _menuItemTreeElement?.Menu;

            _treeElementDisposables.Dispose();
            _treeElementDisposables = new CompositeDisposable();

            // Initialize tree element
            if (_menuItemListViewState.TreeViewState == null)
            {
                _menuItemListViewState.TreeViewState = new TreeViewState();
            }
            _menuItemTreeElement = new MenuItemTreeElement(_localizationSetting, _modeNameProvider, _animationElement, _thumbnailDrawer,
                _aV3Setting, _thumbnailSetting, _menuItemListViewState).AddTo(_treeElementDisposables);

            // Tree element event handlers
            _menuItemTreeElement.OnModePropertiesModified.Synchronize().Subscribe(OnModePropertiesModified).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnGroupPropertiesModified.Synchronize().Subscribe(OnGroupPropertiesModified).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnSelectionChanged.Synchronize().Subscribe(x => OnSelectionChanged(x.menu, x.menuItemIds)).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnDropped.Synchronize().Subscribe(OnDropped).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnEnteredIntoGroup.Synchronize().Subscribe(OnEnteredIntoGroup).AddTo(_treeElementDisposables);
            _menuItemTreeElement.OnAnimationChanged.Synchronize().Subscribe(OnAnimationChanged).AddTo(_treeElementDisposables);

            // Update display
            _menuItemTreeElement.Setup(menu);
            UpdateDisplay();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
            if (_titleLabel != null) { _titleLabel.text = localizationTable.MenuItemListView_Title; }
            if (_hierarchyViewToggle != null) { _hierarchyViewToggle.text = localizationTable.MenuItemListView_TreeViewToggle; }

            if (_addModeButton != null) { _addModeButton.tooltip = localizationTable.MenuItemListView_Tooltip_AddMode; }
            if (_addGroupButton != null) { _addGroupButton.tooltip = localizationTable.MenuItemListView_Tooltip_AddGroup; }
            if (_copyButton != null) { _copyButton.tooltip = localizationTable.MenuItemListView_Tooltip_Copy; }
            if (_removeButton != null) { _removeButton.tooltip = localizationTable.MenuItemListView_Tooltip_Delete ; }
        }

        private void SetIcon()
        {
            if (_addModeButton != null)     { _addModeButton.Add(ViewUtility.GetIconElement("note_add_FILL0_wght400_GRAD200_opsz48.png")); }
            if (_addGroupButton != null)    { _addGroupButton.Add(ViewUtility.GetIconElement("create_new_folder_FILL0_wght400_GRAD200_opsz48.png")); }
            if (_copyButton != null)        { _copyButton.Add(ViewUtility.GetIconElement("content_copy_FILL0_wght400_GRAD200_opsz48.png")); }
            if (_removeButton != null)      { _removeButton.Add(ViewUtility.GetIconElement("delete_FILL0_wght400_GRAD200_opsz48.png")); }
        }

        private void UpdateDisplay()
        {
            _addModeButton?.SetEnabled(false);
            _addGroupButton?.SetEnabled(false);
            _copyButton?.SetEnabled(false);
            _removeButton?.SetEnabled(false);

            var menu = _menuItemTreeElement?.Menu;
            var selectedMenuItemIds = _menuItemTreeElement?.GetSelectedMenuItemIds();

            if (menu is null)
            {
                return;
            }

            _addressBarElement?.SetPath(menu, _menuItemListViewState.RootGroupId);

            // Add button
            var registeredFreeSpace = AV3Utility.GetActualRegisteredListFreeSpace(menu, _aV3Setting);
            if (_menuItemListViewState?.RootGroupId is null)
            {
                _addModeButton?.SetEnabled(false);
                _addGroupButton?.SetEnabled(false);
            }
            else if (_menuItemListViewState?.RootGroupId == Domain.Menu.RegisteredId && registeredFreeSpace > 0)
            {
                _addModeButton?.SetEnabled(true);
                _addGroupButton?.SetEnabled(true);
            }
            else if (_menuItemListViewState?.RootGroupId == Domain.Menu.UnregisteredId)
            {
                _addModeButton?.SetEnabled(true);
                _addGroupButton?.SetEnabled(true);
            }
            else if (menu.ContainsGroup(_menuItemListViewState?.RootGroupId) && !menu.GetGroup(_menuItemListViewState?.RootGroupId).IsFull)
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

            // Copy button
            if (_addModeButton != null && _addGroupButton != null && _copyButton != null && _removeButton != null)
            {
                _copyButton?.SetEnabled(_addModeButton.enabledSelf && _addGroupButton.enabledSelf && _removeButton.enabledSelf);
            }
            else
            {
                _copyButton?.SetEnabled(false);
            }
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _menuItemTreeElement?.Setup(menu);
            UpdateDisplay();
        }

        private void OnSelectionChanged(IMenu menu, IReadOnlyList<string> selectedMenuItemIds)
        {
            if (selectedMenuItemIds.Count == 1)
            {
                _selectionSynchronizer.ChangeMenuItemListViewSelection(selectedMenuItemIds[0]);
            }
            else
            {
                UpdateDisplay();
            }
        }

        private void OnEnteredIntoGroup(string groupId)
        {
            _selectionSynchronizer.ChangeHierarchyViewSelection(groupId);
        }

        private void OnSynchronizeSelection(ViewSelection viewSelection)
        {
            if (_menuItemTreeElement?.Menu is IMenu menu)
            {
                string rootGroupId = null;
                if (viewSelection.HierarchyView == Domain.Menu.RegisteredId ||
                    viewSelection.HierarchyView == Domain.Menu.UnregisteredId ||
                    menu.ContainsGroup(viewSelection.HierarchyView))
                {
                    rootGroupId = viewSelection.HierarchyView;
                }
                else if (menu.ContainsGroup(viewSelection.MenuItemListView))
                {
                    rootGroupId = menu.GetGroup(viewSelection.MenuItemListView).Parent.GetId();
                }
                else if (menu.ContainsMode(viewSelection.MenuItemListView))
                {
                    rootGroupId = menu.GetMode(viewSelection.MenuItemListView).Parent.GetId();
                }

                _menuItemTreeElement.ChangeRootGroup(rootGroupId);
            }

            _menuItemTreeElement?.SelectMenuItems(new[] { viewSelection.MenuItemListView });
            UpdateDisplay();
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
            bool? changeDefaultFace,
            string displayName,
            bool? useAnimationNameAsDisplayName,
            EyeTrackingControl? eyeTrackingControl,
            MouthTrackingControl? mouthTrackingControl,
            bool? blinkEnabled,
            bool? mouthMorphCancelerEnabled) args)
        {
            _modifyModePropertiesUseCase.Handle(menuId: "", modeId: args.modeId,
                changeDefaultFace: args.changeDefaultFace,
                displayName: args.displayName,
                useAnimationNameAsDisplayName: args.useAnimationNameAsDisplayName,
                eyeTrackingControl: args.eyeTrackingControl,
                mouthTrackingControl: args.mouthTrackingControl,
                blinkEnabled: args.blinkEnabled,
                mouthMorphCancelerEnabled: args.mouthMorphCancelerEnabled);
        }

        private void OnGroupPropertiesModified((string groupId, string displayName) args)
        {
            _modifyGroupPropertiesUseCase.Handle("", args.groupId, args.displayName);
        }

        private void OnAnimationChanged((string modeId, string clipGUID) args)
        {
            _setExistingAnimationUseCase.Handle("", new Domain.Animation(args.clipGUID), args.modeId);
        }

        private void OnDropped((IReadOnlyList<string> source, string destination, int? index) args)
        {
            if (args.source is null || args.source.Count == 0)
            {
                return;
            }

            var destination = args.destination ?? _menuItemListViewState.RootGroupId;
            if (destination is null)
            {
                return;
            }

            var menu = (_menuItemTreeElement.Menu as Domain.Menu); // TODO: Add usecase
            if (menu is null || !menu.CanMoveMenuItemTo(args.source, destination))
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_InvalidDestination, "OK");
                return;
            }

            _moveMenuItemUseCase.Handle("", args.source, destination, args.index);
        }

        private void OnAddButtonClicked(AddMenuItemType addMenuItemType)
        {
            if (_menuItemListViewState.RootGroupId is string)
            {
                if (addMenuItemType == AddMenuItemType.Group)
                {
                    _addMenuItemUseCase.Handle("", _menuItemListViewState.RootGroupId, addMenuItemType,
                        displayName: _localizationTable.ModeNameProvider_NewGroup,
                        defaultsProvider: _defaultProviderGenerator.Generate());
                }
                else
                {
                    _addMenuItemUseCase.Handle("", _menuItemListViewState.RootGroupId, addMenuItemType,
                        displayName: _localizationTable.ModeNameProvider_NewMode,
                        defaultsProvider: _defaultProviderGenerator.Generate());
                }
            }
        }

        private void OnCopyButtonClicked()
        {
            var ids = _menuItemTreeElement.GetSelectedMenuItemIds();
            if (ids is IReadOnlyList<string> && ids.Count == 1)
            {
                var id = ids[0];
                var menu = _menuItemTreeElement.Menu;
                var copiedName = "_copy";

                if (menu.ContainsGroup(id))
                {
                    copiedName = menu.GetGroup(id).DisplayName + copiedName;
                }
                else if (menu.ContainsMode(id))
                {
                    copiedName = menu.GetMode(id).DisplayName + copiedName;
                }

                _copyMenuItemUseCase.Handle(id, copiedName);
            }
        }

        private void OnRemoveButtonClicked()
        {
            var ids = _menuItemTreeElement.GetSelectedMenuItemIds();
            if (ids is IReadOnlyList<string> && ids.Count == 1)
            {
                var id = ids[0];
                var menu = _menuItemTreeElement.Menu;

                if (menu.ContainsGroup(id))
                {
                    var groupName = menu.GetGroup(id).DisplayName;

                    var ok = OptoutableDialog.Show(DomainConstants.SystemName,
                        _localizationTable.Common_Message_DeleteGroup + "\n\n" + groupName,
                        _localizationTable.Common_Delete, _localizationTable.Common_Cancel,
                        DetailConstants.KeyGroupDeleteConfirmation, DetailConstants.DefaultGroupDeleteConfirmation, isRiskyAction: true);

                    if (!ok) { return; }
                }
                else if (menu.ContainsMode(id))
                {
                    var modeName = _modeNameProvider.Provide(menu.GetMode(id));
                    var ok = OptoutableDialog.Show(DomainConstants.SystemName,
                        _localizationTable.Common_Message_DeleteMode + "\n\n" + modeName,
                        _localizationTable.Common_Delete, _localizationTable.Common_Cancel,
                        DetailConstants.KeyModeDeleteConfirmation, DetailConstants.DefaultModeDeleteConfirmation, isRiskyAction: true);
                    if (!ok) { return; }
                }

                _removeMenuItemUseCase.Handle("", id);
            }
        }

        private void OnAddMenuItemPresenterCompleted(
            (AddMenuItemResult addMenuItemResult, string addedItemId, IMenu menu, string errorMessage) args)
        {
            if (args.addMenuItemResult == AddMenuItemResult.Succeeded)
            {
                _selectionSynchronizer.ChangeMenuItemListViewSelection(args.addedItemId);
            }
        }

        private void OnCopyMenuItemPresenterCompleted(
            (CopyMenuItemResult copyMenuItemResult, string copiedItemId, IMenu menu, string errorMessage) args)
        {
            if (args.copyMenuItemResult == CopyMenuItemResult.Succeeded)
            {
                _selectionSynchronizer.ChangeMenuItemListViewSelection(args.copiedItemId);
            }
        }

        private void OnRemoveMenuItemPresenterCompleted(
            (RemoveMenuItemResult removeMenuItemResult, string removedItemId, IReadOnlyList<string> orderBeforeDeletion, IMenu menu, string errorMessage) args)
        {
            if (args.removeMenuItemResult == RemoveMenuItemResult.Succeeded)
            {
                var id = string.Empty;

                var index = args.orderBeforeDeletion.ToList().IndexOf(args.removedItemId);
                var root = _menuItemTreeElement.GetRootMenuItemList();
                if (root?.Order?.Count > 0)
                {
                    if (0 <= index && index < root.Order.Count)
                    {
                        id = root.Order[index];
                    }
                    else if (index >= root.Order.Count)
                    {
                        id = root.Order.LastOrDefault();
                    }
                    else
                    {
                        id = root.Order.FirstOrDefault();
                    }
                }

                _selectionSynchronizer.ChangeMenuItemListViewSelection(id);
            }
        }

        private void OnHierarchyViewToggleValueChanged(ChangeEvent<bool> changeEvent)
        {
            EditorPrefsStore.HierarchyViewVisible = changeEvent.newValue;
        }
    }
}
