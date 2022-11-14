using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public interface IRemoveConditionUseCase
    {
        void Handle(string menuId, string modeId, int branchIndex, int conditionIndex);
    }

    public interface IRemoveConditionPresenter
    {
        event Action<RemoveConditionResult, IMenu, string> OnCompleted;

        void Complete(RemoveConditionResult removeConditionResult, in IMenu menu, string errorMessage = "");
    }

    public enum RemoveConditionResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidCondition,
        ArgumentNull,
        Error,
    }

    public class RemoveConditionPresenter : IRemoveConditionPresenter
    {
        public event Action<RemoveConditionResult, IMenu, string> OnCompleted;

        public void Complete(RemoveConditionResult removeConditionResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(removeConditionResult, menu, errorMessage);
        }
    }

    public class RemoveConditionUseCase : IRemoveConditionUseCase
    {
        IMenuRepository _menuRepository;
        IRemoveConditionPresenter _removeConditionPresenter;

        public RemoveConditionUseCase(IMenuRepository menuRepository, IRemoveConditionPresenter removeConditionPresenter)
        {
            _menuRepository = menuRepository;
            _removeConditionPresenter = removeConditionPresenter;
        }

        public void Handle(string menuId, string modeId, int branchIndex, int conditionIndex)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _removeConditionPresenter.Complete(RemoveConditionResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _removeConditionPresenter.Complete(RemoveConditionResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanRemoveCondition(modeId, branchIndex, conditionIndex))
                {
                    _removeConditionPresenter.Complete(RemoveConditionResult.InvalidCondition, menu);
                    return;
                }

                menu.RemoveCondition(modeId, branchIndex, conditionIndex);

                _menuRepository.Save(menuId, menu);
                _removeConditionPresenter.Complete(RemoveConditionResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _removeConditionPresenter.Complete(RemoveConditionResult.Error, null, ex.ToString());
            }
        }
    }
}
