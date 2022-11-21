using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Adapter.ViewModel;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.IMGUI.Controls;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View.Element
{
    public class HierarchyTreeElement : TreeView
    {
        public HierarchyTreeElement() : base(new TreeViewState()) { }

        protected override TreeViewItem BuildRoot()
        {
            throw new NotImplementedException();
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            throw new NotImplementedException();
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        protected override bool CanStartDrag(CanStartDragArgs args) => true;

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
