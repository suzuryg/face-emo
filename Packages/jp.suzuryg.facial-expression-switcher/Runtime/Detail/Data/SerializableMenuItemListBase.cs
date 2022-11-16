using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public abstract class SerializableMenuItemListBase : ScriptableObject
    {
        public List<MenuItemType> Types;
        public List<string> Ids;
        public List<SerializableMode> Modes;
        public List<SerializableGroup> Groups;

        public void Save(IMenuItemList menuItemList)
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
                        serializableMode.Save(menuItemList.GetMode(id));
                        Modes.Add(serializableMode);
                        break;
                    case MenuItemType.Group:
                        Types.Add(type);
                        Ids.Add(id);
                        var serializableGroup = CreateInstance<SerializableGroup>();
                        serializableGroup.Save(menuItemList.GetGroup(id));
                        Groups.Add(serializableGroup);
                        break;
                }
            }
        }

        public virtual void Load(Menu menu, string destination)
        {
            if (Types.Count != Modes.Count + Groups.Count)
            {
                throw new FacialExpressionSwitcherException("Order.Count != Modes.Count + Groups.Count");
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
                        throw new FacialExpressionSwitcherException("Invalid MenuItemType");
                }
            }

            if (idQueue.Count > 0 || modeQueue.Count > 0 || groupQueue.Count > 0)
            {
                throw new FacialExpressionSwitcherException("Queue is not empty.");
            }
        }
    }
}
