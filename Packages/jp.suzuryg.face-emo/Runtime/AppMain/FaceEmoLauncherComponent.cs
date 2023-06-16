using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.View;
using UnityEngine;

namespace Suzuryg.FaceEmo.AppMain
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
