using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Data;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
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

namespace Suzuryg.FaceEmo.Detail.View.Element
{
    public class BranchListElement : IDisposable
    {
        private static readonly int ScrollBottomMargin = 10;
        // private static readonly int ScrollRightMargin = 5; // Not in use at this time
        private static readonly int Padding = 10;
        private static readonly int VerticalMargin = 5;
        private static readonly int MinHorizontalMargin = 5;
        private static readonly int PropertiesWidth = 150;
        private static readonly int ToggleWidth = 15;
        private static readonly int MinHeight = 100;
        private static readonly int SimplifiedHeight = 65;
        private static readonly int ReorderableListDragHandleWidth = 50;
        private static readonly Color ActiveElementColor = new Color(0f, 0.5f, 1f, 0.4f);
        private static readonly Color FocusedElementColor = new Color(0f, 0.5f, 1f, 0.4f);

        public IMenu Menu { get; private set; }
        public string SelectedModeId { get; private set; }
        public bool IsSimplified { get; set; } = false;

        public IObservable<(
            string clipGUID,
            string modeId,
            int branchIndex,
            BranchAnimationType branchAnimationType)> OnAnimationChanged => _onAnimationChanged.AsObservable();

        public IObservable<(string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl,
            MouthTrackingControl? mouthTrackingControl,
            bool? blinkEnabled,
            bool? mouthMorphCancelerEnabled,
            bool? isLeftTriggerUsed,
            bool? isRightTriggerUsed)> OnModifyBranchPropertiesButtonClicked => _onModifyBranchPropertiesButtonClicked.AsObservable();
        public IObservable<(string modeId, int from, int to)> OnBranchOrderChanged => _onBranchOrderChanged.AsObservable();

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

        private Subject<(string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl,
            MouthTrackingControl? mouthTrackingControl,
            bool? blinkEnabled,
            bool? mouthMorphCancelerEnabled,
            bool? isLeftTriggerUsed,
            bool? isRightTriggerUsed)> _onModifyBranchPropertiesButtonClicked = new Subject<(string modeId, int branchIndex, EyeTrackingControl? eyeTrackingControl, MouthTrackingControl? mouthTrackingControl, bool? blinkEnabled, bool? mouthMorphCancelerEnabled, bool? isLeftTriggerUsed, bool? isRightTriggerUsed)>();
        private Subject<(string modeId, int from, int to)> _onBranchOrderChanged = new Subject<(string modeId, int from, int to)>();

        private Subject<(string modeId, int branchIndex, Condition condition)> _onAddConditionButtonClicked = new Subject<(string modeId, int branchIndex, Condition condition)>();
        private Subject<(string modeId, int branchIndex, int conditionIndex, Condition condition)> _onModifyConditionButtonClicked = new Subject<(string modeId, int branchIndex, int conditionIndex, Condition condition)>();
        private Subject<(string modeId, int branchIndex, int from, int to)> _onConditionOrderChanged = new Subject<(string modeId, int branchIndex, int from, int to)>();
        private Subject<(string modeId, int branchIndex, int conditionIndex)> _onRemoveConditionButtonClicked = new Subject<(string modeId, int branchIndex, int conditionIndex)>();
        private Subject<int> _onBranchSelectionChanged = new Subject<int>();

        private AnimationElement _animationElement;
        private MainThumbnailDrawer _thumbnailDrawer;
        private AV3Setting _aV3Setting;
        private ThumbnailSetting _thumbnailSetting;
        private IReadOnlyLocalizationSetting _localizationSetting;
        private LocalizationTable _localizationTable;

        private ReorderableList _reorderableList;
        private List<ConditionListElement> _conditionListElements = new List<ConditionListElement>();
        private Vector2 _scrollPosition = Vector2.zero;
        private Texture2D _activeBackgroundTexture;
        private Texture2D _focusedBackgroundTexture;

