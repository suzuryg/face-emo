using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IMenuItemList
    {
        int Count { get; }
        IReadOnlyList<string> Order { get; }

        IMenuItemList Parent { get; }
        string GetId();

        bool IsFull { get; }
        int FreeSpace { get; }

        MenuItemType GetType(string id);
        IMode GetMode(string id);
        IGroup GetGroup(string id);

        bool HasModeAt(int index);
        IMode GetModeAt(int index);
        bool HasGroupAt(int index);
        IGroup GetGroupAt(int index);
    }

    public enum MenuItemType
    {
        Mode,
        Group,
    }

    public abstract class MenuItemListBase : IMenuItemList
    {
        public int Count => _order.Count;
        public IReadOnlyList<string> Order => _order;

        IMenuItemList IMenuItemList.Parent => Parent;
        public abstract MenuItemListBase Parent { get; }
        public abstract string GetId();

        public abstract bool IsFull { get; }
        public abstract int FreeSpace { get; }

        public MenuItemType GetType(string id) => _types[id];
        public IMode GetMode(string id) => _modes[id];
        public IGroup GetGroup(string id) => _groups[id];

        private List<string> _order = new List<string>();
        private Dictionary<string, MenuItemType> _types = new Dictionary<string, MenuItemType>();
        private Dictionary<string, Mode> _modes = new Dictionary<string, Mode>();
        private Dictionary<string, Group> _groups = new Dictionary<string, Group>();

        public bool HasModeAt(int index) => index < Order.Count && GetType(Order[index]) == MenuItemType.Mode;

        public IMode GetModeAt(int index) => GetMode(Order[index]);

        public bool HasGroupAt(int index) => index < Order.Count && GetType(Order[index]) == MenuItemType.Group;

        public IGroup GetGroupAt(int index) => GetGroup(Order[index]);

        public void Insert(Mode mode, string id, int? index = null)
        {
            if (ReferenceEquals(this, mode))
            {
                throw new FacialExpressionSwitcherException("The source and the destination are same.");
            }

            Insert(id, index);
            _types[id] = MenuItemType.Mode;
            _modes[id] = mode;
        }

        public void Insert(Group group, string id, int? index = null)
        {
            if (ReferenceEquals(this, group))
            {
                throw new FacialExpressionSwitcherException("The source and the destination are same.");
            }

            Insert(id, index);
            _types[id] = MenuItemType.Group;
            _groups[id] = group;
        }

        private void Insert(string id, int? index = null)
        {
            if (IsFull)
            {
                throw new FacialExpressionSwitcherException("The number of menu items exceeds the limit.");
            }

            if (_order.Any(x => x == id))
            {
                throw new FacialExpressionSwitcherException("The inserted menu item is already exists in this list.");
            }

            if (!index.HasValue || index.Value >= _order.Count)
            {
                _order.Add(id);
            }
            else
            {
                _order.Insert(Math.Max(index.Value, 0), id);
            }
        }

        public void Remove(string id)
        {
            if (_modes.ContainsKey(id) && _types.ContainsKey(id) && _order.Contains(id))
            {
                _modes.Remove(id);
                _types.Remove(id);
                _order.Remove(id);
            }
            else if (_groups.ContainsKey(id) && _types.ContainsKey(id) && _order.Contains(id))
            {
                _groups.Remove(id);
                _types.Remove(id);
                _order.Remove(id);
            }
            else
            {
                throw new FacialExpressionSwitcherException("This MenuItemList does not contain the specified MenuItem.");
            }
        }

        public List<string> GetDescendantsId()
        {
            var ret = new List<string>();

            foreach (var id in _modes.Keys)
            {
                ret.Add(id);
            }

            foreach (var id in _groups.Keys)
            {
                ret.Add(id);
                foreach (var descendant in _groups[id].GetDescendantsId())
                {
                    ret.Add(descendant);
                }
            }

            return ret;
        }
    }
}
