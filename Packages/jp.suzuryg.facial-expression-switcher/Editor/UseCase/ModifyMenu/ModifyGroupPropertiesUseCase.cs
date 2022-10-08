using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IModifyGroupPropertiesUseCase
    {
        void SetPresenter(IModifyGroupPropertiesPresenter modifyGroupPropertiesPresenter);
        void Handle(
            string id,
            string displayName = null);
    }

    public interface IModifyGroupPropertiesPresenter
    {
        void Complete(ModifyGroupPropertiesResult modifyGroupPropertiesResult, in Menu menu, string errorMessage = "");
    }

    public enum ModifyGroupPropertiesResult
    {
        Succeeded,
        MenuIsNotOpened,
        GroupIsNotContained,
        ArgumentNull,
        Error,
    }

    public class ModifyGroupPropertiesUseCase : IModifyGroupPropertiesUseCase
    {
        IModifyGroupPropertiesPresenter _modifyGroupPropertiesPresenter;
        MenuEditingSession _menuEditingSession;

        public ModifyGroupPropertiesUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IModifyGroupPropertiesPresenter modifyGroupPropertiesPresenter)
        {
            _modifyGroupPropertiesPresenter = modifyGroupPropertiesPresenter;
        }

        public void Handle(
            string id,
            string displayName = null)
        {
            try
            {
                if (id is null)
                {
                    _modifyGroupPropertiesPresenter?.Complete(ModifyGroupPropertiesResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _modifyGroupPropertiesPresenter?.Complete(ModifyGroupPropertiesResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.ContainsGroup(id))
                {
                    _modifyGroupPropertiesPresenter?.Complete(ModifyGroupPropertiesResult.GroupIsNotContained, menu);
                    return;
                }

                menu.ModifyGroupProperties(id, displayName);

                _menuEditingSession.SetAsModified();
                _modifyGroupPropertiesPresenter?.Complete(ModifyGroupPropertiesResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _modifyGroupPropertiesPresenter?.Complete(ModifyGroupPropertiesResult.Error, null, ex.ToString());
            }
        }
    }
}
