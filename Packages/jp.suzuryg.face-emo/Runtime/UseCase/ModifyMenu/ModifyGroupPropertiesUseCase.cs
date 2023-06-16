using Suzuryg.FaceEmo.Domain;
using System;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu
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
        IObservable<(ModifyGroupPropertiesResult modifyGroupPropertiesResult, string modifiedGroupId, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(ModifyGroupPropertiesResult modifyGroupPropertiesResult, string modifiedGroupId, in IMenu menu, string errorMessage = "");
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
        public IObservable<(ModifyGroupPropertiesResult modifyGroupPropertiesResult, string modifiedGroupId, IMenu menu, string errorMessage)> Observable => _subject.AsObservable();

        private Subject<(ModifyGroupPropertiesResult modifyGroupPropertiesResult, string modifiedGroupId, IMenu menu, string errorMessage)> _subject = new Subject<(ModifyGroupPropertiesResult modifyGroupPropertiesResult, string modifiedGroupId, IMenu menu, string errorMessage)>();

        public void Complete(ModifyGroupPropertiesResult modifyGroupPropertiesResult, string modifiedGroupId, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((modifyGroupPropertiesResult, modifiedGroupId, menu, errorMessage));
        }
    }

    public class ModifyGroupPropertiesUseCase : IModifyGroupPropertiesUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        IModifyGroupPropertiesPresenter _modifyGroupPropertiesPresenter;

        public ModifyGroupPropertiesUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, IModifyGroupPropertiesPresenter modifyGroupPropertiesPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
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
                    _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.ArgumentNull, groupId, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.MenuDoesNotExist, groupId, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.ContainsGroup(groupId))
                {
                    _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.GroupIsNotContained, groupId, menu);
                    return;
                }

                menu.ModifyGroupProperties(groupId, displayName);

                _menuRepository.Save(menuId, menu, "ModifyGroupProperties");
                _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.Succeeded, groupId, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _modifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult.Error, groupId, null, ex.ToString());
            }
        }
    }
}
