using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class RegisteredMenuItemList : MenuItemListBase
    {
        public override bool IsFull => Order.Count >= DomainConstants.MenuItemNums;

        public IReadOnlyList<int> InsertIndices => _insertIndices;

        private List<int> _insertIndices = new List<int>();

        public bool CanGetMergedMenu(IReadOnlyList<IExistingMenuItem> existingMenuItems)
        {
            if (existingMenuItems is List<IExistingMenuItem> && existingMenuItems.Count <= DomainConstants.MenuItemNums)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public MergedMenuItemList GetMergedMenu(IReadOnlyList<IExistingMenuItem> existingMenuItems)
        {
            NullChecker.Check(existingMenuItems);

            var existingQueue = new Queue<IExistingMenuItem>(existingMenuItems);
            var registeredQueue = new Queue<string>(Order);
            var insertIndicesQueue = new Queue<int>(_insertIndices);

            MergedMenuItemList mergedMenuItemList = new MergedMenuItemList();
            int index = 0;
            while (existingQueue.Count > 0 || registeredQueue.Count > 0)
            {
                if (existingQueue.Count == 0)
                {
                    var id = registeredQueue.Dequeue();
                    switch (GetType(id))
                    {
                        case MenuItemType.Mode:
                            mergedMenuItemList.Add(GetMode(id), id);
                            break;
                        case MenuItemType.Group:
                            mergedMenuItemList.Add(GetGroup(id), id);
                            break;
                    }
                }
                else if (insertIndicesQueue.Count > 0 && index == insertIndicesQueue.Peek() && registeredQueue.Count > 0)
                {
                    var id = registeredQueue.Dequeue();
                    switch (GetType(id))
                    {
                        case MenuItemType.Mode:
                            mergedMenuItemList.Add(GetMode(id), id);
                            break;
                        case MenuItemType.Group:
                            mergedMenuItemList.Add(GetGroup(id), id);
                            break;
                    }
                    insertIndicesQueue.Dequeue();
                }
                else
                {
                    mergedMenuItemList.Add(existingQueue.Dequeue());
                }
                index++;
            }

            return mergedMenuItemList;
        }

        public bool CanUpdateInsertIndices(MergedMenuItemList mergedMenuItemList)
        {
            if (mergedMenuItemList is MergedMenuItemList && mergedMenuItemList.Order.Count <= DomainConstants.MenuItemNums)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateInsertIndices(MergedMenuItemList mergedMenuItemList)
        {
            NullChecker.Check(mergedMenuItemList);

            _insertIndices.Clear();
            for (int i = 0; i < mergedMenuItemList.Order.Count; i++)
            {
                var id = mergedMenuItemList.Order[i];
                if (mergedMenuItemList.GetType(id) != MergedMenuItemType.Existing)
                {
                    _insertIndices.Add(i);
                }
            }
        }

        public void SetInsertIndices(IReadOnlyList<int> insertIndices)
        {
            NullChecker.Check(insertIndices);

            if (insertIndices.Count > 0)
            {
                if (insertIndices.Min() < 0 || insertIndices.Max() >= DomainConstants.MenuItemNums)
                {
                    throw new FacialExpressionSwitcherException($"InsertIndices must be in [{0}, {DomainConstants.MenuItemNums - 1}].");
                }
            }

            if (insertIndices.Count != new HashSet<int>(insertIndices).Count)
            {
                throw new FacialExpressionSwitcherException($"InsertIndices can't be duplicated.");
            }

            _insertIndices = new List<int>(insertIndices);
        }

        /*public List<IMenuItem> Merge(List<IMenuItem> existingMenuItems)
        {
            var registeredQueue = new Queue<(IMenuItem item, int order)>(_children.OrderBy(x => x.order));

            // Exclude existing menu items that have the same name as the merging target.
            var registeredNames = new HashSet<string>(_children.Select(x => x.item.DisplayName));
            var existingQueue = new Queue<IMenuItem>(existingMenuItems.Where(x => !registeredNames.Contains(x.DisplayName)));

            if (registeredQueue.Count + existingQueue.Count > CommonSetting.MenuItemNums)
            {
                throw new FacialExpressionSwitcherException($"The number of merged menu items exceeds {CommonSetting.MenuItemNums}.");
            }

            // Insert the menu item to be merged at the specified position.
            var mergedMenuItems = new List<IMenuItem>();
            for (int order = 1; order <= CommonSetting.MenuItemNums; order++)
            {
                if (registeredQueue.Count > 0 && registeredQueue.Peek().order <= order)
                {
                    mergedMenuItems.Add(registeredQueue.Dequeue().item);
                }
                else if (existingQueue.Count > 0)
                {
                    mergedMenuItems.Add(existingQueue.Dequeue());
                }
            }

            return mergedMenuItems;
        }

        public List<IMenuItem> UnMerge(List<IMenuItem> mergedMenuItems)
        {
            var registeredNames = new HashSet<string>(_children.Select(x => x.item.DisplayName));
            return mergedMenuItems.Where(x => !registeredNames.Contains(x.DisplayName)).ToList();
        }*/
    }
}
