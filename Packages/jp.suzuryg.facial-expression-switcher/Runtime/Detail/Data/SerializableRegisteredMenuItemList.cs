using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public class SerializableRegisteredMenuItemList : SerializableMenuItemListBase
    {
        public List<int> InsertIndices;

        public void Save(IMenuItemList menuItemList, IReadOnlyList<int> insertIndices)
        {
            base.Save(menuItemList);
            InsertIndices = new List<int>(insertIndices);
        }
        
        public void Load(Menu menu)
        {
            Load(menu, Menu.RegisteredId);
            menu.SetInsertIndices(InsertIndices);
        }
    }
}
