using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using System;
using UnityEngine;
using UnityEditor;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    [CustomEditor(typeof(FESLauncherComponent))]
    public class FESLauncher : Editor
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
            if (_rootObject is null)
            {
                _disposables?.Dispose();
                _disposables = new CompositeDisposable();

                _rootObject = (target as FESLauncherComponent).gameObject;
                var installer = new FESInstaller(_rootObject);
                _inspectorView = installer.Container.Resolve<InspectorView>().AddTo(_disposables);
                _inspectorView.OnLaunchButtonClicked.Synchronize().Subscribe(_ => Launch()).AddTo(_disposables);
                _inspectorView.OnLocaleChanged.Synchronize().Subscribe(ChangeLocale).AddTo(_disposables);

                var menuRepository = installer.Container.Resolve<IMenuRepository>();
                var updateMenuSubject = installer.Container.Resolve<UpdateMenuSubject>();
                updateMenuSubject.OnNext(menuRepository.Load(null));
            }
        }

        private void Launch()
        {
            try
            {
                if (!UnpackPrefab())
                {
                    return;
                }

                var rootObject = (target as FESLauncherComponent).gameObject;

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
        }

        private void ChangeLocale(Locale locale)
        {
            // Change all instance's locale
            foreach (var window in Resources.FindObjectsOfTypeAll<MainWindow>())
            {
                window.ChangeLocale(locale);
            }
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

            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(gameObject.GetComponent<FESLauncherComponent>(), true);
        }
    }
}
