using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IAddMenuItemUseCase
    {
        void Handle(string menuId, string menuItemListId, AddMenuItemType type);
    }

    public interface IAddMenuItemPresenter
    {
        event Action<AddMenuItemResult, IMenu, string> OnCompleted;

        void Complete(AddMenuItemResult addMenuItemResult, in IMenu menu, string errorMessage = "");
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
        public event Action<AddMenuItemResult, IMenu, string> OnCompleted;

        public void Complete(AddMenuItemResult addMenuItemResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(addMenuItemResult, menu, errorMessage);
        }
    }

    public class AddMenuItemUseCase : IAddMenuItemUseCase
    {
        IMenuRepository _menuRepository;
        IAddMenuItemPresenter _addMenuItemPresenter;

        public AddMenuItemUseCase(IMenuRepository menuRepository, IAddMenuItemPresenter addMenuItemPresenter)
        {
            _menuRepository = menuRepository;
            _addMenuItemPresenter = addMenuItemPresenter;
        }

        public void Handle(string menuId, string menuItemListId, AddMenuItemType type)
        {
            try
            {
                if (menuId is null || menuItemListId is null)
                {
                    _addMenuItemPresenter.Complete(AddMenuItemResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _addMenuItemPresenter.Complete(AddMenuItemResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (type == AddMenuItemType.Mode)
                {
                    if (menu.CanAddModeTo(menuItemListId))
                    {
                        menu.AddMode(menuItemListId);
                    }
                    else
                    {
                        _addMenuItemPresenter.Complete(AddMenuItemResult.InvalidMenuItemListId, menu);
                        return;
                    }
                }
                else if (type == AddMenuItemType.Group)
                {
                    if (menu.CanAddGroupTo(menuItemListId))
                    {
                        menu.AddGroup(menuItemListId);
                    }
                    else
                    {
                        _addMenuItemPresenter.Complete(AddMenuItemResult.InvalidMenuItemListId, menu);
                        return;
                    }
                }
                else
                {
                    throw new FacialExpressionSwitcherException("Unknown AddMenuItemType");
                }

                _menuRepository.Save(menuId, menu, "AddMenuItem");
                _addMenuItemPresenter.Complete(AddMenuItemResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _addMenuItemPresenter.Complete(AddMenuItemResult.Error, null, ex.ToString());
            }
        }
    }
}
