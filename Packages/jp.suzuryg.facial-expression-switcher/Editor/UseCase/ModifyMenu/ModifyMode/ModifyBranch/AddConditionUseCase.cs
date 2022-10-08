using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public interface IAddConditionUseCase
    {
        void SetPresenter(IAddConditionPresenter addConditionPresenter);
        void Handle(string modeId, int branchIndex, Condition condition);
    }

    public interface IAddConditionPresenter
    {
        void Complete(AddConditionResult addConditionResult, in Menu menu, string errorMessage = "");
    }

    public enum AddConditionResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class AddConditionUseCase : IAddConditionUseCase
    {
        IAddConditionPresenter _addConditionPresenter;
        MenuEditingSession _menuEditingSession;

        public AddConditionUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IAddConditionPresenter addConditionPresenter)
        {
            _addConditionPresenter = addConditionPresenter;
        }

        public void Handle(string modeId, int branchIndex, Condition condition)
        {
            try
            {
                if (modeId is null)
                {
                    _addConditionPresenter?.Complete(AddConditionResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _addConditionPresenter?.Complete(AddConditionResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanAddConditionTo(modeId, branchIndex))
                {
                    _addConditionPresenter?.Complete(AddConditionResult.InvalidBranch, menu);
                    return;
                }

                menu.AddCondition(modeId, branchIndex, condition);
                _menuEditingSession.SetAsModified();
                _addConditionPresenter?.Complete(AddConditionResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _addConditionPresenter?.Complete(AddConditionResult.Error, null, ex.ToString());
            }
        }
    }
}
