using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public class UseCaseErrorHandler : IDisposable
    {
        private CompositeDisposable _disposables = new CompositeDisposable();

        public UseCaseErrorHandler(
            ICreateMenuPresenter createMenuPresenter,
            IAddMenuItemPresenter addMenuItemPresenter,
            IModifyModePropertiesPresenter modifyModePropertiesPresenter,
            IModifyGroupPropertiesPresenter modifyGroupPropertiesPresenter,
            IMoveMenuItemPresenter moveMenuItemPresenter,
            IRemoveMenuItemPresenter removeMenuItemPresenter,
            IMergeExistingMenuPresenter mergeExistingMenuPresenter,
            IApplyMenuPresenter applyMenuPresenter,
            IAddBranchPresenter addBranchPresenter,
            IModifyBranchPropertiesPresenter modifyBranchPropertiesPresenter,
            IChangeBranchOrderPresenter changeBranchOrderPresenter,
            IRemoveBranchPresenter removeBranchPresenter,
            IAddConditionPresenter addConditionPresenter,
            IChangeConditionOrderPresenter changeConditionOrderPresenter,
            IModifyConditionPresenter modifyConditionPresenter,
            IRemoveConditionPresenter removeConditionPresenter,
            ISetNewAnimationPresenter setNewAnimationPresenter,
            ISetExistingAnimationPresenter setExistingAnimationPresenter)
        {
            createMenuPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.createMenuResult != CreateMenuResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName, $"{x.createMenuResult.GetType().Name}: {x.createMenuResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            //addMenuItemPresenter
            //modifyModePropertiesPresenter
            //modifyGroupPropertiesPresenter

            moveMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.moveMenuItemResult != MoveMenuItemResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName, $"{x.moveMenuItemResult.GetType().Name}: {x.moveMenuItemResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            //removeMenuItemPresenter
            //mergeExistingMenuPresenter
            //applyMenuPresenter
            //addBranchPresenter
            //modifyBranchPropertiesPresenter
            //changeBranchOrderPresenter
            //removeBranchPresenter
            //addConditionPresenter
            //changeConditionOrderPresenter
            //modifyConditionPresenter
            //removeConditionPresenter
            //setNewAnimationPresenter
            //setExistingAnimationPresenter
        }

        public void Dispose() => _disposables.Dispose();
    }
}
