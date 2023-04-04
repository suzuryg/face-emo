using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public interface IGenerateFxUseCase
    {
        void Handle(string menuId);
    }

    public interface IGenerateFxPresenter
    {
        IObservable<(GenerateFxResult generateFxResult, string errorMessage)> Observable { get; }

        void Complete(GenerateFxResult generateFxResult, string errorMessage = "");
    }

    public interface IFxGenerator
    {
        void Generate(IMenu menu);
    }

    public enum GenerateFxResult
    {
        Succeeded,
        MenuDoesNotExist,
        ArgumentNull,
        Error,
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
        IGenerateFxPresenter _generateFxPresenter;

        public GenerateFxUseCase(IMenuRepository menuRepository, IFxGenerator fxGenerator, IGenerateFxPresenter generateFxPresenter)
        {
            _menuRepository = menuRepository;
            _fxGenerator = fxGenerator;
            _generateFxPresenter = generateFxPresenter;
        }

        public void Handle(string menuId)
        {
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

                _fxGenerator.Generate(menu);

                _generateFxPresenter.Complete(GenerateFxResult.Succeeded);
            }
            catch (Exception ex)
            {
                _generateFxPresenter.Complete(GenerateFxResult.Error, ex.ToString());
            }
        }
    }
}
