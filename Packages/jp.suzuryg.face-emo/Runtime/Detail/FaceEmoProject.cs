using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Data;
using Suzuryg.FaceEmo.Detail.Drawing;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail
{
    public class FaceEmoProject : ScriptableObject
    {
        public SerializableMenu SerializableMenu;
        public AV3Setting AV3Setting;
        public ExpressionEditorSetting ExpressionEditorSetting;
        public ThumbnailSetting ThumbnailSetting;
    }
}
