using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IMode
    {
        string DisplayName { get; }
        bool UseAnimationNameAsDisplayName { get; }
        EyeTrackingControl EyeTrackingControl { get; }
        MouthTrackingControl MouthTrackingControl { get; }

        IReadOnlyList<IBranch> Branches { get; }
        IBranch GetGestureCell(HandGesture left, HandGesture right);

        IMenuItemList Parent { get; }
        string GetId();

        Animation Animation { get; }
    }

    public class Mode : IMode
    {
        public static readonly IReadOnlyList<HandGesture> GestureList = Enum.GetValues(typeof(HandGesture)).Cast<HandGesture>().ToList();

        public string DisplayName { get; set; }
        public bool UseAnimationNameAsDisplayName { get; set; } = true;
        public EyeTrackingControl EyeTrackingControl { get; set; } = EyeTrackingControl.Tracking;
        public MouthTrackingControl MouthTrackingControl { get; set; } = MouthTrackingControl.Tracking;

        public IReadOnlyList<IBranch> Branches => _branches;
        public IBranch GetGestureCell(HandGesture left, HandGesture right) => _gestureTable[(left, right)];

        public Animation Animation { get; private set; }

        IMenuItemList IMode.Parent => Parent;
        public MenuItemListBase Parent { get; set; }

        private List<Branch> _branches = new List<Branch>();
        private Dictionary<(HandGesture left, HandGesture right), Branch> _gestureTable = new Dictionary<(HandGesture left, HandGesture right), Branch>();

        public Mode(string displayName, MenuItemListBase parent)
        {
            DisplayName = displayName;
            Parent = parent;

            foreach (var left in GestureList)
            {
                foreach (var right in GestureList)
                {
                    _gestureTable[(left, right)] = null;
                }
            }
        }

        public string GetId()
        {
            foreach (var id in Parent.Order)
            {
                if (Parent.GetType(id) == MenuItemType.Mode && ReferenceEquals(Parent.GetMode(id), this))
                {
                    return id;
                }
            }
            throw new FacialExpressionSwitcherException("The parent does not have this mode.");
        }
        
        public void AddBranch(IEnumerable<Condition> conditions = null)
        {
            _branches.Add(new Branch(conditions));
            UpdateTable();
        }

        public void ModifyBranchProperties(int branchIndex,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? isLeftTriggerUsed = null,
            bool? isRightTriggerUsed = null)
        {
            var branch = _branches[branchIndex];
            branch.EyeTrackingControl = eyeTrackingControl ?? branch.EyeTrackingControl;
            branch.MouthTrackingControl = mouthTrackingControl ?? branch.MouthTrackingControl;
            branch.IsLeftTriggerUsed = isLeftTriggerUsed ?? branch.IsLeftTriggerUsed;
            branch.IsRightTriggerUsed = isRightTriggerUsed ?? branch.IsRightTriggerUsed;
        }

        public void ChangeBranchOrder(int from, int to)
        {
            var branch = _branches[from];
            _branches.RemoveAt(from);

            if (to < 0)
            {
                _branches.Insert(0, branch);
            }
            else if (to > _branches.Count)
            {
                _branches.Add(branch);
            }
            else
            {
                _branches.Insert(to, branch);
            }

            UpdateTable();
        }

        public void RemoveBranch(int index)
        {
            _branches.RemoveAt(index);
            UpdateTable();
        }

        public void AddCondition(int branchIndex, Condition condition)
        {
            _branches[branchIndex].AddCondition(condition);
            UpdateTable();
        }

        public void ModifyCondition(int branchIndex, int conditionIndex, Condition condition)
        {
            _branches[branchIndex].ModifyCondition(conditionIndex, condition);
            UpdateTable();
        }

        public void RemoveCondition(int branchIndex, int conditionIndex)
        {
            _branches[branchIndex].RemoveCondition(conditionIndex);
            UpdateTable();
        }

        public void ChangeConditionOrder(int branchIndex, int from, int to)
        {
            _branches[branchIndex].ChangeBranchOrder(from, to);
        }

        public void SetAnimation(Animation animation, int? branchIndex, BranchAnimationType? branchAnimationType)
        {
            if (branchIndex.HasValue)
            {
                _branches[branchIndex.Value].SetAnimation(animation, branchAnimationType);
            }
            else
            {
                Animation = animation;
            }
        }

        private void UpdateTable()
        {
            foreach (var branch in _branches)
            {
                branch.CanLeftTriggerUsed = false;
                branch.CanRightTriggerUsed = false;
            }

            var keys = _gestureTable.Keys.ToList();

            foreach (var gesture in keys)
            {
                _gestureTable[gesture] = null;
            }

            foreach (var branch in _branches)
            {
                branch.IsReachable = false;
            }

            foreach (var gesture in keys)
            {
                foreach (var branch in _branches)
                {
                    if (branch.IsMatched(gesture.left, gesture.right))
                    {
                        _gestureTable[gesture] = branch;
                        branch.IsReachable = true;
                        break;
                    }
                }
            }

            foreach (var right in GestureList)
            {
                if (_gestureTable[(HandGesture.Fist, right)] is Branch)
                {
                    _gestureTable[(HandGesture.Fist, right)].CanLeftTriggerUsed = true;
                }
            }

            foreach (var left in GestureList)
            {
                if (_gestureTable[(left, HandGesture.Fist)] is Branch)
                {
                    _gestureTable[(left, HandGesture.Fist)].CanRightTriggerUsed = true;
                }
            }
        }
    }
}
