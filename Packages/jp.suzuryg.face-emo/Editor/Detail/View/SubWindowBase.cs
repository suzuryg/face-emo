using UnityEditor;
using System.Reflection;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class SubWindowBase : EditorWindow, ISubWindow
    {
        public bool IsInitialized { get; set; } = false;

        public bool IsDocked
        {
            get
            {
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                MethodInfo method = GetType().GetProperty( "docked", flags ).GetGetMethod( true );
                return (bool)method.Invoke( this, null );
            }
        }

        public void CloseIfNotDocked()
        {
            if (!IsDocked)
            {
                Close();
            }
            else
            {
                // Must be initialized the next time opened from the main window.
                rootVisualElement.Clear();
                IsInitialized = false;
            }
        }

        protected virtual void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            wantsMouseMove = true;
        }

        protected virtual void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode ||
                playModeStateChange == PlayModeStateChange.EnteredPlayMode)
            {
                rootVisualElement.Clear();
                CloseIfNotDocked();
            }
        }
    }
}
