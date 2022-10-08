using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public interface IRemoveBranchUseCase
    {
        void SetPresenter(IRemoveBranchPresenter removeBranchPresenter);
        void Handle(string modeId, int branchIndex);
    }

    public interface IRemoveBranchPresenter
    {
        void Complete(RemoveBranchResult removeBranchResult, in Menu menu, string errorMessage = "");
    }

    public enum RemoveBranchResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class RemoveBranchUseCase : IRemoveBranchUseCase
    {
        IRemoveBranchPresenter _removeBranchPresenter;
        MenuEditingSession _menuEditingSession;

        public RemoveBranchUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IRemoveBranchPresenter removeBranchPresenter)
        {
            _removeBranchPresenter = removeBranchPresenter;
        }

        public void Handle(string modeId, int branchIndex)
        {
            try
            {
                if (modeId is null)
                {
                    _removeBranchPresenter?.Complete(RemoveBranchResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _removeBranchPresenter?.Complete(RemoveBranchResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanRemoveBranch(modeId, branchIndex))
                {
                    _removeBranchPresenter?.Complete(RemoveBranchResult.InvalidBranch, menu);
                    return;
                }

                menu.RemoveBranch(modeId, branchIndex);
                _menuEditingSession.SetAsModified();
                _removeBranchPresenter?.Complete(RemoveBranchResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _removeBranchPresenter?.Complete(RemoveBranchResult.Error, null, ex.ToString());
            }
        }
    }
}
