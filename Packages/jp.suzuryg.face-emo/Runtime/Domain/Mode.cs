using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FaceEmo.Domain
{
    public interface IMode
    {
        bool ChangeDefaultFace { get; }
        string DisplayName { get; }
        bool UseAnimationNameAsDisplayName { get; }
        EyeTrackingControl EyeTrackingControl { get; }
        MouthTrackingControl MouthTrackingControl { get; }
        bool BlinkEnabled { get; }
        bool MouthMorphCancelerEnabled { get; }

        IReadOnlyList<IBranch> Branches { get; }
        IBranch GetGestureCell(HandGesture left, HandGesture right);

        IMenuItemList Parent { get; }
        string GetId();

        Animation Animation { get; }
    }

    public class Mode : IMode
    {
        public static readonly IReadOnlyList<HandGesture> GestureList = Enum.GetValues(typeof(HandGesture)).Cast<HandGesture>().ToList();

        public bool ChangeDefaultFace { get; set; } = false;
        public string DisplayName { get; set; }
        public bool UseAnimationNameAsDisplayName { get; set; } = false;
        public EyeTrackingControl EyeTrackingControl { get; set; } = EyeTrackingControl.Tracking;
        public MouthTrackingControl MouthTrackingControl { get; set; } = MouthTrackingControl.Tracking;
        public bool BlinkEnabled { get; set; } = true;
        public bool MouthMorphCancelerEnabled { get; set; } = true;

        public IReadOnlyList<IBranch> Branches => _branches;
        public IBranch GetGestureCell(HandGesture left, HandGesture right) => _gestureTable[(left, right)];

        public Animation Animation { get; private set; }

        IMenuItemList IMode.Parent => Parent;
        public MenuItemListBase Parent => _parent;
        private MenuItemListBase _parent;

        private List<Branch> _branches = new List<Branch>();
        private Dictionary<(HandGesture left, HandGesture right), Branch> _gestureTable = new Dictionary<(HandGesture left, HandGesture right), Branch>();

        public Mode(string displayName, MenuItemListBase parent)
        {
            DisplayName = displayName;
            _parent = parent;

            foreach (var left in GestureList)
            {
                foreach (var right in GestureList)
                {
                    _gestureTable[(left, right)] = null;
                }
            }
        }

        public void ChangeParent(MenuItemListBase parent)
        {
            _parent = parent;
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
            throw new FaceEmoException("The parent does not have this mode.");
        }
        
        public void AddBranch(IEnumerable<Condition> conditions = null, DefaultsProvider defaultsProvider = null)
        {
            _branches.Add(new Branch(conditions));

            if (defaultsProvider is DefaultsProvider)
            {
                var branchIndex = _branches.Count - 1;
                ModifyBranchProperties(
                    branchIndex: branchIndex,
                    eyeTrackingControl: defaultsProvider.EyeTrackingControl,
                    mouthTrackingControl: defaultsProvider.MouthTrackingControl,
                    blinkEnabled: defaultsProvider.BlinkEnabled,
                    mouthMorphCancelerEnabled: defaultsProvider.MouthMorphCancelerEnabled);
            }

            UpdateTable();
        }

        public void ModifyBranchProperties(int branchIndex,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? blinkEnabled = null,
            bool? mouthMorphCancelerEnabled = null,
            bool? isLeftTriggerUsed = null,
            bool? isRightTriggerUsed = null)
        {
            var branch = _branches[branchIndex];
            branch.EyeTrackingControl = eyeTrackingControl ?? branch.EyeTrackingControl;
            branch.MouthTrackingControl = mouthTrackingControl ?? branch.MouthTrackingControl;
            branch.BlinkEnabled = blinkEnabled ?? branch.BlinkEnabled;
            branch.MouthMorphCancelerEnabled = mouthMorphCancelerEnabled ?? branch.MouthMorphCancelerEnabled;
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
