using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation
{
    public interface ISetNewAnimationUseCase
    {
        void Handle(string menuId, string animationPath, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null);
    }

    public interface ISetNewAnimationPresenter
    {
        event Action<SetNewAnimationResult, IMenu, string> OnCompleted;

        void Complete(SetNewAnimationResult setNewAnimationResult, in IMenu menu, string errorMessage = "");
    }

    public enum SetNewAnimationResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class SetNewAnimationPresenter : ISetNewAnimationPresenter
    {
        public event Action<SetNewAnimationResult, IMenu, string> OnCompleted;

        public void Complete(SetNewAnimationResult setNewAnimationResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(setNewAnimationResult, menu, errorMessage);
        }
    }

    public class SetNewAnimationUseCase : ISetNewAnimationUseCase
    {
        IMenuRepository _menuRepository;
        IAnimationEditor _animationEditor;
        ISetNewAnimationPresenter _setNewAnimationPresenter;

        public SetNewAnimationUseCase(IMenuRepository menuRepository, IAnimationEditor animationEditor, ISetNewAnimationPresenter setNewAnimationPresenter)
        {
            _menuRepository = menuRepository;
            _animationEditor = animationEditor;
            _setNewAnimationPresenter = setNewAnimationPresenter;
        }

        public void Handle(string menuId, string animationPath, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null)
        {
            try
            {
                if (menuId is null || animationPath is null || modeId is null)
                {
                    _setNewAnimationPresenter.Complete(SetNewAnimationResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _setNewAnimationPresenter.Complete(SetNewAnimationResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanSetAnimationTo(modeId, branchIndex, branchAnimationType))
                {
                    _setNewAnimationPresenter.Complete(SetNewAnimationResult.InvalidDestination, menu);
                    return;
                }

                var animation = _animationEditor.Create(animationPath);
                menu.SetAnimation(animation, modeId, branchIndex, branchAnimationType);

                _menuRepository.Save(menuId, menu, "SetNewAnimation");
                _setNewAnimationPresenter.Complete(SetNewAnimationResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _setNewAnimationPresenter.Complete(SetNewAnimationResult.Error, null, ex.ToString());
            }
        }
    }
}
