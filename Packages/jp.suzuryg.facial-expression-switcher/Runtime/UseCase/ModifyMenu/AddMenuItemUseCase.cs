using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IAddMenuItemUseCase
    {
        void Handle(string menuId, string menuItemListId, AddMenuItemType type);
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
            }
            catch (Exception ex)
            {
                _addMenuItemPresenter.Complete(AddMenuItemResult.Error, null, null, ex.ToString());
            }
        }
    }
}
