using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using Suzuryg.FacialExpressionSwitcher.Detail.View.Element;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    public class MainWindow : EditorWindow
    {
        [SerializeField] private string _launcherObjectPath;

        private MainView _mainView;
        private Undo.UndoRedoCallback _undoRedoCallback;
        private FESInstaller _installer;
        private ISubWindowManager _subWindowManager;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public MainWindow()
        {
            wantsMouseMove = true;
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

                _subWindowManager = _installer.Container.Resolve<ISubWindowManager>().AddTo(_disposables);
                _subWindowManager.Initialize(titleContent.text, _installer);

                // Disposables
                _installer.Container.Resolve<ModeNameProvider>().AddTo(_disposables);
                _installer.Container.Resolve<AnimationElement>().AddTo(_disposables);
                _installer.Container.Resolve<MainThumbnailDrawer>().AddTo(_disposables);
                _installer.Container.Resolve<GestureTableThumbnailDrawer>().AddTo(_disposables);
                _installer.Container.Resolve<ExMenuThumbnailDrawer>().AddTo(_disposables);
                var expressionEditor = _installer.Container.Resolve<ExpressionEditor>().AddTo(_disposables);
                expressionEditor.OnClipUpdated.Synchronize().ObserveOnMainThread().Subscribe(_ => Repaint()).AddTo(_disposables);

                // Initialize menu display
                var menuRepository = _installer.Container.Resolve<IMenuRepository>();
                var updateMenuSubject = _installer.Container.Resolve<UpdateMenuSubject>();
                updateMenuSubject.OnNext(menuRepository.Load(null));

                // Register undo/redo callback
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
            _subWindowManager?.CloseAllSubWinodows();

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            if (_undoRedoCallback is Undo.UndoRedoCallback)
            {
                Undo.undoRedoPerformed -= _undoRedoCallback;
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            Build();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            _installer?.SaveUIStates();
            Clean();
        }

        private void OnPlayModeChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                rootVisualElement.Clear();
                Build();
            }
        }
    }
}
