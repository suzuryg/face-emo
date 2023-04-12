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

        public IObservable<(HandGesture left, HandGesture right)?> OnSelectionChanged => _onSelectionChanged.AsObservable();
        public IObservable<Unit> OnBranchIndexExceeded => _onBranchIndexExceeded.AsObservable();

        public IMenu Menu { get; private set; }
        public string SelectedModeId  { get; private set; }
        public int SelectedBranchIndex  { get; private set; }
        public (HandGesture left, HandGesture right)? SelectedCell { get; private set; }

        private Subject<(HandGesture left, HandGesture right)?> _onSelectionChanged = new Subject<(HandGesture left, HandGesture right)?>();
        private Subject<Unit> _onBranchIndexExceeded = new Subject<Unit>();

        private GestureTableThumbnailDrawer _thumbnailDrawer;

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
            GestureTableThumbnailDrawer thumbnailDrawer)
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

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void OnGUI(Rect rect)
        {
            // Update thumbnails
            var animations = GetAnimations();
            foreach (var animation in animations)
            {
                _thumbnailDrawer.GetThumbnail(animation);
            }
            _thumbnailDrawer.Update();

            // Draw table
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

        public void Setup(IMenu menu)
        {
            Menu = menu;
        }

        public void ChangeSelection(string modeId, int branchIndex, (HandGesture left, HandGesture right)? cell)
        {
            SelectedModeId = modeId;
            SelectedBranchIndex = branchIndex;
            SelectedCell = cell;
        }

        private float GetThumbnailWidth()
        {
            return EditorPrefs.HasKey(DetailConstants.KeyGestureThumbnailWidth) ? EditorPrefs.GetInt(DetailConstants.KeyGestureThumbnailWidth) : DetailConstants.DefaultGestureThumbnailWidth;
        }

        private float GetThumbnailHeight()
        {
            return EditorPrefs.HasKey(DetailConstants.KeyGestureThumbnailHeight) ? EditorPrefs.GetInt(DetailConstants.KeyGestureThumbnailHeight) : DetailConstants.DefaultGestureThumbnailHeight;
        }

        private float GetElementWidth()
        {
            return ElementPadding + Math.Max(GetThumbnailWidth(), MinLabelWidth) + ElementPadding;
        }

        private float GetElementHeight()
        {
            return ElementPadding + EditorGUIUtility.singleLineHeight + GetThumbnailHeight() + ElementPadding;
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
            IBranch selectedBranch = null;
            if (SelectedBranchIndex < 0)
            {
                // NOP
            }
            else if (mode.Branches.Count <= SelectedBranchIndex)
            {
                _onBranchIndexExceeded.OnNext(Unit.Default);
            }
            else
            {
                selectedBranch = mode.Branches[SelectedBranchIndex];
            }

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
                        thumbnail = _thumbnailDrawer.GetThumbnail(branch.BaseAnimation);
                    }

                    var thumbnailWidth = GetThumbnailWidth();
                    var thumbnailHeight = GetThumbnailHeight();
                    var contentWidth = Math.Max(thumbnailWidth, MinLabelWidth);

                    var elementRect = new Rect(
                        Padding + rect.x + col * GetElementWidth(),
                        Padding + rect.y + row * GetElementHeight(),
                        GetElementWidth(),
                        GetElementHeight());

                    if (elementRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        OnElementClicked(row, col);
                    }

                    var isHighlighted = false;
                    if (selectedBranch is IBranch)
                    {
                        if (ReferenceEquals(selectedBranch, mode.GetGestureCell(leftHand, rightHand)))
                        {
                            isHighlighted = true;
                        }
                    }
                    else if (!(SelectedCell is null))
                    {
                        if (leftHand == SelectedCell.Value.left && rightHand == SelectedCell.Value.right)
                        {
                            isHighlighted = true;
                        }
                    }

                    if (isHighlighted)
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
                            elementRect.x + ElementPadding + (contentWidth - thumbnailWidth) / 2,
                            elementRect.y + ElementPadding + EditorGUIUtility.singleLineHeight,
                            thumbnailWidth,
                            thumbnailHeight),
                            thumbnail);
                    }
                }
            }
        }

        private void OnElementClicked(int row, int col)
        {
            var gestureList = Mode.GestureList;

            var leftHand = gestureList[row];
            var rightHand = gestureList[col];
            SelectedCell = (leftHand, rightHand);

            _onSelectionChanged.OnNext((leftHand, rightHand));
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
    }
}
