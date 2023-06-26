using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Detail.View.Element;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniRx;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using System.Linq;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class GestureTableView : IDisposable
    {
        private IAddBranchUseCase _addBranchUseCase;
        private ISetExistingAnimationUseCase _setExistingAnimationUseCase;

        private IReadOnlyLocalizationSetting _localizationSetting;
        private ISubWindowProvider _subWindowProvider;
        private DefaultsProviderGenerator _defaultProviderGenerator;
        private UpdateMenuSubject _updateMenuSubject;
        private SelectionSynchronizer _selectionSynchronizer;
        private GestureTableThumbnailDrawer _thumbnailDrawer;
        private SerializedObject _thumbnailSetting;

        private GestureTableElement _gestureTableElement;
        private AnimationElement _animationElement;
        private AV3.ExpressionEditor _expressionEditor;

        private IMGUIContainer _gestureTableContainer;
        private Label _thumbnailWidthLabel;
        private Label _thumbnailHeightLabel;
        private SliderInt _thumbnailWidthSlider;
        private SliderInt _thumbnailHeightSlider;

        private StyleColor _canAddButtonColor = Color.black;
        private StyleColor _canAddButtonBackgroundColor = Color.yellow;
        private StyleColor _canNotAddButtonColor;
        private StyleColor _canNotAddButtonBackgroundColor;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public GestureTableView(
            IAddBranchUseCase addBranchUseCase,
            ISetExistingAnimationUseCase setExistingAnimationUseCase,
            IReadOnlyLocalizationSetting localizationSetting,
            ISubWindowProvider subWindowProvider,
            DefaultsProviderGenerator defaultProviderGenerator,
            UpdateMenuSubject updateMenuSubject,
            SelectionSynchronizer selectionSynchronizer,
            GestureTableThumbnailDrawer thumbnailDrawer,
            GestureTableElement gestureTableElement,
            AnimationElement animationElement,
            AV3.ExpressionEditor expressionEditor,
            ThumbnailSetting thumbnailSetting)
        {
            // Usecases
            _addBranchUseCase = addBranchUseCase;
            _setExistingAnimationUseCase = setExistingAnimationUseCase;

            // Others
            _localizationSetting = localizationSetting;
            _subWindowProvider = subWindowProvider;
            _defaultProviderGenerator = defaultProviderGenerator;
            _updateMenuSubject = updateMenuSubject;
            _selectionSynchronizer = selectionSynchronizer;
            _thumbnailDrawer = thumbnailDrawer;
            _gestureTableElement = gestureTableElement;
            _animationElement = animationElement;
            _expressionEditor = expressionEditor;
            _thumbnailSetting = new SerializedObject(thumbnailSetting);

            // Gesture table element
            _gestureTableElement.AddTo(_disposables);
            _gestureTableElement.OnSelectionChanged.Synchronize().Subscribe(OnSelectionChanged).AddTo(_disposables);
            _gestureTableElement.OnBranchIndexExceeded.Synchronize().Subscribe(_ => OnBranchIndexExceeded()).AddTo(_disposables);
            _gestureTableElement.OnAddBrandchButtonClicked.Synchronize().Subscribe(OnAddBranchButtonClicked).AddTo(_disposables);
            _gestureTableElement.OnEditClipButtonClicked.Synchronize().Subscribe(OnEditClipButtonClicked).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(x => OnMenuUpdated(x.menu)).AddTo(_disposables);

            // Synchronize selection event handler
            _selectionSynchronizer.OnSynchronizeSelection.Synchronize().Subscribe(OnSynchronizeSelection).AddTo(_disposables);
        }

        public void Dispose()
        {
            _thumbnailWidthSlider.UnregisterValueChangedCallback(OnThumbnailSizeChanged);
            _thumbnailHeightSlider.UnregisterValueChangedCallback(OnThumbnailSizeChanged);
            _disposables.Dispose();
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
            _thumbnailWidthLabel = root.Q<Label>("ThumbnailWidthLabel");
            _thumbnailHeightLabel = root.Q<Label>("ThumbnailHeightLabel");
            _thumbnailWidthSlider = root.Q<SliderInt>("ThumbnailWidthSlider");
            _thumbnailHeightSlider = root.Q<SliderInt>("ThumbnailHeightSlider");
            NullChecker.Check(_gestureTableContainer, _thumbnailWidthLabel, _thumbnailHeightLabel, _thumbnailWidthSlider, _thumbnailHeightSlider);

            // Add event handlers
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
            _thumbnailSetting.Update();

            _thumbnailWidthSlider.bindingPath = nameof(ThumbnailSetting.GestureTable_Width);
            _thumbnailWidthSlider.BindProperty(_thumbnailSetting);
            _thumbnailWidthSlider.lowValue = ThumbnailSetting.GestureTable_MinWidth;
            _thumbnailWidthSlider.highValue = ThumbnailSetting.GestureTable_MaxWidth;
            _thumbnailWidthSlider.value = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.GestureTable_Width)).intValue;

            _thumbnailHeightSlider.bindingPath = nameof(ThumbnailSetting.GestureTable_Height);
            _thumbnailHeightSlider.BindProperty(_thumbnailSetting);
            _thumbnailHeightSlider.lowValue = ThumbnailSetting.GestureTable_MinHeight;
            _thumbnailHeightSlider.highValue = ThumbnailSetting.GestureTable_MaxHeight;
            _thumbnailHeightSlider.value = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.GestureTable_Height)).intValue;

            // Add event handlers
            _thumbnailWidthSlider.RegisterValueChangedCallback(OnThumbnailSizeChanged);
            _thumbnailHeightSlider.RegisterValueChangedCallback(OnThumbnailSizeChanged);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            if (_thumbnailWidthLabel != null) { _thumbnailWidthLabel.text = localizationTable.Common_ThumbnailWidth; }
            if (_thumbnailHeightLabel != null) { _thumbnailHeightLabel.text = localizationTable.Common_ThumbnailHeight; }
        }

        private void OnMenuUpdated(IMenu menu)
        {
            _gestureTableElement.Setup(menu);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            _gestureTableContainer?.MarkDirtyRepaint();
        }

        private void OnSelectionChanged((HandGesture left, HandGesture right)? args)
        {
            if (!(args is null))
            {
                _selectionSynchronizer.ChangeGestureTableViewSelection(args.Value.left, args.Value.right);
            }
        }

        private void OnBranchIndexExceeded()
        {
            var menu = _gestureTableElement.Menu;
            if (menu is null || !menu.ContainsMode(_gestureTableElement.SelectedModeId))
            {
                return;
            }

            var mode = menu.GetMode(_gestureTableElement.SelectedModeId);
            var lastBranchIndex = mode.Branches.Count - 1;
            _selectionSynchronizer.ChangeBranchListViewSelection(lastBranchIndex);
        }

        private void OnSynchronizeSelection(ViewSelection viewSelection)
        {
            _gestureTableElement?.ChangeSelection(viewSelection.MenuItemListView, viewSelection.BranchListView, viewSelection.GestureTableView);
            UpdateDisplay();
        }

        private void OnThumbnailSizeChanged(ChangeEvent<int> changeEvent)
        {
            // TODO: Reduce unnecessary redrawing
            _thumbnailDrawer.ClearCache();
        }

        private void OnAddBranchButtonClicked((HandGesture left, HandGesture right)? args)
        {
            if (!args.HasValue) { return; }

            var conditions = new[]
            {
                new Condition(Hand.Left, args.Value.left, ComparisonOperator.Equals),
                new Condition(Hand.Right, args.Value.right, ComparisonOperator.Equals),
            };
            _addBranchUseCase.Handle("", _gestureTableElement.SelectedModeId,
                conditions: conditions,
                order: 0,
                defaultsProvider: _defaultProviderGenerator.Generate());
        }

        private void OnEditClipButtonClicked((HandGesture left, HandGesture right)? args)
        {
            if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationSetting.GetCurrentLocaleTable().Common_Message_NotPossibleInPlayMode, "OK"); return; }
            else if (!args.HasValue) { return; }
            else if (_gestureTableElement?.Menu?.ContainsMode(_gestureTableElement?.SelectedModeId) != true) { return; }

            var mode = _gestureTableElement.Menu.GetMode(_gestureTableElement.SelectedModeId);
            var selectedBranch = mode.GetGestureCell(args.Value.left, args.Value.right);
            if (selectedBranch == null) { return; }

            for (int branchIndex = 0; branchIndex < mode.Branches.Count; branchIndex++)
            {
                if (ReferenceEquals(selectedBranch, mode.Branches[branchIndex]))
                {
                    CreateAndOpenClip(branchIndex);
                    break;
                }
            }
        }

        private void CreateAndOpenClip(int branchIndex)
        {
            var modeId = _gestureTableElement.SelectedModeId;
            var mode = _gestureTableElement.Menu.GetMode(modeId);
            var animation = mode.Branches[branchIndex].BaseAnimation;
            var path = AssetDatabase.GUIDToAssetPath(animation?.GUID);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            var clipExists = clip != null;
            if (clipExists)
            {
                _expressionEditor.Open(clip);
            }
            else
            {
                var guid = _animationElement.GetAnimationGuidWithDialog(AnimationElement.DialogMode.Create, path, defaultClipName: null);
                if (!string.IsNullOrEmpty(guid))
                {
                    _expressionEditor.Open(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)));
                    _setExistingAnimationUseCase.Handle(string.Empty, new Domain.Animation(guid), modeId, branchIndex, BranchAnimationType.Base);
                }
            }
        }
    }
}
