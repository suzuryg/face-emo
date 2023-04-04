using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation
{
    public interface ISetExistingAnimationUseCase
    {
        void Handle(string menuId, Animation animation, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null);
    }

    public interface ISetExistingAnimationPresenter
    {
        IObservable<(SetExistingAnimationResult setExistingAnimationResult, IMenu menu, string errorMessage)> Observable { get; }

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
        public IObservable<(SetExistingAnimationResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(SetExistingAnimationResult, IMenu, string)> _subject = new Subject<(SetExistingAnimationResult, IMenu, string)>();

        public void Complete(SetExistingAnimationResult setExistingAnimationResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((setExistingAnimationResult, menu, errorMessage));
        }
    }

    public class SetExistingAnimationUseCase : ISetExistingAnimationUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        ISetExistingAnimationPresenter _setExistingAnimationPresenter;

        public SetExistingAnimationUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, ISetExistingAnimationPresenter setExistingAnimationPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
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
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _setExistingAnimationPresenter.Complete(SetExistingAnimationResult.Error, null, ex.ToString());
            }
        }
    }
}
