using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase
{
    public interface IGenerateFxUseCase
    {
        List<string> Prepare();
        void Handle(string menuId, IEnumerable<string> editablePrefabPaths);
    }

    public interface IGenerateFxPresenter
    {
        IObservable<(GenerateFxResult generateFxResult, string errorMessage)> Observable { get; }

        void Complete(GenerateFxResult generateFxResult, string errorMessage = "");
    }

    public interface IFxGenerator
    {
        List<string> GetParentPrefabPathOfMARootObjects();
        void Generate(IMenu menu, IEnumerable<string> editablePrefabPaths);
    }

    public interface IBackupper : IDisposable
    {
        void SetName(string name);
        void AutoBackup();
        void Export(string path);
        void Import(string path);
    }

    public enum GenerateFxResult
    {
        Succeeded,
        MenuDoesNotExist,
        ArgumentNull,
        Error,
        CompileErrorExists,
    }

    public class GenerateFxPresenter : IGenerateFxPresenter
    {
        public IObservable<(GenerateFxResult, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(GenerateFxResult, string)> _subject = new Subject<(GenerateFxResult, string)>();

        public void Complete(GenerateFxResult generateFxResult, string errorMessage = "")
        {
            _subject.OnNext((generateFxResult, errorMessage));
        }
    }

    public class GenerateFxUseCase : IGenerateFxUseCase
    {
        IMenuRepository _menuRepository;
        IFxGenerator _fxGenerator;
        IBackupper _backupper;
        IGenerateFxPresenter _generateFxPresenter;

        public GenerateFxUseCase(IMenuRepository menuRepository, IFxGenerator fxGenerator, IBackupper backupper, IGenerateFxPresenter generateFxPresenter)
        {
            _menuRepository = menuRepository;
            _fxGenerator = fxGenerator;
            _backupper = backupper;
            _generateFxPresenter = generateFxPresenter;
        }

        public List<string> Prepare()
        {
            return _fxGenerator.GetParentPrefabPathOfMARootObjects();
        }

        public void Handle(string menuId, IEnumerable<string> editablePrefabPaths)
        {
            var errorMessages = new List<string>();
            UnityEngine.Application.logMessageReceived += OnLogMessageReceived;
            try
            {
                if (menuId is null)
                {
                    _generateFxPresenter.Complete(GenerateFxResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _generateFxPresenter.Complete(GenerateFxResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                _fxGenerator.Generate(menu, new HashSet<string>(editablePrefabPaths));
                _backupper.AutoBackup();

                _generateFxPresenter.Complete(GenerateFxResult.Succeeded);
            }
            catch (Exception ex)
            {
                var compileErrorExists = errorMessages.Any(x =>
                    x.StartsWith("Please fix compile errors", StringComparison.OrdinalIgnoreCase));
                var result = compileErrorExists ? GenerateFxResult.CompileErrorExists : GenerateFxResult.Error;
                _generateFxPresenter.Complete(result, ex.ToString());
            }
            finally
            {
                UnityEngine.Application.logMessageReceived -= OnLogMessageReceived;
            }
            return;

            void OnLogMessageReceived(string condition, string stackTrace, UnityEngine.LogType type)
            {
                if (type == UnityEngine.LogType.Error) errorMessages.Add(condition);
            }
        }
    }
}
