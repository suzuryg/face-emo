using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif

namespace Suzuryg.FaceEmo.Detail
{
    [InitializeOnLoad]
    public static class PackageVersionChecker
    {
        public static bool IsCompleted { get; private set; } = false;
        public static string FaceEmo { get; private set; } = string.Empty;

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
                        if (package.name == "jp.suzuryg.face-emo")
                        {
                            FaceEmo = package.version;
                        }
                    }
                }

                IsCompleted = true;
                EditorApplication.update -= Progress;
            }
        }
        #endif
    }
}
