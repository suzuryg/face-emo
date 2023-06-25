using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.View;
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
        private string _windowTitle;
        private FaceEmoInstaller _installer;
        private SceneView _lastActiveSceneView;

        private object _lockFindObjects = new object();

        private CompositeDisposable _disposables = new CompositeDisposable();

        public void Dispose()
        {
            _disposables.Dispose();
        }
        
        public void Initialize(string windowTitle, FaceEmoInstaller installer)
        {
            _windowTitle = windowTitle;
            _installer = installer;
        }

        public T Provide<T>() where T : EditorWindow
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

        public T ProvideIfOpenedAlready<T>() where T : EditorWindow
        {
            // SIGSEGV occurred during execution of FindObjectsOfTypeAll(). Occurred due to concurrent access? Exclusion control it to be safe.
            lock (_lockFindObjects)
            {
                var existingWindows = Resources.FindObjectsOfTypeAll<T>().Where(x => x.titleContent.text == _windowTitle);
                if (existingWindows.Any()) { return existingWindows.First(); }
                else { return null; }
            }
        }

        public void CloseAllSubWinodows()
        {
            ProvideIfOpenedAlready<GestureTableWindow>()?.Close();
            ProvideIfOpenedAlready<ExpressionEditorWindow>()?.Close();
            ProvideIfOpenedAlready<ExpressionPreviewWindow>()?.Close();
        }

        private T GetWindow<T>(Action<EditorWindow> initializeAction) where T : EditorWindow
        {
            var existingWindow = ProvideIfOpenedAlready<T>();
            if (existingWindow is T) { return existingWindow; }
            else
            {
                _lastActiveSceneView = SceneView.lastActiveSceneView;

                var window = ScriptableObject.CreateInstance<T>();
                window.titleContent = new GUIContent(_windowTitle);
                window.Show();

                if (initializeAction is Action<EditorWindow>)
                {
                    initializeAction(window);
                }

                return window;
            }
        }
    }
}
