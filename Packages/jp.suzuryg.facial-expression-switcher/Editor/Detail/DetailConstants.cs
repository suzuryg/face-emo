using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public class DetailConstants
    {
        public static readonly string PackageName = "jp.suzuryg.facial-expression-switcher";
        public static string DetailDirectory => $"Packages/{PackageName}/Editor/Detail";
        public static string ViewDirectory => $"{DetailDirectory}/View";
        public static string IconDirectory => $"{ViewDirectory}/Icon";
        public static string LocalizationDirectory => $"{DetailDirectory}/Localization";

        public static readonly string DragAndDropDataKey_MenuItemIds = $"{DomainConstants.SystemName}_MenuItemIds";

        public static readonly string KeyMainThumbnailWidth = $"{DomainConstants.SystemName}_MainThumbnailWidth";
        public static readonly string KeyMainThumbnailHeight = $"{DomainConstants.SystemName}_MainThumbnailHeight";
        public static readonly int DefaultMainThumbnailWidth = 180;
        public static readonly int DefaultMainThumbnailHeight = 150;
        public static readonly int MinMainThumbnailWidth = 125;
        public static readonly int MaxMainThumbnailWidth = 300;
        public static readonly int MinMainThumbnailHeight = 125;
        public static readonly int MaxMainThumbnailHeight = 300;

        public static readonly string KeyGestureThumbnailWidth = $"{DomainConstants.SystemName}_GestureThumbnailWidth";
        public static readonly string KeyGestureThumbnailHeight = $"{DomainConstants.SystemName}_GestureThumbnailHeight";
        public static readonly int DefaultGestureThumbnailWidth = 120;
        public static readonly int DefaultGestureThumbnailHeight = 100;
        public static readonly int MinGestureThumbnailWidth = 70;
        public static readonly int MaxGestureThumbnailWidth = 250;
        public static readonly int MinGestureThumbnailHeight = 70;
        public static readonly int MaxGestureThumbnailHeight = 250;

        public static readonly string KeyExMenuThumbnailWidth = $"{DomainConstants.SystemName}_ExMenuThumbnailWidth";
        public static readonly string KeyExMenuThumbnailHeight = $"{DomainConstants.SystemName}_ExMenuThumbnailHeight";
        public static readonly int DefaultExMenuThumbnailWidth = 256;
        public static readonly int DefaultExMenuThumbnailHeight = 256;

        public static readonly string KeyGroupDeleteConfirmation = $"{DomainConstants.SystemName}_GroupDeleteConfirmation";
        public static readonly string KeyModeDeleteConfirmation = $"{DomainConstants.SystemName}_ModeDeleteConfirmation";
        public static readonly string KeyBranchDeleteConfirmation = $"{DomainConstants.SystemName}_BranchDeleteConfirmation";
        public static readonly bool DefaultGroupDeleteConfirmation = true;
        public static readonly bool DefaultModeDeleteConfirmation = true;
        public static readonly bool DefaultBranchDeleteConfirmation = false;
    }
}
