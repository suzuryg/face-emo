using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.UseCase.ModifyMenu;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UniRx;
using Suzuryg.FaceEmo.Detail.Localization;

namespace Suzuryg.FaceEmo.Detail
{
    public class UseCaseErrorHandler : IDisposable
    {
        private CompositeDisposable _disposables = new CompositeDisposable();

        public UseCaseErrorHandler(
            ICreateMenuPresenter createMenuPresenter,
            IModifyMenuPropertiesPresenter modifyMenuPropertiesPresenter,
            IAddMenuItemPresenter addMenuItemPresenter,
            ICopyMenuItemPresenter copyMenuItemPresenter,
            IModifyModePropertiesPresenter modifyModePropertiesPresenter,
            IModifyGroupPropertiesPresenter modifyGroupPropertiesPresenter,
            IMoveMenuItemPresenter moveMenuItemPresenter,
            IRemoveMenuItemPresenter removeMenuItemPresenter,
            IGenerateFxPresenter generateFxPresenter,
            IAddBranchPresenter addBranchPresenter,
            IAddMultipleBranchesPresenter addMultipleBranchesPresenter,
            IModifyBranchPropertiesPresenter modifyBranchPropertiesPresenter,
            IChangeBranchOrderPresenter changeBranchOrderPresenter,
            IRemoveBranchPresenter removeBranchPresenter,
            IAddConditionPresenter addConditionPresenter,
            IChangeConditionOrderPresenter changeConditionOrderPresenter,
            IModifyConditionPresenter modifyConditionPresenter,
            IRemoveConditionPresenter removeConditionPresenter,
            ISetExistingAnimationPresenter setExistingAnimationPresenter,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            createMenuPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.createMenuResult != CreateMenuResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName, $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.createMenuResult.GetType().Name}: {x.createMenuResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            modifyMenuPropertiesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyMenuPropertiesResult != ModifyMenuPropertiesResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyMenuPropertiesResult.GetType().Name}: {x.modifyMenuPropertiesResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            addMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.addMenuItemResult != AddMenuItemResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.addMenuItemResult.GetType().Name}: {x.addMenuItemResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            copyMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.copyMenuItemResult != CopyMenuItemResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.copyMenuItemResult.GetType().Name}: {x.copyMenuItemResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            modifyModePropertiesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyModePropertiesResult != ModifyModePropertiesResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyModePropertiesResult.GetType().Name}: {x.modifyModePropertiesResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            modifyGroupPropertiesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyGroupPropertiesResult != ModifyGroupPropertiesResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyGroupPropertiesResult.GetType().Name}: {x.modifyGroupPropertiesResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            moveMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.moveMenuItemResult != MoveMenuItemResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.moveMenuItemResult.GetType().Name}: {x.moveMenuItemResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            removeMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.removeMenuItemResult != RemoveMenuItemResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.removeMenuItemResult.GetType().Name}: {x.removeMenuItemResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            generateFxPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.generateFxResult != GenerateFxResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.generateFxResult.GetType().Name}: {x.generateFxResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            addBranchPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.addBranchResult != AddBranchResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.addBranchResult.GetType().Name}: {x.addBranchResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            addMultipleBranchesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.addMultipleBranchesResult != AddMultipleBranchesResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.addMultipleBranchesResult.GetType().Name}: {x.addMultipleBranchesResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            modifyBranchPropertiesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyBranchPropertiesResult != ModifyBranchPropertiesResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyBranchPropertiesResult.GetType().Name}: {x.modifyBranchPropertiesResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            changeBranchOrderPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.changeBranchOrderResult != ChangeBranchOrderResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.changeBranchOrderResult.GetType().Name}: {x.changeBranchOrderResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            removeBranchPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.removeBranchResult != RemoveBranchResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.removeBranchResult.GetType().Name}: {x.removeBranchResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            addConditionPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.addConditionResult != AddConditionResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.addConditionResult.GetType().Name}: {x.addConditionResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            changeConditionOrderPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.changeConditionOrderResult != ChangeConditionOrderResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.changeConditionOrderResult.GetType().Name}: {x.changeConditionOrderResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            modifyConditionPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyConditionResult != ModifyConditionResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyConditionResult.GetType().Name}: {x.modifyConditionResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            removeConditionPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.removeConditionResult != RemoveConditionResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.removeConditionResult.GetType().Name}: {x.removeConditionResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);

            setExistingAnimationPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.setExistingAnimationResult != SetExistingAnimationResult.Succeeded)
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,  $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.setExistingAnimationResult.GetType().Name}: {x.setExistingAnimationResult}", "OK");
                    if (!string.IsNullOrEmpty(x.errorMessage)) { Debug.LogError(x.errorMessage); }
                }
            }).AddTo(_disposables);
        }

        public void Dispose() => _disposables.Dispose();
    }
}
