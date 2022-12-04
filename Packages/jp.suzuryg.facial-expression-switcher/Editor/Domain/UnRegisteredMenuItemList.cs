using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class UnregisteredMenuItemList : MenuItemListBase 
    {
        public override bool IsFull { get; } = false;

        public override int FreeSpace { get; } = int.MaxValue;
    }
}
