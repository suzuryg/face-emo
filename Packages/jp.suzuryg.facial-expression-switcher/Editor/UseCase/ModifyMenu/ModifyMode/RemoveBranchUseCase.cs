using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public interface IRemoveBranchUseCase
    {
        void Handle(string menuId, string modeId, int branchIndex);
    }

    public interface IRemoveBranchPresenter
    {
        event Action<RemoveBranchResult, IMenu, string> OnCompleted;

        void Complete(RemoveBranchResult removeBranchResult, in IMenu menu, string errorMessage = "");
    }

    public enum RemoveBranchResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class RemoveBranchPresenter : IRemoveBranchPresenter
    {
        public event Action<RemoveBranchResult, IMenu, string> OnCompleted;

        public void Complete(RemoveBranchResult removeBranchResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(removeBranchResult, menu, errorMessage);
        }
    }

    public class RemoveBranchUseCase : IRemoveBranchUseCase
    {
        IMenuRepository _menuRepository;
        IRemoveBranchPresenter _removeBranchPresenter;

        public RemoveBranchUseCase(IMenuRepository menuRepository, IRemoveBranchPresenter removeBranchPresenter)
        {
            _menuRepository = menuRepository;
            _removeBranchPresenter = removeBranchPresenter;
        }

        public void Handle(string menuId, string modeId, int branchIndex)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _removeBranchPresenter.Complete(RemoveBranchResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _removeBranchPresenter.Complete(RemoveBranchResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanRemoveBranch(modeId, branchIndex))
                {
                    _removeBranchPresenter.Complete(RemoveBranchResult.InvalidBranch, menu);
                    return;
                }

                menu.RemoveBranch(modeId, branchIndex);

                _menuRepository.Save(menuId, menu, "RemoveBranch");
                _removeBranchPresenter.Complete(RemoveBranchResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _removeBranchPresenter.Complete(RemoveBranchResult.Error, null, ex.ToString());
            }
        }
    }
}
