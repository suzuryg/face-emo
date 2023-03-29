using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class MenuItemListViewState : MonoBehaviour
    {
#if UNITY_EDITOR
        public UnityEditor.IMGUI.Controls.TreeViewState TreeViewState;
        public string RootGroupId;
#endif
    }
}
