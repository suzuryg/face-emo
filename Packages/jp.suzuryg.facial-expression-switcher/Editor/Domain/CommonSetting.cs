using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class CommonSetting
    {
        public static readonly int MenuItemNums = 8;
        public static readonly int MaxModeNums = 256;
        public static readonly string SystemName = "FacialExpressionSwitcher";
        public static readonly string PackageName = "jp.suzuryg.facial-expression-switcher";

        public static string ViewDirectory => $"Packages/{PackageName}/Editor/Detail/View";
    }
}
