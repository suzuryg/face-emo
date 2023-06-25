using Suzuryg.FaceEmo.Domain;
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
    public class GestureTableElement : IDisposable
    {
        private static readonly int ScrollBottomMargin = 10;
        private static readonly int ScrollRightMargin = 5;
        private static readonly int Padding = 5;
        private static readonly int ElementPadding = 5;
        private static readonly int ElementBorderThickness = 2;
        private static readonly int MinLabelWidth = 110;
        private static readonly Color ElementBorderColor = Color.gray;

        public IObservable<(HandGesture left, HandGesture right)?> OnSelectionChanged => _onSelectionChanged.AsObservable();
        public IObservable<Unit> OnBranchIndexExceeded => _onBranchIndexExceeded.AsObservable();

        public IMenu Menu { get; private set; }
        public string SelectedModeId  { get; private set; }
        public int SelectedBranchIndex  { get; private set; }
        public (HandGesture left, HandGesture right)? SelectedCell { get; private set; }

        private Subject<(HandGesture left, HandGesture right)?> _onSelectionChanged = new Subject<(HandGesture left, HandGesture right)?>();
        private Subject<Unit> _onBranchIndexExceeded = new Subject<Unit>();

        private ISubWindowProvider _subWindowProvider;
        private GestureTableThumbnailDrawer _thumbnailDrawer;
        private ThumbnailSetting _thumbnailSetting;
        private LocalizationTable _localizationTable;

        private Vector2 _scrollPosition = Vector2.zero;
        private Texture2D _elementBorderTexture;
        private Texture2D _selectedElementTexture;

        private GUIStyle _gestureLabelStyle;
        private GUIStyle _centerUpperStyle;
        private Color _gestureLabelColor;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public GestureTableElement(
            IReadOnlyLocalizationSetting localizationSetting,
            ISubWindowProvider subWindowProvider,
            GestureTableThumbnailDrawer thumbnailDrawer,
            ThumbnailSetting thumbnailSetting)
        {
            // Dependencies
            _subWindowProvider = subWindowProvider;
            _thumbnailDrawer = thumbnailDrawer;
            _thumbnailSetting = thumbnailSetting;

            // Styles
            try
            {
                _gestureLabelStyle = new GUIStyle(EditorStyles.label);
                _centerUpperStyle = new GUIStyle(EditorStyles.label);
            }
            catch (NullReferenceException)
            {
                // Workaround for play mode
                _gestureLabelStyle = new GUIStyle();
                _centerUpperStyle = new GUIStyle();
            }
            _gestureLabelStyle.alignment = TextAnchor.MiddleCenter;
            _gestureLabelColor = _gestureLabelStyle.normal.textColor;
            _centerUpperStyle.alignment = TextAnchor.UpperCenter;

            // Textures
            _elementBorderTexture = new Texture2D(1, 1);
            _elementBorderTexture.SetPixel(0, 0, ElementBorderColor);
            _elementBorderTexture.Apply();

            _selectedElementTexture = new Texture2D(1, 1);
            _selectedElementTexture.SetPixel(0, 0, ViewUtility.GetEmphasizedBackgroundColor());
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
            var width = GetRectWidth();
            var height = GetRectHeight();
            var viewRect = new Rect(rect.x, rect.y, width, height);
            using (var scope = new GUI.ScrollViewScope(rect, _scrollPosition, viewRect))
            {
                DrawTable(rect);
                _scrollPosition = scope.scrollPosition;
            }

            // Set max window size
            const float bottomRowHeight = 30;
            const float padding = 5;
            var window = _subWindowProvider.ProvideIfOpenedAlready<GestureTableWindow>();
            if (window != null) { window.maxSize = new Vector2(width + padding * 2, height + bottomRowHeight + padding * 2); }
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
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
            return _thumbnailSetting.GestureTable_Width;
        }

        private float GetThumbnailHeight()
        {
            return _thumbnailSetting.GestureTable_Height;
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

        private float GetRectWidth()
        {
            return Padding + GetTableWidth() + Padding + ScrollRightMargin;
        }

        private float GetRectHeight()
        {
            return Padding + GetTableHeight() + Padding + ScrollBottomMargin;
        }

        private void DrawTable(Rect rect)
        {
            if (Menu is null || !Menu.ContainsMode(SelectedModeId))
            {
                GUI.Label(new Rect(rect.x + Padding, rect.y + Padding + EditorGUIUtility.singleLineHeight * 2,
                    rect.width - Padding * 2 , rect.height - Padding * 2),
                    _localizationTable.GestureTableView_ModeIsNotSelected, _centerUpperStyle);
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
                        _gestureLabelStyle.normal.textColor = ViewUtility.GetEmphasizedTextColor();
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
                        GetGestureText(leftHand) + _localizationTable.GestureTableView_Separator + GetGestureText(rightHand), _gestureLabelStyle);
                    // Thumbnail
                    if (thumbnail != null)
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
                    return _localizationTable.Common_Neutral;
                case HandGesture.Fist:
                    return _localizationTable.Common_Fist;
                case HandGesture.HandOpen:
                    return _localizationTable.Common_HandOpen;
                case HandGesture.Fingerpoint:
                    return _localizationTable.Common_Fingerpoint;
                case HandGesture.Victory:
                    return _localizationTable.Common_Victory;
                case HandGesture.RockNRoll:
                    return _localizationTable.Common_RockNRoll;
                case HandGesture.HandGun:
                    return _localizationTable.Common_HandGun;
                case HandGesture.ThumbsUp:
                    return _localizationTable.Common_ThumbsUp;
                default:
                    throw new FaceEmoException("Unknown hand gesture.");
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
