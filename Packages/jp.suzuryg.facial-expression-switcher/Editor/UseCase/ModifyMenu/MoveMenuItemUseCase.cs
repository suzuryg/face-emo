using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IMoveMenuItemUseCase
    {
        void SetPresenter(IMoveMenuItemPresenter moveMenuItemPresenter);
        void Handle(string source, string destination, int? index = null);
    }

    public interface IMoveMenuItemPresenter
    {
        void Complete(MoveMenuItemResult moveMenuItemResult, in Menu menu, string errorMessage = "");
    }

    public enum MoveMenuItemResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidSource,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class MoveMenuItemUseCase : IMoveMenuItemUseCase
    {
        IMoveMenuItemPresenter _moveMenuItemPresenter;
        MenuEditingSession _menuEditingSession;

        public MoveMenuItemUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IMoveMenuItemPresenter moveMenuItemPresenter)
        {
            _moveMenuItemPresenter = moveMenuItemPresenter;
        }

        public void Handle(string source, string destination, int? index = null)
        {
            try
            {
                if (source is null || destination is null)
                {
                    _moveMenuItemPresenter?.Complete(MoveMenuItemResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _moveMenuItemPresenter?.Complete(MoveMenuItemResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanMoveMenuItemFrom(source))
                {
                    _moveMenuItemPresenter?.Complete(MoveMenuItemResult.InvalidSource, menu);
                    return;
                }

                if (!menu.CanMoveMenuItemTo(source, destination))
                {
                    _moveMenuItemPresenter?.Complete(MoveMenuItemResult.InvalidDestination, menu);
                    return;
                }

                menu.MoveMenuItem(source, destination, index);
                _menuEditingSession.SetAsModified();
                _moveMenuItemPresenter?.Complete(MoveMenuItemResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _moveMenuItemPresenter?.Complete(MoveMenuItemResult.Error, null, ex.ToString());
            }
        }
    }
}
