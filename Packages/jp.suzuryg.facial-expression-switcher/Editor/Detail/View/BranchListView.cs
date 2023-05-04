using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
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

        private IAddBranchPresenter _addBranchPresenter;

        private IReadOnlyLocalizationSetting _localizationSetting;
        private LocalizationTable _localizationTable;

        private ModeNameProvider _modeNameProvider;
        private UpdateMenuSubject _updateMenuSubject;
        private SelectionSynchronizer _selectionSynchronizer;
        private AV3Setting _aV3Setting;
        private MainThumbnailDrawer _thumbnailDrawer;

        private Label _titleLabel;
        private IMGUIContainer _branchListContainer;

        private AnimationElement _animationElement;
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

            IAddBranchPresenter addBranchPresenter,

            IReadOnlyLocalizationSetting localizationSetting,
            ModeNameProvider modeNameProvider,
            UpdateMenuSubject updateMenuSubject,
            SelectionSynchronizer selectionSynchronizer,
            AV3Setting aV3Setting,
            MainThumbnailDrawer thumbnailDrawer,
            BranchListElement branchListElement,
            AnimationElement animationElement)
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
            _addBranchPresenter = addBranchPresenter;

            // Others
            _localizationSetting = localizationSetting;
            _modeNameProvider = modeNameProvider;
            _updateMenuSubject = updateMenuSubject;
            _selectionSynchronizer = selectionSynchronizer;
            _aV3Setting = aV3Setting;
            _thumbnailDrawer = thumbnailDrawer;
            _branchListElement = branchListElement;
            _animationElement = animationElement;

            // Branch list element
            _branchListElement.AddTo(_disposables);
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

            // Synchronize selection event handler
            _selectionSynchronizer.OnSynchronizeSelection.Synchronize().Subscribe(OnSynchronizeSelection).AddTo(_disposables);

            // Presenter event handlers
            _addBranchPresenter.Observable.Synchronize().Subscribe(OnAddBranchPresenterCompleted).AddTo(_disposables);
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

        public float GetMinWidth()
        {
            if (_branchListElement != null) { return _branchListElement.GetWidth(); }
            else { return 0; }
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
            _titleLabel.text = localizationTable.BranchListView_Title;
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _branchListElement.Setup(menu);
            _branchListContainer.MarkDirtyRepaint();
        }

        private void OnBranchListViewSelectionChanged(int branchIndex)
        {
            _selectionSynchronizer.ChangeBranchListViewSelection(branchIndex);
        }

        private void OnSynchronizeSelection(ViewSelection viewSelection)
        {
            _branchListElement?.ChangeModeSelection(viewSelection.MenuItemListView);
            _branchListElement?.ChangeBranchSelection(viewSelection.BranchListView);
            _branchListContainer?.MarkDirtyRepaint();
        }

        private void OnBranchOrderChanged((string modeId, int from, int to) args)
        {
            _changeBranchOrderUseCase.Handle("", args.modeId, args.from, args.to);
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

        private void OnAddBranchButtonClicked(string modeId)
        {
            _addBranchUseCase.Handle("", modeId);
        }

        private void OnRemoveBranchButtonClicked((string modeId, int branchIndex) args)
        {
            var branchDeleteConfirmation = EditorPrefs.HasKey(DetailConstants.KeyBranchDeleteConfirmation) ? EditorPrefs.GetBool(DetailConstants.KeyBranchDeleteConfirmation) : DetailConstants.DefaultBranchDeleteConfirmation;
            if (branchDeleteConfirmation)
            {
                var ok = EditorUtility.DisplayDialog(DomainConstants.SystemName,
                    _localizationTable.Common_Message_DeleteBranch,
                    _localizationTable.Common_Delete, _localizationTable.Common_Cancel);
                if (!ok) { return; }
            }

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

        private void OnAddBranchPresenterCompleted(
            (AddBranchResult addBranchResult, IMenu menu, string errorMessage) args)
        {
            if (args.addBranchResult == AddBranchResult.Succeeded)
            {
                // Make the created branch selected in this view
                OnMenuUpdated(args.menu);
                _branchListElement?.SelectNewestBranch();

                // Make the created branch selected in GestureTableView
                _selectionSynchronizer.ChangeBranchListViewSelection(_branchListElement?.GetNumOfBranches() - 1 ?? -1);
            }
        }
    }
}
