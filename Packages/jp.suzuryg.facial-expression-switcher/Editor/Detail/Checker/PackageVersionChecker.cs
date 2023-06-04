using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif

namespace Suzuryg.FacialExpressionSwitcher.Detail.Checker
{
    [InitializeOnLoad]
    public static class PackageVersionChecker
    {
        public static bool IsCompleted { get; private set; } = false;
        public static string FacialExpressionSwitcher { get; private set; } = string.Empty;
        public static string ModularAvatar { get; private set; } = string.Empty;

        #if UNITY_EDITOR
        private static ListRequest _request;
        #endif

        static PackageVersionChecker()
        {
            #if UNITY_EDITOR
            _request = Client.List();
            EditorApplication.update += Progress;
            #endif
        }

        #if UNITY_EDITOR
        private static void Progress()
        {
            if (_request.IsCompleted)
            {
                if (_request.Status == StatusCode.Success)
                {
                    foreach (var package in _request.Result)
                    {
                        if (package.name == "jp.suzuryg.facial-expression-switcher")
                        {
                            FacialExpressionSwitcher = package.version;
                        }
                        else if (package.name == "nadena.dev.modular-avatar")
                        {
                            ModularAvatar = package.version;
                        }
                    }
                }

                if (string.IsNullOrEmpty(ModularAvatar))
                {
                    const string line0 = "FacialExpressionSwitcherを使用するためには、";
                    const string line1 = "Modular Avatarをインストールする必要があります！";
                    const string line2 = "Modular Avatar must be installed in order to use FacialExpressionSwitcher!";

                    EditorUtility.DisplayDialog("FacialExpressionSwitcher", line0 + "\n" + line1 + "\n\n" + line2, "OK");
                    Debug.LogError(line0 +  line1 + "\n" + line2);
                }

                IsCompleted = true;
                EditorApplication.update -= Progress;
            }
        }
        #endif
    }
}
