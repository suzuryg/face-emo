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
        IObservable<(ModifyModePropertiesResult, IMenu, string)> Observable { get; }

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
        public IObservable<(ModifyModePropertiesResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(ModifyModePropertiesResult, IMenu, string)> _subject = new Subject<(ModifyModePropertiesResult, IMenu, string)>();

        public void Complete(ModifyModePropertiesResult modifyModePropertiesResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((modifyModePropertiesResult, menu, errorMessage));
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

                _menuRepository.Save(menuId, menu, "ModifyModeProperties");
                _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.Error, null, ex.ToString());
            }
        }
    }
}
