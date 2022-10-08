using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public interface IModifyConditionUseCase
    {
        void SetPresenter(IModifyConditionPresenter modifyConditionPresenter);
        void Handle(string modeId, int branchIndex, int conditionIndex, Condition condition);
    }

    public interface IModifyConditionPresenter
    {
        void Complete(ModifyConditionResult modifyConditionResult, in Menu menu, string errorMessage = "");
    }

    public enum ModifyConditionResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidCondition,
        ArgumentNull,
        Error,
    }

    public class ModifyConditionUseCase : IModifyConditionUseCase
    {
        IModifyConditionPresenter _modifyConditionPresenter;
        MenuEditingSession _menuEditingSession;

        public ModifyConditionUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IModifyConditionPresenter modifyConditionPresenter)
        {
            _modifyConditionPresenter = modifyConditionPresenter;
        }

        public void Handle(string modeId, int branchIndex, int conditionIndex, Condition condition)
        {
            try
            {
                if (modeId is null)
                {
                    _modifyConditionPresenter?.Complete(ModifyConditionResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _modifyConditionPresenter?.Complete(ModifyConditionResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanModifyCondition(modeId, branchIndex, conditionIndex))
                {
                    _modifyConditionPresenter?.Complete(ModifyConditionResult.InvalidCondition, menu);
                    return;
                }

                menu.ModifyCondition(modeId, branchIndex, conditionIndex, condition);
                _menuEditingSession.SetAsModified();
                _modifyConditionPresenter?.Complete(ModifyConditionResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _modifyConditionPresenter?.Complete(ModifyConditionResult.Error, null, ex.ToString());
            }
        }
    }
}
