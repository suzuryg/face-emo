using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class GestureTableWindow : EditorWindow
    {
        public GestureTableWindow()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            wantsMouseMove = true;
        }

        ~GestureTableWindow()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange playModeStateChange)
        {
            // UI is enabled only in EditMode.
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    rootVisualElement.Clear();
                    // TODO: Clear visual element
                    Close();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    rootVisualElement.Clear();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    rootVisualElement.Clear();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    rootVisualElement.Clear();
                    break;
            }
        }
    }
}
