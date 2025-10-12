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

        public static class ExpressionEditorSettings
        {
            private static readonly string KeyShowOnlyDifferFromDefaultValue =
                $"{DomainConstants.SystemName}_ExpressionEditor_ShowOnlyDifferFromDefaultValue";
            private static bool _showOnlyDifferFromDefaultValue =
                EditorPrefs.GetBool(KeyShowOnlyDifferFromDefaultValue, true);
            public static bool ShowOnlyDifferFromDefaultValue
            {
                get => _showOnlyDifferFromDefaultValue;
                set => EditorPrefs.SetBool(KeyShowOnlyDifferFromDefaultValue, _showOnlyDifferFromDefaultValue = value);
            }

            private static readonly string KeyReflectInPreviewOnMouseOver =
                $"{DomainConstants.SystemName}_ExpressionEditor_ReflectInPreviewOnMouseOver";
            private static bool _reflectInPreviewOnMouseOver =
                EditorPrefs.GetBool(KeyReflectInPreviewOnMouseOver, true);
            public static bool ReflectInPreviewOnMouseOver
            {
                get => _reflectInPreviewOnMouseOver;
                set => EditorPrefs.SetBool(KeyReflectInPreviewOnMouseOver, _reflectInPreviewOnMouseOver = value);
            }

            private static readonly string KeyUseMouseWheel =
                $"{DomainConstants.SystemName}_ExpressionEditor_UseMouseWheel";
            private static bool _useMouseWheel =
                EditorPrefs.GetBool(KeyUseMouseWheel, false);
            public static bool UseMouseWheel
            {
                get => _useMouseWheel;
                set => EditorPrefs.SetBool(KeyUseMouseWheel, _useMouseWheel = value);
            }

            private static readonly string KeyShowBlinkBlendShapes =
                $"{DomainConstants.SystemName}_ExpressionEditor_ShowBlinkBlendShapes";
            private static bool _showBlinkBlendShapes =
                EditorPrefs.GetBool(KeyShowBlinkBlendShapes, true);
            public static bool ShowBlinkBlendShapes
            {
                get => _showBlinkBlendShapes;
                set => EditorPrefs.SetBool(KeyShowBlinkBlendShapes, _showBlinkBlendShapes = value);
            }

            private static readonly string KeyShowLipSyncBlendShapes =
                $"{DomainConstants.SystemName}_ExpressionEditor_ShowLipSyncBlendShapes";
            private static bool _showLipSyncBlendShapes =
                EditorPrefs.GetBool(KeyShowLipSyncBlendShapes, false);
            public static bool ShowLipSyncBlendShapes
            {
                get => _showLipSyncBlendShapes;
                set => EditorPrefs.SetBool(KeyShowLipSyncBlendShapes, _showLipSyncBlendShapes = value);
            }
        }
    }
}
