using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IGroup : IMenuItemList
    {
        string DisplayName { get; }

        IMenuItemList Parent { get; }
        string GetId();
    }

    public class Group : MenuItemListBase, IGroup
    {
        public override bool IsFull => Order.Count >= DomainConstants.MenuItemNums;

        public override int FreeSpace => DomainConstants.MenuItemNums - Count;

        public string DisplayName { get; set; }

        IMenuItemList IGroup.Parent => Parent;
        public MenuItemListBase Parent { get; set; }

        public Group(string displayName, MenuItemListBase parent)
        {
            DisplayName = displayName;
            Parent = parent;
        }

        public string GetId()
        {
            foreach (var id in Parent.Order)
            {
                if (Parent.GetType(id) == MenuItemType.Group && ReferenceEquals(Parent.GetGroup(id), this))
                {
                    return id;
                }
            }
            throw new FacialExpressionSwitcherException("The parent does not have this group.");
        }
    }
}
