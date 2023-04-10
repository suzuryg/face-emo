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
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                rootVisualElement.Clear();
                Close();
            }
        }
    }
}
