using Suzuryg.FaceEmo.Domain;
using UnityEngine;

namespace Suzuryg.FaceEmo.Components.Data
{
    public class SerializableMenu : ScriptableObject
    {
        public double Version = 1.0;
        public string DefaultSelection;
        public SerializableRegisteredMenuItemList Registered;
        public SerializableUnregisteredMenuItemList Unregistered;

        public void Save(IMenu menu, bool isAsset)
        {
            DefaultSelection = menu.DefaultSelection;

            Registered = CreateInstance<SerializableRegisteredMenuItemList>();
#if UNITY_EDITOR
            if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(Registered, this); }
#else
            if (isAsset) { throw new FaceEmoException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
            Registered.Save(menu.Registered, isAsset);
            Registered.name = Domain.Menu.RegisteredId;

            Unregistered = CreateInstance<SerializableUnregisteredMenuItemList>();
#if UNITY_EDITOR
            if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(Unregistered, this); }
#else
            if (isAsset) { throw new FaceEmoException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
            Unregistered.Save(menu.Unregistered, isAsset);
            Unregistered.name = Domain.Menu.UnregisteredId;
        }

        public Domain.Menu Load()
        {
            var menu = new Domain.Menu();

            Registered?.Load(menu);
            Unregistered?.Load(menu);

            menu.SetDefaultSelection(DefaultSelection);

            return menu;
        }
    }
}
