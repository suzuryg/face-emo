using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class MenuItemListViewState : ScriptableObject
    {
#if UNITY_EDITOR
        public UnityEditor.IMGUI.Controls.TreeViewState TreeViewState;
        public string RootGroupId = Domain.Menu.RegisteredId;
#endif
    }
}
