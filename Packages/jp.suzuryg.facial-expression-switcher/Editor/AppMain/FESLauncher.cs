using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using System;
using UnityEngine;
using UnityEditor;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using UniRx;
using VRC.SDK3.Avatars.Components;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    [CustomEditor(typeof(FESLauncherComponent))]
    public class FESLauncher : ScriptlessEditor
    {
        GameObject _rootObject;
        InspectorView _inspectorView;
        CompositeDisposable _disposables = new CompositeDisposable();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Initialize();
            _inspectorView?.OnGUI();
        }

        private void Initialize()
        {
            if (_rootObject == null)
            {
                _disposables?.Dispose();
                _disposables = new CompositeDisposable();

                _rootObject = (target as FESLauncherComponent).gameObject;
                var installer = new FESInstaller(_rootObject);

                // Inspector view
                _inspectorView = installer.Container.Resolve<InspectorView>().AddTo(_disposables);
                _inspectorView.OnLaunchButtonClicked.Synchronize().Subscribe(_ => Launch(target as FESLauncherComponent)).AddTo(_disposables);
                _inspectorView.OnLocaleChanged.Synchronize().Subscribe(ChangeLocale).AddTo(_disposables);

                // Disposables
                installer.Container.Resolve<InspectorThumbnailDrawer>().AddTo(_disposables);

                // Initialize
                var menuRepository = installer.Container.Resolve<IMenuRepository>();
                var updateMenuSubject = installer.Container.Resolve<UpdateMenuSubject>();
                updateMenuSubject.OnNext(menuRepository.Load(null));
            }
        }

        public static void Launch(FESLauncherComponent launcher)
        {
#if USE_MODULAR_AVATAR
            try
            {
                var rootObject = launcher.gameObject;

                var windowTitle = rootObject.name;
                foreach (var window in Resources.FindObjectsOfTypeAll<MainWindow>())
                {
                    if (window.titleContent.text == windowTitle)
                    {
                        window.Show();
                        return;
                    }
                }

                var mainWindow = CreateInstance<MainWindow>();
                mainWindow.titleContent = new GUIContent(windowTitle);
                mainWindow.Initialize(rootObject.GetFullPath());
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"Failed to launch. Please see the console.", "OK");
                Debug.LogError(ex.ToString());
            }
#else
            EditorUtility.DisplayDialog(DomainConstants.SystemName, LocalizationSetting.GetTable(LocalizationSetting.GetLocale()).Common_Message_MAIsNotInstalled, "OK");
#endif
        }

        private void ChangeLocale(Locale locale)
        {
            // Change all instance's locale
            foreach (var window in Resources.FindObjectsOfTypeAll<MainWindow>())
            {
                window.ChangeLocale(locale);
            }
        }

        [MenuItem("FacialExpressionSwitcher/New Menu", false, 0)]
        [MenuItem("GameObject/FacialExpressionSwitcher/New Menu", false, 20)]
        public static GameObject Create(MenuCommand menuCommand)
        {
            var gameObject = GetLauncherObject(menuCommand);

            // Create GameObject which has unique name in acitive scene.
            var baseName = DomainConstants.SystemName;
            var objectName = baseName;
            var cnt = 1;
            while (GameObject.Find(objectName))
            {
                objectName = $"{baseName}_{cnt}";
                cnt++;
            }
            gameObject.name = objectName;

            return gameObject;
        }

        [MenuItem("GameObject/FacialExpressionSwitcher/Restore Menu", false, 21)]
        public static void Restore(MenuCommand menuCommand)
        {
            var selectedPath = EditorUtility.OpenFilePanelWithFilters(title: null, directory: null, filters: new[] { "FESProject" , "asset" });
            if (string.IsNullOrEmpty(selectedPath)) { return; }

            // OpenFilePanel path is in OS format, so convert it to Unity format
            var unityPath = PathConverter.ToUnityPath(selectedPath);

            var gameObject = GetLauncherObject(menuCommand);
            var name = System.IO.Path.GetFileName(selectedPath).Replace(".asset", string.Empty);
            gameObject.name = name;

            var installer = new FESInstaller(gameObject);
            var backupper = installer.Container.Resolve<IBackupper>();
            try
            {
                backupper.Import(unityPath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                var message = ex is FacialExpressionSwitcherException ? ex.Message : "Failed to open FESProject.";
                EditorUtility.DisplayDialog(DomainConstants.SystemName, message, "OK");
                if (gameObject != null) { DestroyImmediate(gameObject); }
            }
        }

        private static GameObject GetLauncherObject(MenuCommand menuCommand)
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<FESLauncherComponent>();
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {DomainConstants.SystemName} Object");
            Selection.activeObject = gameObject;
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(gameObject.GetComponent<FESLauncherComponent>(), true);
            return gameObject;
        }

        [InitializeOnLoadMethod]
        private static void AddHierarchyItemOnGUI()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (EditorPrefs.GetBool(DetailConstants.KeyHideHierarchyIcon, DetailConstants.DefaultHideHierarchyIcon))
            {
                return;
            }

            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null)
            {
                return;
            }

            var avatarDescriptor = gameObject.GetComponent<VRCAvatarDescriptor>();
            if (avatarDescriptor == null)
            {
                return;
            }

            var loc = LocalizationSetting.GetTable(LocalizationSetting.GetLocale());

            const float buttonWidth = 30;
            selectionRect.xMin += selectionRect.width - buttonWidth;
            selectionRect.yMin += 1;
            selectionRect.yMax -= 1;
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("b8710db71cc992745987c4e92d53fcd1")); // logo
            if (GUI.Button(selectionRect, new GUIContent(string.Empty, loc.Common_Tooltip_LaunchFromHierarchy)))
            {
                var exists = false;
                foreach (var launcher in FindObjectsOfType<FESLauncherComponent>()?.OrderBy(x => x.name))
                {
                    if (ReferenceEquals(launcher?.AV3Setting?.TargetAvatar, avatarDescriptor))
                    {
                        Launch(launcher);
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    var launcherObject = Create(new MenuCommand(null, 0));
                    new FESInstaller(launcherObject);

                    var launcher = launcherObject.GetComponent<FESLauncherComponent>();
                    launcher.AV3Setting.TargetAvatar = avatarDescriptor;
                    Launch(launcher);
                }
            }
            GUI.DrawTexture(selectionRect, icon, ScaleMode.ScaleToFit, alphaBlend: true);
        }

        private const string HideHierarchyIconPath = "FacialExpressionSwitcher/Hide Hierarchy Icon";
        private const int HideHierarchyIconOrder = 99;

        [MenuItem(HideHierarchyIconPath, false, HideHierarchyIconOrder)]
        private static void HideHierarchyIcon()
        {
            EditorPrefs.SetBool(DetailConstants.KeyHideHierarchyIcon, !EditorPrefs.GetBool(DetailConstants.KeyHideHierarchyIcon, DetailConstants.DefaultHideHierarchyIcon));
        }

        [MenuItem(HideHierarchyIconPath, true, HideHierarchyIconOrder)]
        private static bool HideHierarchyIconValidate()
        {
            UnityEditor.Menu.SetChecked(HideHierarchyIconPath, EditorPrefs.GetBool(DetailConstants.KeyHideHierarchyIcon, DetailConstants.DefaultHideHierarchyIcon));
            return true;
        }
    }
}
