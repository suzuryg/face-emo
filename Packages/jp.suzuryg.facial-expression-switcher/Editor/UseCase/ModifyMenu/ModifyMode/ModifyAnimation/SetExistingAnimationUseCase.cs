using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation
{
    public interface ISetExistingAnimationUseCase
    {
        void Handle(string menuId, Animation animation, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null);
    }

    public interface ISetExistingAnimationPresenter
    {
        event Action<SetExistingAnimationResult, IMenu, string> OnCompleted;

        void Complete(SetExistingAnimationResult setExistingAnimationResult, in IMenu menu, string errorMessage = "");
    }

    public enum SetExistingAnimationResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class SetExistingAnimationPresenter : ISetExistingAnimationPresenter
    {
        public event Action<SetExistingAnimationResult, IMenu, string> OnCompleted;

        public void Complete(SetExistingAnimationResult setExistingAnimationResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(setExistingAnimationResult, menu, errorMessage);
        }
    }

    public class SetExistingAnimationUseCase : ISetExistingAnimationUseCase
    {
        IMenuRepository _menuRepository;
        ISetExistingAnimationPresenter _setExistingAnimationPresenter;

        public SetExistingAnimationUseCase(IMenuRepository menuRepository, ISetExistingAnimationPresenter setExistingAnimationPresenter)
        {
            _menuRepository = menuRepository;
            _setExistingAnimationPresenter = setExistingAnimationPresenter;
        }

        public void Handle(string menuId, Animation animation, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null)
        {
            try
            {
                if (menuId is null || animation is null || modeId is null)
                {
                    _setExistingAnimationPresenter.Complete(SetExistingAnimationResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _setExistingAnimationPresenter.Complete(SetExistingAnimationResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanSetAnimationTo(modeId, branchIndex, branchAnimationType))
                {
                    _setExistingAnimationPresenter.Complete(SetExistingAnimationResult.InvalidDestination, menu);
                    return;
                }

                menu.SetAnimation(animation, modeId, branchIndex, branchAnimationType);

                _menuRepository.Save(menuId, menu, "SetExistingAnimation");
                _setExistingAnimationPresenter.Complete(SetExistingAnimationResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _setExistingAnimationPresenter.Complete(SetExistingAnimationResult.Error, null, ex.ToString());
            }
        }
    }
}
