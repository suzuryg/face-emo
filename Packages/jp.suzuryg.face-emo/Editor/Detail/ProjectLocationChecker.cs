#if UNITY_EDITOR
using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail
{
    [InitializeOnLoad]
    internal static class ProjectLocationChecker
    {
        static ProjectLocationChecker()
        {
            try
            {
                if (!AreAllAsciiChar(Application.dataPath))
                {
                    var currentCulture = CultureInfo.CurrentUICulture;
                    if (currentCulture.Name == "ja-JP")
                    {
                        Debug.Log("プロジェクトの保存先に日本語が含まれている場合、アバターのアップロードに失敗することがあります。" +
                            "アップロードに失敗する場合、半角英数字だけを使用したフォルダにプロジェクトをコピーして再試行してみてください。");
                    }
                    else
                    {
                        Debug.Log("If the project location contains non-ASCII characters, the avatar may fail to upload. " +
                            "If the upload fails, please copy the project to a folder with only ASCII characters and try again.");
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static bool AreAllAsciiChar(string text)
        {
            foreach (char c in text)
            {
                if (c > sbyte.MaxValue)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
#endif

