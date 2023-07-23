using Suzuryg.FaceEmo.Components.Data;
using Suzuryg.FaceEmo.Components.Settings;
using UnityEngine;

namespace Suzuryg.FaceEmo.Components
{
    public class FaceEmoProject : ScriptableObject
    {
        public SerializableMenu SerializableMenu;
        public AV3Setting AV3Setting;
        public ExpressionEditorSetting ExpressionEditorSetting;
        public ThumbnailSetting ThumbnailSetting;
    }
}
