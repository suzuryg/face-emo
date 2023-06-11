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
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class HierarchyView : IDisposable
    {
        private IAddMenuItemUseCase _addMenuItemUseCase;
        private ICopyMenuItemUseCase _copyMenuItemUseCase;
        private IRemoveMenuItemUseCase _removeMenuItemUseCase;
        private IModifyModePropertiesUseCase _modifyModePropertiesUseCase;
        private IModifyGroupPropertiesUseCase _modifyGroupPropertiesUseCase;
        private IMoveMenuItemUseCase _moveMenuItemUseCase;

        private IReadOnlyLocalizationSetting _localizationSetting;
        private LocalizationTable _localizationTable;

        private DefaultsProviderGenerator _defaultProviderGenerator;
        private ModeNameProvider _modeNameProvider;
        private UpdateMenuSubject _updateMenuSubject;
        private SelectionSynchronizer _selectionSynchronizer;
        private AV3Setting _aV3Setting;
        private HierarchyViewState _hierarchyViewState;

        private Label _titleLabel;
        private Button _addModeButton;
        private Button _addGroupButton;
        private Button _copyButton;
        private Button _removeButton;
        private IMGUIContainer _treeViewContainer;

        private HierarchyTreeElement _hierarchyTreeElement;

        private CompositeDisposable _disposables = new CompositeDisposable();
        private CompositeDisposable _treeElementDisposables = new CompositeDisposable();

        public HierarchyView(
            IAddMenuItemUseCase addMenuItemUseCase,
            ICopyMenuItemUseCase copyMenuItemUseCase,
            IRemoveMenuItemUseCase removeMenuItemUseCase,
            IModifyModePropertiesUseCase modifyModePropertiesUseCase,
            IModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase,
            IMoveMenuItemUseCase moveMenuItemUseCase,

            IReadOnlyLocalizationSetting localizationSetting,
            DefaultsProviderGenerator defaultProviderGenerator,
            ModeNameProvider modeNameProvider,
            UpdateMenuSubject updateMenuSubject,
            SelectionSynchronizer selectionSynchronizer,
            AV3Setting aV3Setting,
            HierarchyViewState hierarchyViewState)
        {
            // Usecases
            _addMenuItemUseCase = addMenuItemUseCase;
            _copyMenuItemUseCase = copyMenuItemUseCase;
            _removeMenuItemUseCase = removeMenuItemUseCase;
            _modifyModePropertiesUseCase = modifyModePropertiesUseCase;
            _modifyGroupPropertiesUseCase = modifyGroupPropertiesUseCase;
            _moveMenuItemUseCase = moveMenuItemUseCase;

            // Others
            _localizationSetting = localizationSetting;
            _defaultProviderGenerator = defaultProviderGenerator;
            _modeNameProvider = modeNameProvider;
            _updateMenuSubject = updateMenuSubject;
            _selectionSynchronizer = selectionSynchronizer;
            _aV3Setting = aV3Setting;
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
            _copyButton = root.Q<Button>("CopyButton");
            _removeButton = root.Q<Button>("RemoveButton");
            _treeViewContainer = root.Q<IMGUIContainer>("TreeViewContainer");
            NullChecker.Check(_titleLabel, _addModeButton, _addGroupButton, _copyButton, _removeButton, _treeViewContainer);

            // Add event handlers
            Observable.FromEvent(x => _addModeButton.clicked += x, x => _addModeButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnAddButtonClicked(AddMenuItemType.Mode)).AddTo(_disposables);
            Observable.FromEvent(x => _addGroupButton.clicked += x, x => _addGroupButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnAddButtonClicked(AddMenuItemType.Group)).AddTo(_disposables);
            Observable.FromEvent(x => _copyButton.clicked += x, x => _copyButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnCopyButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _removeButton.clicked += x, x => _removeButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnRemoveButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _treeViewContainer.onGUIHandler += x, x => _treeViewContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => _hierarchyTreeElement?.OnGUI(_treeViewContainer.contentRect)).AddTo(_disposables);
            _treeViewContainer.RegisterCallback<MouseLeaveEvent>(OnMouseLeft);

            // Set icon
            SetIcon();

            // Set text
            SetText(_localizationSetting.Table);
        }

        public float GetWidth() => 195;

        private void InitializeTreeElement()
        {
            var menu = _hierarchyTreeElement?.Menu;

            _treeElementDisposables.Dispose();
            _treeElementDisposables = new CompositeDisposable();

            // Initialize tree element
            if (_hierarchyViewState.TreeViewState == null)
            {
                _hierarchyViewState.TreeViewState = new TreeViewState();
            }
            _hierarchyTreeElement = new HierarchyTreeElement(_localizationSetting, _modeNameProvider, _hierarchyViewState.TreeViewState).AddTo(_treeElementDisposables);

            // Tree element event handlers
            _hierarchyTreeElement.OnModeRenamed.Synchronize().Subscribe(x => _modifyModePropertiesUseCase.Handle(menuId: "", modeId: x.menuItemId, displayName: x.displayName)).AddTo(_treeElementDisposables);
            _hierarchyTreeElement.OnGroupRenamed.Synchronize().Subscribe(x => _modifyGroupPropertiesUseCase.Handle("", x.menuItemId, x.displayName)).AddTo(_treeElementDisposables);
            _hierarchyTreeElement.OnSelectionChanged.Synchronize().Subscribe(x => OnSelectionChanged(x.menu, x.menuItemIds)).AddTo(_treeElementDisposables);
            _hierarchyTreeElement.OnDropped.Synchronize().Subscribe(OnDropped).AddTo(_treeElementDisposables);

            // Update display
            _hierarchyTreeElement.Setup(menu);
            UpdateDisplay();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
            if (_titleLabel != null) { _titleLabel.text = localizationTable.HierarchyView_Title; }

            if (_addModeButton != null) { _addModeButton.tooltip = localizationTable.HierarchyView_Tooltip_AddMode; }
            if (_addGroupButton != null) { _addGroupButton.tooltip = localizationTable.HierarchyView_Tooltip_AddGroup; }
            if (_copyButton != null) { _copyButton.tooltip = localizationTable.HierarchyView_Tooltip_Copy; }
            if (_removeButton != null) { _removeButton.tooltip = localizationTable.HierarchyView_Tooltip_Delete ; }
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

            var menu = _hierarchyTreeElement?.Menu;
            var selectedMenuItemIds = _hierarchyTreeElement?.GetSelectedMenuItemIds();

            if (menu is null || selectedMenuItemIds is null || selectedMenuItemIds.Count != 1 || selectedMenuItemIds[0] is null)
            {
                return;
            }

            var id = selectedMenuItemIds[0];
            var parentId = GetParentId(menu, id);

            // Add button
            var registeredFreeSpace = AV3Utility.GetActualRegisteredListFreeSpace(menu, _aV3Setting);
            if (parentId == Domain.Menu.RegisteredId)
            {
                _addModeButton?.SetEnabled(registeredFreeSpace > 0);
                _addGroupButton?.SetEnabled(registeredFreeSpace > 0);
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
                _removeButton?.SetEnabled(false);
            }
            else if (id == Domain.Menu.UnregisteredId)
            {
                _removeButton?.SetEnabled(false);
            }
            else if (menu.ContainsGroup(id))
            {
                _removeButton?.SetEnabled(true);
            }
            else if (menu.ContainsMode(id))
            {
                _removeButton?.SetEnabled(true);
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

            var menu = (_hierarchyTreeElement.Menu as Domain.Menu); // TODO: Add usecase
            if (menu is null || !menu.CanMoveMenuItemTo(args.source, args.destination))
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_InvalidDestination, "OK");
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

                if (addMenuItemType == AddMenuItemType.Group)
                {
                    _addMenuItemUseCase.Handle("", parentId, addMenuItemType,
                        displayName: _localizationTable.ModeNameProvider_NewGroup,
                        defaultsProvider: _defaultProviderGenerator.Generate());
                }
                else
                {
                    _addMenuItemUseCase.Handle("", parentId, addMenuItemType,
                        displayName: _localizationTable.ModeNameProvider_NewMode,
                        defaultsProvider: _defaultProviderGenerator.Generate());
                }
            }
        }

        private void OnCopyButtonClicked()
        {
            var ids = _hierarchyTreeElement.GetSelectedMenuItemIds();
            if (ids is IReadOnlyList<string> && ids.Count == 1)
            {
                var id = ids[0];
                var menu = _hierarchyTreeElement.Menu;
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
            var ids = _hierarchyTreeElement.GetSelectedMenuItemIds();
            if (ids is IReadOnlyList<string> && ids.Count == 1)
            {
                var id = ids[0];
                var menu = _hierarchyTreeElement.Menu;

                if (menu.ContainsGroup(id))
                {
                    var groupDeleteConfirmation = EditorPrefs.HasKey(DetailConstants.KeyGroupDeleteConfirmation) ? EditorPrefs.GetBool(DetailConstants.KeyGroupDeleteConfirmation) : DetailConstants.DefaultGroupDeleteConfirmation;
                    if (groupDeleteConfirmation)
                    {
                        var groupName = menu.GetGroup(id).DisplayName;
                        var ok = EditorUtility.DisplayDialog(DomainConstants.SystemName,
                            _localizationTable.Common_Message_DeleteGroup + "\n\n" + groupName,
                            _localizationTable.Common_Delete, _localizationTable.Common_Cancel);
                        if (!ok) { return; }
                    }
                }
                else if (menu.ContainsMode(id))
                {
                    var modeDeleteConfirmation = EditorPrefs.HasKey(DetailConstants.KeyModeDeleteConfirmation) ? EditorPrefs.GetBool(DetailConstants.KeyModeDeleteConfirmation) : DetailConstants.DefaultModeDeleteConfirmation;
                    if (modeDeleteConfirmation)
                    {
                        var modeName = _modeNameProvider.Provide(menu.GetMode(id));
                        var ok = EditorUtility.DisplayDialog(DomainConstants.SystemName,
                            _localizationTable.Common_Message_DeleteMode + "\n\n" + modeName,
                            _localizationTable.Common_Delete, _localizationTable.Common_Cancel);
                        if (!ok) { return; }
                    }
                }

                _removeMenuItemUseCase.Handle("", id);
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
