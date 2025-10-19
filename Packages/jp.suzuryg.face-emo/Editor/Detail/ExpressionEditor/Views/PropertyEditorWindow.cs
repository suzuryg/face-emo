using JetBrains.Annotations;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class PropertyEditorWindow : EditorWindow
    {
        public bool InUse => ReferenceEquals(focusedWindow, this) || ReferenceEquals(mouseOverWindow, this);

        [CanBeNull] private LocalizationTable _loc;
        [CanBeNull] private PropertyEditorViewFacade _viewFacade;
        private bool _isInitialized;

        public void Initialize(PropertyEditorViewFacade viewFacade)
        {
            _viewFacade = viewFacade;
            _isInitialized = true;
        }

        public void CloseIfNotDocked()
        {
            _isInitialized = false;
            if (!docked) Close();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent($"{DomainConstants.SystemName} Editor");
        }

        private void OnGUI()
        {
            if (!_isInitialized)
            {
                if (_loc == null) _loc = LocalizationSetting.GetTable(LocalizationSetting.GetLocale());
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_loc?.ExpressionEditorView_Message_NotInitialized);
                    GUILayout.FlexibleSpace();
                }
                return;
            }

            var minWindowSize = _viewFacade?.OnGUI(position.width, position.height);
            if (minWindowSize.HasValue)
            {
                minSize = minWindowSize.Value;
            }

            if (_viewFacade?.IsRepaintRequested != true) return;
            Repaint();
            _viewFacade.IsRepaintRequested = false;
        }
    }
}
