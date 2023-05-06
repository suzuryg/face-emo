using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using Suzuryg.FacialExpressionSwitcher.Detail.View.ExpressionEditor;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    public class SubWindowManager : ISubWindowManager
    {
        private string _windowTitle;
        private FESInstaller _installer;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public void Dispose()
        {
            _disposables.Dispose();
        }
        
        public void Initialize(string windowTitle, FESInstaller installer)
        {
            _windowTitle = windowTitle;
            _installer = installer;
        }

        public T Provide<T>() where T : EditorWindow
        {
            Action<EditorWindow> initializeAction = (window) =>
            {
                if (_installer is FESInstaller)
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
                            expressionPreviewWindow.Initialize(expressionEditor);
                        }
                    }
                    else
                    {
                        throw new FacialExpressionSwitcherException($"Unknown window: {typeof(T)}");
                    }

                    // Initialize display
                    var menuRepository = _installer.Container.Resolve<IMenuRepository>();
                    var updateMenuSubject = _installer.Container.Resolve<UpdateMenuSubject>();
                    updateMenuSubject.OnNext(menuRepository.Load(null));
                }
            };

            return GetWindow<T>(initializeAction);
        }

        public T ProvideIfOpenedAlready<T>() where T : EditorWindow
        {
            var existingWindows = Resources.FindObjectsOfTypeAll<T>().Where(x => x.titleContent.text == _windowTitle);
            if (existingWindows.Any()) { return existingWindows.First(); }
            else { return null; }
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
