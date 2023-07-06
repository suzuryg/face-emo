using System;
using System.Threading;
using UnityEngine;
using UnityEditor;
using Suzuryg.FaceEmo.Detail.Localization;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class ExpressionEditorWindow : SubWindowBase
    {
        private ISubWindowProvider _subWindowProvider;

        public void SetProvider(ISubWindowProvider subWindowProvider)
        {
            _subWindowProvider = subWindowProvider;
        }

        private void OnGUI()
        {
            if (!IsInitialized)
            {
                var loc = LocalizationSetting.GetTable(LocalizationSetting.GetLocale());
                GUILayout.Label(loc.ExpressionEditorView_Message_NotInitialized);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Avoid duplicate invocations of blend shape preview on mouseover.
            wantsMouseMove = false;
        }

        protected override void OnDisable()
        {
            _subWindowProvider?.Provide<ExpressionPreviewWindow>()?.CloseIfNotDocked();
        }
    }
}
