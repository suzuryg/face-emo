using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public interface IModifyBranchPropertiesUseCase
    {
        void SetPresenter(IModifyBranchPropertiesPresenter modifyBranchPropertiesPresenter);
        void Handle(string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? isLeftTriggerUsed = null,
            bool? isRightTriggerUsed = null);
    }

    public interface IModifyBranchPropertiesPresenter
    {
        void Complete(ModifyBranchPropertiesResult modifyBranchPropertiesResult, in Menu menu, string errorMessage = "");
    }

    public enum ModifyBranchPropertiesResult
    {
        Succeeded,
        MenuIsNotOpened,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class ModifyBranchPropertiesUseCase : IModifyBranchPropertiesUseCase
    {
        IModifyBranchPropertiesPresenter _modifyBranchPropertiesPresenter;
        MenuEditingSession _menuEditingSession;

        public ModifyBranchPropertiesUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IModifyBranchPropertiesPresenter modifyBranchPropertiesPresenter)
        {
            _modifyBranchPropertiesPresenter = modifyBranchPropertiesPresenter;
        }

        public void Handle(string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? isLeftTriggerUsed = null,
            bool? isRightTriggerUsed = null)
        {
            try
            {
                if (modeId is null)
                {
                    _modifyBranchPropertiesPresenter?.Complete(ModifyBranchPropertiesResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _modifyBranchPropertiesPresenter?.Complete(ModifyBranchPropertiesResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanModifyBranchProperties(modeId, branchIndex))
                {
                    _modifyBranchPropertiesPresenter?.Complete(ModifyBranchPropertiesResult.InvalidBranch, menu);
                    return;
                }

                menu.ModifyBranchProperties(modeId, branchIndex, eyeTrackingControl, mouthTrackingControl, isLeftTriggerUsed, isRightTriggerUsed);
                _menuEditingSession.SetAsModified();
                _modifyBranchPropertiesPresenter?.Complete(ModifyBranchPropertiesResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _modifyBranchPropertiesPresenter?.Complete(ModifyBranchPropertiesResult.Error, null, ex.ToString());
            }
        }
    }
}
