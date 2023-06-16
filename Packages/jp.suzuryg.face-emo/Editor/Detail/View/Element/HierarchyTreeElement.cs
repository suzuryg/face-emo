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
using Suzuryg.FaceEmo.Detail.AV3;

namespace Suzuryg.FaceEmo.Detail.View.Element
{
    public class HierarchyTreeElement : TreeElementBase
    {
        private static readonly Color SelectedRowColor = new Color(0f, 0.5f, 1f, 0.4f);

        public IObservable<(string menuItemId, string displayName)> OnModeRenamed => _onModeRenamed.AsObservable();
        public IObservable<(string menuItemId, string displayName)> OnGroupRenamed => _onGroupRenamed.AsObservable();

        private Subject<(string menuItemId, string displayName)> _onModeRenamed = new Subject<(string menuItemId, string displayName)>();
        private Subject<(string menuItemId, string displayName)> _onGroupRenamed = new Subject<(string menuItemId, string displayName)>();

        private ModeNameProvider _modeNameProvider;

        private string _registeredText;
        private string _unregisteredText;

        private Texture2D _fileIcon;
        private Texture2D _openFolderIcon;
        private Texture2D _closeFolderIcon;
        private Texture2D _selectedBackgroundTexture;

        public HierarchyTreeElement(IReadOnlyLocalizationSetting localizationSetting, ModeNameProvider modeNameProvider, TreeViewState treeViewState) : base(localizationSetting, treeViewState)
        {
            // Dependencies
            _modeNameProvider = modeNameProvider;

            // Set icon
            _fileIcon = ViewUtility.GetIconTexture("description_FILL0_wght400_GRAD200_opsz20.png");
            _openFolderIcon = ViewUtility.GetIconTexture("folder_open_FILL0_wght400_GRAD200_opsz20.png");
            _closeFolderIcon = ViewUtility.GetIconTexture("folder_FILL0_wght400_GRAD200_opsz20.png");
            NullChecker.Check(_fileIcon, _openFolderIcon, _closeFolderIcon);

            // Textures
            _selectedBackgroundTexture = new Texture2D(1, 1);
            _selectedBackgroundTexture.SetPixel(0, 0, SelectedRowColor);
            _selectedBackgroundTexture.Apply();
        }

        protected override void SetText(LocalizationTable localizationTable)
        {
            _registeredText = localizationTable.HierarchyView_RegisteredMenuItemList;
            _unregisteredText = localizationTable.HierarchyView_UnregisteredMenuItemList;
            base.SetText(localizationTable);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();

            if (Menu is null)
            {
                return rows;
            }

            // Registered menu items
            var registeredTree = new TreeViewItem { id = GetElementId(Domain.Menu.RegisteredId), displayName = _registeredText };
            root.AddChild(registeredTree);
            rows.Add(registeredTree);
            if (IsExpanded(registeredTree.id) && Menu is IMenu)
            {
                registeredTree.icon = _openFolderIcon;
                AddChildrenRecursive(Menu.Registered, registeredTree, rows);
            }
            else
            {
                registeredTree.icon = _closeFolderIcon;
                registeredTree.children = CreateChildListForCollapsedParent();
            }

            // Unregistered menu items
            var unregisteredTree = new TreeViewItem { id = GetElementId(Domain.Menu.UnregisteredId), displayName = _unregisteredText };
            root.AddChild(unregisteredTree);
            rows.Add(unregisteredTree);
            if (IsExpanded(unregisteredTree.id) && Menu is IMenu)
            {
                unregisteredTree.icon = _openFolderIcon;
                AddChildrenRecursive(Menu.Unregistered, unregisteredTree, rows);
            }
            else
            {
                unregisteredTree.icon = _closeFolderIcon;
                unregisteredTree.children = CreateChildListForCollapsedParent();
            }

            SetupDepthsFromParentsAndChildren(root);

            return rows;
        }

        private void AddChildrenRecursive(IMenuItemList menuItemList, TreeViewItem item, IList<TreeViewItem> rows)
        {
            foreach (var menuItemId in menuItemList.Order)
            {
                if (menuItemList.GetType(menuItemId) == MenuItemType.Mode)
                {
                    var mode = menuItemList.GetMode(menuItemId);
                    var modeTree = new TreeViewItem { id = GetElementId(menuItemId), displayName = _modeNameProvider.Provide(mode) };
                    item.AddChild(modeTree);
                    rows.Add(modeTree);
                    modeTree.icon = _fileIcon;
                }
                else if (menuItemList.GetType(menuItemId) == MenuItemType.Group)
                {
                    var group = menuItemList.GetGroup(menuItemId);
                    var groupTree = new TreeViewItem { id = GetElementId(menuItemId), displayName = group.DisplayName };
                    item.AddChild(groupTree);
                    rows.Add(groupTree);
                    if (IsExpanded(groupTree.id))
                    {
                        groupTree.icon = _openFolderIcon;
                        AddChildrenRecursive(group, groupTree, rows);
                    }
                    else
                    {
                        groupTree.icon = _closeFolderIcon;
                        if (group.Count > 0)
                        {
                            groupTree.children = CreateChildListForCollapsedParent();
                        }
                    }
                }
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            // Draw background
            if (args.selected)
            {
                GUI.DrawTexture(args.rowRect, _selectedBackgroundTexture);
            }
            base.RowGUI(args);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return item.id != _menuItemIdToElementId[Domain.Menu.RegisteredId] && item.id != _menuItemIdToElementId[Domain.Menu.UnregisteredId];
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (args.acceptedRename && args.originalName != args.newName && Menu is IMenu)
            {
                var menuItemId = GetMenuItemId(args.itemID);
                if (menuItemId is string)
                {
                    if (Menu.ContainsMode(menuItemId))
                    {
                        var mode = Menu.GetMode(menuItemId);
                        if (mode.UseAnimationNameAsDisplayName)
                        {
                            EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.HierarchyView_Message_CanNotRename, "OK");
                        }
                        else
                        {
                            _onModeRenamed.OnNext((menuItemId, args.newName));
                        }
                    }
                    else if (Menu.ContainsGroup(menuItemId))
                    {
                        _onGroupRenamed.OnNext((menuItemId, args.newName));
                    }
                }
            }
        }
    }
}
