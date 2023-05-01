using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IExistingMenuItem
    {
        string DisplayName { get; }
    }

    public enum MergedMenuItemType
    {
        Mode = 0,
        Group = 1,
        Existing = 2,
    }

    public class MergedMenuItemList
    {
        public int Count => _order.Count;
        public IReadOnlyList<string> Order => _order;

        public MergedMenuItemType GetType(string id) => _types[id];
        public IMode GetMode(string id) => _modes[id];
        public IGroup GetGroup(string id) => _groups[id];
        public IExistingMenuItem GetExistingMenuItem(string id) => _existingMenuItems[id];

        public bool ContainsMode(string id) => _modes.ContainsKey(id);
        public bool ContainsGroup(string id) => _groups.ContainsKey(id);
        public bool ContainsExistingMenuItem(string id) => _existingMenuItems.ContainsKey(id);

        public bool HasModeAt(int index) => index < Order.Count && GetType(Order[index]) == MergedMenuItemType.Mode;
        public IMode GetModeAt(int index) => GetMode(Order[index]);
        public bool HasGroupAt(int index) => index < Order.Count && GetType(Order[index]) == MergedMenuItemType.Group;
        public IGroup GetGroupAt(int index) => GetGroup(Order[index]);
        public bool HasExistingMenuItemAt(int index) => index < Order.Count && GetType(Order[index]) == MergedMenuItemType.Existing;
        public IExistingMenuItem GetExistingMenuItemAt(int index) => GetExistingMenuItem(Order[index]);

        private List<string> _order { get; } = new List<string>();
        private Dictionary<string, MergedMenuItemType> _types { get; } = new Dictionary<string, MergedMenuItemType>();
        private Dictionary<string, IMode> _modes { get; } = new Dictionary<string, IMode>();
        private Dictionary<string, IGroup> _groups { get; } = new Dictionary<string, IGroup>();
        private Dictionary<string, IExistingMenuItem> _existingMenuItems { get; } = new Dictionary<string, IExistingMenuItem>();

        public void Add(IMode mode, string id)
        {

            _order.Add(id);
            _types[id] = MergedMenuItemType.Mode;
            _modes[id] = mode;
        }

        public void Add(IGroup group, string id)
        {
            _order.Add(id);
            _types[id] = MergedMenuItemType.Group;
            _groups[id] = group;
        }

        public void Add(IExistingMenuItem existingMenuItem)
        {
            var id = Guid.NewGuid().ToString("N");
            while (_types.ContainsKey(id))
            {
                id = Guid.NewGuid().ToString("N");
            }
            _order.Add(id);
            _types[id] = MergedMenuItemType.Existing;
            _existingMenuItems[id] = existingMenuItem;
        }
    }
}
