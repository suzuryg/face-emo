using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public class SerializableGroup : SerializableMenuItemListBase
    {
        public string DisplayName;

        public void Save(IGroup group)
        {
            base.Save(group);
            DisplayName = group.DisplayName;
        }

        public override void Load(Menu menu, string destination)
        {
            menu.ModifyGroupProperties(destination, displayName: DisplayName);
            base.Load(menu, destination);
        }
    }
}
