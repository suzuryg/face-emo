using Suzuryg.FaceEmo.Domain;
using System;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu
{
    public interface IModifyModePropertiesUseCase
    {
        void Handle(
            string menuId,
            string modeId,
            bool? changeDefaultFace = null,
            string displayName = null,
            bool? useAnimationNameAsDisplayName = null,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? blinkEnabled = null,
            bool? mouthMorphCancelerEnabled = null);
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
        UpdateMenuSubject _updateMenuSubject;
        IModifyModePropertiesPresenter _modifyModePropertiesPresenter;

        public ModifyModePropertiesUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, IModifyModePropertiesPresenter modifyModePropertiesPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _modifyModePropertiesPresenter = modifyModePropertiesPresenter;
        }

        public void Handle(
            string menuId,
            string modeId,
            bool? changeDefaultFace = null,
            string displayName = null,
            bool? useAnimationNameAsDisplayName = null,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? blinkEnabled = null,
            bool? mouthMorphCancelerEnabled = null)
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

                menu.ModifyModeProperties(
                    id: modeId,
                    changeDefaultFace: changeDefaultFace,
                    displayName: displayName,
                    useAnimationNameAsDisplayName: useAnimationNameAsDisplayName, 
                    eyeTrackingControl: eyeTrackingControl,
                    mouthTrackingControl: mouthTrackingControl,
                    blinkEnabled: blinkEnabled,
                    mouthMorphCancelerEnabled: mouthMorphCancelerEnabled);

                _menuRepository.Save(menuId, menu, "ModifyModeProperties");
                _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.Succeeded, modeId, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _modifyModePropertiesPresenter.Complete(ModifyModePropertiesResult.Error, modeId, null, ex.ToString());
            }
        }
    }
}
