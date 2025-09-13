﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FaceEmo.Domain
{
    public class RegisteredMenuItemList : MenuItemListBase
    {
        // Setting menu uses one slot.
        public static readonly int Capacity = DomainConstants.MenuItemNums - 1;

        public override bool IsFull => Order.Count >= Capacity;

        public override int FreeSpace => Capacity - Count;

        public override MenuItemListBase Parent => null;

        public override string GetId() => Menu.RegisteredId;
    }
}
