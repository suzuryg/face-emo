using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public class PathConverter
    {
        public static string ToUnityPath(string systemPath)
        {
            var unityPath = systemPath.Replace(Path.DirectorySeparatorChar, '/');
            unityPath = unityPath.Replace(UnityEngine.Application.dataPath, "Assets");
            return unityPath;
        }

        public static string ToSystemPath(string unityPath)
        {
            var systemPath = UnityEngine.Application.dataPath.Replace("Assets", unityPath);
            systemPath = systemPath.Replace('/', Path.DirectorySeparatorChar);
            return systemPath;
        }
    }
}
