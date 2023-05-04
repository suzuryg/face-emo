using UnityEditor;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class MainWindowProvider
    {
        public Subject<EditorWindow> OnGUI = new Subject<EditorWindow>();
    }
}
