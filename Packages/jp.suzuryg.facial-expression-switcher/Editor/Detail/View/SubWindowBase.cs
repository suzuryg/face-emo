using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class SubWindowBase : EditorWindow
    {
        public SubWindowBase()
        {
            wantsMouseMove = true;
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
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
