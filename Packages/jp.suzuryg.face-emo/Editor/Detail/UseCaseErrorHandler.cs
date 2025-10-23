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
            ICopyBranchPresenter copyBranchPresenter,
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
                if (x.createMenuResult == CreateMenuResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.createMenuResult.GetType().Name}: {x.createMenuResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            modifyMenuPropertiesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyMenuPropertiesResult == ModifyMenuPropertiesResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyMenuPropertiesResult.GetType().Name}: {x.modifyMenuPropertiesResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            addMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.addMenuItemResult == AddMenuItemResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.addMenuItemResult.GetType().Name}: {x.addMenuItemResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            copyMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.copyMenuItemResult == CopyMenuItemResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.copyMenuItemResult.GetType().Name}: {x.copyMenuItemResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            modifyModePropertiesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyModePropertiesResult == ModifyModePropertiesResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyModePropertiesResult.GetType().Name}: {x.modifyModePropertiesResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            modifyGroupPropertiesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyGroupPropertiesResult == ModifyGroupPropertiesResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyGroupPropertiesResult.GetType().Name}: {x.modifyGroupPropertiesResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            moveMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.moveMenuItemResult == MoveMenuItemResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.moveMenuItemResult.GetType().Name}: {x.moveMenuItemResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            removeMenuItemPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.removeMenuItemResult == RemoveMenuItemResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.removeMenuItemResult.GetType().Name}: {x.removeMenuItemResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            generateFxPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.generateFxResult == GenerateFxResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.generateFxResult.GetType().Name}: {x.generateFxResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            addBranchPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.addBranchResult == AddBranchResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.addBranchResult.GetType().Name}: {x.addBranchResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            addMultipleBranchesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.addMultipleBranchesResult == AddMultipleBranchesResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.addMultipleBranchesResult.GetType().Name}: {x.addMultipleBranchesResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            copyBranchPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.copyBranchResult == CopyBranchResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.copyBranchResult.GetType().Name}: {x.copyBranchResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            modifyBranchPropertiesPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyBranchPropertiesResult == ModifyBranchPropertiesResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyBranchPropertiesResult.GetType().Name}: {x.modifyBranchPropertiesResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            changeBranchOrderPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.changeBranchOrderResult == ChangeBranchOrderResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.changeBranchOrderResult.GetType().Name}: {x.changeBranchOrderResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            removeBranchPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.removeBranchResult == RemoveBranchResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.removeBranchResult.GetType().Name}: {x.removeBranchResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            addConditionPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.addConditionResult == AddConditionResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.addConditionResult.GetType().Name}: {x.addConditionResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            changeConditionOrderPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.changeConditionOrderResult == ChangeConditionOrderResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.changeConditionOrderResult.GetType().Name}: {x.changeConditionOrderResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            modifyConditionPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.modifyConditionResult == ModifyConditionResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.modifyConditionResult.GetType().Name}: {x.modifyConditionResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            removeConditionPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.removeConditionResult == RemoveConditionResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.removeConditionResult.GetType().Name}: {x.removeConditionResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);

            setExistingAnimationPresenter.Observable.Synchronize().Subscribe(x =>
            {
                if (x.setExistingAnimationResult == SetExistingAnimationResult.Succeeded) return;
                ReadableErrorWindow.Open(DomainConstants.SystemName,
                    $"{localizationSetting.GetCurrentLocaleTable().ErrorHandler_Message_ErrorOccured}\n{x.setExistingAnimationResult.GetType().Name}: {x.setExistingAnimationResult}",
                    x.errorMessage);
                if (!string.IsNullOrEmpty(x.errorMessage)) Debug.LogError(x.errorMessage);
            }).AddTo(_disposables);
        }

        public void Dispose() => _disposables.Dispose();
    }
}
