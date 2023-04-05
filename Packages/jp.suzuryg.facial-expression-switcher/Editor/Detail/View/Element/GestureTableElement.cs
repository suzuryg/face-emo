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
    public class GestureTableElement : IDisposable
    {
        private static readonly int ScrollBottomMargin = 10;
        private static readonly int ScrollRightMargin = 5;
        private static readonly int Padding = 5;
        private static readonly int ElementPadding = 5;
        private static readonly int ElementBorderThickness = 2;
        private static readonly int MinLabelWidth = 110;
        private static readonly Color ElementBorderColor = Color.gray;
        private static readonly Color SelectedElementTextColor = Color.black;
        private static readonly Color SelectedElementBackgroudColor = Color.yellow;

        public string SelectedModeId { get; private set; }
        public HandGesture TargetLeftHand { get; private set; }
        public HandGesture TargetRightHand { get; private set; }
        public ReactiveProperty<bool> CanAddBranch { get; } = new ReactiveProperty<bool>(false);
        public IObservable<int> OnBranchSelected => _onBranchSelected.AsObservable();

        private Subject<int> _onBranchSelected = new Subject<int>();

        private ThumbnailDrawer _thumbnailDrawer;

        private int _selectedBranchIndex = -1;

        private HashSet<(int row, int col)> _selectedCells = new HashSet<(int row, int col)>();
        private Vector2 _scrollPosition = Vector2.zero;
        private Texture2D _elementBorderTexture;
        private Texture2D _selectedElementTexture;

        private GUIStyle _gestureLabelStyle;
        private Color _gestureLabelColor;

        private string _neutralText;
        private string _fistText;
        private string _handOpenText;
        private string _fingerpointText;
        private string _victoryText;
        private string _rockNRollText;
        private string _handGunText;
        private string _thumbsUpText;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public GestureTableElement(
            IReadOnlyLocalizationSetting localizationSetting,
            ThumbnailDrawer thumbnailDrawer)
        {
            // Dependencies
            _thumbnailDrawer = thumbnailDrawer;

            // Styles
            try
            {
                _gestureLabelStyle = new GUIStyle(EditorStyles.label);
            }
            catch (NullReferenceException)
            {
                // Workaround for play mode
                _gestureLabelStyle = new GUIStyle();
            }
            _gestureLabelStyle.alignment = TextAnchor.MiddleCenter;
            _gestureLabelColor = _gestureLabelStyle.normal.textColor;

            // Textures
            _elementBorderTexture = new Texture2D(1, 1);
            _elementBorderTexture.SetPixel(0, 0, ElementBorderColor);
            _elementBorderTexture.Apply();

            _selectedElementTexture = new Texture2D(1, 1);
            _selectedElementTexture.SetPixel(0, 0, SelectedElementBackgroudColor);
            _selectedElementTexture.Apply();

            // Set text
            SetText(localizationSetting.Table);
            localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);
        }

        public IMenu Menu { get; private set; }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Setup(IMenu menu)
        {
            Menu = menu;
            UpdateCellSelection();
        }

        public void ChangeModeSelection(string modeId)
        {
            if (SelectedModeId != modeId)
            {
                SelectedModeId = modeId;
                _selectedBranchIndex = -1;
                UpdateCellSelection();
            }
        }

        public void ChangeBranchSelection(int branchIndex)
        {
            // If no branches are selected in BranchLiewView, do not update selection.
            if (branchIndex >= 0)
            {
                _selectedBranchIndex = branchIndex;
                UpdateCellSelection();
            }
        }

        public void OnGUI(Rect rect)
        {
            var viewRect = new Rect(
                rect.x, rect.y,
                Padding + GetTableWidth() + Padding + ScrollRightMargin,
                Padding + GetTableHeight() + Padding + ScrollBottomMargin);

            using (var scope = new GUI.ScrollViewScope(rect, _scrollPosition, viewRect))
            {
                DrawTable(rect);
                _scrollPosition = scope.scrollPosition;
            }
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _neutralText = localizationTable.GestureTableView_Neutral;
            _fistText = localizationTable.GestureTableView_Fist;
            _handOpenText = localizationTable.GestureTableView_HandOpen;
            _fingerpointText = localizationTable.GestureTableView_Fingerpoint;
            _victoryText = localizationTable.GestureTableView_Victory;
            _rockNRollText = localizationTable.GestureTableView_RockNRoll;
            _handGunText = localizationTable.GestureTableView_HandGun;
            _thumbsUpText = localizationTable.GestureTableView_ThumbsUp;
        }

        private void UpdateCellSelection()
        {
            // When selection is updated by other views, update cell selection.
            _selectedCells.Clear();
            CanAddBranch.Value = false;

            if (Menu is null || !Menu.ContainsMode(SelectedModeId))
            {
                return;
            }

            var mode = Menu.GetMode(SelectedModeId);

            if (_selectedBranchIndex < 0 || mode.Branches.Count() <= _selectedBranchIndex)
            {
                return;
            }
            var selectedBranch = mode.Branches[_selectedBranchIndex];

            var gestureList = Mode.GestureList;
            for (int row = 0; row < gestureList.Count; row++)
            {
                for (int col = 0; col < gestureList.Count; col++)
                {
                    var leftHand = gestureList[row];
                    var rightHand = gestureList[col];

                    if (ReferenceEquals(mode.GetGestureCell(leftHand, rightHand), selectedBranch))
                    {
                        _selectedCells.Add((row, col));
                    }
                }
            }
        }

        private float GetThumbnailSize()
        {
            return EditorPrefs.HasKey(DetailConstants.KeyGestureThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyGestureThumbnailSize) : DetailConstants.MinGestureThumbnailSize;
        }

        private float GetElementWidth()
        {
            return ElementPadding + Math.Max(GetThumbnailSize(), MinLabelWidth) + ElementPadding;
        }

        private float GetElementHeight()
        {
            return ElementPadding + EditorGUIUtility.singleLineHeight + GetThumbnailSize() + ElementPadding;
        }

        private float GetTableWidth()
        {
            return GetElementWidth() * Mode.GestureList.Count;
        }

        private float GetTableHeight()
        {
            return GetElementHeight() * Mode.GestureList.Count;
        }

        private void DrawTable(Rect rect)
        {
            if (Menu is null || !Menu.ContainsMode(SelectedModeId))
            {
                return;
            }

            var mode = Menu.GetMode(SelectedModeId);

            var gestureList = Mode.GestureList;
            for (int row = 0; row < gestureList.Count; row++)
            {
                for (int col = 0; col < gestureList.Count; col++)
                {
                    var leftHand = gestureList[row];
                    var rightHand = gestureList[col];
                    Texture2D thumbnail = null;
                    var branch = mode.GetGestureCell(leftHand, rightHand);
                    if (branch is IBranch)
                    {
                        thumbnail = _thumbnailDrawer.GetThumbnail(branch.BaseAnimation).gesture;
                    }

                    var thumbnailSize = GetThumbnailSize();
                    var contentWidth = Math.Max(thumbnailSize, MinLabelWidth);

                    var elementRect = new Rect(
                        Padding + rect.x + col * GetElementWidth(),
                        Padding + rect.y + row * GetElementHeight(),
                        GetElementWidth(),
                        GetElementHeight());

                    if (elementRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        OnElementClicked(row, col);
                    }

                    if (_selectedCells.Contains((row, col)))
                    {
                        GUI.DrawTexture(elementRect, _selectedElementTexture, ScaleMode.StretchToFill);
                        _gestureLabelStyle.normal.textColor = SelectedElementTextColor;
                    }
                    else
                    {
                        _gestureLabelStyle.normal.textColor = _gestureLabelColor;
                    }

                    // Backgroud
                    GUI.DrawTexture(new Rect(
                        elementRect.x - ElementBorderThickness / 2,
                        elementRect.y - ElementBorderThickness / 2,
                        elementRect.width + ElementBorderThickness,
                        elementRect.height + ElementBorderThickness),
                        _elementBorderTexture, ScaleMode.StretchToFill, true, 0, Color.white, ElementBorderThickness, 0);
                    // Label
                    GUI.Label(new Rect(
                        elementRect.x + ElementPadding,
                        elementRect.y + ElementPadding,
                        contentWidth,
                        EditorGUIUtility.singleLineHeight),
                        GetGestureText(leftHand) + " ▶ " + GetGestureText(rightHand), _gestureLabelStyle);
                    // Thumbnail
                    if (thumbnail is Texture2D)
                    {
                        GUI.DrawTexture(new Rect(
                            elementRect.x + ElementPadding + (contentWidth - thumbnailSize) / 2,
                            elementRect.y + ElementPadding + EditorGUIUtility.singleLineHeight,
                            thumbnailSize,
                            thumbnailSize),
                            thumbnail);
                    }
                }
            }
        }

        private void OnElementClicked(int row, int col)
        {
            _selectedCells.Clear();
            _selectedCells.Add((row, col));

            if (Menu is null || !Menu.ContainsMode(SelectedModeId))
            {
                return;
            }

            var mode = Menu.GetMode(SelectedModeId);

            var gestureList = Mode.GestureList;
            var leftHand = gestureList[row];
            var rightHand = gestureList[col];
            var selectedBranch = mode.GetGestureCell(leftHand, rightHand);

            if (selectedBranch is IBranch)
            {
                for (int branchIndex = 0; branchIndex < mode.Branches.Count(); branchIndex++)
                {
                    if (ReferenceEquals(mode.Branches[branchIndex], selectedBranch))
                    {
                        _onBranchSelected.OnNext(branchIndex);
                        CanAddBranch.Value = false;
                        break;
                    }
                }
            }
            else
            {
                _onBranchSelected.OnNext(-1);
                TargetLeftHand = leftHand;
                TargetRightHand = rightHand;
                CanAddBranch.Value = true;
            }
        }

        private string GetGestureText(HandGesture handGesture)
        {
            switch (handGesture)
            {
                case HandGesture.Neutral:
                   return _neutralText;
                case HandGesture.Fist:
                    return _fistText;
                case HandGesture.HandOpen:
                    return _handOpenText;
                case HandGesture.Fingerpoint:
                    return _fingerpointText;
                case HandGesture.Victory:
                    return _victoryText;
                case HandGesture.RockNRoll:
                    return _rockNRollText;
                case HandGesture.HandGun:
                    return _handGunText;
                case HandGesture.ThumbsUp:
                    return _thumbsUpText;
                default:
                    throw new FacialExpressionSwitcherException("Unknown hand gesture.");
            }
        }
    }
}
