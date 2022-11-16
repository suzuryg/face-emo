using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public interface IAddConditionUseCase
    {
        void Handle(string menuId, string modeId, int branchIndex, Condition condition);
    }

    public interface IAddConditionPresenter
    {
        event Action<AddConditionResult, IMenu, string> OnCompleted;

        void Complete(AddConditionResult addConditionResult, in IMenu menu, string errorMessage = "");
    }

    public enum AddConditionResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class AddConditionPresenter : IAddConditionPresenter
    {
        public event Action<AddConditionResult, IMenu, string> OnCompleted;

        public void Complete(AddConditionResult addConditionResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(addConditionResult, menu, errorMessage);
        }
    }

    public class AddConditionUseCase : IAddConditionUseCase
    {
        IMenuRepository _menuRepository;
        IAddConditionPresenter _addConditionPresenter;

        public AddConditionUseCase(IMenuRepository menuRepository, IAddConditionPresenter addConditionPresenter)
        {
            _menuRepository = menuRepository;
            _addConditionPresenter = addConditionPresenter;
        }

        public void Handle(string menuId, string modeId, int branchIndex, Condition condition)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _addConditionPresenter.Complete(AddConditionResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _addConditionPresenter.Complete(AddConditionResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanAddConditionTo(modeId, branchIndex))
                {
                    _addConditionPresenter.Complete(AddConditionResult.InvalidBranch, menu);
                    return;
                }

                menu.AddCondition(modeId, branchIndex, condition);

                _menuRepository.Save(menuId, menu, "AddCondition");
                _addConditionPresenter.Complete(AddConditionResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _addConditionPresenter.Complete(AddConditionResult.Error, null, ex.ToString());
            }
        }
    }
}
