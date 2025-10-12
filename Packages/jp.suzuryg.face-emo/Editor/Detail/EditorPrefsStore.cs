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

        private static readonly string KeyMainViewThumbnailWidth = $"{DomainConstants.SystemName}_MainViewThumbnailWidth";
        private static readonly string KeyMainViewThumbnailHeight = $"{DomainConstants.SystemName}_MainViewThumbnailHeight";
        public static int MainViewThumbnailWidthInMemory =
            EditorPrefs.GetInt(KeyMainViewThumbnailWidth, DetailConstants.MainViewThumbnailMinWidth);
        public static int MainViewThumbnailHeightInMemory =
            EditorPrefs.GetInt(KeyMainViewThumbnailHeight, DetailConstants.MainViewThumbnailMinHeight);
        public static void SaveMainViewThumbnailWidth() =>
            EditorPrefs.SetInt(KeyMainViewThumbnailWidth, MainViewThumbnailWidthInMemory);
        public static void SaveMainViewThumbnailHeight() =>
            EditorPrefs.SetInt(KeyMainViewThumbnailHeight, MainViewThumbnailHeightInMemory);

        private static readonly string KeyGestureTableThumbnailWidth = $"{DomainConstants.SystemName}_GestureTableThumbnailWidth";
        private static readonly string KeyGestureTableThumbnailHeight = $"{DomainConstants.SystemName}_GestureTableThumbnailHeight";
        public static int GestureTableThumbnailWidthInMemory =
            EditorPrefs.GetInt(KeyGestureTableThumbnailWidth, 110);
        public static int GestureTableThumbnailHeightInMemory =
            EditorPrefs.GetInt(KeyGestureTableThumbnailHeight, 85);
        public static void SaveGestureTableThumbnailWidth() =>
            EditorPrefs.SetInt(KeyGestureTableThumbnailWidth, GestureTableThumbnailWidthInMemory);
        public static void SaveGestureTableThumbnailHeight() =>
            EditorPrefs.SetInt(KeyGestureTableThumbnailHeight, GestureTableThumbnailHeightInMemory);
    }
}
