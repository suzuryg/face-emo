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
    public class ListTreeView<T> : TreeView
    {
        private static readonly int RootId = int.MinValue;

        private IReadOnlyList<T> _contents;

        public ListTreeView(IReadOnlyList<T> contents) : base(new TreeViewState())
        {
            _contents = contents;
            Reload();
        }

        public List<T> GetSelectedItems()
        {
            var ret = new List<T>();
            foreach (var id in GetSelection())
            {
                if (id < _contents.Count)
                {
                    ret.Add(_contents[id]);
                }
            }
            return ret;
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem { id = RootId, depth = -1, displayName = "Root" };
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();

            if (_contents is null)
            {
                return rows;
            }

            for (int i = 0; i < _contents.Count; i++)
            {
                var row = new TreeViewItem { id = i, displayName = _contents[i].ToString() };
                root.AddChild(row);
                rows.Add(row);
            }

            return rows;
        }
    }
}
