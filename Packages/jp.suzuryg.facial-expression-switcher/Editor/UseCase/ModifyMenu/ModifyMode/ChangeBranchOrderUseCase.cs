using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public interface IChangeBranchOrderUseCase
    {
        void Handle(string menuId, string modeId, int from, int to);
    }

    public interface IChangeBranchOrderPresenter
    {
        IObservable<(ChangeBranchOrderResult, IMenu, string)> Observable { get; }

        void Complete(ChangeBranchOrderResult changeBranchOrderResult, in IMenu menu, string errorMessage = "");
    }

    public enum ChangeBranchOrderResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class ChangeBranchOrderPresenter : IChangeBranchOrderPresenter
    {
        public IObservable<(ChangeBranchOrderResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(ChangeBranchOrderResult, IMenu, string)> _subject = new Subject<(ChangeBranchOrderResult, IMenu, string)>();

        public void Complete(ChangeBranchOrderResult changeBranchOrderResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((changeBranchOrderResult, menu, errorMessage));
        }
    }

    public class ChangeBranchOrderUseCase : IChangeBranchOrderUseCase
    {
        IMenuRepository _menuRepository;
        IChangeBranchOrderPresenter _changeBranchOrderPresenter;

        public ChangeBranchOrderUseCase(IMenuRepository menuRepository, IChangeBranchOrderPresenter changeBranchOrderPresenter)
        {
            _menuRepository = menuRepository;
            _changeBranchOrderPresenter = changeBranchOrderPresenter;
        }

        public void Handle(string menuId, string modeId, int from, int to)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _changeBranchOrderPresenter.Complete(ChangeBranchOrderResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _changeBranchOrderPresenter.Complete(ChangeBranchOrderResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanChangeBranchOrder(modeId, from))
                {
                    _changeBranchOrderPresenter.Complete(ChangeBranchOrderResult.InvalidBranch, menu);
                    return;
                }

                menu.ChangeBranchOrder(modeId, from, to);

                _menuRepository.Save(menuId, menu, "ChangeBranchOrder");
                _changeBranchOrderPresenter.Complete(ChangeBranchOrderResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _changeBranchOrderPresenter.Complete(ChangeBranchOrderResult.Error, null, ex.ToString());
            }
        }
    }
}
