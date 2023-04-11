using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Suzuryg.FacialExpressionSwitcher.UseCase;

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

        public void Open<T>() where T : EditorWindow
        {
            Action<VisualElement> initializeAction = (visualElement) =>
            {
                if (_installer is FESInstaller)
                {
                    // Get ViewModel
                    if (typeof(T) == typeof(GestureTableWindow))
                    {
                        var gestureTableView = _installer.Container.Resolve<GestureTableView>().AddTo(_disposables);
                        gestureTableView.Initialize(visualElement);
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

            // Get window
            GetWindow<T>(initializeAction);
        }

        public void CloseAllSubWinodows()
        {
            GetWindow<GestureTableWindow>(null)?.Close();
        }

        private T GetWindow<T>(Action<VisualElement> initializeAction) where T : EditorWindow
        {
            var existingWindows = Resources.FindObjectsOfTypeAll<T>().Where(x => x.titleContent.text == _windowTitle);
            if (existingWindows.Any())
            {
                return existingWindows.First();
            }
            else
            {
                var window = ScriptableObject.CreateInstance<T>();
                window.titleContent = new GUIContent(_windowTitle);
                window.Show();

                if (initializeAction is Action<VisualElement>)
                {
                    initializeAction(window.rootVisualElement);
                }

                return window;
            }
        }
    }
}
