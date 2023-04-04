using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public class SerializableMenu : MonoBehaviour
    {
        public double Version = 1.0;
        public string DefaultSelection;
        public List<string> MouthMorphBlendShapes = new List<string>();
        public SerializableRegisteredMenuItemList Registered;
        public SerializableUnregisteredMenuItemList Unregistered;

        public void Save(IMenu menu)
        {
            DefaultSelection = menu.DefaultSelection;

            Registered = ScriptableObject.CreateInstance<SerializableRegisteredMenuItemList>();
            Registered.Save(menu.Registered);

            Unregistered = ScriptableObject.CreateInstance<SerializableUnregisteredMenuItemList>();
            Unregistered.Save(menu.Unregistered);
        }

        public Menu Load()
        {
            var menu = new Menu();

            Registered?.Load(menu);
            Unregistered?.Load(menu);

            menu.SetDefaultSelection(DefaultSelection);

            return menu;
        }
    }
}
