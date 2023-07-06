using UnityEngine;
using Suzuryg.FaceEmo.Detail.Localization;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class GestureTableWindow : SubWindowBase
    {
        private void OnEnable()
        {
            minSize = new Vector2(800, 500);
        }

        private void OnGUI()
        {
            if (!IsInitialized)
            {
                var loc = LocalizationSetting.GetTable(LocalizationSetting.GetLocale());
                GUILayout.Label(loc.GestureTableView_Message_NotInitialized);
            }
        }
    }
}
