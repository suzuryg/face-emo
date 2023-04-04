using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.Subject;
using Suzuryg.FacialExpressionSwitcher.Detail.View.Element;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UniRx;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class BranchListView : IDisposable
    {
        private ISetExistingAnimationUseCase _setExistingAnimationUseCase;
        private IAddBranchUseCase _addBranchUseCase;
        private IChangeBranchOrderUseCase _changeBranchOrderUseCase;
        private IModifyBranchPropertiesUseCase _modifyBranchPropertiesUseCase;
        private IRemoveBranchUseCase _removeBranchUseCase;
        private IAddConditionUseCase _addConditionUseCase;
        private IChangeConditionOrderUseCase _changeConditionOrderUseCase;
        private IModifyConditionUseCase _modifyConditionUseCase;
        private IRemoveConditionUseCase _removeConditionUseCase;

        // TODO: 他のViewで使用しているPresenterに対応する
        private ISetExistingAnimationPresenter _setExistingAnimationPresenter;
        private IAddBranchPresenter _addBranchPresenter;
        private IChangeBranchOrderPresenter _changeBranchOrderPresenter;
        private IModifyBranchPropertiesPresenter _modifyBranchPropertiesPresenter;
        private IRemoveBranchPresenter _removeBranchPresenter;
        private IAddConditionPresenter _addConditionPresenter;
        private IChangeConditionOrderPresenter _changeConditionOrderPresenter;
        private IModifyConditionPresenter _modifyConditionPresenter;
        private IRemoveConditionPresenter _removeConditionPresenter;

        private IReadOnlyLocalizationSetting _localizationSetting;
        private UpdateMenuSubject _updateMenuSubject;
        private ChangeMenuItemListSelectionSubject _changeMenuItemListSelectionSubject;
        private ChangeBranchSelectionSubject _changeBranchSelectionSubject;
        private AV3Setting _aV3Setting;
        private ThumbnailDrawer _thumbnailDrawer;

        private Label _titleLabel;
        private IMGUIContainer _branchListContainer;

        private BranchListElement _branchListElement;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public BranchListView(
            ISetExistingAnimationUseCase setExistingAnimationUseCase,
            IAddBranchUseCase addBranchUseCase,
            IChangeBranchOrderUseCase changeBranchOrderUseCase,
            IModifyBranchPropertiesUseCase modifyBranchPropertiesUseCase,
            IRemoveBranchUseCase removeBranchUseCase,
            IAddConditionUseCase addConditionUseCase,
            IChangeConditionOrderUseCase changeConditionOrderUseCase,
            IModifyConditionUseCase modifyConditionUseCase,
            IRemoveConditionUseCase removeConditionUseCase,

            ISetExistingAnimationPresenter setExistingAnimationPresenter,
            IAddBranchPresenter addBranchPresenter,
            IChangeBranchOrderPresenter changeBranchOrderPresenter,
            IModifyBranchPropertiesPresenter modifyBranchPropertiesPresenter,
            IRemoveBranchPresenter removeBranchPresenter,
            IAddConditionPresenter addConditionPresenter,
            IChangeConditionOrderPresenter changeConditionOrderPresenter,
            IModifyConditionPresenter modifyConditionPresenter,
            IRemoveConditionPresenter removeConditionPresenter,

            IReadOnlyLocalizationSetting localizationSetting,
            UpdateMenuSubject updateMenuSubject,
            ChangeMenuItemListSelectionSubject changeMenuItemListSelectionSubject,
            ChangeBranchSelectionSubject changeBranchSelectionSubject,
            AV3Setting aV3Setting,
            ThumbnailDrawer thumbnailDrawer)
        {
            // Usecases
            _setExistingAnimationUseCase = setExistingAnimationUseCase;
            _addBranchUseCase = addBranchUseCase;
            _changeBranchOrderUseCase = changeBranchOrderUseCase;
            _modifyBranchPropertiesUseCase = modifyBranchPropertiesUseCase;
            _removeBranchUseCase = removeBranchUseCase;
            _addConditionUseCase = addConditionUseCase;
            _changeConditionOrderUseCase = changeConditionOrderUseCase;
            _modifyConditionUseCase = modifyConditionUseCase;
            _removeConditionUseCase = removeConditionUseCase;

            // Presenters
            _setExistingAnimationPresenter = setExistingAnimationPresenter;
            _addBranchPresenter = addBranchPresenter;
            _changeBranchOrderPresenter = changeBranchOrderPresenter;
            _modifyBranchPropertiesPresenter = modifyBranchPropertiesPresenter;
            _removeBranchPresenter = removeBranchPresenter;
            _addConditionPresenter = addConditionPresenter;
            _changeConditionOrderPresenter = changeConditionOrderPresenter;
            _modifyConditionPresenter = modifyConditionPresenter;
            _removeConditionPresenter = removeConditionPresenter;

            // Others
            _localizationSetting = localizationSetting;
            _updateMenuSubject = updateMenuSubject;
            _changeMenuItemListSelectionSubject = changeMenuItemListSelectionSubject;
            _changeBranchSelectionSubject = changeBranchSelectionSubject;
            _aV3Setting = aV3Setting;
            _thumbnailDrawer = thumbnailDrawer;

            // Branch list element
            _branchListElement = new BranchListElement(_localizationSetting, _aV3Setting, _thumbnailDrawer).AddTo(_disposables);

            _branchListElement.OnAnimationChanged.Synchronize().Subscribe(OnAnimationChanged).AddTo(_disposables);

            _branchListElement.OnAddBranchButtonClicked.Synchronize().Subscribe(OnAddBranchButtonClicked).AddTo(_disposables);
            _branchListElement.OnModifyBranchPropertiesButtonClicked.Synchronize().Subscribe(OnModifyBranchPropertiesButtonClicked).AddTo(_disposables);
            _branchListElement.OnBranchOrderChanged.Synchronize().Subscribe(OnBranchOrderChanged).AddTo(_disposables);
            _branchListElement.OnRemoveBranchButtonClicked.Synchronize().Subscribe(OnRemoveBranchButtonClicked).AddTo(_disposables);

            _branchListElement.OnAddConditionButtonClicked.Synchronize().Subscribe(OnAddConditionButtonClicked).AddTo(_disposables);
            _branchListElement.OnModifyConditionButtonClicked.Synchronize().Subscribe(OnModifyConditionButtonClicked).AddTo(_disposables);
            _branchListElement.OnConditionOrderChanged.Synchronize().Subscribe(OnConditionOrderChanged).AddTo(_disposables);
            _branchListElement.OnRemoveConditionButtonClicked.Synchronize().Subscribe(OnRemoveConditionButtonClicked).AddTo(_disposables);
            _branchListElement.OnBranchSelectionChanged.Synchronize().Subscribe(OnBranchListViewSelectionChanged).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(OnMenuUpdated).AddTo(_disposables);

            // Change menu item list event handler
            _changeMenuItemListSelectionSubject.Observable.Synchronize().Subscribe(OnMenuItemListSelectionChanged).AddTo(_disposables);

            // Change branch selection event handler
            _changeBranchSelectionSubject.Observable.Synchronize().Subscribe(OnGestureTableViewSelectionChanged).AddTo(_disposables);

            // Presenter event handlers
            _setExistingAnimationPresenter.Observable.Synchronize().Subscribe(OnSetExistingAnimationPresenterCompleted).AddTo(_disposables);

            _addBranchPresenter.Observable.Synchronize().Subscribe(OnAddBranchPresenterCompleted).AddTo(_disposables);
            _changeBranchOrderPresenter.Observable.Synchronize().Subscribe(OnChangeBranchOrderPresenterCompleted).AddTo(_disposables);
            _modifyBranchPropertiesPresenter.Observable.Synchronize().Subscribe(OnModifyBranchPropertiesPresenterCompleted).AddTo(_disposables);
            _removeBranchPresenter.Observable.Synchronize().Subscribe(OnRemoveBranchPresenterCompleted).AddTo(_disposables);

            _addConditionPresenter.Observable.Synchronize().Subscribe(OnAddConditionPresenterCompleted).AddTo(_disposables);
            _changeConditionOrderPresenter.Observable.Synchronize().Subscribe(OnChangeConditionOrderPresenterCompleted).AddTo(_disposables);
            _modifyConditionPresenter.Observable.Synchronize().Subscribe(OnModifyConditionPresenterCompleted).AddTo(_disposables);
            _removeConditionPresenter.Observable.Synchronize().Subscribe(OnRemoveConditionPresenterCompleted).AddTo(_disposables);
        }

        public void Dispose() => _disposables.Dispose();

        public void Initialize(VisualElement root)
        {
            // UXML and style
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(BranchListView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(BranchListView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            // Query elements
            _titleLabel = root.Q<Label>("TitleLabel");
            _branchListContainer = root.Q<IMGUIContainer>("BranchListContainer");
            NullChecker.Check(_titleLabel, _branchListContainer);

            // Add event handlers
            Observable.FromEvent(x => _branchListContainer.onGUIHandler += x, x => _branchListContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ =>
                {
                    _branchListElement?.OnGUI(_branchListContainer.contentRect);

                    // To draw thumbnail ovarlay.
                    if (Event.current.type == EventType.MouseMove)
                    {
                        _branchListContainer.MarkDirtyRepaint();
                    }
                }).AddTo(_disposables);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _titleLabel.text = localizationTable.BranchListView_Title;
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _branchListElement.Setup(menu);
        }

        private void OnMenuItemListSelectionChanged(IReadOnlyList<string> selectedMenuItemIds)
        {
            if (selectedMenuItemIds.Count == 1)
            {
                _branchListElement.ChangeModeSelection(selectedMenuItemIds[0]);
            }
        }

        private void OnAddBranchButtonClicked(string modeId)
        {
            _addBranchUseCase.Handle("", modeId);
        }

        private void OnModifyBranchPropertiesButtonClicked((string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl,
            MouthTrackingControl? mouthTrackingControl,
            bool? blinkEnabled,
            bool? mouthMorphCancelerEnabled,
            bool? isLeftTriggerUsed,
            bool? isRightTriggerUsed) args)
        {
            _modifyBranchPropertiesUseCase.Handle("", args.modeId, args.branchIndex, args.eyeTrackingControl, args.mouthTrackingControl, args.blinkEnabled, args.mouthMorphCancelerEnabled, args.isLeftTriggerUsed, args.isRightTriggerUsed);
        }

        private void OnBranchOrderChanged((string modeId, int from, int to) args)
        {
            _changeBranchOrderUseCase.Handle("", args.modeId, args.from, args.to);
        }

        private void OnBranchListViewSelectionChanged(int branchIndex)
        {
            _changeBranchSelectionSubject.OnNext(branchIndex);
        }

        private void OnGestureTableViewSelectionChanged(int branchIndex)
        {
            _branchListElement.ChangeBranchSelection(branchIndex);
            _branchListContainer?.MarkDirtyRepaint();
        }

        private void OnRemoveBranchButtonClicked((string modeId, int branchIndex) args)
        {
            _removeBranchUseCase.Handle("", args.modeId, args.branchIndex);
        }

        private void OnAddConditionButtonClicked((string modeId, int branchIndex, Condition condition) args)
        {
            _addConditionUseCase.Handle("", args.modeId, args.branchIndex, args.condition);
        }

        private void OnModifyConditionButtonClicked((string modeId, int branchIndex, int conditionIndex, Condition condition) args)
        {
            _modifyConditionUseCase.Handle("", args.modeId, args.branchIndex, args.conditionIndex, args.condition);
        }

        private void OnConditionOrderChanged((string modeId, int branchIndex, int from, int to) args)
        {
            _changeConditionOrderUseCase.Handle("", args.modeId, args.branchIndex, args.from, args.to);
        }

        private void OnRemoveConditionButtonClicked((string modeId, int branchIndex, int conditionIndex) args)
        {
            _removeConditionUseCase.Handle("", args.modeId, args.branchIndex, args.conditionIndex);
        }

        public void OnAnimationChanged((
            string clipGUID,
            string modeId,
            int branchIndex,
            BranchAnimationType branchAnimationType) args)
        {
            _setExistingAnimationUseCase.Handle("", new Domain.Animation(args.clipGUID), args.modeId, args.branchIndex, args.branchAnimationType);
        }

        private void OnSetExistingAnimationPresenterCompleted(
            (SetExistingAnimationResult setExistingAnimationResult, IMenu menu, string errorMessage) args)
        {
            if (args.setExistingAnimationResult == SetExistingAnimationResult.Succeeded)
            {
                _branchListElement.Setup(args.menu);
            }
        }

        private void OnAddBranchPresenterCompleted(
            (AddBranchResult addBranchResult, IMenu menu, string errorMessage) args)
        {
            if (args.addBranchResult == AddBranchResult.Succeeded)
            {
                _branchListElement.Setup(args.menu);
                // This presenter can be called by gesture table view.
                _branchListContainer.MarkDirtyRepaint();
            }
        }

        private void OnChangeBranchOrderPresenterCompleted(
            (ChangeBranchOrderResult changeBranchOrderResult, IMenu menu, string errorMessage) args)
        {
            if (args.changeBranchOrderResult == ChangeBranchOrderResult.Succeeded)
            {
                _branchListElement.Setup(args.menu);
            }
        }

        private void OnModifyBranchPropertiesPresenterCompleted(
            (ModifyBranchPropertiesResult modifyBranchPropertiesResult, IMenu menu, string errorMessage) args)
        {
            if (args.modifyBranchPropertiesResult == ModifyBranchPropertiesResult.Succeeded)
            {
                _branchListElement.Setup(args.menu);
            }
        }

        private void OnRemoveBranchPresenterCompleted(
            (RemoveBranchResult removeBranchResult, IMenu menu, string errorMessage) args)
        {
            if (args.removeBranchResult == RemoveBranchResult.Succeeded)
            {
                _branchListElement.Setup(args.menu);
            }
        }

        private void OnAddConditionPresenterCompleted(
            (AddConditionResult addConditionResult, IMenu menu, string errorMessage) args)
        {
            if (args.addConditionResult == AddConditionResult.Succeeded)
            {
                _branchListElement.Setup(args.menu);
            }
        }

        private void OnChangeConditionOrderPresenterCompleted(
            (ChangeConditionOrderResult changeConditionOrderResult, IMenu menu, string errorMessage) args)
        {
            if (args.changeConditionOrderResult == ChangeConditionOrderResult.Succeeded)
            {
                _branchListElement.Setup(args.menu);
            }
        }

        private void OnModifyConditionPresenterCompleted(
            (ModifyConditionResult modifyConditionResult, IMenu menu, string errorMessage) args)
        {
            if (args.modifyConditionResult == ModifyConditionResult.Succeeded)
            {
                _branchListElement.Setup(args.menu);
            }
        }

        private void OnRemoveConditionPresenterCompleted(
            (RemoveConditionResult removeConditionResult, IMenu menu, string errorMessage) args)
        {
            if (args.removeConditionResult == RemoveConditionResult.Succeeded)
            {
                _branchListElement.Setup(args.menu);
            }
        }
    }
}
