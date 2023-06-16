using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode
{
    public interface IModifyBranchPropertiesUseCase
    {
        void Handle(string menuId, string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? blinkEnabled = null,
            bool? mouthMorphCancelerEnabled = null,
            bool? isLeftTriggerUsed = null,
            bool? isRightTriggerUsed = null);
    }

    public interface IModifyBranchPropertiesPresenter
    {
        IObservable<(ModifyBranchPropertiesResult modifyBranchPropertiesResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(ModifyBranchPropertiesResult modifyBranchPropertiesResult, in IMenu menu, string errorMessage = "");
    }

    public enum ModifyBranchPropertiesResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class ModifyBranchPropertiesPresenter : IModifyBranchPropertiesPresenter
    {
        public IObservable<(ModifyBranchPropertiesResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(ModifyBranchPropertiesResult, IMenu, string)> _subject = new Subject<(ModifyBranchPropertiesResult, IMenu, string)>();

        public void Complete(ModifyBranchPropertiesResult modifyBranchPropertiesResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((modifyBranchPropertiesResult, menu, errorMessage));
        }
    }

    public class ModifyBranchPropertiesUseCase : IModifyBranchPropertiesUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        IModifyBranchPropertiesPresenter _modifyBranchPropertiesPresenter;

        public ModifyBranchPropertiesUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, IModifyBranchPropertiesPresenter modifyBranchPropertiesPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _modifyBranchPropertiesPresenter = modifyBranchPropertiesPresenter;
        }

        public void Handle(string menuId, string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? blinkEnabled = null,
            bool? mouthMorphCancelerEnabled = null,
            bool? isLeftTriggerUsed = null,
            bool? isRightTriggerUsed = null)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _modifyBranchPropertiesPresenter.Complete(ModifyBranchPropertiesResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _modifyBranchPropertiesPresenter.Complete(ModifyBranchPropertiesResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanModifyBranchProperties(modeId, branchIndex))
                {
                    _modifyBranchPropertiesPresenter.Complete(ModifyBranchPropertiesResult.InvalidBranch, menu);
                    return;
                }

                menu.ModifyBranchProperties(modeId, branchIndex, eyeTrackingControl, mouthTrackingControl, blinkEnabled, mouthMorphCancelerEnabled, isLeftTriggerUsed, isRightTriggerUsed);

                _menuRepository.Save(menuId, menu, "ModifyBranchProperties");
                _modifyBranchPropertiesPresenter.Complete(ModifyBranchPropertiesResult.Succeeded, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _modifyBranchPropertiesPresenter.Complete(ModifyBranchPropertiesResult.Error, null, ex.ToString());
            }
        }
    }
}
