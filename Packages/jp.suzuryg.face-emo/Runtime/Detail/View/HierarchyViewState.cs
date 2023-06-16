using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class HierarchyViewState : ScriptableObject
    {
#if UNITY_EDITOR
        public UnityEditor.IMGUI.Controls.TreeViewState TreeViewState;
#endif
    }
}
