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
    public class AddressBarElement : IDisposable
    {
        public IObservable<string> OnAddressClicked => _onAddressClicked.AsObservable();

        private Subject<string> _onAddressClicked = new Subject<string>();

        private LocalizationTable _localizationTable;

        private List<(string id, string displayName)> _path = new List<(string id, string displayName)>();

        private string _registeredText;
        private string _unregisteredText;

        private GUIStyle _buttonStyle;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public AddressBarElement(IReadOnlyLocalizationSetting localizationSetting)
        {
            // Localization
            SetText(localizationSetting.Table);
            localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Styles
            try
            {
                _buttonStyle = new GUIStyle(EditorStyles.label);
            }
            catch (NullReferenceException)
            {
                // Workaround for play mode
                _buttonStyle = new GUIStyle();
            }
        }

        public void Dispose() => _disposables.Dispose();

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;

            _registeredText = localizationTable.HierarchyView_RegisteredMenuItemList;
            _unregisteredText = localizationTable.HierarchyView_UnregisteredMenuItemList;

            for (int i = 0; i < _path.Count; i++)
            {
                if (_path[i].id == Domain.Menu.RegisteredId)
                {
                    _path[i] = (_path[i].id, _registeredText);
                }
                else if (_path[i].id == Domain.Menu.UnregisteredId)
                {
                    _path[i] = (_path[i].id, _unregisteredText);
                }
            }
        }

        public void SetPath(IMenu menu, string rootGroupId)
        {
            if (menu is null || rootGroupId is null)
            {
                return;
            }

            _path.Clear();
            IMenuItemList current;
            if (rootGroupId == Domain.Menu.RegisteredId)
            {
                current = menu.Registered;
            }
            else if (rootGroupId == Domain.Menu.UnregisteredId)
            {
                current = menu.Unregistered;
            }
            else if (menu.ContainsGroup(rootGroupId))
            {
                current = menu.GetGroup(rootGroupId);
            }
            else
            {
                return;
            }

            while (current is IMenuItemList)
            {
                _path.Add(GetElement(menu, current));
                current = current.Parent;
            }

            _path.Reverse();
        }

        public void OnGUI(Rect rect)
        {
            using (new GUILayout.AreaScope(rect))
            using (new EditorGUILayout.HorizontalScope())
            {
                var path = GetFitPath(rect).ToList();

                if (path.Count != _path.Count)
                {
                    GUILayout.Button(">", _buttonStyle);
                }

                for (int i = 0; i < path.Count; i++)
                {
                    if (GUILayout.Button(new GUIContent(path[i].displayName, _localizationTable.MenuItemListView_Tooltip_ClickAddressBar), _buttonStyle))
                    {
                        _onAddressClicked.OnNext(path[i].id);
                    }

                    if (i < path.Count - 1)
                    {
                        GUILayout.Button(">", _buttonStyle);
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }

        private (string id, string displayName) GetElement(IMenu menu, IMenuItemList menuItemList)
        {
            var id = menuItemList.GetId();
            if (id == Domain.Menu.RegisteredId)
            {
                return (id, _registeredText);
            }
            else if (id == Domain.Menu.UnregisteredId)
            {
                return (id, _unregisteredText);
            }
            else if (menu.ContainsGroup(id))
            {
                return (id, menu.GetGroup(id).DisplayName);
            }
            else
            {
                throw new FaceEmoException("Failed to get path element.");
            }
        }

        private IEnumerable<(string id, string displayName)> GetFitPath(Rect rect)
        {
            if (_path.Count == 0 || double.IsNaN(rect.width) || rect.width <= 0)
            {
                return _path;
            }

            var path = new List<(string id, string displayName)>(_path);
            while (path.Count > 1 && !IsFit(path.Select(x => x.displayName), rect.width, path.Count == _path.Count))
            {
                path = path.Skip(1).ToList();
            }

            return path;
        }

        private bool IsFit(IEnumerable<string> displayNames, float width, bool isFull)
        {
            var combined = string.Join(" > ", displayNames);
            if (!isFull)
            {
                combined = "> " + combined;
            }
            var size = _buttonStyle.CalcSize(new GUIContent(combined));
            return size.x <= width;
        }
    }
}
