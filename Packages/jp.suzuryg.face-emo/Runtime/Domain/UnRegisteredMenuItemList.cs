using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FaceEmo.Domain
{
    public class UnregisteredMenuItemList : MenuItemListBase 
    {
        public override bool IsFull { get; } = false;

        public override int FreeSpace { get; } = int.MaxValue;

        public override MenuItemListBase Parent => null;

        public override string GetId() => Menu.UnregisteredId;
    }
}
