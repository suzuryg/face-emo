using UnityEditor;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class SubWindowBase : EditorWindow, ISubWindow
    {
        public bool IsInitialized { get; set; } = false;

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
