using System;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class ExpressionEditorWindow : SubWindowBase
    {
        private ISubWindowProvider _subWindowProvider;

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
