using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IAddMenuItemUseCase
    {
        void Handle(string menuId, string menuItemListId, AddMenuItemType type);
        void Handle(string menuId, string menuItemListId, AddMenuItemType type, string displayName);
    }

    public interface IAddMenuItemPresenter
    {
        IObservable<(AddMenuItemResult addMenuItemResult, string addedItemId, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(AddMenuItemResult addMenuItemResult, string addedItemId, in IMenu menu, string errorMessage = "");
    }

    public enum AddMenuItemResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidMenuItemListId,
        ArgumentNull,
        Error,
    }

    public enum AddMenuItemType
    {
        Group,
        Mode,
    }

    public class AddMenuItemPresenter : IAddMenuItemPresenter
    {
        public IObservable<(AddMenuItemResult addMenuItemResult, string addedItemId, IMenu menu, string errorMessage)> Observable => _subject.AsObservable();

        private Subject<(AddMenuItemResult addMenuItemResult, string addedItemId, IMenu menu, string errorMessage)> _subject = new Subject<(AddMenuItemResult addMenuItemResult, string addedItemId, IMenu menu, string errorMessage)>();

        public void Complete(AddMenuItemResult addMenuItemResult, string addedItemId, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((addMenuItemResult, addedItemId, menu, errorMessage));
        }
    }

    public class AddMenuItemUseCase : IAddMenuItemUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        IAddMenuItemPresenter _addMenuItemPresenter;

        public AddMenuItemUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, IAddMenuItemPresenter addMenuItemPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _addMenuItemPresenter = addMenuItemPresenter;
        }

        public void Handle(string menuId, string menuItemListId, AddMenuItemType type) => Handle(menuId, menuItemListId, type, null);

        public void Handle(string menuId, string menuItemListId, AddMenuItemType type, string displayName)
        {
            try
            {
                if (menuId is null || menuItemListId is null)
                {
                    _addMenuItemPresenter.Complete(AddMenuItemResult.ArgumentNull, null, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _addMenuItemPresenter.Complete(AddMenuItemResult.MenuDoesNotExist, null, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);
                string addedItemId = null;

                if (type == AddMenuItemType.Mode)
                {
                    if (menu.CanAddModeTo(menuItemListId))
                    {
                        addedItemId = menu.AddMode(menuItemListId);

                        if (!string.IsNullOrEmpty(displayName))
                        {
                            menu.ModifyModeProperties(id: addedItemId, displayName: displayName);
                        }
                    }
                    else
                    {
                        _addMenuItemPresenter.Complete(AddMenuItemResult.InvalidMenuItemListId, addedItemId, menu);
                        return;
                    }
                }
                else if (type == AddMenuItemType.Group)
                {
                    if (menu.CanAddGroupTo(menuItemListId))
                    {
                        addedItemId = menu.AddGroup(menuItemListId);

                        if (!string.IsNullOrEmpty(displayName))
                        {
                            menu.ModifyGroupProperties(id: addedItemId, displayName: displayName);
                        }
                    }
                    else
                    {
                        _addMenuItemPresenter.Complete(AddMenuItemResult.InvalidMenuItemListId, addedItemId, menu);
                        return;
                    }
                }
                else
                {
                    throw new FacialExpressionSwitcherException("Unknown AddMenuItemType");
                }

                _menuRepository.Save(menuId, menu, "AddMenuItem");
                _addMenuItemPresenter.Complete(AddMenuItemResult.Succeeded, addedItemId, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _addMenuItemPresenter.Complete(AddMenuItemResult.Error, null, null, ex.ToString());
            }
        }
    }
}
