using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.Data
{
    public class SerializableUnregisteredMenuItemList : SerializableMenuItemListBase
    {
        public void Load(Menu menu)
        {
            Load(menu, Menu.UnregisteredId);
        }
    }
}