        private string _eyeTrackingText;
        private string _mouthTrackingText;
        private string _blinkText;
        private string _mouthMorphCancelerText;
        private string _emptyText;
        private string _useLeftTriggerText;
        private string _useRightTriggerText;
        private string _notReachableBranchText;
        private string _leftTriggerAnimationText;
        private string _rightTriggerAnimationText;
        private string _bothTriggersAnimationText;

        private Texture2D _redTexture; // Store to avoid destruction
        private GUIStyle _centerStyle;
        private GUIStyle _centerUpperStyle;
        private GUIStyle _warningStyle;

        private CompositeDisposable _disposables = new CompositeDisposable();
        private CompositeDisposable _conditionDisposables = new CompositeDisposable();

        public BranchListElement(
            IReadOnlyLocalizationSetting localizationSetting,
            AV3Setting aV3Setting,
            ThumbnailSetting thumbnailSetting,
            AnimationElement animationElement,
            MainThumbnailDrawer thumbnailDrawer)
        {
            // Dependencies
            _localizationSetting = localizationSetting;
            _animationElement = animationElement;
            _thumbnailDrawer = thumbnailDrawer;
            _aV3Setting = aV3Setting;
            _thumbnailSetting = thumbnailSetting;

            // Reorderable List
            _reorderableList = new ReorderableList(new List<IBranch>(), typeof(IBranch));
            _reorderableList.headerHeight = 0;
            _reorderableList.displayAdd = false;
            _reorderableList.displayRemove = false;
            _reorderableList.drawElementCallback = DrawElement;
            _reorderableList.drawElementBackgroundCallback = DrawBackground;
            _reorderableList.drawNoneElementCallback = DrawEmpty;
            _reorderableList.elementHeightCallback = GetElementHeight;
            _reorderableList.onReorderCallbackWithDetails = OnElementOrderChanged;
            _reorderableList.onSelectCallback = OnElementSelectionChanged;

            // Styles
            try
            {
                _centerStyle = new GUIStyle(EditorStyles.label);
                _centerUpperStyle = new GUIStyle(EditorStyles.label);
                _warningStyle = new GUIStyle(EditorStyles.label);
            }
            catch (NullReferenceException)
            {
                // Workaround for play mode
                _centerStyle = new GUIStyle();
                _centerUpperStyle = new GUIStyle();
                _warningStyle = new GUIStyle();
            }
            _centerStyle.alignment = TextAnchor.MiddleCenter;
            _centerUpperStyle.alignment = TextAnchor.UpperCenter;

            if (EditorGUIUtility.isProSkin)
            {
                _warningStyle.normal.textColor = Color.red;
            }
            else
            {
                _redTexture = ViewUtility.MakeTexture(Color.red);
                _warningStyle.normal.background = _redTexture;
                _warningStyle.normal.textColor = Color.black;
            }

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
            // Update thumbnails
            if (!IsSimplified)
            {
                var animations = GetAnimations();
                foreach (var animation in animations)
                {
                    _thumbnailDrawer.GetThumbnail(animation);
                }
                _thumbnailDrawer.Update();
            }

            // Show Hints
            var hintRect = ShowHints();
            rect.y += hintRect.height;
            rect.height -= hintRect.height;

            // Draw list
            if (Menu is null || !Menu.ContainsMode(SelectedModeId))
            {
                GUI.Label(new Rect(rect.x + Padding, rect.y + Padding + EditorGUIUtility.singleLineHeight * 2,
                    rect.width - Padding * 2 , rect.height - Padding * 2),
                    _localizationTable.BranchListView_ModeIsNotSelected, _centerUpperStyle);
                return;
            }

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
            if (SelectedModeId != modeId)
            {
                SelectedModeId = modeId;
                UpdateList();
                ChangeBranchSelection(-1);
            }
        }

