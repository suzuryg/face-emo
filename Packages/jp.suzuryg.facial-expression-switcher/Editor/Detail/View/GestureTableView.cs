using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.Subject;
using Suzuryg.FacialExpressionSwitcher.Detail.View.Element;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniRx;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class GestureTableView : IDisposable
    {
        private IAddBranchUseCase _addBranchUseCase;

        // TODO: 他のViewで使用しているPresenterに対応する
        private IAddBranchPresenter _addBranchPresenter;

        private IReadOnlyLocalizationSetting _localizationSetting;
        private UpdateMenuSubject _updateMenuSubject;
        private ChangeMenuItemListSelectionSubject _changeMenuItemListSelectionSubject;
        private ChangeBranchSelectionSubject _changeBranchSelectionSubject;
        private ThumbnailDrawer _thumbnailDrawer;

        private GestureTableElement _gestureTableElement;

        private IMGUIContainer _gestureTableContainer;
        private SliderInt _thumbnailSizeSlider;
        private Button _addBranchButton;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public GestureTableView(
            IAddBranchUseCase addBranchUseCase,

            IAddBranchPresenter addBranchPresenter,

            IReadOnlyLocalizationSetting localizationSetting,
            UpdateMenuSubject updateMenuSubject,
            ChangeMenuItemListSelectionSubject changeMenuItemListSelectionSubject,
            ChangeBranchSelectionSubject changeBranchSelectionSubject,
            ThumbnailDrawer thumbnailDrawer)
        {
            // Usecases
            _addBranchUseCase = addBranchUseCase;

            // Presenters
            _addBranchPresenter = addBranchPresenter;

            // Others
            _localizationSetting = localizationSetting;
            _updateMenuSubject = updateMenuSubject;
            _changeMenuItemListSelectionSubject = changeMenuItemListSelectionSubject;
            _changeBranchSelectionSubject = changeBranchSelectionSubject;
            _thumbnailDrawer = thumbnailDrawer;

            // Gesture table element
            _gestureTableElement = new GestureTableElement(_localizationSetting, _thumbnailDrawer).AddTo(_disposables);
            _gestureTableElement.OnBranchSelected.Synchronize().Subscribe(OnGestureTableViewSelectionChanged).AddTo(_disposables);
            _gestureTableElement.CanAddBranch.Synchronize().Subscribe(canAddBranch => _addBranchButton?.SetEnabled(canAddBranch)).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(OnMenuUpdated).AddTo(_disposables);

            // Change menu item list event handler
            _changeMenuItemListSelectionSubject.Observable.Synchronize().Subscribe(OnMenuItemListSelectionChanged).AddTo(_disposables);

            // Change branch selection event handler
            _changeBranchSelectionSubject.Observable.Synchronize().Subscribe(OnBranchListViewSelectionChanged).AddTo(_disposables);

            // Presenter event handlers
            _addBranchPresenter.Observable.Synchronize().Subscribe(OnAddBranchPresenterCompleted).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _thumbnailSizeSlider.UnregisterValueChangedCallback(OnThumbnailSizeChanged);
        }

        public void Initialize(VisualElement root)
        {
            // Load UXML and style
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(GestureTableView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(GestureTableView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            // Query Elements
            _gestureTableContainer = root.Q<IMGUIContainer>("GestureTableContainer");
            _thumbnailSizeSlider = root.Q<SliderInt>("ThumbnailSizeSlider");
            _addBranchButton = root.Q<Button>("AddBranchButton");
            NullChecker.Check(_gestureTableContainer, _thumbnailSizeSlider, _addBranchButton);

            // Add event handlers
            Observable.FromEvent(x => _addBranchButton.clicked += x, x => _addBranchButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnAddBranchButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _gestureTableContainer.onGUIHandler += x, x => _gestureTableContainer.onGUIHandler -= x)
                .Synchronize().Subscribe(_ =>
                {
                    _gestureTableElement?.OnGUI(_gestureTableContainer.contentRect);

                    // To draw gesture cell selection
                    if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDown)
                    {
                        _gestureTableContainer.MarkDirtyRepaint();
                    }
                }).AddTo(_disposables);

            // Initialize fields
            _thumbnailSizeSlider.lowValue = DetailConstants.MinGestureThumbnailSize;
            _thumbnailSizeSlider.highValue = DetailConstants.MaxGestureThumbnailSize;
            _thumbnailSizeSlider.value = EditorPrefs.HasKey(DetailConstants.KeyGestureThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyGestureThumbnailSize) : DetailConstants.MinGestureThumbnailSize;
            _addBranchButton.SetEnabled(_gestureTableElement.CanAddBranch.Value);

            // Add event handlers
            _thumbnailSizeSlider.RegisterValueChangedCallback(OnThumbnailSizeChanged);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _addBranchButton.text = localizationTable.GestureTableView_AddBranch;
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _gestureTableElement.Setup(menu);
        }

        private void OnMenuItemListSelectionChanged(IReadOnlyList<string> selectedMenuItemIds)
        {
            if (selectedMenuItemIds.Count == 1)
            {
                _gestureTableElement.ChangeModeSelection(selectedMenuItemIds[0]);
                _gestureTableContainer?.MarkDirtyRepaint();
            }
        }

        private void OnBranchListViewSelectionChanged(int branchIndex)
        {
            _gestureTableElement.ChangeBranchSelection(branchIndex);
            _gestureTableContainer?.MarkDirtyRepaint();
        }

        private void OnGestureTableViewSelectionChanged(int branchIndex)
        {
            _changeBranchSelectionSubject.OnNext(branchIndex);
        }

        private void OnThumbnailSizeChanged(ChangeEvent<int> changeEvent)
        {
            EditorPrefs.SetInt(DetailConstants.KeyGestureThumbnailSize, changeEvent.newValue);
            _thumbnailDrawer.UpdateAll();
        }

        private void OnAddBranchButtonClicked()
        {
            var conditions = new[]
            {
                new Condition(Hand.Left, _gestureTableElement.TargetLeftHand, ComparisonOperator.Equals),
                new Condition(Hand.Right, _gestureTableElement.TargetRightHand, ComparisonOperator.Equals),
            };
            _addBranchUseCase.Handle("", _gestureTableElement.SelectedModeId, conditions);
        }

        private void OnAddBranchPresenterCompleted(
            (AddBranchResult addBranchResult, IMenu menu, string errorMessage) args)
        {
            if (args.addBranchResult == AddBranchResult.Succeeded)
            {
                _gestureTableElement.Setup(args.menu);
            }
        }
    }
}
