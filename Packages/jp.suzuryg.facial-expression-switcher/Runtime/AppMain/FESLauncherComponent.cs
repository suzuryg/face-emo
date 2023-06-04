using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    public class FESLauncherComponent : MonoBehaviour
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
