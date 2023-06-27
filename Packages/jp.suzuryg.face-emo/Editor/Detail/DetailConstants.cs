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
        public static string ExternalDirectory => $"Packages/{PackageName}/External";
        public static string ViewDirectory => $"{DetailDirectory}/View";
        public static string IconDirectory => $"{ViewDirectory}/Icon";
        public static string LocalizationDirectory => $"{DetailDirectory}/Localization";

        public static readonly string DragAndDropDataKey_MenuItemIds = $"{DomainConstants.SystemName}_MenuItemIds";

        public static readonly string KeyGroupDeleteConfirmation = $"{DomainConstants.SystemName}_GroupDeleteConfirmation";
        public static readonly string KeyModeDeleteConfirmation = $"{DomainConstants.SystemName}_ModeDeleteConfirmation";
        public static readonly string KeyBranchDeleteConfirmation = $"{DomainConstants.SystemName}_BranchDeleteConfirmation";
        public static readonly bool DefaultGroupDeleteConfirmation = true;
        public static readonly bool DefaultModeDeleteConfirmation = true;
        public static readonly bool DefaultBranchDeleteConfirmation = true;

        public static readonly string KeyShowHints = $"{DomainConstants.SystemName}_ShowHints";
        public static readonly bool DefaultShowHints = true;

        public static readonly string KeyAutoSave = $"{DomainConstants.SystemName}_AutoSave";
        public static readonly bool DefaultAutoSave = true;

        public static readonly string KeyHideHierarchyIcon = $"{DomainConstants.SystemName}_HideHierarchyIcon";
        public static readonly bool DefaultHideHierarchyIcon = false;

        public static readonly string KeyShowClipFieldInGestureTable = $"{DomainConstants.SystemName}_ShowClipFieldInGestureTable";
        public static readonly bool DefaultShowClipFieldInGestureTable = false;
    }
}
