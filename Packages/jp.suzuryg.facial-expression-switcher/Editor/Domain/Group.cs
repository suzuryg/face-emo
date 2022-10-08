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
        public string DisplayName { get; set; }
        public override bool IsFull => Order.Count >= CommonSetting.MenuItemNums;
        public MenuItemListBase Parent { get; set; }

        public Group(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
