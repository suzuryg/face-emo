using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class HierarchyViewState : MonoBehaviour
    {
#if UNITY_EDITOR
        public UnityEditor.IMGUI.Controls.TreeViewState TreeViewState;
#endif
    }
}
