using Suzuryg.FaceEmo.Detail.View;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class ExpressionEditorStyles
    {
        public GUIStyle RemoveButtonStyle => _removeButtonStyle ??= new GUIStyle(GUI.skin.button);
        public GUIStyle AddPropertyLabelStyle => _addPropertyLabelStyle ??= new GUIStyle(GUI.skin.label)
        {
            fixedHeight = EditorGUIUtility.singleLineHeight * 1.5f
        };
        public GUIStyle AddPropertyButtonStyle => _addPropertyButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = EditorGUIUtility.singleLineHeight * 1.5f
        };
        public GUIStyle AddPropertyButtonMouseOverStyle => _addPropertyButtonMouseOverStyle ??= new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            normal =
            {
                background = _emphasizedTexture,
                scaledBackgrounds = new[] { _emphasizedTexture },
                textColor = ViewUtility.GetEmphasizedTextColor()
            },
            fixedHeight = EditorGUIUtility.singleLineHeight * 1.5f
        };
        public GUIStyle WarningTextStyle => _warningTextStyle ??= CreateWarningTextStyle();
        public GUIStyle NormalPropertyStyle => _normalPropertyStyle ??= new GUIStyle(GUI.skin.label);
        public GUIStyle WarnedPropertyStyle => _warnedPropertyStyle ??= new GUIStyle(GUI.skin.label)
        {
            normal =
            {
                background = _redTexture,
                scaledBackgrounds = new[] { _redTexture },
                textColor = Color.black
            }
        };
        public GUIStyle NormalLabelStyle => _normalLabelStyle ??= new GUIStyle(GUI.skin.label);
        public GUIStyle HighlightedLabelStyle => _highlightedLabelStyle ??= new GUIStyle(GUI.skin.label)
        {
            normal =
            {
                background = _emphasizedTexture,
                scaledBackgrounds = new[] { _emphasizedTexture },
                textColor = ViewUtility.GetEmphasizedTextColor()
            }
        };

        private readonly Texture2D _redTexture = ViewUtility.MakeTexture(Color.red);
        private readonly Texture2D _emphasizedTexture =
            ViewUtility.MakeTexture(ViewUtility.GetEmphasizedBackgroundColor());

        private GUIStyle _removeButtonStyle;
        private GUIStyle _addPropertyLabelStyle;
        private GUIStyle _addPropertyButtonStyle;
        private GUIStyle _addPropertyButtonMouseOverStyle;
        private GUIStyle _warningTextStyle;
        private GUIStyle _normalPropertyStyle;
        private GUIStyle _warnedPropertyStyle;
        private GUIStyle _normalLabelStyle;
        private GUIStyle _highlightedLabelStyle;

        private GUIStyle CreateWarningTextStyle()
        {
            var style = new GUIStyle(GUI.skin.label);
            if (EditorGUIUtility.isProSkin)
            {
                style.normal.textColor = Color.red;
            }
            else
            {
                style.normal.background = _redTexture;
                style.normal.scaledBackgrounds = new[] { _redTexture };
                style.normal.textColor = Color.black;
            }

            return style;
        }
    }
}
