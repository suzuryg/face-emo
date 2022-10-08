using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public interface IAddBranchUseCase
    {
        void SetPresenter(IAddBranchPresenter addBranchPresenter);
        void Handle(string destination, IEnumerable<Condition> conditions = null);
    }

    public interface IAddBranchPresenter
    {
        void Complete(AddBranchResult addBranchResult, in Menu menu, string errorMessage = "");
    }

    public enum AddBranchResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class AddBranchUseCase : IAddBranchUseCase
    {
        IAddBranchPresenter _addBranchPresenter;
        MenuEditingSession _menuEditingSession;

        public AddBranchUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IAddBranchPresenter addBranchPresenter)
        {
            _addBranchPresenter = addBranchPresenter;
        }

        public void Handle(string destination, IEnumerable<Condition> conditions = null)
        {
            try
            {
                if (destination is null)
                {
                    _addBranchPresenter?.Complete(AddBranchResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _addBranchPresenter?.Complete(AddBranchResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanAddBranchTo(destination))
                {
                    _addBranchPresenter?.Complete(AddBranchResult.InvalidDestination, menu);
                    return;
                }

                menu.AddBranch(destination, conditions);
                _menuEditingSession.SetAsModified();
                _addBranchPresenter?.Complete(AddBranchResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _addBranchPresenter?.Complete(AddBranchResult.Error, null, ex.ToString());
            }
        }
    }
}
