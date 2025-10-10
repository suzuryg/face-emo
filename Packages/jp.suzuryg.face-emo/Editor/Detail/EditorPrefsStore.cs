using Suzuryg.FaceEmo.Domain;
using UnityEditor;

namespace Suzuryg.FaceEmo.Detail
{
    internal static class EditorPrefsStore
    {
        private static readonly string KeyHierarchyViewVisible = $"{DomainConstants.SystemName}_HierarchyViewVisible";
        private static bool _hierarchyViewVisible = EditorPrefs.GetBool(KeyHierarchyViewVisible, false);
        public static bool HierarchyViewVisible
        {
            get => _hierarchyViewVisible;
            set => EditorPrefs.SetBool(KeyHierarchyViewVisible, _hierarchyViewVisible = value);
        }
    }
}
