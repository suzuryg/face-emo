using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Detail.Data;
using Suzuryg.FaceEmo.Detail.Localization;
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

namespace Suzuryg.FaceEmo.Detail.View.Element
{
    public abstract class TreeElementBase : TreeView, IDisposable
    {
        protected static readonly int RootId = int.MinValue;
        protected static readonly string NullMenuItemId = "NullMenuItemId";

        public IObservable<(IReadOnlyList<string> source, string destination, int? index)> OnDropped => _onDropped.AsObservable();
        public IObservable<(IMenu menu, IReadOnlyList<string> menuItemIds)> OnSelectionChanged => _onSelectionChanged.AsObservable();

        public IMenu Menu { get; private set; } 

        protected Subject<(IReadOnlyList<string> source, string destination, int? index)> _onDropped = new Subject<(IReadOnlyList<string> source, string destination, int? index)>();
        protected Subject<(IMenu menu, IReadOnlyList<string> menuItemIds)> _onSelectionChanged = new Subject<(IMenu menu, IReadOnlyList<string> menuItemIds)>();

        protected Dictionary<string, int> _menuItemIdToElementId = new Dictionary<string, int>();
        protected LocalizationTable _localizationTable;

        private TreeViewState _treeViewState;

        protected CompositeDisposable _disposables = new CompositeDisposable();

        public TreeElementBase(IReadOnlyLocalizationSetting localizationSetting, TreeViewState treeViewState) : base(treeViewState)
        {
            _treeViewState = treeViewState;
            SetText(localizationSetting.Table);
            localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);
        }

        public void Dispose()
        {
            RemoveUnusedExpandedId();
            _disposables.Dispose();
        }

        public void Setup(IMenu menu)
        {
            Menu = menu;
            Reload();
        }

        public IReadOnlyList<string> GetSelectedMenuItemIds() => GetSelection().Select(x => GetMenuItemId(x)).ToList();

        public void SelectMenuItems(IReadOnlyList<string> menuItemIds)
        {
            var elements = new List<TreeViewItem>();
            foreach (var menuItemId in menuItemIds)
            {
                var elementId = GetElementId(menuItemId);
                var element = FindItem(elementId, rootItem);
                if (element != null)
                {
                    elements.Add(element);
                }
            }

            if (elements.Count > 0)
            {
                SetSelection(elements.Select(x => x.id).ToList());
            }
            else
            {
                SetSelection(new int[0]);
            }
        }

        public void RevealMenuItem(string menuItemId)
        {
            var path = new List<string>();
            var current = menuItemId;

            while (current is string)
            {
                path.Add(current);
                if (Menu.ContainsMode(current))
                {
                    current = Menu.GetMode(current).Parent.GetId();
                }
                else if (Menu.ContainsGroup(current))
                {
                    current = Menu.GetGroup(current).Parent.GetId();
                }
                else
                {
                    current = null;
                }
            }

            path.Reverse();
            for (int i = 0; i < path.Count - 1; i++)
            {
                SetExpanded(GetElementId(path[i]), true);
                Reload();
            }
        }

        public string GetNearestMenuItemId(string menuItemId)
        {
            var elementId = GetElementId(menuItemId);
            var element = FindItem(elementId, rootItem);
            if (element is null || element.parent is null)
            {
                return null;
            }

            for (int i = 0; i < element.parent.children.Count; i++)
            {
                var child = element.parent.children[i];
                if (child.id == element.id)
                {
                    if (i + 1 < element.parent.children.Count)
                    {
                        return GetMenuItemId(element.parent.children[i + 1].id);
                    }
                    else if (i - 1 >= 0)
                    {
                        return GetMenuItemId(element.parent.children[i - 1].id);
                    }
                    else
                    {
                        return GetMenuItemId(element.parent.id);
                    }
                }
            }

            return null;
        }

        protected virtual void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
            Reload();
        }

        protected int GetElementId(string menuItemId)
        {
            if (menuItemId is null)
            {
                menuItemId = NullMenuItemId;
            }

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

        protected string GetMenuItemId(int elementId)
        {
            var matched = _menuItemIdToElementId.Where(x => x.Value == elementId);
            if (matched is null || matched.Count() == 0)
            {
                return null;
            }
            return matched.First().Key;
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem { id = RootId, depth = -1, displayName = "Root" };
        }

        protected override bool CanBeParent(TreeViewItem item) => true;

        protected override bool CanMultiSelect(TreeViewItem item) => true;

        protected override void SelectionChanged(IList<int> selectedIds) => _onSelectionChanged.OnNext((Menu, GetSelectedMenuItemIds()));

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            if (args.draggedItemIDs is null || args.draggedItemIDs.Count == 0)
            {
                return false;
            }
            else
            {
                if (args.draggedItemIDs.Contains(GetElementId(Domain.Menu.RegisteredId)) || args.draggedItemIDs.Contains(GetElementId(Domain.Menu.UnregisteredId)))
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

            var menuItemIds = SortItemIDsInRowOrder(args.draggedItemIDs).Select(x => GetMenuItemId(x)).ToList();
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
                    var parentMenuItemId = GetMenuItemId(args.parentItem.id);
                    if (args.dragAndDropPosition == DragAndDropPosition.UponItem)
                    {
                        // Insert to the end
                        _onDropped.OnNext((menuItemIds, parentMenuItemId, null));
                    }
                    else if (args.dragAndDropPosition == DragAndDropPosition.BetweenItems)
                    {
                        _onDropped.OnNext((menuItemIds, parentMenuItemId, args.insertAtIndex));
                    }
                }

                return DragAndDropVisualMode.None;
            }
            else
            {
                if (args.dragAndDropPosition == DragAndDropPosition.UponItem)
                {
                    var parentMenuItemId = GetMenuItemId(args.parentItem.id);
                    if (parentMenuItemId is string &&
                        (parentMenuItemId == Domain.Menu.RegisteredId ||
                         parentMenuItemId == Domain.Menu.UnregisteredId ||
                         Menu.ContainsGroup(parentMenuItemId)))
                    {
                        return DragAndDropVisualMode.Move;
                    }
                    else
                    {
                        return DragAndDropVisualMode.None;
                    }
                }
                else if (args.dragAndDropPosition == DragAndDropPosition.BetweenItems)
                {
                    return DragAndDropVisualMode.Move;
                }
                else
                {
                    return DragAndDropVisualMode.None;
                }
            }
        }

        private void RemoveUnusedExpandedId()
        {
            if (Menu is null)
            {
                return;
            }

            var usedElementIds = new List<int>();
            foreach (var elementId in _treeViewState.expandedIDs)
            {
                var menuItemId = GetMenuItemId(elementId);
                if (menuItemId is null)
                {
                    continue;
                }

                if (menuItemId == Domain.Menu.RegisteredId ||
                    menuItemId == Domain.Menu.UnregisteredId ||
                    Menu.ContainsMode(menuItemId) ||
                    Menu.ContainsGroup(menuItemId))
                {
                    usedElementIds.Add(elementId);
                }
            }

            _treeViewState.expandedIDs.Clear();
            foreach (var elementId in usedElementIds)
            {
                _treeViewState.expandedIDs.Add(elementId);
            }
        }
    }
}
