using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IRemoveMenuItemUseCase
    {
        void Handle(string menuId, string menuItemId);
    }

    public interface IRemoveMenuItemPresenter
    {
        event Action<RemoveMenuItemResult, IMenu, string> OnCompleted;

        void Complete(RemoveMenuItemResult removeMenuItemResult, in IMenu menu, string errorMessage = "");
    }

    public enum RemoveMenuItemResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidId,
        ArgumentNull,
        Error,
    }

    public class RemoveMenuItemPresenter : IRemoveMenuItemPresenter
    {
        public event Action<RemoveMenuItemResult, IMenu, string> OnCompleted;

        public void Complete(RemoveMenuItemResult removeMenuItemResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(removeMenuItemResult, menu, errorMessage);
        }
    }

    public class RemoveMenuItemUseCase : IRemoveMenuItemUseCase
    {
        IMenuRepository _menuRepository;
        IRemoveMenuItemPresenter _removeMenuItemPresenter;

        public RemoveMenuItemUseCase(IMenuRepository menuRepository, IRemoveMenuItemPresenter removeMenuItemPresenter)
        {
            _menuRepository = menuRepository;
            _removeMenuItemPresenter = removeMenuItemPresenter;
        }

        public void Handle(string menuId, string menuItemId)
        {
            try
            {
                if (menuId is null || menuItemId is null)
                {
                    _removeMenuItemPresenter.Complete(RemoveMenuItemResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _removeMenuItemPresenter.Complete(RemoveMenuItemResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanRemoveMenuItem(menuItemId))
                {
                    _removeMenuItemPresenter.Complete(RemoveMenuItemResult.InvalidId, menu);
                    return;
                }

                menu.RemoveMenuItem(menuItemId);

                _menuRepository.Save(menuId, menu);
                _removeMenuItemPresenter.Complete(RemoveMenuItemResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _removeMenuItemPresenter.Complete(RemoveMenuItemResult.Error, null, ex.ToString());
            }
        }
    }
}
