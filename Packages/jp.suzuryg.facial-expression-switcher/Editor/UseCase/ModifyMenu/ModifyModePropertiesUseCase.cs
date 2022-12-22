using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using UniRx;

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
        IObservable<(ModifyModePropertiesResult modifyModePropertiesResult, string modifiedModeId, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(ModifyModePropertiesResult modifyModePropertiesResult, string modifiedModeId, in IMenu menu, string errorMessage = "");
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
        public IObservable<(ModifyModePropertiesResult modifyModePropertiesResult, string modifiedModeId, IMenu menu, string errorMessage)> Observable => _subject.AsObservable();

        private Subject<(ModifyModePropertiesResult modifyModePropertiesResult, string modifiedModeId, IMenu menu, string errorMessage)> _subject = new Subject<(ModifyModePropertiesResult modifyModePropertiesResult, string modifiedModeId, IMenu menu, string errorMessage)>();

        public void Complete(ModifyModePropertiesResult modifyModePropertiesResult, string modifiedModeId, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((modifyModePropertiesResult, modifiedModeId, menu, errorMessage));
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
                    _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.ArgumentNull, modeId, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.MenuDoesNotExist, modeId, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.ContainsMode(modeId))
                {
                    _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.ModeIsNotContained, modeId, null);
                    return;
                }

                menu.ModifyModeProperties(modeId, displayName, useAnimationNameAsDisplayName, eyeTrackingControl, mouthTrackingControl);

                _menuRepository.Save(menuId, menu, "ModifyModeProperties");
                _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.Succeeded, modeId, menu);
            }
            catch (Exception ex)
            {
                _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.Error, modeId, null, ex.ToString());
            }
        }
    }
}
