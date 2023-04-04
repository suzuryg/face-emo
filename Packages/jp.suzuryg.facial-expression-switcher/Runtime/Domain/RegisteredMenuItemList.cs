using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class RegisteredMenuItemList : MenuItemListBase
    {
        public override bool IsFull => Order.Count >= DomainConstants.MenuItemNums;

        public override int FreeSpace => DomainConstants.MenuItemNums - Count;

        public override MenuItemListBase Parent => null;

        public override string GetId() => Menu.RegisteredId;
    }
}
