using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public interface IRemoveConditionUseCase
    {
        void SetPresenter(IRemoveConditionPresenter removeConditionPresenter);
        void Handle(string modeId, int branchIndex, int conditionIndex);
    }

    public interface IRemoveConditionPresenter
    {
        void Complete(RemoveConditionResult removeConditionResult, in Menu menu, string errorMessage = "");
    }

    public enum RemoveConditionResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidCondition,
        ArgumentNull,
        Error,
    }

    public class RemoveConditionUseCase : IRemoveConditionUseCase
    {
        IRemoveConditionPresenter _removeConditionPresenter;
        MenuEditingSession _menuEditingSession;

        public RemoveConditionUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IRemoveConditionPresenter removeConditionPresenter)
        {
            _removeConditionPresenter = removeConditionPresenter;
        }

        public void Handle(string modeId, int branchIndex, int conditionIndex)
        {
            try
            {
                if (modeId is null)
                {
                    _removeConditionPresenter?.Complete(RemoveConditionResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _removeConditionPresenter?.Complete(RemoveConditionResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanRemoveCondition(modeId, branchIndex, conditionIndex))
                {
                    _removeConditionPresenter?.Complete(RemoveConditionResult.InvalidCondition, menu);
                    return;
                }

                menu.RemoveCondition(modeId, branchIndex, conditionIndex);
                _menuEditingSession.SetAsModified();
                _removeConditionPresenter?.Complete(RemoveConditionResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _removeConditionPresenter?.Complete(RemoveConditionResult.Error, null, ex.ToString());
            }
        }
    }
}
