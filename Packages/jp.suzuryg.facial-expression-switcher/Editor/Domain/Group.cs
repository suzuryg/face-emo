using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IGroup : IMenuItemList
    {
        string DisplayName { get; }

        IMenuItemList Parent { get; }
    }

    public class Group : MenuItemListBase, IGroup
    {
        public override bool IsFull => Order.Count >= DomainConstants.MenuItemNums;

        public override int FreeSpace => DomainConstants.MenuItemNums - Count;

        public string DisplayName { get; set; }

        IMenuItemList IGroup.Parent => Parent;
        public MenuItemListBase Parent { get; set; }

        public Group(string displayName, MenuItemListBase parent)
        {
            DisplayName = displayName;
            Parent = parent;
        }
    }
}
