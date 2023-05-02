using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class HierarchyViewState : ScriptableObject
    {
#if UNITY_EDITOR
        public UnityEditor.IMGUI.Controls.TreeViewState TreeViewState;
#endif
    }
}
