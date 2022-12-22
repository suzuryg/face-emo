using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IRemoveMenuItemUseCase
    {
        void Handle(string menuId, string menuItemId);
    }

    public interface IRemoveMenuItemPresenter
    {
        IObservable<(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(RemoveMenuItemResult removeMenuItemResult, string removedItemId, in IMenu menu, string errorMessage = "");
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
        public IObservable<(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IMenu menu, string errorMessage)> Observable => _subject.AsObservable();

        private Subject<(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IMenu menu, string errorMessage)> _subject = new Subject<(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IMenu menu, string errorMessage)>();

        public void Complete(RemoveMenuItemResult removeMenuItemResult, string removedItemId, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((removeMenuItemResult, removedItemId, menu, errorMessage));
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
                    _removeMenuItemPresenter.Complete(RemoveMenuItemResult.ArgumentNull, menuItemId, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _removeMenuItemPresenter.Complete(RemoveMenuItemResult.MenuDoesNotExist, menuItemId, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (menu.ContainsMode(menuItemId))
                {
                    var mode = menu.GetMode(menuItemId);
                    var parent = mode.Parent;
                }
                else if (menu.ContainsGroup(menuItemId))
                {
                }

                if (!menu.CanRemoveMenuItem(menuItemId))
                {
                    _removeMenuItemPresenter.Complete(RemoveMenuItemResult.InvalidId, menuItemId, menu);
                    return;
                }

                menu.RemoveMenuItem(menuItemId);

                _menuRepository.Save(menuId, menu, "RemoveMenuItem");
                _removeMenuItemPresenter.Complete(RemoveMenuItemResult.Succeeded, menuItemId, menu);
            }
            catch (Exception ex)
            {
                _removeMenuItemPresenter.Complete(RemoveMenuItemResult.Error, menuItemId, null, ex.ToString());
            }
        }
    }
}
