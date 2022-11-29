using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IModifyGroupPropertiesUseCase
    {
        void Handle(
            string menuId,
            string groupId,
            string displayName = null);
    }

    public interface IModifyGroupPropertiesPresenter
    {
        IObservable<(ModifyGroupPropertiesResult modifyGroupPropertiesResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(ModifyGroupPropertiesResult modifyGroupPropertiesResult, in IMenu menu, string errorMessage = "");
    }

    public enum ModifyGroupPropertiesResult
    {
        Succeeded,
        MenuDoesNotExist,
        GroupIsNotContained,
        ArgumentNull,
        Error,
    }

    public class ModifyGroupPropertiesPresenter : IModifyGroupPropertiesPresenter
    {
        public IObservable<(ModifyGroupPropertiesResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(ModifyGroupPropertiesResult, IMenu, string)> _subject = new Subject<(ModifyGroupPropertiesResult, IMenu, string)>();

        public void Complete(ModifyGroupPropertiesResult modifyGroupPropertiesResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((modifyGroupPropertiesResult, menu, errorMessage));
        }
    }

    public class ModifyGroupPropertiesUseCase : IModifyGroupPropertiesUseCase
    {
        IMenuRepository _menuRepository;
        IModifyGroupPropertiesPresenter _modifyGroupPropertiesPresenter;

        public ModifyGroupPropertiesUseCase(IMenuRepository menuRepository, IModifyGroupPropertiesPresenter modifyGroupPropertiesPresenter)
        {
            _menuRepository = menuRepository;
            _modifyGroupPropertiesPresenter = modifyGroupPropertiesPresenter;
        }

        public void Handle(
            string menuId,
            string groupId,
            string displayName = null)
        {
            try
            {
                if (menuId is null || groupId is null)
                {
                    _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.ContainsGroup(groupId))
                {
                    _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.GroupIsNotContained, menu);
                    return;
                }

                menu.ModifyGroupProperties(groupId, displayName);

                _menuRepository.Save(menuId, menu, "ModifyGroupProperties");
                _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.Error, null, ex.ToString());
            }
        }
    }
}
