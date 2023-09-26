using Suzuryg.FaceEmo.Components.States;
using Suzuryg.FaceEmo.Components.Settings;
using UnityEngine;

namespace Suzuryg.FaceEmo.Components
{
    public class FaceEmoLauncherComponent : MonoBehaviour
    {
#if !SHOW_FACE_EMO_FIELDS
        [HideInInspector]
#endif
        public int InstanceId;

#if !SHOW_FACE_EMO_FIELDS
        [HideInInspector]
#endif
        public AV3Setting AV3Setting;

#if !SHOW_FACE_EMO_FIELDS
        [HideInInspector]
#endif
        public ThumbnailSetting ThumbnailSetting;

#if !SHOW_FACE_EMO_FIELDS
        [HideInInspector]
#endif
        public ExpressionEditorSetting ExpressionEditorSetting;

#if !SHOW_FACE_EMO_FIELDS
        [HideInInspector]
#endif
        public HierarchyViewState HierarchyViewState;

#if !SHOW_FACE_EMO_FIELDS
        [HideInInspector]
#endif
        public MenuItemListViewState MenuItemListViewState;

#if !SHOW_FACE_EMO_FIELDS
        [HideInInspector]
#endif
        public ViewSelection ViewSelection;

#if !SHOW_FACE_EMO_FIELDS
        [HideInInspector]
#endif
        public InspectorViewState InspectorViewState;
    }
}
