using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public interface IModifyConditionUseCase
    {
        void Handle(string menuId, string modeId, int branchIndex, int conditionIndex, Condition condition);
    }

    public interface IModifyConditionPresenter
    {
        IObservable<(ModifyConditionResult modifyConditionResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(ModifyConditionResult modifyConditionResult, in IMenu menu, string errorMessage = "");
    }

    public enum ModifyConditionResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidCondition,
        ArgumentNull,
        Error,
    }

    public class ModifyConditionPresenter : IModifyConditionPresenter
    {
        public IObservable<(ModifyConditionResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(ModifyConditionResult, IMenu, string)> _subject = new Subject<(ModifyConditionResult, IMenu, string)>();

        public void Complete(ModifyConditionResult modifyConditionResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((modifyConditionResult, menu, errorMessage));
        }
    }

    public class ModifyConditionUseCase : IModifyConditionUseCase
    {
        IMenuRepository _menuRepository;
        IModifyConditionPresenter _modifyConditionPresenter;

        public ModifyConditionUseCase(IMenuRepository menuRepository, IModifyConditionPresenter modifyConditionPresenter)
        {
            _menuRepository = menuRepository;
            _modifyConditionPresenter = modifyConditionPresenter;
        }

        public void Handle(string menuId, string modeId, int branchIndex, int conditionIndex, Condition condition)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _modifyConditionPresenter.Complete(ModifyConditionResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _modifyConditionPresenter.Complete(ModifyConditionResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanModifyCondition(modeId, branchIndex, conditionIndex))
                {
                    _modifyConditionPresenter.Complete(ModifyConditionResult.InvalidCondition, menu);
                    return;
                }

                menu.ModifyCondition(modeId, branchIndex, conditionIndex, condition);

                _menuRepository.Save(menuId, menu, "ModifyCondition");
                _modifyConditionPresenter.Complete(ModifyConditionResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _modifyConditionPresenter.Complete(ModifyConditionResult.Error, null, ex.ToString());
            }
        }
    }
}
