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
        IObservable<(GenerateFxResult generateFxResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(GenerateFxResult generateFxResult, in IMenu menu, string errorMessage = "");
    }

    public interface IFxGenerator
    {
        void Generate(IMenu menu);
    }

    public enum GenerateFxResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidArgument,
        ArgumentNull,
        Error,
    }

    public class GenerateFxPresenter : IGenerateFxPresenter
    {
        public IObservable<(GenerateFxResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(GenerateFxResult, IMenu, string)> _subject = new Subject<(GenerateFxResult, IMenu, string)>();

        public void Complete(GenerateFxResult generateFxResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((generateFxResult, menu, errorMessage));
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

                _generateFxPresenter.Complete(GenerateFxResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _generateFxPresenter.Complete(GenerateFxResult.Error, null, ex.ToString());
            }
        }
    }
}
