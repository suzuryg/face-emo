using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public interface IChangeConditionOrderUseCase
    {
        void SetPresenter(IChangeConditionOrderPresenter changeConditionOrderPresenter);
        void Handle(string modeId, int branchIndex, int from, int to);
    }

    public interface IChangeConditionOrderPresenter
    {
        void Complete(ChangeConditionOrderResult changeConditionOrderResult, in Menu menu, string errorMessage = "");
    }

    public enum ChangeConditionOrderResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidCondition,
        ArgumentNull,
        Error,
    }

    public class ChangeConditionOrderUseCase : IChangeConditionOrderUseCase
    {
        IChangeConditionOrderPresenter _changeConditionOrderPresenter;
        MenuEditingSession _menuEditingSession;

        public ChangeConditionOrderUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IChangeConditionOrderPresenter changeConditionOrderPresenter)
        {
            _changeConditionOrderPresenter = changeConditionOrderPresenter;
        }

        public void Handle(string modeId, int branchIndex, int from, int to)
        {
            try
            {
                if (modeId is null)
                {
                    _changeConditionOrderPresenter?.Complete(ChangeConditionOrderResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _changeConditionOrderPresenter?.Complete(ChangeConditionOrderResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanChangeConditionOrder(modeId, branchIndex, from))
                {
                    _changeConditionOrderPresenter?.Complete(ChangeConditionOrderResult.InvalidCondition, menu);
                    return;
                }

                menu.ChangeConditionOrder(modeId, branchIndex, from, to);
                _menuEditingSession.SetAsModified();
                _changeConditionOrderPresenter?.Complete(ChangeConditionOrderResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _changeConditionOrderPresenter?.Complete(ChangeConditionOrderResult.Error, null, ex.ToString());
            }
        }
    }
}
