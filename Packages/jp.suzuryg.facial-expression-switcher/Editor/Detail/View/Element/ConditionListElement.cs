using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View.Element
{
    public class ConditionListElement : IDisposable
    {
        private static readonly int ScrollBottomMargin = 10;
        private static readonly int ScrollRightMargin = 5;
        private static readonly int Padding = 0;
        private static readonly int Margin = 2;
        private static readonly int HandWidth = 125;
        private static readonly int HandGestureWidth = 70;
        private static readonly int ComparisonOperatorWidth = 75;
        private static readonly int MinHeight = 100;

        public IObservable<(int branchIndex, Condition condition)> OnAddConditionButtonClicked => _onAddConditionButtonClicked.AsObservable();
        public IObservable<(int branchIndex, int conditionIndex, Condition condition)> OnModifyConditionButtonClicked => _onModifyConditionButtonClicked.AsObservable();
        public IObservable<(int branchIndex, int from, int to)> OnConditionOrderChanged => _onConditionOrderChanged.AsObservable();
        public IObservable<(int branchIndex, int conditionIndex)> OnRemoveConditionButtonClicked => _onRemoveConditionButtonClicked.AsObservable();

        private Subject<(int branchIndex, Condition condition)> _onAddConditionButtonClicked = new Subject<(int branchIndex, Condition condition)>();
        private Subject<(int branchIndex, int conditionIndex, Condition condition)> _onModifyConditionButtonClicked = new Subject<(int branchIndex, int conditionIndex, Condition condition)>();
        private Subject<(int branchIndex, int from, int to)> _onConditionOrderChanged = new Subject<(int branchIndex, int from, int to)>();
        private Subject<(int branchIndex, int conditionIndex)> _onRemoveConditionButtonClicked = new Subject<(int branchIndex, int conditionIndex)>();

        private int _branchIndex;
        private IReadOnlyList<Condition> _conditions;

        private ReorderableList _reorderableList;
        private Vector2 _scrollPosition = Vector2.zero;

        private string _emptyText;
        private string _conditionText;

        private string _leftText;
        private string _rightText;
        private string _oneSideText;
        private string _eitherText;
        private string _bothText;

        private string _neutralText;
        private string _fistText;
        private string _handOpenText;
        private string _fingerpointText;
        private string _victoryText;
        private string _rockNRollText;
        private string _handGunText;
        private string _thumbsUpText;

        private string _equalsText;
        private string _notEqualText;

        private GUIStyle _centerStyle;

        private List<string> _handList = new List<string>();
        private List<string> _handGestureList = new List<string>();
        private List<string> _comparisonOperatorList = new List<string>();

        private CompositeDisposable _disposables = new CompositeDisposable();

        public ConditionListElement(
            int branchIndex,
            IReadOnlyList<Condition> conditions,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            // Dependencies
            _branchIndex = branchIndex;
            _conditions = conditions;

            // Reorderable List
            _reorderableList = new ReorderableList(new List<Condition>(), typeof(Condition));
            _reorderableList.list = _conditions.ToList();
            _reorderableList.headerHeight = EditorGUIUtility.singleLineHeight;
            _reorderableList.drawHeaderCallback = DrawHeader;
            _reorderableList.drawElementCallback = DrawElement;
            _reorderableList.drawNoneElementCallback = DrawEmpty;
            _reorderableList.onAddCallback = OnElementAdded;
            _reorderableList.onRemoveCallback = OnElementRemoved;
            _reorderableList.elementHeightCallback = GetElementHeight;
            _reorderableList.onReorderCallbackWithDetails = OnElementOrderChanged;

            // Styles
            try
            {
                _centerStyle = new GUIStyle(EditorStyles.label);
            }
            catch (NullReferenceException)
            {
                // Workaround for play mode
                _centerStyle = new GUIStyle();
            }
            _centerStyle.alignment = TextAnchor.MiddleCenter;

            // Set text
            SetText(localizationSetting.Table);
            localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _emptyText = localizationTable.BranchListView_EmptyCondition;
            _conditionText = localizationTable.BranchListView_Condition;

            _leftText = localizationTable.BranchListView_Left;
            _rightText = localizationTable.BranchListView_Right;
            _oneSideText = localizationTable.BranchListView_OneSide;
            _eitherText = localizationTable.BranchListView_Either;
            _bothText = localizationTable.BranchListView_Both;

            _handList = new List<string>()
            {
                _leftText,
                _rightText,
                _oneSideText,
                _eitherText,
                _bothText,
            };

            _neutralText = localizationTable.Common_Neutral;
            _fistText = localizationTable.Common_Fist;
            _handOpenText = localizationTable.Common_HandOpen;
            _fingerpointText = localizationTable.Common_Fingerpoint;
            _victoryText = localizationTable.Common_Victory;
            _rockNRollText = localizationTable.Common_RockNRoll;
            _handGunText = localizationTable.Common_HandGun;
            _thumbsUpText = localizationTable.Common_ThumbsUp;

            _handGestureList = new List<string>()
            {
                _neutralText,
                _fistText,
                _handOpenText,
                _fingerpointText,
                _victoryText,
                _rockNRollText,
                _handGunText,
                _thumbsUpText,
            };

            _equalsText = localizationTable.BranchListView_Equals;
            _notEqualText = localizationTable.BranchListView_NotEqual;

            _comparisonOperatorList = new List<string>()
            {
                _equalsText,
                _notEqualText,
            };
        }

        public void OnGUI(Rect rect)
        {
            float totalHeight = 0;
            totalHeight += _reorderableList.headerHeight;
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

        private float GetElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight + Padding * 2;
        }

        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, _conditionText);
        }

        public static float GetWidth()
        {
            return ReorderableList.Defaults.dragHandleWidth + ReorderableList.Defaults.padding + ScrollRightMargin
                + Padding + HandWidth + Margin + HandGestureWidth + Margin + ComparisonOperatorWidth + Padding;
        }

        public static float GetMinHeight() => MinHeight;

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_conditions.Count <= index)
            {
                return;
            }

            var condition = _conditions[index];

            var xBegin = rect.x + Padding;
            var yBegin = rect.y + Padding;
            var xCurrent = xBegin;
            var yCurrent = yBegin;

            // Hand
            var oldHand = _handList.IndexOf(GetHandString(condition.Hand));
            var newHand = EditorGUI.Popup(new Rect(xCurrent, yCurrent, HandWidth, EditorGUIUtility.singleLineHeight),
                string.Empty, oldHand, _handList.ToArray());
            if (oldHand != newHand)
            {
                var newCondition = new Condition(GetHandEnum(_handList[newHand]), condition.HandGesture, condition.ComparisonOperator);
                _onModifyConditionButtonClicked.OnNext((_branchIndex, index, newCondition));
            }
            xCurrent += HandWidth + Margin;

            // Hand gesture
            var oldHandGesture = _handGestureList.IndexOf(GetHandGestureString(condition.HandGesture));
            var newHandGesture = EditorGUI.Popup(new Rect(xCurrent, yCurrent, HandGestureWidth, EditorGUIUtility.singleLineHeight),
                string.Empty, oldHandGesture, _handGestureList.ToArray());
            if (oldHandGesture != newHandGesture)
            {
                var newCondition = new Condition(condition.Hand, GetHandGestureEnum(_handGestureList[newHandGesture]), condition.ComparisonOperator);
                _onModifyConditionButtonClicked.OnNext((_branchIndex, index, newCondition));
            }
            xCurrent += HandGestureWidth + Margin;

            // Hand gesture
            var oldOperator = _comparisonOperatorList.IndexOf(GetComparisonOperatorString(condition.ComparisonOperator));
            var newOperator = EditorGUI.Popup(new Rect(xCurrent, yCurrent, ComparisonOperatorWidth, EditorGUIUtility.singleLineHeight),
                string.Empty, oldOperator, _comparisonOperatorList.ToArray());
            if (oldOperator != newOperator)
            {
                var newCondition = new Condition(condition.Hand, condition.HandGesture, GetComparisonOperatorEnum(_comparisonOperatorList[newOperator]));
                _onModifyConditionButtonClicked.OnNext((_branchIndex, index, newCondition));
            }
        }

        private void DrawEmpty(Rect rect)
        {
            GUI.Label(rect, _emptyText, _centerStyle);
        }

        private void OnElementAdded(ReorderableList reorderableList)
        {
            _onAddConditionButtonClicked.OnNext((_branchIndex, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
        }

        private void OnElementRemoved(ReorderableList reorderableList)
        {
            _onRemoveConditionButtonClicked.OnNext((_branchIndex, _reorderableList.index));
        }

        private void OnElementOrderChanged(ReorderableList reorderableList, int oldIndex, int newIndex)
        {
            _onConditionOrderChanged.OnNext((_branchIndex, oldIndex, newIndex));
        }

        // TODO: Use dictionary
        private string GetHandString(Hand hand)
        {
            switch (hand)
            {
                case Hand.Left:
                    return _leftText;
                case Hand.Right:
                    return _rightText;
                case Hand.OneSide:
                    return _oneSideText;
                case Hand.Either:
                    return _eitherText;
                case Hand.Both:
                    return _bothText;
                default:
                    throw new FacialExpressionSwitcherException("Unknown hand type.");
            }
        }

        private Hand GetHandEnum(string hand)
        {
            if (hand == _leftText)
            {
                return Hand.Left;
            }
            else if (hand == _rightText)
            {
                return Hand.Right;
            }
            else if (hand == _oneSideText)
            {
                return Hand.OneSide;
            }
            else if (hand == _eitherText)
            {
                return Hand.Either;
            }
            else if (hand == _bothText)
            {
                return Hand.Both;
            }
            else
            {
                throw new FacialExpressionSwitcherException("Unknown hand string.");
            }
        }

        private string GetHandGestureString(HandGesture handGesture)
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
                    throw new FacialExpressionSwitcherException("Unknown hand gesture type.");
            }
        }

        private HandGesture GetHandGestureEnum(string handGesture)
        {
            if (handGesture == _neutralText)
            {
                return HandGesture.Neutral;
            }
            else if (handGesture == _fistText)
            {
                return HandGesture.Fist;
            }
            else if (handGesture == _handOpenText)
            {
                return HandGesture.HandOpen;
            }
            else if (handGesture == _fingerpointText)
            {
                return HandGesture.Fingerpoint;
            }
            else if (handGesture == _victoryText)
            {
                return HandGesture.Victory;
            }
            else if (handGesture == _rockNRollText)
            {
                return HandGesture.RockNRoll;
            }
            else if (handGesture == _handGunText)
            {
                return HandGesture.HandGun;
            }
            else if (handGesture == _thumbsUpText)
            {
                return HandGesture.ThumbsUp;
            }
            else
            {
                throw new FacialExpressionSwitcherException("Unknown hand gesture string.");
            }
        }

        private string GetComparisonOperatorString(ComparisonOperator comparisonOperator)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.Equals:
                    return _equalsText;
                case ComparisonOperator.NotEqual:
                    return _notEqualText;
                default:
                    throw new FacialExpressionSwitcherException("Unknown comparison operator type.");
            }
        }

        private ComparisonOperator GetComparisonOperatorEnum(string comparisonOperator)
        {
            if (comparisonOperator == _equalsText)
            {
                return ComparisonOperator.Equals;
            }
            else if (comparisonOperator == _notEqualText)
            {
                return ComparisonOperator.NotEqual;
            }
            else
            {
                throw new FacialExpressionSwitcherException("Unknown comparison operator string.");
            }
        }
    }
}
