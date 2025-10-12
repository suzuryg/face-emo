using System;
using System.Linq;
using Microsoft.Win32;
using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;

namespace FaceEmoDevTools
{
    internal static class EditorPrefsDeleter
    {
        [MenuItem("Tools/FaceEmo DevTools/Delete All EditorPrefs with Domain Prefix")]
        private static void DeleteAllEditorPrefsWithDomainPrefix()
        {
            const string logPrefix = "FaceEmoDevTools";
            if (!EditorUtility.DisplayDialog("Delete FaceEmo EditorPrefs",
                "This will delete all EditorPrefs entries whose keys start with the FaceEmo prefix. " +
                "Do you really want to continue?",
                "Delete",
                "Cancel"))
            {
                Debug.Log($"[{logPrefix}] Deletion was cancelled by the user.");
                return;
            }

            var prefix = DomainConstants.SystemName + "_";

#if UNITY_EDITOR_WIN
            DeleteKeysOnWindows(prefix, logPrefix);
#elif UNITY_EDITOR_OSX
            Debug.LogWarning($"[{logPrefix}] Deleting EditorPrefs by prefix is not implemented on macOS.");
#elif UNITY_EDITOR_LINUX
            Debug.LogWarning($"[{logPrefix}] Deleting EditorPrefs by prefix is not implemented on Linux.");
#else
            Debug.LogWarning($"[{logPrefix}] Unsupported editor platform.");
#endif
        }

#if UNITY_EDITOR_WIN
        private static void DeleteKeysOnWindows(string prefix, string logPrefix)
        {
            const string baseKeyPath = @"Software\Unity Technologies\Unity Editor 5.x";
            using var baseKey = Registry.CurrentUser.OpenSubKey(baseKeyPath, writable: true);
            if (baseKey == null)
            {
                Debug.LogWarning($"[{logPrefix}] Registry key '{baseKeyPath}' was not found.");
                return;
            }

            var valueNames = baseKey.GetValueNames() ?? Array.Empty<string>();
            var keysToDelete = valueNames.Where(name => name.StartsWith(prefix, StringComparison.Ordinal)).ToList();
            foreach (var name in keysToDelete)
            {
                baseKey.DeleteValue(name, throwOnMissingValue: false);
                Debug.Log($"[{logPrefix}] Deleted key: {name}");
            }

            if (keysToDelete.Count == 0)
            {
                Debug.Log($"[{logPrefix}] No keys matched the specified prefix.");
            }
        }
#endif
    }
}
