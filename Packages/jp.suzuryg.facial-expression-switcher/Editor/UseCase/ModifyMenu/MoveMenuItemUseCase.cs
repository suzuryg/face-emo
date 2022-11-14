using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IMoveMenuItemUseCase
    {
        void Handle(string menuId, string source, string destination, int? index = null);
    }

    public interface IMoveMenuItemPresenter
    {
        event Action<MoveMenuItemResult, IMenu, string> OnCompleted;

        void Complete(MoveMenuItemResult moveMenuItemResult, in IMenu menu, string errorMessage = "");
    }

    public enum MoveMenuItemResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidSource,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class MoveMenuItemPresenter : IMoveMenuItemPresenter
    {
        public event Action<MoveMenuItemResult, IMenu, string> OnCompleted;

        public void Complete(MoveMenuItemResult moveMenuItemResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(moveMenuItemResult, menu, errorMessage);
        }
    }

    public class MoveMenuItemUseCase : IMoveMenuItemUseCase
    {
        IMenuRepository _menuRepository;
        IMoveMenuItemPresenter _moveMenuItemPresenter;

        public MoveMenuItemUseCase(IMenuRepository menuRepository, IMoveMenuItemPresenter moveMenuItemPresenter)
        {
            _menuRepository = menuRepository;
            _moveMenuItemPresenter = moveMenuItemPresenter;
        }

        public void Handle(string menuId, string source, string destination, int? index = null)
        {
            try
            {
                if (menuId is null || source is null || destination is null)
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanMoveMenuItemFrom(source))
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.InvalidSource, menu);
                    return;
                }

                if (!menu.CanMoveMenuItemTo(source, destination))
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.InvalidDestination, menu);
                    return;
                }

                menu.MoveMenuItem(source, destination, index);
                _menuRepository.Save(menuId, menu);
                _moveMenuItemPresenter.Complete(MoveMenuItemResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _moveMenuItemPresenter.Complete(MoveMenuItemResult.Error, null, ex.ToString());
            }
        }
    }
}
