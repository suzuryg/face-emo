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
        private IAddMultipleBranchesUseCase _addMultipleBranchesUseCase;
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

        private ISubWindowProvider _subWindowProvider;
        private ModeNameProvider _modeNameProvider;
        private UpdateMenuSubject _updateMenuSubject;
        private SelectionSynchronizer _selectionSynchronizer;
        private AV3Setting _aV3Setting;
        private MainThumbnailDrawer _thumbnailDrawer;

        private Label _titleLabel;
        private Toggle _simplifyToggle;
        private IMGUIContainer _presetContainer;
        private Button _openGestureTableWindowButton;
        private IMGUIContainer _branchListContainer;

        private AnimationElement _animationElement;
        private BranchListElement _branchListElement;

        private int _selectedPresetIndex = 0;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public BranchListView(
            ISetExistingAnimationUseCase setExistingAnimationUseCase,
            IAddBranchUseCase addBranchUseCase,
            IAddMultipleBranchesUseCase addMultipleBranchesUseCase,
            IChangeBranchOrderUseCase changeBranchOrderUseCase,
            IModifyBranchPropertiesUseCase modifyBranchPropertiesUseCase,
            IRemoveBranchUseCase removeBranchUseCase,
            IAddConditionUseCase addConditionUseCase,
            IChangeConditionOrderUseCase changeConditionOrderUseCase,
            IModifyConditionUseCase modifyConditionUseCase,
            IRemoveConditionUseCase removeConditionUseCase,

            IAddBranchPresenter addBranchPresenter,

            IReadOnlyLocalizationSetting localizationSetting,
            ISubWindowProvider subWindowProvider,
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
            _addMultipleBranchesUseCase = addMultipleBranchesUseCase;
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
            _subWindowProvider = subWindowProvider;
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

        public void Dispose()
        {
            _simplifyToggle.UnregisterValueChangedCallback(OnSimplifyValueChanged);
            _disposables.Dispose();
        }

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
            _simplifyToggle = root.Q<Toggle>("SimplifyToggle");
            _presetContainer = root.Q<IMGUIContainer>("PresetContainer");
            _openGestureTableWindowButton = root.Q<Button>("OpenGestureTableWindowButton");
            _branchListContainer = root.Q<IMGUIContainer>("BranchListContainer");
            NullChecker.Check(_titleLabel, _simplifyToggle, _presetContainer, _openGestureTableWindowButton, _branchListContainer);

            // Initialize elements
            _simplifyToggle.value = _branchListElement.IsSimplified;

            // Add event handlers
            _simplifyToggle.RegisterValueChangedCallback(OnSimplifyValueChanged);
            Observable.FromEvent(x => _presetContainer.onGUIHandler += x, x => _presetContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => OnBranchPresetGUI(_presetContainer.contentRect)).AddTo(_disposables);
            Observable.FromEvent(x => _openGestureTableWindowButton.clicked += x, x => _openGestureTableWindowButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnOpenGestureTableWindowButtonClicked()).AddTo(_disposables);
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

        public float GetWidth()
        {
            if (_branchListElement != null) { return _branchListElement.GetWidth(); }
            else { return 100; }
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
            _simplifyToggle.text = localizationTable.BranchListView_Simplify;
            _openGestureTableWindowButton.text = localizationTable.BranchListView_OpenGestureTable;
            _titleLabel.text = localizationTable.BranchListView_Title;
        }

        private void OnSimplifyValueChanged(ChangeEvent<bool> changeEvent)
        {
            _branchListElement.IsSimplified = changeEvent.newValue;
        }

        private void OnOpenGestureTableWindowButtonClicked()
        {
            _subWindowProvider.Provide<GestureTableWindow>()?.Focus();
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

        private void OnBranchPresetGUI(Rect rect)
        {
            var menu = _branchListElement.Menu;
            var modeId = _branchListElement.SelectedModeId;
            var enabled = true;
            if (menu is null || string.IsNullOrEmpty(modeId) || !menu.ContainsMode(modeId))
            {
                enabled = false;
            }

            var presets = new[]
            {
                _localizationTable.BranchListView_Preset_LeftOnly,
                _localizationTable.BranchListView_Preset_RightOnly,
                _localizationTable.BranchListView_Preset_LeftPriority,
                _localizationTable.BranchListView_Preset_RightPriority,
                _localizationTable.BranchListView_Preset_Combination,
            };

            using (new EditorGUI.DisabledScope(!enabled))
            using (new EditorGUILayout.HorizontalScope())
            {
                // Select preset
                GUILayout.Label(_localizationTable.BranchListView_Preset);
                _selectedPresetIndex = EditorGUILayout.Popup(_selectedPresetIndex, presets);
                if (GUILayout.Button(_localizationTable.Common_Add, GUILayout.Width(50)))
                {
                    if (_selectedPresetIndex < 0 || _selectedPresetIndex >= presets.Length)
                    {
                        EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.BranchListView_Message_InvalidPreset, "OK");
                        return;
                    }

                    // Add preset
                    var preset = presets[_selectedPresetIndex];
                    if (EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.BranchListView_Message_AddPreset + "\n" + preset,
                        _localizationTable.Common_Yes, _localizationTable.Common_No))
                    {
                        var gestures = new[]
                        {
                            HandGesture.Neutral,
                            HandGesture.Fist,
                            HandGesture.HandOpen,
                            HandGesture.Fingerpoint,
                            HandGesture.Victory,
                            HandGesture.RockNRoll,
                            HandGesture.HandGun,
                            HandGesture.ThumbsUp,
                        };
                        // Left only
                        if (preset == _localizationTable.BranchListView_Preset_LeftOnly)
                        {
                            var branches = new List<Condition[]>();
                            foreach (var gesture in gestures)
                            {
                                if (gesture == HandGesture.Neutral) { continue; }
                                branches.Add(new[] { new Condition(Hand.Left, gesture, ComparisonOperator.Equals) });
                            }
                            _addMultipleBranchesUseCase.Handle("", modeId, branches);
                        }
                        // Right only
                        else if (preset == _localizationTable.BranchListView_Preset_RightOnly)
                        {
                            var branches = new List<Condition[]>();
                            foreach (var gesture in gestures)
                            {
                                if (gesture == HandGesture.Neutral) { continue; }
                                branches.Add(new[] { new Condition(Hand.Right, gesture, ComparisonOperator.Equals) });
                            }
                            _addMultipleBranchesUseCase.Handle("", modeId, branches);
                        }
                        // Left priority
                        else if (preset == _localizationTable.BranchListView_Preset_LeftPriority)
                        {
                            var branches = new List<Condition[]>();
                            foreach (var gesture in gestures)
                            {
                                if (gesture == HandGesture.Neutral) { continue; }
                                branches.Add(new[] { new Condition(Hand.Left, gesture, ComparisonOperator.Equals) });
                            }
                            foreach (var gesture in gestures)
                            {
                                if (gesture == HandGesture.Neutral) { continue; }
                                branches.Add(new[] { new Condition(Hand.Right, gesture, ComparisonOperator.Equals) });
                            }
                            _addMultipleBranchesUseCase.Handle("", modeId, branches);
                        }
                        // Right priority
                        else if (preset == _localizationTable.BranchListView_Preset_RightPriority)
                        {
                            var branches = new List<Condition[]>();
                            foreach (var gesture in gestures)
                            {
                                if (gesture == HandGesture.Neutral) { continue; }
                                branches.Add(new[] { new Condition(Hand.Right, gesture, ComparisonOperator.Equals) });
                            }
                            foreach (var gesture in gestures)
                            {
                                if (gesture == HandGesture.Neutral) { continue; }
                                branches.Add(new[] { new Condition(Hand.Left, gesture, ComparisonOperator.Equals) });
                            }
                            _addMultipleBranchesUseCase.Handle("", modeId, branches);
                        }
                        // Combination
                        else if (preset == _localizationTable.BranchListView_Preset_Combination)
                        {
                            var branches = new List<Condition[]>();
                            foreach (var left in gestures)
                            {
                                foreach (var right in gestures)
                                {
                                    if (left == HandGesture.Neutral && right == HandGesture.Neutral) { continue; }
                                    branches.Add(new[]
                                    {
                                        new Condition(Hand.Left, left, ComparisonOperator.Equals),
                                        new Condition(Hand.Right, right, ComparisonOperator.Equals),
                                    });
                                }
                            }
                            _addMultipleBranchesUseCase.Handle("", modeId, branches);
                        }
                        // Invalid preset
                        else
                        {
                            EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.BranchListView_Message_InvalidPreset, "OK");
                            return;
                        }
                    }
                }
            }
        }
    }
}
