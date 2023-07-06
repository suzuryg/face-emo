using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.Detail;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Data;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Detail.View;
using Suzuryg.FaceEmo.Detail.View.Element;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UniRx;

namespace Suzuryg.FaceEmo.AppMain
{
    public class MainWindow : EditorWindow
    {
        [SerializeField] private string _launcherObjectPath;

        private MainView _mainView;
        private Undo.UndoRedoCallback _undoRedoCallback;
        private FaceEmoInstaller _installer;
        private ISubWindowManager _subWindowManager;
        private MainWindowProvider _mainWindowProvider;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public void Initialize(string launcherObjectPath)
        {
            _launcherObjectPath = launcherObjectPath;
            Build();
        }

        public void ChangeLocale(Locale locale)
        {
            if (_installer is FaceEmoInstaller)
            {
                _installer.Container.Resolve<ILocalizationSetting>().SetLocale(locale);
            }
        }

        private void Build()
        {
            Clean();
            if (_launcherObjectPath is string)
            {
                _installer = FaceEmoInstaller.GetInstaller(_launcherObjectPath);
                if (_installer == null) { Clean(); return; }

                _mainWindowProvider = _installer.Container.Resolve<MainWindowProvider>();
                _mainView = _installer.Container.Resolve<MainView>().AddTo(_disposables);
                _mainView.Initialize(rootVisualElement);

                _subWindowManager = _installer.Container.Resolve<ISubWindowManager>().AddTo(_disposables);
                _subWindowManager.Initialize(_installer);

                var backupper = _installer.Container.Resolve<IBackupper>().AddTo(_disposables);
                backupper.SetName(_installer.RootObjectName);

                // Disposables
                _installer.Container.Resolve<ISubWindowManager>().AddTo(_disposables);
                _installer.Container.Resolve<IBackupper>().AddTo(_disposables);
                _installer.Container.Resolve<SelectionSynchronizer>().AddTo(_disposables);
                _installer.Container.Resolve<MainThumbnailDrawer>().AddTo(_disposables);
                _installer.Container.Resolve<GestureTableThumbnailDrawer>().AddTo(_disposables);
                _installer.Container.Resolve<ExMenuThumbnailDrawer>().AddTo(_disposables);
                _installer.Container.Resolve<InspectorThumbnailDrawer>().AddTo(_disposables);
                _installer.Container.Resolve<ModeNameProvider>().AddTo(_disposables);
                _installer.Container.Resolve<AnimationElement>().AddTo(_disposables);
                _installer.Container.Resolve<ExpressionEditor>().AddTo(_disposables);

                var expressionEditor = _installer.Container.Resolve<ExpressionEditor>().AddTo(_disposables);
                expressionEditor.OnClipUpdated.Synchronize().ObserveOnMainThread().Subscribe(_ => Repaint()).AddTo(_disposables);

                // Initialize menu display
                var menuRepository = _installer.Container.Resolve<IMenuRepository>();
                var updateMenuSubject = _installer.Container.Resolve<UpdateMenuSubject>();
                updateMenuSubject.OnNext(menuRepository.Load(null), isModified: false);

                // Register undo/redo callback
                _undoRedoCallback = () =>
                {
                    if (menuRepository is MenuRepository && updateMenuSubject is UpdateMenuSubject)
                    {
                        updateMenuSubject.OnNext(menuRepository.Load(null), isModified: false); // Do not set the dirty flag of the apply button when undo is performed
                    }
                };
                Undo.undoRedoPerformed += _undoRedoCallback;
            }
        }

        private void OnGUI()
        {
            _mainWindowProvider?.OnGUI.OnNext(this);
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
            wantsMouseMove = true;

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
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode ||
                playModeStateChange == PlayModeStateChange.EnteredPlayMode)
            {
                rootVisualElement.Clear();
                Build();
            }
        }
    }
}
