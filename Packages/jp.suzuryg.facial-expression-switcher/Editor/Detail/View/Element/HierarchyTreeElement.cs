using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
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

namespace Suzuryg.FacialExpressionSwitcher.Detail.View.Element
{
    public class HierarchyTreeElement : TreeView
    {
        private static readonly int RootId = int.MinValue;

        public IObservable<(string menuItemId, string displayName)> OnGroupRenamed => _onGroupRenamed.AsObservable();
        public IObservable<(IMenu menu, IReadOnlyList<string> menuItemIds)> OnSelectionChanged => _onSelectionChanged.AsObservable();
        public IObservable<(IReadOnlyList<string> source, string destination, int? index)> OnDropped => _onDropped.AsObservable();

        private IMenu _menu;
        private Dictionary<string, int> _menuItemIdToElementId = new Dictionary<string, int>();
        private Subject<(string menuItemId, string displayName)> _onGroupRenamed = new Subject<(string menuItemId, string displayName)>();
        private Subject<(IMenu menu, IReadOnlyList<string> menuItemIds)> _onSelectionChanged = new Subject<(IMenu menu, IReadOnlyList<string> menuItemIds)>();
        private Subject<(IReadOnlyList<string> source, string destination, int? index)> _onDropped = new Subject<(IReadOnlyList<string> source, string destination, int? index)>();

        private string Text_RegisteredMenuItemList;
        private string Text_UnregisteredMenuItemList;

        private Texture2D _openFolder;
        private Texture2D _closeFolder;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public HierarchyTreeElement(IReadOnlyLocalizationSetting localizationSetting, TreeViewState treeViewState) : base(treeViewState)
        {
            Action<LocalizationTable> setText = loc =>
            {
                Text_RegisteredMenuItemList = loc.HierarchyView_RegisteredMenuItemList;
                Text_UnregisteredMenuItemList = loc.HierarchyView_UnregisteredMenuItemList;
                Reload();
            };
            setText(localizationSetting.Table);
            localizationSetting.OnTableChanged.Synchronize().Subscribe(setText).AddTo(_disposables);

            _openFolder = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/folder_open_FILL0_wght400_GRAD200_opsz20.png");
            _closeFolder = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/folder_FILL0_wght400_GRAD200_opsz20.png");
            NullChecker.Check(_openFolder, _closeFolder);
        }

        public void Setup(IMenu menu)
        {
            _menu = menu;
            Reload();
        }

        public IReadOnlyList<string> GetSelectedMenuItemIds() => GetMenuItemIds(GetSelection());

        private IReadOnlyList<string> GetMenuItemIds(IList<int> elementIds)
        {
            if (elementIds is null || elementIds.Count == 0 || _menuItemIdToElementId.Count == 0)
            {
                return null;
            }

            var menuItemIds = new List<string>();
            foreach (var elementId in elementIds)
            {
                var matched = _menuItemIdToElementId.Where(x => x.Value == elementId);
                if (matched is null || matched.Count() == 0)
                {
                    return null;
                }
                menuItemIds.Add(matched.First().Key);
            }

            return menuItemIds;
        }

        // TODO: Use presenter's argument
        public void ExpandSelectedElement()
        {
            var selection = GetSelection();
            if (selection is null || selection.Count == 0)
            {
                return;
            }

            var elementId = selection[0];
            SetExpanded(elementId, true);
        }

