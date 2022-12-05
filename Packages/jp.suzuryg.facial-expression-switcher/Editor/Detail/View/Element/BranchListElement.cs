using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View.Element
{
    public class BranchListElement : IDisposable
    {
        private static readonly int ScrollBottomMargin = 10;
        private static readonly int ScrollRightMargin = 5;
        private static readonly int Padding = 10;
        private static readonly int VerticalMargin = 5;
        private static readonly int MinHorizontalMargin = 5;
        private static readonly int PropertiesWidth = 150;
        private static readonly int ToggleWidth = 15;
        private static readonly int MinHeight = 100;
        private static readonly Color ActiveElementColor = new Color(0f, 0.5f, 1f, 0.4f);
        private static readonly Color FocusedElementColor = new Color(0f, 0.5f, 1f, 0.4f);

        public IMenu Menu { get; private set; }

        public IObservable<(
            string clipGUID,
            string modeId,
            int branchIndex,
            BranchAnimationType branchAnimationType)> OnAnimationChanged => _onAnimationChanged.AsObservable();

        public IObservable<string> OnAddBranchButtonClicked => _onAddBranchButtonClicked.AsObservable();
        public IObservable<(string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl,
            MouthTrackingControl? mouthTrackingControl,
            bool? isLeftTriggerUsed,
            bool? isRightTriggerUsed)> OnModifyBranchPropertiesButtonClicked => _onModifyBranchPropertiesButtonClicked.AsObservable();
        public IObservable<(string modeId, int from, int to)> OnBranchOrderChanged => _onBranchOrderChanged.AsObservable();
        public IObservable<(string modeId, int branchIndex)> OnRemoveBranchButtonClicked => _onRemoveBranchButtonClicked.AsObservable();

        public IObservable<(string modeId, int branchIndex, Condition condition)> OnAddConditionButtonClicked => _onAddConditionButtonClicked.AsObservable();
        public IObservable<(string modeId, int branchIndex, int conditionIndex, Condition condition)> OnModifyConditionButtonClicked => _onModifyConditionButtonClicked.AsObservable();
        public IObservable<(string modeId, int branchIndex, int from, int to)> OnConditionOrderChanged => _onConditionOrderChanged.AsObservable();
        public IObservable<(string modeId, int branchIndex, int conditionIndex)> OnRemoveConditionButtonClicked => _onRemoveConditionButtonClicked.AsObservable();
        public IObservable<int> OnBranchSelectionChanged => _onBranchSelectionChanged.AsObservable();

        private Subject<(
            string clipGUID,
            string modeId,
            int branchIndex,
            BranchAnimationType branchAnimationType)> _onAnimationChanged = new Subject<(string clipGUID, string modeId, int branchIndex, BranchAnimationType branchAnimationType)>();

        private Subject<string> _onAddBranchButtonClicked = new Subject<string>();
        private Subject<(string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl,
            MouthTrackingControl? mouthTrackingControl,
            bool? isLeftTriggerUsed,
            bool? isRightTriggerUsed)> _onModifyBranchPropertiesButtonClicked = new Subject<(string modeId, int branchIndex, EyeTrackingControl? eyeTrackingControl, MouthTrackingControl? mouthTrackingControl, bool? isLeftTriggerUsed, bool? isRightTriggerUsed)>();
        private Subject<(string modeId, int from, int to)> _onBranchOrderChanged = new Subject<(string modeId, int from, int to)>();
        private Subject<(string modeId, int branchIndex)> _onRemoveBranchButtonClicked = new Subject<(string modeId, int branchIndex)>(); 

        private Subject<(string modeId, int branchIndex, Condition condition)> _onAddConditionButtonClicked = new Subject<(string modeId, int branchIndex, Condition condition)>();
        private Subject<(string modeId, int branchIndex, int conditionIndex, Condition condition)> _onModifyConditionButtonClicked = new Subject<(string modeId, int branchIndex, int conditionIndex, Condition condition)>();
        private Subject<(string modeId, int branchIndex, int from, int to)> _onConditionOrderChanged = new Subject<(string modeId, int branchIndex, int from, int to)>();
        private Subject<(string modeId, int branchIndex, int conditionIndex)> _onRemoveConditionButtonClicked = new Subject<(string modeId, int branchIndex, int conditionIndex)>();
        private Subject<int> _onBranchSelectionChanged = new Subject<int>();

        private ThumbnailDrawer _thumbnailDrawer;
        private IReadOnlyLocalizationSetting _localizationSetting;

        private ReorderableList _reorderableList;
        private List<ConditionListElement> _conditionListElements = new List<ConditionListElement>();
        private string _selectedModeId;
        private Vector2 _scrollPosition = Vector2.zero;
        private Texture2D _activeBackgroundTexture;
        private Texture2D _focusedBackgroundTexture;

        private string _blinkingText;
        private string _lipSyncText;
        private string _enableText;
        private string _disableText;
        private string _emptyText;
        private string _useLeftTriggerText;
        private string _useRightTriggerText;
        private string _notReachableBranchText;
        private string _leftTriggerAnimationText;
        private string _rightTriggerAnimationText;
        private string _bothTriggersAnimationText;

        private GUIStyle _centerStyle;
        private GUIStyle _warningStyle;

        private CompositeDisposable _disposables = new CompositeDisposable();
        private CompositeDisposable _conditionDisposables = new CompositeDisposable();

        public BranchListElement(
            IReadOnlyLocalizationSetting localizationSetting,
            ThumbnailDrawer thumbnailDrawer)
        {
            // Dependencies
            _localizationSetting = localizationSetting;
            _thumbnailDrawer = thumbnailDrawer;

            // Reorderable List
            _reorderableList = new ReorderableList(new List<IBranch>(), typeof(IBranch));
            _reorderableList.headerHeight = 0;
            _reorderableList.drawElementCallback = DrawElement;
            _reorderableList.drawElementBackgroundCallback = DrawBackground;
            _reorderableList.drawNoneElementCallback = DrawEmpty;
            _reorderableList.onAddCallback = OnElementAdded;
            _reorderableList.onRemoveCallback = OnElementRemoved;
            _reorderableList.elementHeightCallback = GetElementHeight;
            _reorderableList.onReorderCallbackWithDetails = OnElementOrderChanged;
            _reorderableList.onSelectCallback = OnElementSelectionChanged;

            // Styles
            try
            {
                _centerStyle = new GUIStyle(EditorStyles.label);
                _warningStyle = new GUIStyle(EditorStyles.label);
            }
            catch (NullReferenceException)
            {
                // Workaround for play mode
                _centerStyle = new GUIStyle();
                _warningStyle = new GUIStyle();
            }
            _centerStyle.alignment = TextAnchor.MiddleCenter;
            _warningStyle.normal.textColor = Color.red;

            // Textures
            _activeBackgroundTexture = new Texture2D(1, 1);
            _activeBackgroundTexture.SetPixel(0, 0, ActiveElementColor);
            _activeBackgroundTexture.Apply();

            _focusedBackgroundTexture = new Texture2D(1, 1);
            _focusedBackgroundTexture.SetPixel(0, 0, FocusedElementColor);
            _focusedBackgroundTexture.Apply();

            // Set text
            SetText(localizationSetting.Table);
            localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _conditionDisposables.Dispose();
        }

        public void OnGUI(Rect rect)
        {
            float totalHeight = 0;
            for (int i = 0; i < _reorderableList.list.Count; i++)
            {
                totalHeight += GetElementHeight(i);
            }
            var viewRect = new Rect(rect.x, rect.y,
                rect.width - EditorGUIUtility.singleLineHeight,
                totalHeight + EditorGUIUtility.singleLineHeight + ScrollBottomMargin);

            using (var scope = new GUI.ScrollViewScope(rect, _scrollPosition, viewRect))
            {
                _reorderableList?.DoList(rect);
                _scrollPosition = scope.scrollPosition;
            }
        }

        public void Setup(IMenu menu)
        {
            Menu = menu;
            UpdateList();
        }

        public void ChangeModeSelection(string modeId)
        {
            _selectedModeId = modeId;
            UpdateList();
            ChangeBranchSelection(-1);
        }

        public void ChangeBranchSelection(int branchIndex)
        {
            if (branchIndex < _reorderableList.count)
            {
                _reorderableList.index = branchIndex;
            }
            else
            {
                try
                {
                    _reorderableList.index = -1;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Ignore exception in order to select none.
                }
            }
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _blinkingText = localizationTable.MenuItemListView_Blinking;
            _lipSyncText = localizationTable.MenuItemListView_LipSync;
            _enableText = localizationTable.MenuItemListView_Enable;
            _disableText = localizationTable.MenuItemListView_Disable;
            _emptyText = localizationTable.BranchListView_EmptyBranch;
            _useLeftTriggerText = localizationTable.BranchListView_UseLeftTrigger;
            _useRightTriggerText = localizationTable.BranchListView_UseRightTrigger;
            _notReachableBranchText = localizationTable.BranchListView_NotReachableBranch;
            _leftTriggerAnimationText = localizationTable.BranchListView_LeftTriggerAnimation;
            _rightTriggerAnimationText = localizationTable.BranchListView_RightTriggerAnimation;
            _bothTriggersAnimationText = localizationTable.BranchListView_BothTriggersAnimation;
        }

        private void UpdateList()
        {
            List<IBranch> branches = new List<IBranch>();
            if (Menu is IMenu && Menu.ContainsMode(_selectedModeId))
            {
                branches = Menu.GetMode(_selectedModeId).Branches.ToList();
            }

            _reorderableList.list = branches;

            _conditionDisposables.Dispose();
            _conditionDisposables = new CompositeDisposable();
            _conditionListElements.Clear();
            for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
            {
                var branch = branches[branchIndex];
                var conditionElement = new ConditionListElement(branchIndex, branch.Conditions, _localizationSetting).AddTo(_conditionDisposables);
                conditionElement.OnAddConditionButtonClicked.Subscribe(x => _onAddConditionButtonClicked.OnNext((_selectedModeId, x.branchIndex, x.condition))).AddTo(_conditionDisposables);
                conditionElement.OnModifyConditionButtonClicked.Subscribe(x => _onModifyConditionButtonClicked.OnNext((_selectedModeId, x.branchIndex, x.conditionIndex, x.condition))).AddTo(_conditionDisposables);
                conditionElement.OnConditionOrderChanged.Subscribe(x => _onConditionOrderChanged.OnNext((_selectedModeId, x.branchIndex, x.from, x.to))).AddTo(_conditionDisposables);
                conditionElement.OnRemoveConditionButtonClicked.Subscribe(x => _onRemoveConditionButtonClicked.OnNext((_selectedModeId, x.branchIndex, x.conditionIndex))).AddTo(_conditionDisposables);
                _conditionListElements.Add(conditionElement);
            }
        }

        private static float GetUpperContentWidth()
        {
            return ConditionListElement.GetWidth() + PropertiesWidth + AnimationElement.GetWidth();
        }

        private static float GetLowerContentWidth()
        {
            return AnimationElement.GetWidth() * 3;
        }

        private static float GetUpperHorizontalMargin()
        {
            var upper = GetUpperContentWidth();
            var lower = GetLowerContentWidth();
            var diff = Math.Max(lower - upper, MinHorizontalMargin * 2);
            var margin = Math.Max(diff / 2, MinHorizontalMargin);
            return margin;
        }

        private static float GetLowerHorizontalMargin()
        {
            var upper = GetUpperContentWidth();
            var lower = GetLowerContentWidth();
            var diff = Math.Max(upper - lower, MinHorizontalMargin * 2);
            var margin = Math.Max(diff / 2, MinHorizontalMargin);
            return margin;
        }

        private float GetWidth()
        {
            return Padding + Math.Max(GetUpperContentWidth(), GetLowerContentWidth()) + MinHorizontalMargin * 2 + Padding;
        }

        private float GetElementHeight(int index)
        {
            if (Menu is null || !Menu.ContainsMode(_selectedModeId))
            {
                return MinHeight;
            }

            var mode = Menu.GetMode(_selectedModeId);

            if (mode.Branches.Count <= index || _conditionListElements.Count <= index)
            {
                return MinHeight;
            }

            var branch = mode.Branches[index];
            var animationHeight = AnimationElement.GetHeight();

            var height = Padding + Math.Max(MinHeight, Math.Max(ConditionListElement.GetMinHeight(), animationHeight)) + Padding;

            var isTriggerUsed = (branch.CanLeftTriggerUsed && branch.IsLeftTriggerUsed) || (branch.CanRightTriggerUsed && branch.IsRightTriggerUsed);
            if (isTriggerUsed)
            {
                height += VerticalMargin + animationHeight + EditorGUIUtility.singleLineHeight;
            }

            return height;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Menu is null || !Menu.ContainsMode(_selectedModeId))
            {
                return;
            }

            var mode = Menu.GetMode(_selectedModeId);

            if (mode.Branches.Count <= index || _conditionListElements.Count <= index)
            {
                return;
            }

            var branch = mode.Branches[index];
            var condition = _conditionListElements[index];

            var xBegin = rect.x + Padding;
            var yBegin = rect.y + Padding;
            var xCurrent = xBegin;
            var yCurrent = yBegin;

            var upperHorizontalMargin = GetUpperHorizontalMargin();
            var lowerHorizontalMargin = GetLowerHorizontalMargin();

            var centerStyle = new GUIStyle();
            centerStyle.alignment = TextAnchor.MiddleCenter;

            // Conditions
            var conditionHeight = Math.Max(ConditionListElement.GetMinHeight(), AnimationElement.GetHeight());
            condition.OnGUI(new Rect(xCurrent, yCurrent, ConditionListElement.GetWidth(), conditionHeight));

            xCurrent += ConditionListElement.GetWidth() + upperHorizontalMargin;

            // Blinking
            GUI.Label(new Rect(xCurrent, yCurrent, PropertiesWidth / 2, EditorGUIUtility.singleLineHeight), _blinkingText);
            var oldBlinking = branch.EyeTrackingControl == EyeTrackingControl.Tracking ? 0 : 1;
            var newBlinking = EditorGUI.Popup(new Rect(xCurrent + PropertiesWidth / 2, yCurrent, PropertiesWidth / 2, EditorGUIUtility.singleLineHeight),
                string.Empty, oldBlinking, new[] { _enableText, _disableText });
            if (newBlinking != oldBlinking)
            {
                if (newBlinking == 0)
                {
                    _onModifyBranchPropertiesButtonClicked.OnNext((_selectedModeId, index, EyeTrackingControl.Tracking, null, null, null));
                }
                else
                {
                    _onModifyBranchPropertiesButtonClicked.OnNext((_selectedModeId, index, EyeTrackingControl.Animation, null, null, null));
                }
            }

            yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;

            // Lip sync
            GUI.Label(new Rect(xCurrent, yCurrent, PropertiesWidth / 2, EditorGUIUtility.singleLineHeight), _lipSyncText);
            var oldLipSync = branch.MouthTrackingControl == MouthTrackingControl.Tracking ? 0 : 1;
            var newLipSync = EditorGUI.Popup(new Rect(xCurrent + PropertiesWidth / 2, yCurrent, PropertiesWidth / 2, EditorGUIUtility.singleLineHeight),
                string.Empty, oldLipSync, new[] { _enableText, _disableText });
            if (newLipSync != oldLipSync)
            {
                if (newLipSync == 0)
                {
                    _onModifyBranchPropertiesButtonClicked.OnNext((_selectedModeId, index, null, MouthTrackingControl.Tracking, null, null));
                }
                else
                {
                    _onModifyBranchPropertiesButtonClicked.OnNext((_selectedModeId, index, null, MouthTrackingControl.Animation, null, null));
                }
            }

            yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;

            // Is left trigger used
            using (new EditorGUI.DisabledScope(!branch.CanLeftTriggerUsed))
            {
                var useLeftTrigger = GUI.Toggle(new Rect(xCurrent, yCurrent, ToggleWidth, EditorGUIUtility.singleLineHeight), branch.IsLeftTriggerUsed, string.Empty);
                GUI.Label(new Rect(xCurrent + ToggleWidth, yCurrent, PropertiesWidth - ToggleWidth, EditorGUIUtility.singleLineHeight), _useLeftTriggerText);
                if (useLeftTrigger != branch.IsLeftTriggerUsed)
                {
                    _onModifyBranchPropertiesButtonClicked.OnNext((_selectedModeId, index, null, null, useLeftTrigger, null));
                }
            }

            yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;

            // Is right trigger used
            using (new EditorGUI.DisabledScope(!branch.CanRightTriggerUsed))
            {
                var useRightTrigger = GUI.Toggle(new Rect(xCurrent, yCurrent, ToggleWidth, EditorGUIUtility.singleLineHeight), branch.IsRightTriggerUsed, string.Empty);
                GUI.Label(new Rect(xCurrent + ToggleWidth, yCurrent, PropertiesWidth - ToggleWidth, EditorGUIUtility.singleLineHeight), _useRightTriggerText);
                if (useRightTrigger != branch.IsRightTriggerUsed)
                {
                    _onModifyBranchPropertiesButtonClicked.OnNext((_selectedModeId, index, null, null, null, useRightTrigger));
                }
            }

            yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;

            // Warning
            if (!branch.IsReachable)
            {
                GUI.Label(new Rect(xCurrent, yCurrent, PropertiesWidth, EditorGUIUtility.singleLineHeight), "⚠ " + _notReachableBranchText, _warningStyle);
            }

            xCurrent += PropertiesWidth + upperHorizontalMargin;
            yCurrent = yBegin;

            // Base animation
            var thumbnailSize = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailSize) : DetailConstants.MinMainThumbnailSize;
            AnimationElement.Draw(new Rect(xCurrent, yCurrent, thumbnailSize, thumbnailSize + EditorGUIUtility.singleLineHeight), branch.BaseAnimation, _thumbnailDrawer,
                newGUID => { return; },
                newGUID => { _onAnimationChanged.OnNext((newGUID, _selectedModeId, index, BranchAnimationType.Base)); },
                newGUID => { return; },
                () => { return; });

            xCurrent = xBegin;
            yCurrent += AnimationElement.GetHeight() + VerticalMargin;

            // Left trigger animation
            if (branch.CanLeftTriggerUsed && branch.IsLeftTriggerUsed)
            {
                AnimationElement.Draw(new Rect(xCurrent, yCurrent, thumbnailSize, thumbnailSize + EditorGUIUtility.singleLineHeight), branch.LeftHandAnimation, _thumbnailDrawer,
                    newGUID => { return; },
                    newGUID => { _onAnimationChanged.OnNext((newGUID, _selectedModeId, index, BranchAnimationType.Left)); },
                    newGUID => { return; },
                    () => { return; });
                GUI.Label(new Rect(xCurrent, yCurrent + AnimationElement.GetHeight(), AnimationElement.GetWidth(), EditorGUIUtility.singleLineHeight), _leftTriggerAnimationText, _centerStyle);
            }

            xCurrent += AnimationElement.GetWidth() + lowerHorizontalMargin;

            // Right trigger animation
            if (branch.CanRightTriggerUsed && branch.IsRightTriggerUsed)
            {
                AnimationElement.Draw(new Rect(xCurrent, yCurrent, thumbnailSize, thumbnailSize + EditorGUIUtility.singleLineHeight), branch.RightHandAnimation, _thumbnailDrawer,
                    newGUID => { return; },
                    newGUID => { _onAnimationChanged.OnNext((newGUID, _selectedModeId, index, BranchAnimationType.Right)); },
                    newGUID => { return; },
                    () => { return; });
                GUI.Label(new Rect(xCurrent, yCurrent + AnimationElement.GetHeight(), AnimationElement.GetWidth(), EditorGUIUtility.singleLineHeight), _rightTriggerAnimationText, _centerStyle);
            }

            xCurrent += AnimationElement.GetWidth() + lowerHorizontalMargin;

            // Both triggers animations
            if (branch.CanLeftTriggerUsed && branch.IsLeftTriggerUsed && branch.CanRightTriggerUsed && branch.IsRightTriggerUsed)
            {
                AnimationElement.Draw(new Rect(xCurrent, yCurrent, thumbnailSize, thumbnailSize + EditorGUIUtility.singleLineHeight), branch.BothHandsAnimation, _thumbnailDrawer,
                    newGUID => { return; },
                    newGUID => { _onAnimationChanged.OnNext((newGUID, _selectedModeId, index, BranchAnimationType.Both)); },
                    newGUID => { return; },
                    () => { return; });
                GUI.Label(new Rect(xCurrent, yCurrent + AnimationElement.GetHeight(), AnimationElement.GetWidth(), EditorGUIUtility.singleLineHeight), _bothTriggersAnimationText, _centerStyle);
            }
        }

        private void DrawBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (isFocused)
            {
                GUI.DrawTexture(rect, _focusedBackgroundTexture);
            }
            else if (isActive)
            {
                GUI.DrawTexture(rect, _activeBackgroundTexture);
            }
        }

        private void DrawEmpty(Rect rect)
        {
            GUI.Label(rect, _emptyText);
        }

        private void OnElementAdded(ReorderableList reorderableList)
        {
            _onAddBranchButtonClicked.OnNext(_selectedModeId);
        }

        private void OnElementRemoved(ReorderableList reorderableList)
        {
            _onRemoveBranchButtonClicked.OnNext((_selectedModeId, _reorderableList.index));
        }

        private void OnElementOrderChanged(ReorderableList reorderableList, int oldIndex, int newIndex)
        {
            _onBranchOrderChanged.OnNext((_selectedModeId, oldIndex, newIndex));
        }

        private void OnElementSelectionChanged(ReorderableList reorderableList)
        {
            _onBranchSelectionChanged.OnNext(_reorderableList.index);
        }
    }
}
