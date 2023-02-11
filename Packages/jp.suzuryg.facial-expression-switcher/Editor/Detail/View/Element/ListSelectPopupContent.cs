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
    public class ListSelectPopupContent<T> : PopupWindowContent
    {
        private static readonly float Margin = 5;
        private static readonly float ButtonHeight = 25;

        private ListTreeView<T> _listTreeView;
        private string _okText;
        private string _cancelText;
        private Action<IReadOnlyList<T>> _okAction;

        public ListSelectPopupContent(IReadOnlyList<T> contents, string okText, string cancelText, Action<IReadOnlyList<T>> okAction)
        {
            _listTreeView = new ListTreeView<T>(contents);
            _okText = okText;
            _cancelText = cancelText;
            _okAction = okAction;
        }

        public override void OnGUI(Rect rect)
        {
            _listTreeView.OnGUI(new Rect(
                rect.x + Margin,
                rect.y + Margin,
                rect.width - Margin * 2,
                rect.height - ButtonHeight - Margin * 4));

            if (GUI.Button(new Rect(
                rect.x + Margin,
                rect.y + rect.height - ButtonHeight - Margin * 2,
                rect.width / 2 - Margin * 2,
                ButtonHeight), _okText))
            {
                _okAction(_listTreeView.GetSelectedItems());
                editorWindow.Close();
            }

            if (GUI.Button(new Rect(
                rect.x + rect.width / 2 + Margin,
                rect.y + rect.height - ButtonHeight - Margin * 2,
                rect.width / 2 - Margin * 2,
                ButtonHeight), _cancelText))
            {
                editorWindow.Close();
            }
        }
    }
}
