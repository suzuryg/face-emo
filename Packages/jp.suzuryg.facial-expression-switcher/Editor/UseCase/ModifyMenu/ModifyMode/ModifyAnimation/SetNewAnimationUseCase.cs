using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation
{
    public interface ISetNewAnimationUseCase
    {
        void SetPresenter(ISetNewAnimationPresenter setNewAnimationPresenter);
        void Handle(string animationPath, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null);
    }

    public interface ISetNewAnimationPresenter
    {
        void Complete(SetNewAnimationResult setNewAnimationResult, in Menu menu, string errorMessage = "");
    }

    public enum SetNewAnimationResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class SetNewAnimationUseCase : ISetNewAnimationUseCase
    {
        ISetNewAnimationPresenter _setNewAnimationPresenter;
        MenuEditingSession _menuEditingSession;
        IAnimationEditor _animationEditor;

        public SetNewAnimationUseCase(MenuEditingSession menuEditingSession, IAnimationEditor animationEditor)
        {
            _menuEditingSession = menuEditingSession;
            _animationEditor = animationEditor;
        }

        public void SetPresenter(ISetNewAnimationPresenter setNewAnimationPresenter)
        {
            _setNewAnimationPresenter = setNewAnimationPresenter;
        }

        public void Handle(string animationPath, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null)
        {
            try
            {
                if (animationPath is null || modeId is null)
                {
                    _setNewAnimationPresenter?.Complete(SetNewAnimationResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _setNewAnimationPresenter?.Complete(SetNewAnimationResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanSetAnimationTo(modeId, branchIndex, branchAnimationType))
                {
                    _setNewAnimationPresenter?.Complete(SetNewAnimationResult.InvalidDestination, menu);
                    return;
                }

                var animation = _animationEditor.Create(animationPath);
                menu.SetAnimation(animation, modeId, branchIndex, branchAnimationType);
                _menuEditingSession.SetAsModified();
                _setNewAnimationPresenter?.Complete(SetNewAnimationResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _setNewAnimationPresenter?.Complete(SetNewAnimationResult.Error, null, ex.ToString());
            }
        }
    }
}
