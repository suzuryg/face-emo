using System;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    internal class HelpBoxDrawer
    {
        private static GUIStyle _helpBoxStyle;
        private static Texture ErrorIcon => EditorGUIUtility.IconContent("console.erroricon").image;
        private static Texture WarnIcon => EditorGUIUtility.IconContent("console.warnicon").image;
        private static Texture InfoIcon => EditorGUIUtility.IconContent("console.infoicon").image;

        public static void Error(Rect rect, string message) => Label(rect, message, ErrorIcon);
        public static void Warn(Rect rect, string message) => Label(rect, message, WarnIcon);
        public static void Info(Rect rect, string message) => Label(rect, message, InfoIcon);
        private static void Label(Rect rect, string message, Texture icon) => GUI.Label(rect, new GUIContent(message, icon), GetStyle());

        public static Rect ErrorLayout(string message) => LabelLayout(message, ErrorIcon);
        public static Rect WarnLayout(string message) => LabelLayout(message, WarnIcon);
        public static Rect InfoLayout(string message) => LabelLayout(message, InfoIcon);
        private static Rect LabelLayout(string message, Texture icon)
        {
            var content = new GUIContent(message, icon);
            GUILayout.Label(content, GetStyle());
            return GUILayoutUtility.GetRect(content, GetStyle());
        }
        
        private static GUIStyle GetStyle()
        {
            if (_helpBoxStyle == null)
            {
                try
                {
                    _helpBoxStyle = new GUIStyle(EditorStyles.helpBox);
                    _helpBoxStyle.fontSize = EditorStyles.label.fontSize;
                }
                catch (NullReferenceException)
                {
                    // Workaround for play mode
                    _helpBoxStyle = new GUIStyle();
                }
            }

            return _helpBoxStyle;
        }
    }
}
