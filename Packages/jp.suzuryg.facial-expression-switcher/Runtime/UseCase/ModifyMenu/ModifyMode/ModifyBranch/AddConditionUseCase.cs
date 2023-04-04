using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public interface IAddConditionUseCase
    {
        void Handle(string menuId, string modeId, int branchIndex, Condition condition);
    }

    public interface IAddConditionPresenter
    {
        IObservable<(AddConditionResult addConditionResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(AddConditionResult addConditionResult, in IMenu menu, string errorMessage = "");
    }

    public enum AddConditionResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class AddConditionPresenter : IAddConditionPresenter
    {
        public IObservable<(AddConditionResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(AddConditionResult, IMenu, string)> _subject = new Subject<(AddConditionResult, IMenu, string)>();

        public void Complete(AddConditionResult addConditionResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((addConditionResult, menu, errorMessage));
        }
    }

    public class AddConditionUseCase : IAddConditionUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        IAddConditionPresenter _addConditionPresenter;

        public AddConditionUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, IAddConditionPresenter addConditionPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _addConditionPresenter = addConditionPresenter;
        }

        public void Handle(string menuId, string modeId, int branchIndex, Condition condition)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _addConditionPresenter.Complete(AddConditionResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _addConditionPresenter.Complete(AddConditionResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanAddConditionTo(modeId, branchIndex))
                {
                    _addConditionPresenter.Complete(AddConditionResult.InvalidBranch, menu);
                    return;
                }

                menu.AddCondition(modeId, branchIndex, condition);

                _menuRepository.Save(menuId, menu, "AddCondition");
                _addConditionPresenter.Complete(AddConditionResult.Succeeded, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _addConditionPresenter.Complete(AddConditionResult.Error, null, ex.ToString());
            }
        }
    }
}
