using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class Menu
    {
        public static readonly string RegisteredId = "Registered";
        public static readonly string UnregisteredId = "UnRegistered";

        public bool WriteDefaults { get; } = false;
        public double TransitionDurationSeconds { get; } = 0.1;
        public IMenuItemList Registered => _registered;
        public IMenuItemList Unregistered => _unregistered;

        private IAvatar _avatar;
        private Mode _defaultSelection;
        private RegisteredMenuItemList _registered = new RegisteredMenuItemList();
        private UnregisteredMenuItemList _unregistered = new UnregisteredMenuItemList();
        private Dictionary<string, Mode> _modes = new Dictionary<string, Mode>();
        private Dictionary<string, Group> _groups = new Dictionary<string, Group>();

        public bool ContainsMode(string id) => _modes.ContainsKey(id);

        public IMode GetMode(string id) => _modes[id];

        public void ModifyModeProperties(
            string id,
            string displayName = null,
            bool? useAnimationNameAsDisplayName = null,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null)
        {
            var mode = _modes[id];
            mode.DisplayName = displayName ?? mode.DisplayName;
            mode.UseAnimationNameAsDisplayName = useAnimationNameAsDisplayName ?? mode.UseAnimationNameAsDisplayName;
            mode.EyeTrackingControl = eyeTrackingControl ?? mode.EyeTrackingControl;
            mode.MouthTrackingControl = mouthTrackingControl ?? mode.MouthTrackingControl;
        }

        public bool ContainsGroup(string id) => _groups.ContainsKey(id);

        public IGroup GetGroup(string id) => _groups[id];

        public void ModifyGroupProperties(string id, string displayName = null)
        {
            var group = _groups[id];
            group.DisplayName = displayName ?? group.DisplayName;
        }

        public bool CanAddModeTo(string destination)
        {
            if (destination == RegisteredId)
            {
                return !Registered.IsFull;
            }
            else if (destination == UnregisteredId)
            {
                return true;
            }
            else
            {
                return ContainsGroup(destination) && !_groups[destination].IsFull;
            }
        }

        public void AddMode(string destination)
        {
            NullChecker.Check(destination);

            var mode = new Mode("NewMode");

            var id = GetNewId();

            if (destination == RegisteredId)
            {
                _registered.Insert(mode, id);
                mode.Parent = _registered;
            }
            else if (destination== UnregisteredId)
            {
                _unregistered.Insert(mode, id);
                mode.Parent = _unregistered;
            }
            else
            {
                _groups[destination].Insert(mode, id);
                mode.Parent = _groups[destination];
            }

            _modes[id] = mode;
        }

        public bool CanAddGroupTo(string destination) => CanAddModeTo(destination);

        public void AddGroup(string destination)
        {
            NullChecker.Check(destination);

            var group = new Group("NewGroup");

            var id = GetNewId();

            if (destination == RegisteredId)
            {
                _registered.Insert(group, id);
                group.Parent = _registered;
            }
            else if (destination== UnregisteredId)
            {
                _unregistered.Insert(group, id);
                group.Parent = _unregistered;
            }
            else
            {
                _groups[destination].Insert(group, id);
                group.Parent = _groups[destination];
            }

            _groups[id] = group;
        }

        public bool CanRemoveMenuItem(string id) => ContainsMode(id) || ContainsGroup(id);

        public void RemoveMenuItem(string id)
        {
            NullChecker.Check(id);

            if (ContainsMode(id))
            {
                _modes[id].Parent.Remove(id);
                _modes.Remove(id);
            }
            else if (ContainsGroup(id))
            {
                _groups[id].Parent.Remove(id);
                var descendants = _groups[id].GetDescendantsId();
                _groups.Remove(id);
                foreach (var descendant in descendants)
                {
                    if (ContainsMode(descendant))
                    {
                        _modes.Remove(descendant);
                    }
                    else if (ContainsGroup(descendant))
                    {
                        _groups.Remove(descendant);
                    }
                }
            }
            else
            {
                throw new FacialExpressionSwitcherException("This menu does not contain the specified MenuItem.");
            }
        }

        public bool CanMoveMenuItemFrom(string id) => ContainsMode(id) || ContainsGroup(id);

        public bool CanMoveMenuItemTo(string id, string destination)
        {
            if (id is null || destination is null)
            {
                return false;
            }
            else if (id == destination)
            {
                return false;
            }
            else if (destination == RegisteredId)
            {
                if (Registered.Order.Contains(id))
                {
                    return true;
                }
                else
                {
                    return !Registered.IsFull;
                }
            }
            else if (destination == UnregisteredId)
            {
                return true;
            }
            else if (ContainsGroup(destination))
            {
                if (_groups[destination].Order.Contains(id))
                {
                    return true;
                }
                else
                {
                    return !_groups[destination].IsFull;
                }
            }
            else
            {
                return false;
            }
        }

        public void MoveMenuItem(string id, string destination, int? index = null)
        {
            NullChecker.Check(id, destination);

            if (ContainsMode(id))
            {
                _modes[id].Parent.Remove(id);

                if (destination == RegisteredId)
                {
                    _registered.Insert(_modes[id], id, index);
                    _modes[id].Parent = _registered;
                }
                else if (destination == UnregisteredId)
                {
                    _unregistered.Insert(_modes[id], id, index);
                    _modes[id].Parent = _unregistered;
                }
                else
                {
                    _groups[destination].Insert(_modes[id], id, index);
                    _modes[id].Parent = _groups[destination];
                }
            }
            else if (ContainsGroup(id))
            {
                _groups[id].Parent.Remove(id);

                if (destination == RegisteredId)
                {
                    _registered.Insert(_groups[id], id, index);
                    _groups[id].Parent = _registered;
                }
                else if (destination == UnregisteredId)
                {
                    _unregistered.Insert(_groups[id], id, index);
                    _groups[id].Parent = _unregistered;
                }
                else
                {
                    _groups[destination].Insert(_groups[id], id, index);
                    _groups[id].Parent = _groups[destination];
                }
            }
            else
            {
                throw new FacialExpressionSwitcherException("This menu does not contain the specified MenuItem.");
            }
        }

        public bool CanGetMergedMenu(IReadOnlyList<IExistingMenuItem> existingMenuItems) => _registered.CanGetMergedMenu(existingMenuItems);

        public MergedMenuItemList GetMergedMenu(IReadOnlyList<IExistingMenuItem> existingMenuItems) => _registered.GetMergedMenu(existingMenuItems);

        public bool CanUpdateOrderAndInsertIndices(MergedMenuItemList mergedMenuItemList) => _registered.CanUpdateInsertIndices(mergedMenuItemList);

        public void UpdateOrderAndInsertIndices(MergedMenuItemList mergedMenuItemList)
        {
            NullChecker.Check(mergedMenuItemList);

            _registered.UpdateInsertIndices(mergedMenuItemList);

            var reorderedIds = mergedMenuItemList.Order.Where(x => mergedMenuItemList.ContainsMode(x) || mergedMenuItemList.ContainsGroup(x)).ToList();
            for (int i = 0; i < reorderedIds.Count; i++)
            {
                var id = reorderedIds[i];
                if (!Registered.Order.Contains(id))
                {
                    throw new FacialExpressionSwitcherException("Merged menu contains invalid menu items.");
                }
                MoveMenuItem(id, RegisteredId, i);
            }
        }

        public bool CanAddBranchTo(string destination) => ContainsMode(destination);

        public void AddBranch(string destination, IEnumerable<Condition> conditions = null)
        {
            NullChecker.Check(destination);
            _modes[destination].AddBranch(conditions);
        }

        public bool ContainsBranch(string modeId, int index) => ContainsMode(modeId) && index >= 0 && index < _modes[modeId].Branches.Count;

        public bool CanModifyBranchProperties(string modeId, int branchIndex) => ContainsBranch(modeId, branchIndex);

        public void ModifyBranchProperties(string modeId, int branchIndex,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? isLeftTriggerUsed = null,
            bool? isRightTriggerUsed = null)
        {
            _modes[modeId].ModifyBranchProperties(branchIndex, eyeTrackingControl, mouthTrackingControl, isLeftTriggerUsed, isRightTriggerUsed);
        }

        public bool CanChangeBranchOrder(string modeId, int from) => ContainsBranch(modeId, from);

        public void ChangeBranchOrder(string modeId, int from, int to) => _modes[modeId].ChangeBranchOrder(from, to);

        public bool CanRemoveBranch(string modeId, int branchIndex) => ContainsBranch(modeId, branchIndex);

        public void RemoveBranch(string modeId, int branchIndex) => _modes[modeId].RemoveBranch(branchIndex);

        public bool CanAddConditionTo(string modeId, int branchIndex) => ContainsBranch(modeId, branchIndex);

        public void AddCondition(string modeId, int branchIndex, Condition condition) => _modes[modeId].AddCondition(branchIndex, condition);

        public bool ContainsCondition(string modeId, int branchIndex, int conditionIndex) => ContainsBranch(modeId, branchIndex) && conditionIndex >= 0 && conditionIndex < _modes[modeId].Branches[branchIndex].Conditions.Count;

        public bool CanModifyCondition(string modeId, int branchIndex, int conditionIndex) => ContainsCondition(modeId, branchIndex, conditionIndex);

        public void ModifyCondition(string modeId, int branchIndex, int conditionIndex, Condition condition) => _modes[modeId].ModifyCondition(branchIndex, conditionIndex, condition);

        public bool CanChangeConditionOrder(string modeId, int branchIndex, int from) => ContainsCondition(modeId, branchIndex, from);

        public void ChangeConditionOrder(string modeId, int branchIndex, int from, int to) => _modes[modeId].ChangeConditionOrder(branchIndex, from, to);

        public bool CanRemoveCondition(string modeId, int branchIndex, int conditionIndex) => ContainsCondition(modeId, branchIndex, conditionIndex);

        public void RemoveCondition(string modeId, int branchIndex, int conditionIndex) => _modes[modeId].RemoveCondition(branchIndex, conditionIndex);

        public bool CanSetAnimationTo(string modeId, int? branchIndex, BranchAnimationType? branchAnimationType)
        {
            if (modeId is null || !ContainsMode(modeId))
            {
                return false;
            }

            if (branchIndex.HasValue)
            {
                if (branchAnimationType.HasValue && ContainsBranch(modeId, branchIndex.Value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public void SetAnimation(IAnimation animation, string modeId, int? branchIndex, BranchAnimationType? branchAnimationType) => _modes[modeId].SetAnimation(animation, branchIndex, branchAnimationType);

        private string GetNewId()
        {
            var id = Guid.NewGuid().ToString("N");
            while (ContainsMode(id) || ContainsGroup(id))
            {
                id = Guid.NewGuid().ToString("N");
            }
            return id;
        }
    }
}
