using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.Subject;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UniRx;
using System;
using UnityEngine.UIElements;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    public class MainWindow : EditorWindow
    {
        [SerializeField] private string _launcherObjectPath;
        private MainView _mainView;
        private Undo.UndoRedoCallback _undoRedoCallback;
        private FESInstaller _installer;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public MainWindow()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            wantsMouseMove = true;
        }

        ~MainWindow()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        public void Initialize(string launcherObjectPath)
        {
            _launcherObjectPath = launcherObjectPath;
            Build();
        }

        public void ChangeLocale(Locale locale)
        {
            if (_installer is FESInstaller)
            {
                _installer.Container.Resolve<ILocalizationSetting>().SetLocale(locale);
            }
        }

        private void Build()
        {
            Clean();
            if (_launcherObjectPath is string)
            {
                _installer = FESInstaller.GetInstaller(_launcherObjectPath);
                _mainView = _installer.Container.Resolve<MainView>().AddTo(_disposables);
                _mainView.Initialize(rootVisualElement);

                var menuRepository = _installer.Container.Resolve<IMenuRepository>();
                var updateMenuSubject = _installer.Container.Resolve<UpdateMenuSubject>();
                updateMenuSubject.OnNext(menuRepository.Load(null));

                _undoRedoCallback = () =>
                {
                    if (menuRepository is MenuRepository && updateMenuSubject is UpdateMenuSubject)
                    {
                        updateMenuSubject.OnNext(menuRepository.Load(null));
                    }
                };
                Undo.undoRedoPerformed += _undoRedoCallback;
            }
        }

        private void Clean()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            CloseChildWindows();

            if (_undoRedoCallback is Undo.UndoRedoCallback)
            {
                Undo.undoRedoPerformed -= _undoRedoCallback;
            }
        }

        private void OpenChildWindows()
        {
            GetChildWindow<GestureTableWindow>(visualElement => 
            {
                if (_installer is FESInstaller)
                {
                    var gestureTableView = _installer.Container.Resolve<GestureTableView>().AddTo(_disposables);
                    gestureTableView.Initialize(visualElement);

                    var menuRepository = _installer.Container.Resolve<IMenuRepository>();
                    var updateMenuSubject = _installer.Container.Resolve<UpdateMenuSubject>();
                    updateMenuSubject.OnNext(menuRepository.Load(null));
                }
            });
        }

        private void CloseChildWindows()
        {
            GetChildWindow<GestureTableWindow>(null).Close();
        }

        private T GetChildWindow<T>(Action<VisualElement> initializeAction) where T : EditorWindow
        {
            var windowTitle = titleContent.text;
            var existingWindows = Resources.FindObjectsOfTypeAll<T>().Where(x => x.titleContent.text == windowTitle);
            if (existingWindows.Any())
            {
                return existingWindows.First();
            }
            else
            {
                var window = CreateInstance<T>();
                window.titleContent = new GUIContent(windowTitle);
                window.Show();
                if (initializeAction is Action<VisualElement>)
                {
                    initializeAction(window.rootVisualElement);
                }
                return window;
            }
        }

        private void Update()
        {
            OpenChildWindows();
        }

        private void OnEnable()
        {
            Build();
        }

        private void OnDisable()
        {
            Clean();
        }

        private void OnPlayModeChanged(PlayModeStateChange playModeStateChange)
        {
            // UI is enabled only in EditMode.
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    rootVisualElement.Clear();
                    Build();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    rootVisualElement.Clear();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    rootVisualElement.Clear();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    rootVisualElement.Clear();
                    break;
            }
        }
    }
}
