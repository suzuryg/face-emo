using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IModifyModePropertiesUseCase
    {
        void SetPresenter(IModifyModePropertiesPresenter modifyModePropertiesPresenter);
        void Handle(
            string id,
            string displayName = null,
            bool? useAnimationNameAsDisplayName = null,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null);
    }

    public interface IModifyModePropertiesPresenter
    {
        void Complete(ModifyModePropertiesResult modifyModePropertiesResult, in Menu menu, string errorMessage = "");
    }

    public enum ModifyModePropertiesResult
    {
        Succeeded,
        MenuIsNotOpened,
        ModeIsNotContained,
        ArgumentNull,
        Error,
    }

    public class ModifyModePropertiesUseCase : IModifyModePropertiesUseCase
    {
        IModifyModePropertiesPresenter _modifyModePropertiesPresenter;
        MenuEditingSession _menuEditingSession;

        public ModifyModePropertiesUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IModifyModePropertiesPresenter modifyModePropertiesPresenter)
        {
            _modifyModePropertiesPresenter = modifyModePropertiesPresenter;
        }

        public void Handle(
            string id,
            string displayName = null,
            bool? useAnimationNameAsDisplayName = null,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null)
        {
            try
            {
                if (id is null)
                {
                    _modifyModePropertiesPresenter?.Complete(ModifyModePropertiesResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _modifyModePropertiesPresenter?.Complete(ModifyModePropertiesResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.ContainsMode(id))
                {
                    _modifyModePropertiesPresenter?.Complete(ModifyModePropertiesResult.ModeIsNotContained, null);
                    return;
                }

                menu.ModifyModeProperties(id, displayName, useAnimationNameAsDisplayName, eyeTrackingControl, mouthTrackingControl);

                _menuEditingSession.SetAsModified();
                _modifyModePropertiesPresenter?.Complete(ModifyModePropertiesResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _modifyModePropertiesPresenter?.Complete(ModifyModePropertiesResult.Error, null, ex.ToString());
            }
        }
    }
}
