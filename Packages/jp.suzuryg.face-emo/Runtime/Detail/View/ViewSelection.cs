using Suzuryg.FaceEmo.Domain;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class ViewSelection : ScriptableObject
    {
        public string HierarchyView;
        public string MenuItemListView;
        public int BranchListView;
        // ValueTuple is not serialized, but the GestureTable selection does not need to be saved.
        public (HandGesture left, HandGesture right)? GestureTableView;
    }
}
