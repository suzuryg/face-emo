using UnityEditor;
using UniRx;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class MainWindowProvider
    {
        public Subject<EditorWindow> OnGUI = new Subject<EditorWindow>();
    }
}
