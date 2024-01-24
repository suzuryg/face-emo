using Suzuryg.FaceEmo.Detail.Localization;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class InspectorWindow : SubWindowBase
    {
        private InspectorView _inspectorView;
        private Vector2 _scrollPosition = Vector2.zero;

        public void Initialize(InspectorView inspectorView)
        {
            _inspectorView = inspectorView;
        }

        private void OnGUI()
        {
            if (_inspectorView == null)
            {
                var loc = LocalizationSetting.GetTable(LocalizationSetting.GetLocale());
                GUILayout.Label(loc.Common_Message_NotInitializedWindow);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            try
            {
                _inspectorView.OnGUI(hideLaunchButton: true);
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            minSize = new Vector2(350, 700);
        }
    }
}
