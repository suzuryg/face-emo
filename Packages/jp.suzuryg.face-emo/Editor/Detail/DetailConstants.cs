using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suzuryg.FaceEmo.Detail
{
    public class DetailConstants
    {
        public static readonly string PackageName = "jp.suzuryg.face-emo";
        public static string DetailDirectory => $"Packages/{PackageName}/Editor/Detail";
        public static string ExternalDirectory => $"Packages/{PackageName}/Ext";
        public static string ViewDirectory => $"{DetailDirectory}/View";
        public static string IconDirectory => $"{ViewDirectory}/Icon";
        public static string LocalizationDirectory => $"{DetailDirectory}/Localization";

        public static float UiScale = 1.0f;

        public static readonly string DragAndDropDataKey_MenuItemIds = $"{DomainConstants.SystemName}_MenuItemIds";

        public static readonly string KeyGroupDeleteConfirmation = $"{DomainConstants.SystemName}_GroupDeleteConfirmation";
        public static readonly string KeyModeDeleteConfirmation = $"{DomainConstants.SystemName}_ModeDeleteConfirmation";
        public static readonly string KeyBranchDeleteConfirmation = $"{DomainConstants.SystemName}_BranchDeleteConfirmation";
        public static readonly string KeyEditPrefabsConfirmation = $"{DomainConstants.SystemName}_EditPrefabsConfirmation";
        public static readonly string KeyPrefixDisableConfirmation = $"{DomainConstants.SystemName}_PrefixDisableConfirmation";
        public static readonly bool DefaultGroupDeleteConfirmation = true;
        public static readonly bool DefaultModeDeleteConfirmation = true;
        public static readonly bool DefaultBranchDeleteConfirmation = true;
        public static readonly bool DefaultEditPrefabsConfirmation = true;
        public static readonly bool DefaultPrefixDisableConfirmation = true;

        public static readonly string KeyShowHints = $"{DomainConstants.SystemName}_ShowHints";
        public static readonly bool DefaultShowHints = true;

        public static readonly string KeyAutoSave = $"{DomainConstants.SystemName}_AutoSave";
        public static readonly bool DefaultAutoSave = true;

        public static readonly string KeyHierarchyIconOffset = $"{DomainConstants.SystemName}_HierarchyIconOffset";
        public static readonly string KeyHideHierarchyIcon = $"{DomainConstants.SystemName}_HideHierarchyIcon";
        public static readonly float DefaultHierarchyIconOffset = 20;
        public static readonly bool DefaultHideHierarchyIcon = false;

        public static readonly string KeyShowClipFieldInGestureTable = $"{DomainConstants.SystemName}_ShowClipFieldInGestureTable";
        public static readonly bool DefaultShowClipFieldInGestureTable = false;

        public static readonly string Key_ExpressionEditor_ShowOnlyDifferFromDefaultValue = $"{DomainConstants.SystemName}_ExpressionEditor_ShowOnlyDifferFromDefaultValue";
        public static readonly string Key_ExpressionEditor_ReflectInPreviewOnMouseOver = $"{DomainConstants.SystemName}_ExpressionEditor_ReflectInPreviewOnMouseOver";
        public static readonly string Key_ExpressionEditor_UseMouseWheel = $"{DomainConstants.SystemName}_ExpressionEditor_UseMouseWheel";
        public static readonly string Key_ExpressionEditor_ShowBlinkBlendShapes = $"{DomainConstants.SystemName}_ExpressionEditor_ShowBlinkBlendShapes";
        public static readonly string Key_ExpressionEditor_ShowLipSyncBlendShapes = $"{DomainConstants.SystemName}_ExpressionEditor_ShowLipSyncBlendShapes";
        public static readonly bool Default_ExpressionEditor_ShowOnlyDifferFromDefaultValue = true;
        public static readonly bool Default_ExpressionEditor_ReflectInPreviewOnMouseOver = true;
        public static readonly bool Default_ExpressionEditor_UseMouseWheel = false;
        public static readonly bool Default_ExpressionEditor_ShowBlinkBlendShapes = true;
        public static readonly bool Default_ExpressionEditor_ShowLipSyncBlendShapes = false;

        public static readonly int MainViewThumbnailMinWidth = 125;
        public static readonly int MainViewThumbnailMaxWidth = 300;
        public static readonly int MainViewThumbnailMinHeight = 125;
        public static readonly int MainViewThumbnailMaxHeight = 300;
        public static readonly int GestureTableThumbnailMinWidth = 70;
        public static readonly int GestureTableThumbnailMaxWidth = 250;
        public static readonly int GestureTableThumbnailMinHeight = 70;
        public static readonly int GestureTableThumbnailMaxHeight = 250;
        public static readonly int ExMenuThumbnailInnerWidth = 208;
        public static readonly int ExMenuThumbnailInnerHeight = 208;
        public static readonly int ExMenuThumbnailOuterWidth = 256;
        public static readonly int ExMenuThumbnailOuterHeight = 256;
    }
}
