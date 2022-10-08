using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IAddMenuItemUseCase
    {
        void SetPresenter(IAddMenuItemPresenter addMenuItemPresenter);
        void Handle(string destination, AddMenuItemType type);
    }

    public interface IAddMenuItemPresenter
    {
        void Complete(AddMenuItemResult addMenuItemResult, in Menu menu, string errorMessage = "");
    }

    public enum AddMenuItemResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public enum AddMenuItemType
    {
        Group,
        Mode,
    }

    public class AddMenuItemUseCase : IAddMenuItemUseCase
    {
        IAddMenuItemPresenter _addMenuItemPresenter;
        MenuEditingSession _menuEditingSession;

        public AddMenuItemUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }
        
        public void SetPresenter(IAddMenuItemPresenter addMenuItemPresenter)
        {
            _addMenuItemPresenter = addMenuItemPresenter;
        }

        public void Handle(string destination, AddMenuItemType type)
        {
            try
            {
                if (destination is null)
                {
                    _addMenuItemPresenter?.Complete(AddMenuItemResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _addMenuItemPresenter?.Complete(AddMenuItemResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (type == AddMenuItemType.Mode)
                {
                    if (menu.CanAddModeTo(destination))
                    {
                        menu.AddMode(destination);
                    }
                    else
                    {
                        _addMenuItemPresenter?.Complete(AddMenuItemResult.InvalidDestination, menu);
                        return;
                    }
                }
                else if (type == AddMenuItemType.Group)
                {
                    if (menu.CanAddGroupTo(destination))
                    {
                        menu.AddGroup(destination);
                    }
                    else
                    {
                        _addMenuItemPresenter?.Complete(AddMenuItemResult.InvalidDestination, menu);
                        return;
                    }
                }
                else
                {
                    throw new FacialExpressionSwitcherException("Unknown AddMenuItemType");
                }

                _menuEditingSession.SetAsModified();
                _addMenuItemPresenter?.Complete(AddMenuItemResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _addMenuItemPresenter?.Complete(AddMenuItemResult.Error, null, ex.ToString());
            }
        }
    }
}
