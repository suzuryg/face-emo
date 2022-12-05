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
        [HideInInspector] public string AvatarPath;
        [HideInInspector] public bool WriteDefaults;
        [HideInInspector] public double TransitionDurationSeconds;
        [HideInInspector] public string DefaultSelection;
        [HideInInspector] public SerializableRegisteredMenuItemList Registered;
        [HideInInspector] public SerializableUnregisteredMenuItemList Unregistered;

        public void Save(IMenu menu)
        {
            AvatarPath = menu.Avatar?.Path;
            WriteDefaults = menu.WriteDefaults;
            TransitionDurationSeconds = menu.TransitionDurationSeconds;
            DefaultSelection = menu.DefaultSelection;

            Registered = ScriptableObject.CreateInstance<SerializableRegisteredMenuItemList>();
            Registered.Save(menu.Registered, menu.InsertIndices);

            Unregistered = ScriptableObject.CreateInstance<SerializableUnregisteredMenuItemList>();
            Unregistered.Save(menu.Unregistered);
        }

        public Menu Load()
        {
            var menu = new Menu();

            if (AvatarPath is string && AvatarPath.StartsWith("/"))
            {
                menu.Avatar = new Domain.Avatar(AvatarPath);
            }

            menu.WriteDefaults = WriteDefaults;
            menu.TransitionDurationSeconds = TransitionDurationSeconds;

            Registered?.Load(menu);
            Unregistered?.Load(menu);

            menu.SetDefaultSelection(DefaultSelection);

            return menu;
        }
    }
}
