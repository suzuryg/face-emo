using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public class AssetDatabaseUtility
    {
        // https://hacchi-man.hatenablog.com/entry/2020/08/23/220000
        public static void CreateFolderRecursively(string path)
        {
            // Assetsから始まってない場合は処理できない
            if (!path.StartsWith("Assets/"))
                return;
         
            // AssetDatabase なので 区切り文字は /
            var dirs = path.Split('/');
            var combinePath = dirs[0];
            // Assets の部分はスキップ
            foreach (var dir in dirs.Skip(1))
            {
                // ディレクトリの存在確認
                if (!AssetDatabase.IsValidFolder(combinePath + '/' + dir))
                    AssetDatabase.CreateFolder(combinePath, dir);
                combinePath += '/' + dir;
            }
        }

        public static string GetDirectoryName(string path)
        {
            return System.IO.Path.GetDirectoryName(path).Replace(System.IO.Path.DirectorySeparatorChar, '/');
        }

        public static string GetFileName(string path) => System.IO.Path.GetFileName(path);
    }
}
