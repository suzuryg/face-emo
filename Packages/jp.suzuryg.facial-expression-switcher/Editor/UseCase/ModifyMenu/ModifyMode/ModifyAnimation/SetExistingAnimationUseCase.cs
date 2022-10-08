using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation
{
    public interface ISetExistingAnimationUseCase
    {
        void SetPresenter(ISetExistingAnimationPresenter setExistingAnimationPresenter);
        void Handle(IAnimation animation, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null);
    }

    public interface ISetExistingAnimationPresenter
    {
        void Complete(SetExistingAnimationResult setExistingAnimationResult, in Menu menu, string errorMessage = "");
    }

    public enum SetExistingAnimationResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class SetExistingAnimationUseCase : ISetExistingAnimationUseCase
    {
        ISetExistingAnimationPresenter _setExistingAnimationPresenter;
        MenuEditingSession _menuEditingSession;

        public SetExistingAnimationUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(ISetExistingAnimationPresenter setExistingAnimationPresenter)
        {
            _setExistingAnimationPresenter = setExistingAnimationPresenter;
        }

        public void Handle(IAnimation animation, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null)
        {
            try
            {
                if (animation is null || modeId is null)
                {
                    _setExistingAnimationPresenter?.Complete(SetExistingAnimationResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _setExistingAnimationPresenter?.Complete(SetExistingAnimationResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanSetAnimationTo(modeId, branchIndex, branchAnimationType))
                {
                    _setExistingAnimationPresenter?.Complete(SetExistingAnimationResult.InvalidDestination, menu);
                    return;
                }

                menu.SetAnimation(animation, modeId, branchIndex, branchAnimationType);
                _menuEditingSession.SetAsModified();
                _setExistingAnimationPresenter?.Complete(SetExistingAnimationResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _setExistingAnimationPresenter?.Complete(SetExistingAnimationResult.Error, null, ex.ToString());
            }
        }
    }
}
