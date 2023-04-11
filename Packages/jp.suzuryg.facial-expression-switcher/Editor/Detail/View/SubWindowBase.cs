using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class SubWindowBase : EditorWindow
    {
        public SubWindowBase()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            wantsMouseMove = true;
        }

        ~SubWindowBase()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                rootVisualElement.Clear();
                Close();
            }
        }
    }
}
