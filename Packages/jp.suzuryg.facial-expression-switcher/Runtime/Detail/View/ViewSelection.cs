using Suzuryg.FacialExpressionSwitcher.Domain;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class ViewSelection : MonoBehaviour
    {
        public string HierarchyView { get; set; }
        public string MenuItemListView { get; set; }
        public int BranchListView { get; set; }
        public (HandGesture left, HandGesture right)? GestureTableView { get; set; }
    }
}
