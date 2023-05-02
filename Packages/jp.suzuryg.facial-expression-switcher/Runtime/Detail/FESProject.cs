using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public class FESProject : ScriptableObject
    {
        public SerializableMenu SerializableMenu;
        public AV3Setting AV3Setting;
        public ExpressionEditorSetting ExpressionEditorSetting;
        public ThumbnailSetting ThumbnailSetting;
    }
}
