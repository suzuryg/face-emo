using Suzuryg.FaceEmo.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.Data
{
    public abstract class SerializableMenuItemListBase : ScriptableObject
    {
        public List<MenuItemType> Types;
        public List<string> Ids;
        public List<SerializableMode> Modes;
        public List<SerializableGroup> Groups;

        public void Save(IMenuItemList menuItemList, bool isAsset) 
        {
            Types = new List<MenuItemType>();
            Ids = new List<string>();
            Modes = new List<SerializableMode>();
            Groups = new List<SerializableGroup>();

            for (int i = 0; i < menuItemList.Order.Count; i++)
            {
                var id = menuItemList.Order[i];
                var type = menuItemList.GetType(id);
                switch (type)
                {
                    case MenuItemType.Mode:
                        Types.Add(type);
                        Ids.Add(id);
                        var serializableMode = CreateInstance<SerializableMode>();
#if UNITY_EDITOR
                        if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(serializableMode, this); }
#else
                        if (isAsset) { throw new FaceEmoException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
                        serializableMode.Save(menuItemList.GetMode(id), isAsset);
                        serializableMode.name = $"Mode_{id}";
                        Modes.Add(serializableMode);
                        break;
                    case MenuItemType.Group:
                        Types.Add(type);
                        Ids.Add(id);
                        var serializableGroup = CreateInstance<SerializableGroup>();
#if UNITY_EDITOR
                        if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(serializableGroup, this); }
#else
                        if (isAsset) { throw new FaceEmoException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
                        serializableGroup.Save(menuItemList.GetGroup(id), isAsset);
                        serializableGroup.name = $"Group_{id}";
                        Groups.Add(serializableGroup);
                        break;
                }
            }
        }

        public virtual void Load(Domain.Menu menu, string destination)
        {
            if (Types.Count != Modes.Count + Groups.Count)
            {
                throw new FaceEmoException("Order.Count != Modes.Count + Groups.Count");
            }

            var orderQueue = new Queue<MenuItemType>(Types);
            var idQueue = new Queue<string>(Ids);
            var modeQueue = new Queue<SerializableMode>(Modes);
            var groupQueue = new Queue<SerializableGroup>(Groups);
            while (orderQueue.Count > 0)
            {
                var type = orderQueue.Dequeue();
                var id = idQueue.Dequeue();
                switch (type)
                {
                    case MenuItemType.Mode:
                        var mode = modeQueue.Dequeue();
                        var modeId = menu.AddMode(destination, id);
                        mode.Load(menu, modeId);
                        break;
                    case MenuItemType.Group:
                        var group = groupQueue.Dequeue();
                        var groupId = menu.AddGroup(destination, id);
                        group.Load(menu, groupId);
                        break;
                    default:
                        throw new FaceEmoException("Invalid MenuItemType");
                }
            }

            if (idQueue.Count > 0 || modeQueue.Count > 0 || groupQueue.Count > 0)
            {
                throw new FaceEmoException("Queue is not empty.");
            }
        }
    }
}
