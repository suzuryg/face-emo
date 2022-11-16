using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public interface IAddBranchUseCase
    {
        void Handle(string menuId, string modeId, IEnumerable<Condition> conditions = null);
    }

    public interface IAddBranchPresenter
    {
        event Action<AddBranchResult, IMenu, string> OnCompleted;

        void Complete(AddBranchResult addBranchResult, in IMenu menu, string errorMessage = "");
    }

    public enum AddBranchResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class AddBranchPresenter : IAddBranchPresenter
    {
        public event Action<AddBranchResult, IMenu, string> OnCompleted;

        public void Complete(AddBranchResult addBranchResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(addBranchResult, menu, errorMessage);
        }
    }

    public class AddBranchUseCase : IAddBranchUseCase
    {
        IMenuRepository _menuRepository;
        IAddBranchPresenter _addBranchPresenter;

        public AddBranchUseCase(IMenuRepository menuRepository, IAddBranchPresenter addBranchPresenter)
        {
            _menuRepository = menuRepository;
            _addBranchPresenter = addBranchPresenter;
        }

        public void Handle(string menuId, string modeId, IEnumerable<Condition> conditions = null)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _addBranchPresenter.Complete(AddBranchResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _addBranchPresenter.Complete(AddBranchResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanAddBranchTo(modeId))
                {
                    _addBranchPresenter.Complete(AddBranchResult.InvalidDestination, menu);
                    return;
                }

                menu.AddBranch(modeId, conditions);

                _menuRepository.Save(menuId, menu, "AddBranch");
                _addBranchPresenter.Complete(AddBranchResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _addBranchPresenter.Complete(AddBranchResult.Error, null, ex.ToString());
            }
        }
    }
}
