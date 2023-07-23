using Suzuryg.FaceEmo.Components.States;
using Suzuryg.FaceEmo.Components.Settings;
using UnityEngine;

namespace Suzuryg.FaceEmo.Components
{
    public class FaceEmoLauncherComponent : MonoBehaviour
    {
        [HideInInspector] public int InstanceId;
        [HideInInspector] public AV3Setting AV3Setting;
        [HideInInspector] public ThumbnailSetting ThumbnailSetting;
        [HideInInspector] public ExpressionEditorSetting ExpressionEditorSetting;
        [HideInInspector] public HierarchyViewState HierarchyViewState;
        [HideInInspector] public MenuItemListViewState MenuItemListViewState;
        [HideInInspector] public ViewSelection ViewSelection;
        [HideInInspector] public InspectorViewState InspectorViewState;
    }
}
