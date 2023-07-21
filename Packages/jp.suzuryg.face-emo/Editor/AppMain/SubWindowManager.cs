using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Detail.View;
using Suzuryg.FaceEmo.Detail.View.Element;
using Suzuryg.FaceEmo.Detail.View.ExpressionEditor;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Suzuryg.FaceEmo.AppMain
{
    public class SubWindowManager : ISubWindowManager
    {
        private FaceEmoInstaller _installer;
        private SceneView _lastActiveSceneView;

        private object _lockFindObjects = new object();

        private CompositeDisposable _disposables = new CompositeDisposable();

        public void Dispose()
        {
            _disposables.Dispose();
        }
        
        public void Initialize(FaceEmoInstaller installer)
        {
            _installer = installer;

            // Ensure that subwindows are owned by the main window when opening a new main window.
            CloseAllSubWinodows();
        }

        public T Provide<T>() where T : EditorWindow, ISubWindow
        {
            Action<EditorWindow> initializeAction = (window) =>
            {
                if (_installer is FaceEmoInstaller)
                {
                    if (typeof(T) == typeof(GestureTableWindow))
                    {
                        var gestureTableView = _installer.Container.Resolve<GestureTableView>().AddTo(_disposables);
                        gestureTableView.Initialize(window.rootVisualElement);
                    }
                    else if (typeof(T) == typeof(ExpressionEditorWindow))
                    {
                        if (window is ExpressionEditorWindow expressionEditorWindow)
                        {
                            expressionEditorWindow.SetProvider(this);
                        }

                        var expressionEditorView = _installer.Container.Resolve<ExpressionEditorView>().AddTo(_disposables);
                        expressionEditorView.Initialize(window.rootVisualElement);
                    }
                    else if (typeof(T) == typeof(ExpressionPreviewWindow))
                    {
                        if (window is ExpressionPreviewWindow expressionPreviewWindow)
                        {
                            var expressionEditor = _installer.Container.Resolve<ExpressionEditor>();
                            expressionPreviewWindow.Initialize(expressionEditor, _lastActiveSceneView);
                        }
                    }
                    else if (typeof(T) == typeof(CombineClipsDialog))
                    {
                        // NOP
                    }
                    else
                    {
                        throw new FaceEmoException($"Unknown window: {typeof(T)}");
                    }

                    // Initialize display
                    var menuRepository = _installer.Container.Resolve<IMenuRepository>();
                    var updateMenuSubject = _installer.Container.Resolve<UpdateMenuSubject>();
                    updateMenuSubject.OnNext(menuRepository.Load(null), isModified: false);
                }
            };

            return GetWindow<T>(initializeAction);
        }

        public T ProvideIfOpenedAlready<T>() where T : EditorWindow, ISubWindow
        {
            // SIGSEGV occurred during execution of FindObjectsOfTypeAll(). Occurred due to concurrent access? Exclusion control it to be safe.
            lock (_lockFindObjects)
            {
                var existingWindows = Resources.FindObjectsOfTypeAll<T>();
                if (existingWindows.Any()) { return existingWindows.First(); }
                else { return null; }
            }
        }

        public void CloseAllSubWinodows()
        {
            try
            {
                ProvideIfOpenedAlready<GestureTableWindow>()?.CloseIfNotDocked();
                ProvideIfOpenedAlready<ExpressionEditorWindow>()?.CloseIfNotDocked();
                ProvideIfOpenedAlready<ExpressionPreviewWindow>()?.CloseIfNotDocked();
                ProvideIfOpenedAlready<CombineClipsDialog>()?.Close();
            }
            catch (NullReferenceException ex)
            {
                // If a sub-window is docked, an error may occur in Close(), for example, when recompiling a script.
                // Close() is not executed when docked, but the correct docking status may not be obtained.

                // Sometimes the title of the docked EditorWindow becomes "Failed to load" and it does not work properly.
                // In this state, it is not possible to close it from the script, but restarting Unity will restore it while maintaining the docking.
                var loc = LocalizationSetting.GetTable(LocalizationSetting.GetLocale());
                Debug.Log(loc.Common_Message_ErrorWhenClosingSubWindows + "\n" + ex);
            }
        }

        private T GetWindow<T>(Action<EditorWindow> initializeAction) where T : EditorWindow, ISubWindow
        {
            var window = ProvideIfOpenedAlready<T>();
            if (window == null)
            {
                window = ScriptableObject.CreateInstance<T>();
                window.titleContent = new GUIContent(DomainConstants.SystemName);
                window.Show();
            }

            if (!window.IsInitialized)
            {
                _lastActiveSceneView = SceneView.lastActiveSceneView;

                if (initializeAction is Action<EditorWindow>)
                {
                    initializeAction(window);
                    window.IsInitialized = true;
                }
            }

            return window;
        }
    }
}
