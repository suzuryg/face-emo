using System;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class ExpressionEditorWindow : SubWindowBase
    {
        private ISubWindowProvider _subWindowProvider;

        public ExpressionEditorWindow() : base()
        {
            // Avoid duplicate invocations of blend shape preview on mouseover.
            wantsMouseMove = false;
        }

        public void SetProvider(ISubWindowProvider subWindowProvider)
        {
            _subWindowProvider = subWindowProvider;
        }

        private void OnDisable()
        {
            _subWindowProvider?.Provide<ExpressionPreviewWindow>()?.Close();
        }
    }
}
