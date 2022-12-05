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
        public static string ViewDirectory => $"Packages/{PackageName}/Editor/Detail/View";
        public static string IconDirectory => $"{ViewDirectory}/Icon";
        public static string LocalizationDirectory => $"Packages/{PackageName}/Editor/Detail/Localization";
        public static readonly string DragAndDropDataKey_MenuItemIds = $"{DomainConstants.SystemName}_MenuItemIds";

        public static readonly int MinMainThumbnailSize = 100;
        public static readonly int MaxMainThumbnailSize = 300;
        public static readonly string KeyMainThumbnailSize = "MainThumbnailSize";

        public static readonly int MinGestureThumbnailSize = 100;
        public static readonly int MaxGestureThumbnailSize = 300;
        public static readonly string KeyGestureThumbnailSize = "GestureThumbnailSize";
    }
}
