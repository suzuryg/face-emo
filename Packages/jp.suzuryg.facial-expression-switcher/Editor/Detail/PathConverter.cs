using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public class PathConverter
    {
        public static string ToUnityPath(string systemPath)
        {
            var unityPath = systemPath.Replace(Path.DirectorySeparatorChar, '/');
            var regEx = new Regex(@"(.*)/Assets/(.*)");
            var match = regEx.Match(unityPath);
            if (match.Groups.Count == 3)
            {
                return $"Assets/{match.Groups[2]}";
            }
            else
            {
                return null;
            }
        }

        public static string ToSystemPath(string unityPath)
        {
            var systemPath = UnityEngine.Application.dataPath.Replace("Assets", unityPath);
            systemPath = systemPath.Replace('/', Path.DirectorySeparatorChar);
            return systemPath;
        }
    }
}
