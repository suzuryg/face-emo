using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public interface IChangeConditionOrderUseCase
    {
        void Handle(string menuId, string modeId, int branchIndex, int from, int to);
    }

    public interface IChangeConditionOrderPresenter
    {
        event Action<ChangeConditionOrderResult, IMenu, string> OnCompleted;

        void Complete(ChangeConditionOrderResult changeConditionOrderResult, in IMenu menu, string errorMessage = "");
    }

    public enum ChangeConditionOrderResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidCondition,
        ArgumentNull,
        Error,
    }

    public class ChangeConditionOrderPresenter : IChangeConditionOrderPresenter
    {
        public event Action<ChangeConditionOrderResult, IMenu, string> OnCompleted;

        public void Complete(ChangeConditionOrderResult changeConditionOrderResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(changeConditionOrderResult, menu, errorMessage);
        }
    }

    public class ChangeConditionOrderUseCase : IChangeConditionOrderUseCase
    {
        IMenuRepository _menuRepository;
        IChangeConditionOrderPresenter _changeConditionOrderPresenter;

        public ChangeConditionOrderUseCase(IMenuRepository menuRepository, IChangeConditionOrderPresenter changeConditionOrderPresenter)
        {
            _menuRepository = menuRepository;
            _changeConditionOrderPresenter = changeConditionOrderPresenter;
        }

        public void Handle(string menuId, string modeId, int branchIndex, int from, int to)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _changeConditionOrderPresenter.Complete(ChangeConditionOrderResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _changeConditionOrderPresenter.Complete(ChangeConditionOrderResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanChangeConditionOrder(modeId, branchIndex, from))
                {
                    _changeConditionOrderPresenter.Complete(ChangeConditionOrderResult.InvalidCondition, menu);
                    return;
                }

                menu.ChangeConditionOrder(modeId, branchIndex, from, to);

                _menuRepository.Save(menuId, menu, "ChangeConditionOrder");
                _changeConditionOrderPresenter.Complete(ChangeConditionOrderResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _changeConditionOrderPresenter.Complete(ChangeConditionOrderResult.Error, null, ex.ToString());
            }
        }
    }
}
