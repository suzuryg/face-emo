using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public interface IChangeBranchOrderUseCase
    {
        void SetPresenter(IChangeBranchOrderPresenter changeBranchOrderPresenter);
        void Handle(string modeId, int from, int to);
    }

    public interface IChangeBranchOrderPresenter
    {
        void Complete(ChangeBranchOrderResult changeBranchOrderResult, in Menu menu, string errorMessage = "");
    }

    public enum ChangeBranchOrderResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class ChangeBranchOrderUseCase : IChangeBranchOrderUseCase
    {
        IChangeBranchOrderPresenter _changeBranchOrderPresenter;
        MenuEditingSession _menuEditingSession;

        public ChangeBranchOrderUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IChangeBranchOrderPresenter changeBranchOrderPresenter)
        {
            _changeBranchOrderPresenter = changeBranchOrderPresenter;
        }

        public void Handle(string modeId, int from, int to)
        {
            try
            {
                if (modeId is null)
                {
                    _changeBranchOrderPresenter?.Complete(ChangeBranchOrderResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _changeBranchOrderPresenter?.Complete(ChangeBranchOrderResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanChangeBranchOrder(modeId, from))
                {
                    _changeBranchOrderPresenter?.Complete(ChangeBranchOrderResult.InvalidBranch, menu);
                    return;
                }

                menu.ChangeBranchOrder(modeId, from, to);
                _menuEditingSession.SetAsModified();
                _changeBranchOrderPresenter?.Complete(ChangeBranchOrderResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _changeBranchOrderPresenter?.Complete(ChangeBranchOrderResult.Error, null, ex.ToString());
            }
        }
    }
}
