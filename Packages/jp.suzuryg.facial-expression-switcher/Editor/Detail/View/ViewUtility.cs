using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class ViewUtility
    {
        public static void LayoutDummyToggle(string label)
        {
            using (new EditorGUI.DisabledScope(true))
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Toggle(string.Empty, false, GUILayout.Width(15));
                GUILayout.Label(label);
            }
        }

        public static void RectDummyToggle(Rect rect, float toggleWidth, string label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                var useRightTrigger = GUI.Toggle(new Rect(rect.x, rect.y, toggleWidth, rect.height), false, string.Empty);
                GUI.Label(new Rect(rect.x + toggleWidth, rect.y, rect.width - toggleWidth, rect.height), label);
            }
        }
    }
}
