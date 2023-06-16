using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public class ExpressionEditorSetting : ScriptableObject
    {
        public bool ShowOnlyDifferFromDefaultValue = true;
        public bool ReflectInPreviewOnMouseOver = true;
        public string FaceBlendShapeDelimiter = string.Empty;
    }
}
