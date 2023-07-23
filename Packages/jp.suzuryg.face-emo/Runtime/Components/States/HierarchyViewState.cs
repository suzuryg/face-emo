using UnityEngine;

namespace Suzuryg.FaceEmo.Components.States
{
    public class HierarchyViewState : ScriptableObject
    {
#if UNITY_EDITOR
        public UnityEditor.IMGUI.Controls.TreeViewState TreeViewState;
#endif
    }
}
