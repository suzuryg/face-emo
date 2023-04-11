using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public interface ISubWindowProvider
    {
        void Open<T>() where T : EditorWindow;
    }
}
