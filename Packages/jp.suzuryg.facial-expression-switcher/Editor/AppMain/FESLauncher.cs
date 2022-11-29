using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    [CustomEditor(typeof(FESLauncherComponent))]
    public class FESLauncher : Editor
    {
        [MenuItem("GameObject/FacialExpressionSwitcher", false, 20)]
        public static void Create(MenuCommand menuCommand)
        {
            // Create GameObject which has unique name in acitive scene.
            var baseName = DomainConstants.SystemName;
            var objectName = baseName;
            var cnt = 1;
            while (GameObject.Find(objectName))
            {
                objectName = $"{baseName}_{cnt}";
                cnt++;
            }

            var gameObject = new GameObject(objectName, typeof(FESLauncherComponent));
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {DomainConstants.SystemName} Object");
            Selection.activeObject = gameObject;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button($"Launch {DomainConstants.SystemName}"))
            {
                if (!EditorApplication.isPlaying)
                {
                    try
                    {
                        if (UnpackPrefab())
                        {
                            Launch();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        EditorUtility.DisplayDialog(DomainConstants.SystemName, $"Failed to launch. Please see the console.", "OK");
                        Debug.LogError(ex.ToString());
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName, $"Please launch in EditorMode.", "OK");
                }
            }
        }

        private void Launch()
        {
            var rootObject = (target as FESLauncherComponent).gameObject;

            var windowTitle = rootObject.name;
            foreach (var window in Resources.FindObjectsOfTypeAll<MainWindow>())
            {
                if (window.titleContent.text == windowTitle)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName, $"This menu's window is opening.", "OK");
                    return;
                }
            }

            var mainWindow = CreateInstance<MainWindow>();
            mainWindow.Initialize(rootObject.GetFullPath());
            mainWindow.titleContent = new GUIContent(windowTitle);
            mainWindow.Show();
        }

        private bool UnpackPrefab()
        {
            var rootObject = (target as FESLauncherComponent).gameObject;

            if (PrefabUtility.IsAnyPrefabInstanceRoot(rootObject))
            {
                if (EditorUtility.DisplayDialog(DomainConstants.SystemName, "You need to unpack prefab. Continue?", "OK", "Cancel"))
                {
                    PrefabUtility.UnpackPrefabInstance(rootObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