        // TODO: Use presenter's argument
        public void SelectNearest()
        {
            var selection = GetSelection();
            if (selection is null || selection.Count == 0)
            {
                return;
            }

            var elementId = selection[0];
            var element = FindItem(elementId, rootItem);
            if (element is null || element.parent is null)
            {
                return;
            }

            for (int i = 0; i < element.parent.children.Count; i++)
            {
                var child = element.parent.children[i];
                if (child.id == element.id)
                {
                    if (i + 1 < element.parent.children.Count)
                    {
                        SetSelection(new List<int>(){ element.parent.children[i + 1].id });
                    }
                    else if (i - 1 >= 0)
                    {
                        SetSelection(new List<int>(){ element.parent.children[i - 1].id });
                    }
                    else
                    {
                        SetSelection(new List<int>(){ element.parent.id });
                    }
                    break;
                }
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem { id = RootId, depth = -1, displayName = "Root" };
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();

            var registeredTree = new TreeViewItem { id = GetId(Domain.Menu.RegisteredId), displayName = Text_RegisteredMenuItemList };
            root.AddChild(registeredTree);
            rows.Add(registeredTree);
            if (IsExpanded(registeredTree.id) && _menu is IMenu)
            {
                registeredTree.icon = _openFolder;
                AddChildrenRecursive(_menu.Registered, registeredTree, rows);
            }
            else
            {
                registeredTree.icon = _closeFolder;
                registeredTree.children = CreateChildListForCollapsedParent();
            }

            var unregisteredTree = new TreeViewItem { id = GetId(Domain.Menu.UnregisteredId), displayName = Text_UnregisteredMenuItemList };
            root.AddChild(unregisteredTree);
            rows.Add(unregisteredTree);
            if (IsExpanded(unregisteredTree.id) && _menu is IMenu)
            {
                unregisteredTree.icon = _openFolder;
                AddChildrenRecursive(_menu.Unregistered, unregisteredTree, rows);
            }
            else
            {
                unregisteredTree.icon = _closeFolder;
                unregisteredTree.children = CreateChildListForCollapsedParent();
            }

            SetupDepthsFromParentsAndChildren(root);

            return rows;
        }

        private void AddChildrenRecursive (IMenuItemList menuItemList, TreeViewItem item, IList<TreeViewItem> rows)
        {
            foreach (var menuItemId in menuItemList.Order)
            {
                if (menuItemList.GetType(menuItemId) == MenuItemType.Group)
                {
                    var group = menuItemList.GetGroup(menuItemId);
                    var groupTree = new TreeViewItem { id = GetId(menuItemId), displayName = group.DisplayName };
                    item.AddChild(groupTree);
                    rows.Add(groupTree);
                    if (IsExpanded(groupTree.id))
                    {
                        groupTree.icon = _openFolder;
                        AddChildrenRecursive(group, groupTree, rows);
                    }
                    else
                    {
                        groupTree.icon = _closeFolder;
                        groupTree.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        private int GetId(string menuItemId)
        {
            if (_menuItemIdToElementId.ContainsKey(menuItemId))
            {
                return _menuItemIdToElementId[menuItemId];
            }

            var seed = menuItemId;
            int newId = seed.GetHashCode();
            while (_menuItemIdToElementId.ContainsValue(newId) || newId == RootId)
            {
                seed = seed + menuItemId;
                newId = seed.GetHashCode();
            }
            _menuItemIdToElementId[menuItemId] = newId;
            return newId;
        }

        protected override bool CanMultiSelect(TreeViewItem item) => true;

        protected override bool CanRename(TreeViewItem item)
        {
            return item.id != _menuItemIdToElementId[Domain.Menu.RegisteredId] && item.id != _menuItemIdToElementId[Domain.Menu.UnregisteredId];
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (args.acceptedRename)
            {
                var menuItemIds = GetMenuItemIds(new List<int>() { args.itemID });
                if (menuItemIds is IReadOnlyList<string> && menuItemIds.Count == 1)
                {
                    _onGroupRenamed.OnNext((menuItemIds[0], args.newName));
                }
            }
        }

        public void ChangeSelectionDummy() => SelectionChanged(GetSelection());

        protected override void SelectionChanged(IList<int> selectedIds) => _onSelectionChanged.OnNext((_menu, GetSelectedMenuItemIds()));

        protected override bool CanBeParent(TreeViewItem item) => true;

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            if (args.draggedItemIDs is null || args.draggedItemIDs.Count == 0)
            {
                return false;
            }
            else
            {
                if (args.draggedItemIDs.Contains(GetId(Domain.Menu.RegisteredId)) || args.draggedItemIDs.Contains(GetId(Domain.Menu.UnregisteredId)))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (args.draggedItemIDs is null || args.draggedItemIDs.Count == 0)
            {
                return;
            }

            var menuItemIds = GetMenuItemIds(SortItemIDsInRowOrder(args.draggedItemIDs));
            if (menuItemIds is null || menuItemIds.Count == 0)
            {
                return;
            }

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(DetailConstants.DragAndDropDataKey_MenuItemIds, menuItemIds);
            DragAndDrop.StartDrag(DetailConstants.DragAndDropDataKey_MenuItemIds);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (args.performDrop)
            {
                var genericData = DragAndDrop.GetGenericData(DetailConstants.DragAndDropDataKey_MenuItemIds);

                if (genericData is IReadOnlyList<string> menuItemIds && menuItemIds.Count > 0)
                {
                    if (args.dragAndDropPosition == DragAndDropPosition.UponItem)
                    {
                        var parentMenuItemId = GetMenuItemIds(new List<int>() { args.parentItem.id });
                        if (parentMenuItemId is IReadOnlyList<string> && parentMenuItemId.Count == 1)
                        {
                            _onDropped.OnNext((menuItemIds, parentMenuItemId[0], null));
                        }
                    }
                    else if (args.dragAndDropPosition == DragAndDropPosition.BetweenItems)
                    {
                        var parentMenuItemId = GetMenuItemIds(new List<int>() { args.parentItem.id });
                        if (parentMenuItemId is IReadOnlyList<string> && parentMenuItemId.Count == 1)
                        {
                            _onDropped.OnNext((menuItemIds, parentMenuItemId[0], args.insertAtIndex));
                        }
                    }
                }
                else
                {
                    return DragAndDropVisualMode.None;
                }
            }

            return DragAndDropVisualMode.Move;
        }
    }
}
