using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu
{
    public interface IRemoveMenuItemUseCase
    {
        void Handle(string menuId, string menuItemId);
    }

    public interface IRemoveMenuItemPresenter
    {
        IObservable<(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IReadOnlyList<string> orderBeforeDeletion, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IReadOnlyList<string> orderBeforeDeletion, in IMenu menu, string errorMessage = "");
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
        public IObservable<(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IReadOnlyList<string> orderBeforeDeletion, IMenu menu, string errorMessage)> Observable => _subject.AsObservable();

        private Subject<(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IReadOnlyList<string> orderBeforeDeletion, IMenu menu, string errorMessage)> _subject = new Subject<(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IReadOnlyList<string> orderBeforeDeletion, IMenu menu, string errorMessage)>();

        public void Complete(RemoveMenuItemResult removeMenuItemResult, string removedItemId, IReadOnlyList<string> orderBeforeDeletion, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((removeMenuItemResult, removedItemId, orderBeforeDeletion, menu, errorMessage));
        }
    }

    public class RemoveMenuItemUseCase : IRemoveMenuItemUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        IRemoveMenuItemPresenter _removeMenuItemPresenter;

        public RemoveMenuItemUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, IRemoveMenuItemPresenter removeMenuItemPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _removeMenuItemPresenter = removeMenuItemPresenter;
        }

        public void Handle(string menuId, string menuItemId)
        {
            var orderBeforeDeletion = new List<string>();
            try
            {
                if (menuId is null || menuItemId is null)
                {
                    _removeMenuItemPresenter.Complete(RemoveMenuItemResult.ArgumentNull, menuItemId, orderBeforeDeletion, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _removeMenuItemPresenter.Complete(RemoveMenuItemResult.MenuDoesNotExist, menuItemId, orderBeforeDeletion, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (menu.ContainsMode(menuItemId))
                {
                    var mode = menu.GetMode(menuItemId);
                    orderBeforeDeletion = new List<string>(mode.Parent.Order);
                }
                else if (menu.ContainsGroup(menuItemId))
                {
                    var group = menu.GetGroup(menuItemId);
                    orderBeforeDeletion = new List<string>(group.Parent.Order);
                }

                if (!menu.CanRemoveMenuItem(menuItemId))
                {
                    _removeMenuItemPresenter.Complete(RemoveMenuItemResult.InvalidId, menuItemId, orderBeforeDeletion, menu);
                    return;
                }

                menu.RemoveMenuItem(menuItemId);

                _menuRepository.Save(menuId, menu, "RemoveMenuItem");
                _updateMenuSubject.OnNext(menu);

                _removeMenuItemPresenter.Complete(RemoveMenuItemResult.Succeeded, menuItemId, orderBeforeDeletion, menu);
            }
            catch (Exception ex)
            {
                _removeMenuItemPresenter.Complete(RemoveMenuItemResult.Error, menuItemId, orderBeforeDeletion, null, ex.ToString());
            }
        }
    }
}
