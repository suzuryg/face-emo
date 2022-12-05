using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IGroup : IMenuItemList
    {
        string DisplayName { get; }
    }

    public class Group : MenuItemListBase, IGroup
    {
        public override bool IsFull => Order.Count >= DomainConstants.MenuItemNums;

        public override int FreeSpace => DomainConstants.MenuItemNums - Count;

        public string DisplayName { get; set; }

        public override MenuItemListBase Parent => _parent;
        private MenuItemListBase _parent;

        public Group(string displayName, MenuItemListBase parent)
        {
            DisplayName = displayName;
            _parent = parent;
        }

        public void ChangeParent(MenuItemListBase parent)
        {
            _parent = parent;
        }

        public override string GetId()
        {
            foreach (var id in Parent.Order)
            {
                if (Parent.GetType(id) == MenuItemType.Group && ReferenceEquals(Parent.GetGroup(id), this))
                {
                    return id;
                }
            }
            throw new FacialExpressionSwitcherException("The parent does not have this group.");
        }
    }
}
