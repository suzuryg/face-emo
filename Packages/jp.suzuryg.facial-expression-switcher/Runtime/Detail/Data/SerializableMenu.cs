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
        [HideInInspector] public double Version = 1.0;
        [HideInInspector] public bool WriteDefaults;
        [HideInInspector] public double TransitionDurationSeconds;
        [HideInInspector] public SerializableRegisteredMenuItemList Registered;
        [HideInInspector] public SerializableUnregisteredMenuItemList Unregistered;

        public void Save(IMenu menu)
        {
            WriteDefaults = menu.WriteDefaults;
            TransitionDurationSeconds = menu.TransitionDurationSeconds;

            Registered = ScriptableObject.CreateInstance<SerializableRegisteredMenuItemList>();
            Registered.Save(menu.Registered, menu.InsertIndices);

            Unregistered = ScriptableObject.CreateInstance<SerializableUnregisteredMenuItemList>();
            Unregistered.Save(menu.Unregistered);
        }

        public Menu Load()
        {
            var menu = new Menu();

            menu.WriteDefaults = WriteDefaults;
            menu.TransitionDurationSeconds = TransitionDurationSeconds;

            Registered?.Load(menu);
            Unregistered?.Load(menu);

            return menu;
        }
    }
}
