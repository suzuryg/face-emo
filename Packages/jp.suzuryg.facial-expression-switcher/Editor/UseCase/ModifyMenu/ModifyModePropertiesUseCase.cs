using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IModifyModePropertiesUseCase
    {
        void Handle(
            string menuId,
            string modeId,
            string displayName = null,
            bool? useAnimationNameAsDisplayName = null,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null);
    }

    public interface IModifyModePropertiesPresenter
    {
        event Action<ModifyModePropertiesResult, IMenu, string> OnCompleted;

        void Complete(ModifyModePropertiesResult modifyModePropertiesResult, in IMenu menu, string errorMessage = "");
    }

    public enum ModifyModePropertiesResult
    {
        Succeeded,
        MenuDoesNotExist,
        ModeIsNotContained,
        ArgumentNull,
        Error,
    }

    public class ModifyModePropertiesPresenter : IModifyModePropertiesPresenter
    {
        public event Action<ModifyModePropertiesResult, IMenu, string> OnCompleted;

        public void Complete(ModifyModePropertiesResult modifyModePropertiesResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(modifyModePropertiesResult, menu, errorMessage);
        }
    }

    public class ModifyModePropertiesUseCase : IModifyModePropertiesUseCase
    {
        IMenuRepository _menuRepository;
        IModifyModePropertiesPresenter _modifyModePropertiesPresenter;

        public ModifyModePropertiesUseCase(IMenuRepository menuRepository, IModifyModePropertiesPresenter modifyModePropertiesPresenter)
        {
            _menuRepository = menuRepository;
            _modifyModePropertiesPresenter = modifyModePropertiesPresenter;
        }

        public void Handle(
            string menuId,
            string modeId,
            string displayName = null,
            bool? useAnimationNameAsDisplayName = null,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.ContainsMode(modeId))
                {
                    _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.ModeIsNotContained, null);
                    return;
                }

                menu.ModifyModeProperties(modeId, displayName, useAnimationNameAsDisplayName, eyeTrackingControl, mouthTrackingControl);

                _menuRepository.Save(menuId, menu);
                _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.Error, null, ex.ToString());
            }
        }
    }
}
