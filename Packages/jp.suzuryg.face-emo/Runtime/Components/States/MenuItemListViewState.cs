using UnityEngine;

namespace Suzuryg.FaceEmo.Components.States
{
    public class MenuItemListViewState : ScriptableObject
    {
#if UNITY_EDITOR
        public UnityEditor.IMGUI.Controls.TreeViewState TreeViewState;
        public string RootGroupId = Domain.Menu.RegisteredId;
#endif
    }
}
