using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IRemoveMenuItemUseCase
    {
        void SetPresenter(IRemoveMenuItemPresenter removeMenuItemPresenter);
        void Handle(string id);
    }

    public interface IRemoveMenuItemPresenter
    {
        void Complete(RemoveMenuItemResult removeMenuItemResult, in Menu menu, string errorMessage = "");
    }

    public enum RemoveMenuItemResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidId,
        ArgumentNull,
        Error,
    }

    public class RemoveMenuItemUseCase : IRemoveMenuItemUseCase
    {
        IRemoveMenuItemPresenter _removeMenuItemPresenter;
        MenuEditingSession _menuEditingSession;

        public RemoveMenuItemUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IRemoveMenuItemPresenter removeMenuItemPresenter)
        {
            _removeMenuItemPresenter = removeMenuItemPresenter;
        }

        public void Handle(string id)
        {
            try
            {
                if (id is null)
                {
                    _removeMenuItemPresenter?.Complete(RemoveMenuItemResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _removeMenuItemPresenter?.Complete(RemoveMenuItemResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanRemoveMenuItem(id))
                {
                    _removeMenuItemPresenter?.Complete(RemoveMenuItemResult.InvalidId, menu);
                    return;
                }

                menu.RemoveMenuItem(id);
                _menuEditingSession.SetAsModified();
                _removeMenuItemPresenter?.Complete(RemoveMenuItemResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _removeMenuItemPresenter?.Complete(RemoveMenuItemResult.Error, null, ex.ToString());
            }
        }
    }
}