        public void ChangeBranchSelection(int branchIndex)
        {
            if (0 <=  branchIndex && branchIndex < _reorderableList.count)
            {
                _reorderableList.index = branchIndex;

                // Scroll to the selected branch.
                var yPosition = 0f;
                for (int i = 0; i < branchIndex; i++)
                {
                    yPosition += GetElementHeight(i);
                }
                _scrollPosition = new Vector2() { x = 0 , y = yPosition };
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

        public void SelectNewestBranch() => ChangeBranchSelection(_reorderableList.count - 1);

        public int GetSelectedBranchIndex() => _reorderableList.index;

        public int GetNumOfBranches() => _reorderableList.count;

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
            _eyeTrackingText = localizationTable.MenuItemListView_EyeTracking;
            _mouthTrackingText = localizationTable.MenuItemListView_MouthTracking;
            _blinkText = localizationTable.MenuItemListView_Blink;
            _mouthMorphCancelerText = localizationTable.MenuItemListView_MouthMorphCanceler;
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
            if (Menu is IMenu && Menu.ContainsMode(SelectedModeId))
            {
                branches = Menu.GetMode(SelectedModeId).Branches.ToList();
            }

            _reorderableList.index = Math.Min(_reorderableList.index, branches.Count  - 1);
            _reorderableList.list = branches;

            _conditionDisposables.Dispose();
            _conditionDisposables = new CompositeDisposable();
            _conditionListElements.Clear();
            for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
            {
                var branch = branches[branchIndex];
                var conditionElement = new ConditionListElement(branchIndex, branch.Conditions, _localizationSetting).AddTo(_conditionDisposables);
                conditionElement.OnAddConditionButtonClicked.Subscribe(x => _onAddConditionButtonClicked.OnNext((SelectedModeId, x.branchIndex, x.condition))).AddTo(_conditionDisposables);
                conditionElement.OnModifyConditionButtonClicked.Subscribe(x => _onModifyConditionButtonClicked.OnNext((SelectedModeId, x.branchIndex, x.conditionIndex, x.condition))).AddTo(_conditionDisposables);
                conditionElement.OnConditionOrderChanged.Subscribe(x => _onConditionOrderChanged.OnNext((SelectedModeId, x.branchIndex, x.from, x.to))).AddTo(_conditionDisposables);
                conditionElement.OnRemoveConditionButtonClicked.Subscribe(x => _onRemoveConditionButtonClicked.OnNext((SelectedModeId, x.branchIndex, x.conditionIndex))).AddTo(_conditionDisposables);
                _conditionListElements.Add(conditionElement);
            }
        }

        private float GetUpperContentWidth()
        {
            return ConditionListElement.GetWidth() + PropertiesWidth + _animationElement.GetWidth();
        }

        private float GetLowerContentWidth()
        {
            return _animationElement.GetWidth() * 3;
        }

        private float GetUpperHorizontalMargin()
        {
            var upper = GetUpperContentWidth();
            var lower = GetLowerContentWidth();
            var diff = Math.Max(lower - upper, MinHorizontalMargin * 2);
            var margin = Math.Max(diff / 2, MinHorizontalMargin);
            return margin;
        }

        private float GetLowerHorizontalMargin()
        {
            var upper = GetUpperContentWidth();
            var lower = GetLowerContentWidth();
            var diff = Math.Max(upper - lower, MinHorizontalMargin * 2);
            var margin = Math.Max(diff / 2, MinHorizontalMargin);
            return margin;
        }

        public float GetWidth()
        {
            return ReorderableListDragHandleWidth + Padding + Math.Max(GetUpperContentWidth(), GetLowerContentWidth()) + MinHorizontalMargin * 2 + Padding;
        }

        private float GetElementHeight(int index)
        {
            if (IsSimplified) { return SimplifiedHeight; }

            if (Menu is null || !Menu.ContainsMode(SelectedModeId))
            {
                return MinHeight;
            }

            var mode = Menu.GetMode(SelectedModeId);

            if (mode.Branches.Count <= index || _conditionListElements.Count <= index)
            {
                return MinHeight;
            }

            var branch = mode.Branches[index];
            var animationHeight = _animationElement.GetHeight();

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
            if (Menu is null || !Menu.ContainsMode(SelectedModeId))
            {
                return;
            }

            var mode = Menu.GetMode(SelectedModeId);

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

            var thumbnailWidth = _thumbnailSetting.Main_Width;
            var thumbnailHeight = _thumbnailSetting.Main_Height;

            var upperHorizontalMargin = GetUpperHorizontalMargin();
            var lowerHorizontalMargin = GetLowerHorizontalMargin();

            var centerStyle = new GUIStyle();
            centerStyle.alignment = TextAnchor.MiddleCenter;

            // Conditions
            var conditionHeight = Math.Max(ConditionListElement.GetMinHeight(), _animationElement.GetHeight());
            if (!IsSimplified)
            {
                conditionHeight -= EditorGUIUtility.singleLineHeight * 2 + VerticalMargin;
            }
            else
            {
                conditionHeight = SimplifiedHeight;
            }
            condition.OnGUI(new Rect(xCurrent, yCurrent, ConditionListElement.GetWidth(), conditionHeight));

            // Warning
            float xWarning, yWarning, widthWarning, heightWarning;
            if (!IsSimplified)
            {
                xWarning = xBegin;
                yWarning = yBegin + conditionHeight + VerticalMargin;
                widthWarning = ConditionListElement.GetWidth();
                heightWarning = EditorGUIUtility.singleLineHeight * 2;
            }
            else
            {
                xWarning = xBegin + ConditionListElement.GetWidth() + upperHorizontalMargin;
                yWarning = yBegin + EditorGUIUtility.singleLineHeight + 2;
                widthWarning = PropertiesWidth + upperHorizontalMargin + thumbnailWidth;
                heightWarning = EditorGUIUtility.singleLineHeight * 2;
            }
            if (!branch.IsReachable)
            {
                GUI.Label(new Rect(xWarning, yWarning, widthWarning, heightWarning),
                    "⚠ " + _notReachableBranchText + "\n" + _localizationTable.BranchListView_NotReachableBranchAction, _warningStyle);
            }

            xCurrent += ConditionListElement.GetWidth() + upperHorizontalMargin;
            yCurrent = yBegin;

            // Eye tracking
            if (!IsSimplified)
            {
                Func<EyeTrackingControl, bool> eyeToBool = (EyeTrackingControl eyeTrackingControl) => eyeTrackingControl == EyeTrackingControl.Tracking;
                Func<bool, EyeTrackingControl> boolToEye = (bool value) => value ? EyeTrackingControl.Tracking : EyeTrackingControl.Animation;

                var toggleRect = new Rect(xCurrent, yCurrent, ToggleWidth, EditorGUIUtility.singleLineHeight);
                // EditorGUI.LabelField(toggleRect, new GUIContent(string.Empty, _localizationTable.Common_Tooltip_EyeTracking));

                var eyeTracking = GUI.Toggle(toggleRect, eyeToBool(branch.EyeTrackingControl), string.Empty);
                GUI.Label(new Rect(xCurrent + ToggleWidth, yCurrent, PropertiesWidth - ToggleWidth, EditorGUIUtility.singleLineHeight),
                    new GUIContent(_eyeTrackingText, _localizationTable.Common_Tooltip_EyeTracking));
                if (eyeTracking != eyeToBool(branch.EyeTrackingControl))
                {
                    _onModifyBranchPropertiesButtonClicked.OnNext((SelectedModeId, index, boolToEye(eyeTracking), null, null, null, null, null));
                }

                yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;
            }

            // Blink
            if (!IsSimplified)
            {
                using (new EditorGUI.DisabledScope(!_aV3Setting.ReplaceBlink))
                {
                    var toggleRect = new Rect(xCurrent, yCurrent, ToggleWidth, EditorGUIUtility.singleLineHeight);
                    // EditorGUI.LabelField(toggleRect, new GUIContent(string.Empty, _localizationTable.Common_Tooltip_Blink));

                    var blink = GUI.Toggle(toggleRect, branch.BlinkEnabled, string.Empty);
                    GUI.Label(new Rect(xCurrent + ToggleWidth, yCurrent, PropertiesWidth - ToggleWidth, EditorGUIUtility.singleLineHeight),
                        new GUIContent(_blinkText, _localizationTable.Common_Tooltip_Blink));
                    if (blink != branch.BlinkEnabled)
                    {
                        _onModifyBranchPropertiesButtonClicked.OnNext((SelectedModeId, index, null, null, blink, null, null, null));
                    }
                }

                yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;
            }

            // Mouth tracking
            Func<MouthTrackingControl, bool> mouthToBool = (MouthTrackingControl mouthTrackingControl) => mouthTrackingControl == MouthTrackingControl.Tracking;
            Func<bool, MouthTrackingControl> boolToMouth = (bool value) => value ? MouthTrackingControl.Tracking : MouthTrackingControl.Animation;
            if (!IsSimplified)
            {
                var toggleRect = new Rect(xCurrent, yCurrent, ToggleWidth, EditorGUIUtility.singleLineHeight);
                // EditorGUI.LabelField(toggleRect, new GUIContent(string.Empty, _localizationTable.Common_Tooltip_LipSync));

                var mouthTracking = GUI.Toggle(toggleRect, mouthToBool(branch.MouthTrackingControl), string.Empty);
                GUI.Label(new Rect(xCurrent + ToggleWidth, yCurrent, PropertiesWidth - ToggleWidth, EditorGUIUtility.singleLineHeight),
                    new GUIContent(_mouthTrackingText, _localizationTable.Common_Tooltip_LipSync));
                if (mouthTracking != mouthToBool(branch.MouthTrackingControl))
                {
                    _onModifyBranchPropertiesButtonClicked.OnNext((SelectedModeId, index, null, boolToMouth(mouthTracking), null, null, null, null));
                }

                yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;
            }

            // Mouth morph cancel
            if (!IsSimplified)
            {
                if (mouthToBool(branch.MouthTrackingControl))
                {
                    var toggleRect = new Rect(xCurrent, yCurrent, ToggleWidth, EditorGUIUtility.singleLineHeight);
                    // EditorGUI.LabelField(toggleRect, new GUIContent(string.Empty, _localizationTable.Common_Tooltip_MouthMorphCanceler));

                    var mouthMorphCancel = GUI.Toggle(toggleRect, branch.MouthMorphCancelerEnabled, string.Empty);
                    GUI.Label(new Rect(xCurrent + ToggleWidth, yCurrent, PropertiesWidth - ToggleWidth, EditorGUIUtility.singleLineHeight),
                        new GUIContent(_mouthMorphCancelerText, _localizationTable.Common_Tooltip_MouthMorphCanceler));
                    if (mouthMorphCancel != branch.MouthMorphCancelerEnabled)
                    {
                        _onModifyBranchPropertiesButtonClicked.OnNext((SelectedModeId, index, null, null, null, mouthMorphCancel, null, null));
                    }
                }
                else
                {
                    ViewUtility.RectDummyToggle(new Rect(xCurrent, yCurrent, PropertiesWidth, EditorGUIUtility.singleLineHeight), ToggleWidth, _mouthMorphCancelerText);
                }

                yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;
            }

            // Is left trigger used
            if (!IsSimplified)
            {
                if (branch.CanLeftTriggerUsed)
                {
                    var toggleRect = new Rect(xCurrent, yCurrent, ToggleWidth, EditorGUIUtility.singleLineHeight);
                    // EditorGUI.LabelField(toggleRect, new GUIContent(string.Empty, _localizationTable.Common_Tooltip_LeftTrigger));

                    var useLeftTrigger = GUI.Toggle(toggleRect, branch.IsLeftTriggerUsed, string.Empty);
                    GUI.Label(new Rect(xCurrent + ToggleWidth, yCurrent, PropertiesWidth - ToggleWidth, EditorGUIUtility.singleLineHeight),
                        new GUIContent(_useLeftTriggerText, _localizationTable.Common_Tooltip_LeftTrigger));
                    if (useLeftTrigger != branch.IsLeftTriggerUsed)
                    {
                        _onModifyBranchPropertiesButtonClicked.OnNext((SelectedModeId, index, null, null, null, null, useLeftTrigger, null));
                    }
                }
                else
                {
                    ViewUtility.RectDummyToggle(new Rect(xCurrent, yCurrent, PropertiesWidth, EditorGUIUtility.singleLineHeight), ToggleWidth, _useLeftTriggerText);
                }

                yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;
            }

            // Is right trigger used
            if (!IsSimplified)
            {
                if (branch.CanRightTriggerUsed)
                {
                    var toggleRect = new Rect(xCurrent, yCurrent, ToggleWidth, EditorGUIUtility.singleLineHeight);
                    // EditorGUI.LabelField(toggleRect, new GUIContent(string.Empty, _localizationTable.Common_Tooltip_RightTrigger));

                    var useRightTrigger = GUI.Toggle(toggleRect, branch.IsRightTriggerUsed, string.Empty);
                    GUI.Label(new Rect(xCurrent + ToggleWidth, yCurrent, PropertiesWidth - ToggleWidth, EditorGUIUtility.singleLineHeight),
                        new GUIContent(_useRightTriggerText, _localizationTable.Common_Tooltip_RightTrigger));
                    if (useRightTrigger != branch.IsRightTriggerUsed)
                    {
                        _onModifyBranchPropertiesButtonClicked.OnNext((SelectedModeId, index, null, null, null, null, null, useRightTrigger));
                    }
                }
                else
                {
                    ViewUtility.RectDummyToggle(new Rect(xCurrent, yCurrent, PropertiesWidth, EditorGUIUtility.singleLineHeight), ToggleWidth, _useRightTriggerText);
                }

                yCurrent += EditorGUIUtility.singleLineHeight + VerticalMargin;
            }

            xCurrent += PropertiesWidth + upperHorizontalMargin;
            yCurrent = yBegin;

            // Base animation
            if (!IsSimplified)
            {
                _animationElement.Draw(new Rect(xCurrent, yCurrent, thumbnailWidth, thumbnailHeight + EditorGUIUtility.singleLineHeight), branch.BaseAnimation, _thumbnailDrawer,
                    guid => { _onAnimationChanged.OnNext((guid, SelectedModeId, index, BranchAnimationType.Base)); });

                xCurrent = xBegin;
                yCurrent += _animationElement.GetHeight() + VerticalMargin;
            }
            else
            {
                var x = xCurrent - PropertiesWidth - upperHorizontalMargin;
                var width = thumbnailWidth + PropertiesWidth + upperHorizontalMargin;

                var path = AssetDatabase.GUIDToAssetPath(branch.BaseAnimation?.GUID);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                var newClip = EditorGUI.ObjectField(new Rect(x, yCurrent, width, EditorGUIUtility.singleLineHeight), clip, typeof(AnimationClip), false);
                if (!ReferenceEquals(clip, newClip))
                {
                    var newPath = AssetDatabase.GetAssetPath(newClip);
                    var newGUID = AssetDatabase.AssetPathToGUID(newPath);
                    _onAnimationChanged.OnNext((newGUID, SelectedModeId, index, BranchAnimationType.Base));
                }
            }

            // Left trigger animation
            if (!IsSimplified)
            {
                if (branch.CanLeftTriggerUsed && branch.IsLeftTriggerUsed)
                {
                    _animationElement.Draw(new Rect(xCurrent, yCurrent, thumbnailWidth, thumbnailHeight + EditorGUIUtility.singleLineHeight), branch.LeftHandAnimation, _thumbnailDrawer,
                        guid => { _onAnimationChanged.OnNext((guid, SelectedModeId, index, BranchAnimationType.Left)); });
                    GUI.Label(new Rect(xCurrent, yCurrent + _animationElement.GetHeight(), _animationElement.GetWidth(), EditorGUIUtility.singleLineHeight), _leftTriggerAnimationText, _centerStyle);
                }

                xCurrent += _animationElement.GetWidth() + lowerHorizontalMargin;
            }

            // Right trigger animation
            if (!IsSimplified)
            {
                if (branch.CanRightTriggerUsed && branch.IsRightTriggerUsed)
                {
                    _animationElement.Draw(new Rect(xCurrent, yCurrent, thumbnailWidth, thumbnailHeight + EditorGUIUtility.singleLineHeight), branch.RightHandAnimation, _thumbnailDrawer,
                        guid => { _onAnimationChanged.OnNext((guid, SelectedModeId, index, BranchAnimationType.Right)); });
                    GUI.Label(new Rect(xCurrent, yCurrent + _animationElement.GetHeight(), _animationElement.GetWidth(), EditorGUIUtility.singleLineHeight), _rightTriggerAnimationText, _centerStyle);
                }

                xCurrent += _animationElement.GetWidth() + lowerHorizontalMargin;
            }

            // Both triggers animations
            if (!IsSimplified)
            {
                if (branch.CanLeftTriggerUsed && branch.IsLeftTriggerUsed && branch.CanRightTriggerUsed && branch.IsRightTriggerUsed)
                {
                    _animationElement.Draw(new Rect(xCurrent, yCurrent, thumbnailWidth, thumbnailHeight + EditorGUIUtility.singleLineHeight), branch.BothHandsAnimation, _thumbnailDrawer,
                        guid => { _onAnimationChanged.OnNext((guid, SelectedModeId, index, BranchAnimationType.Both)); });
                    GUI.Label(new Rect(xCurrent, yCurrent + _animationElement.GetHeight(), _animationElement.GetWidth(), EditorGUIUtility.singleLineHeight), _bothTriggersAnimationText, _centerStyle);
                }
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
            GUI.Label(rect, _emptyText, _centerStyle);
        }

        private void OnElementOrderChanged(ReorderableList reorderableList, int oldIndex, int newIndex)
        {
            _onBranchOrderChanged.OnNext((SelectedModeId, oldIndex, newIndex));
        }

        private void OnElementSelectionChanged(ReorderableList reorderableList)
        {
            _onBranchSelectionChanged.OnNext(_reorderableList.index);
        }

        private List<Domain.Animation> GetAnimations()
        {
            List<Domain.Animation> animations = new List<Domain.Animation>();

            if (Menu is null || !Menu.ContainsMode(SelectedModeId))
            {
                return animations;
            }

            var mode = Menu.GetMode(SelectedModeId);
            foreach (var branch in mode.Branches)
            {
                animations.Add(branch.BaseAnimation);
                animations.Add(branch.LeftHandAnimation);
                animations.Add(branch.RightHandAnimation);
                animations.Add(branch.BothHandsAnimation);
            }   

            return animations;
        }

        private Rect ShowHints()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (!showHints) { return Rect.zero; }

            if (Menu?.ContainsMode(SelectedModeId) == true)
            {
                var mode = Menu?.GetMode(SelectedModeId);
                if (mode?.Branches?.Count == 0)
                {
                    return HelpBoxDrawer.InfoLayout(_localizationTable.Hints_AddExpression);
                }
                else if (mode?.Branches?.Count == 1)
                {
                    if (IsSimplified)
                    {
                        return HelpBoxDrawer.InfoLayout(_localizationTable.Hints_Simplified);
                    }
                    else
                    {
                        return HelpBoxDrawer.InfoLayout(_localizationTable.Hints_AnimationMenu);
                    }
                }
                else if (mode?.Branches?.Count >= 2)
                {
                    return HelpBoxDrawer.InfoLayout(_localizationTable.Hints_ExpressionPriority);
                }
            }

            return Rect.zero;
        }
    }
}
