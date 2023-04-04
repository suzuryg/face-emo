using System;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IMenu
    {
        Avatar Avatar { get; }
        string DefaultSelection { get; }
        IReadOnlyList<string> MouthMorphBlendShapes { get; }

        IMenuItemList Registered { get; }
        IMenuItemList Unregistered { get; }

        bool ContainsMode(string id);
        IMode GetMode(string id);
        bool ContainsGroup(string id);
        IGroup GetGroup(string id);
    }

    public class Menu : IMenu
    {
        public static readonly string RegisteredId = "Registered";
        public static readonly string UnregisteredId = "UnRegistered";

        public Avatar Avatar { get; set; }
        public string DefaultSelection { get; private set; }
        public IReadOnlyList<string> MouthMorphBlendShapes => _mouthMorphBlendShapes;

        public IMenuItemList Registered => _registered;
        public IMenuItemList Unregistered => _unregistered;

        private List<string> _mouthMorphBlendShapes = new List<string>();
        private RegisteredMenuItemList _registered = new RegisteredMenuItemList();
        private UnregisteredMenuItemList _unregistered = new UnregisteredMenuItemList();
        private Dictionary<string, Mode> _modes = new Dictionary<string, Mode>();
        private Dictionary<string, Group> _groups = new Dictionary<string, Group>();

        public bool ContainsMode(string id)
        {
            return id is string && _modes.ContainsKey(id);
        }

        public IMode GetMode(string id) => _modes[id];

        public void ModifyModeProperties(
            string id,
            string displayName = null,
            bool? useAnimationNameAsDisplayName = null,
            EyeTrackingControl? eyeTrackingControl = null,
            MouthTrackingControl? mouthTrackingControl = null,
            bool? blinkEnabled = null,
            bool? mouthMorphCancelerEnabled = null)
        {
            var mode = _modes[id];
            mode.DisplayName = displayName ?? mode.DisplayName;
            mode.UseAnimationNameAsDisplayName = useAnimationNameAsDisplayName ?? mode.UseAnimationNameAsDisplayName;
            mode.EyeTrackingControl = eyeTrackingControl ?? mode.EyeTrackingControl;
            mode.MouthTrackingControl = mouthTrackingControl ?? mode.MouthTrackingControl;
            mode.BlinkEnabled = blinkEnabled ?? mode.BlinkEnabled;
            mode.MouthMorphCancelerEnabled = mouthMorphCancelerEnabled ?? mode.MouthMorphCancelerEnabled;
        }

        public void AddMouthMorphBlendShape(string name)
        {
            if (!_mouthMorphBlendShapes.Contains(name))
            {
                _mouthMorphBlendShapes.Add(name);
            }
        }

        public void RemoveMouthMorphBlendShape(string name)
        {
            _mouthMorphBlendShapes = _mouthMorphBlendShapes.Where(x => x != name).ToList();
        }

        public bool ContainsGroup(string id)
        {
            return id is string && _groups.ContainsKey(id);
        }

        public IGroup GetGroup(string id) => _groups[id];

        public bool CanSetDefaultSelectionTo(string id) => ContainsMode(id);

        public void SetDefaultSelection(string id)
        {
            if (ContainsMode(id))
            {
                DefaultSelection = id;
            }
        }

        public void ModifyGroupProperties(string id, string displayName = null)
        {
            var group = _groups[id];
            group.DisplayName = displayName ?? group.DisplayName;
        }

        public bool IsUsedId(string id) => ContainsMode(id) || ContainsGroup(id);

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

        public string AddMode(string destination, string id = null)
        {
            NullChecker.Check(destination);

            if (id is string && IsUsedId(id))
            {
                throw new FacialExpressionSwitcherException("The id is used.");
            }

            if (id is null)
            {
                id = GetNewId();
            }

            Mode mode;
            var name = "NewMode";

            if (destination == RegisteredId)
            {
                mode = new Mode(name, _registered);
                _registered.Insert(mode, id);
            }
            else if (destination== UnregisteredId)
            {
                mode = new Mode(name, _unregistered);
                _unregistered.Insert(mode, id);
            }
            else
            {
                mode = new Mode(name, _groups[destination]);
                _groups[destination].Insert(mode, id);
            }

            _modes[id] = mode;

            if (DefaultSelection is null)
            {
                DefaultSelection = id;
            }

            return id;
        }

        public bool CanAddGroupTo(string destination) => CanAddModeTo(destination);

        public string AddGroup(string destination, string id = null)
        {
            NullChecker.Check(destination);

            if (id is string && IsUsedId(id))
            {
                throw new FacialExpressionSwitcherException("The id is used.");
            }

            if (id is null)
            {
                id = GetNewId();
            }

            Group group;
            var name = "NewGroup";

            if (destination == RegisteredId)
            {
                group = new Group(name, _registered);
                _registered.Insert(group, id);
            }
            else if (destination== UnregisteredId)
            {
                group = new Group(name, _unregistered);
                _unregistered.Insert(group, id);
            }
            else
            {
                group = new Group(name, _groups[destination]);
                _groups[destination].Insert(group, id);
            }

            _groups[id] = group;

            return id;
        }

        public bool CanRemoveMenuItem(string id) => ContainsMode(id) || ContainsGroup(id);

        public void RemoveMenuItem(string id)
        {
            NullChecker.Check(id);

            if (ContainsMode(id))
            {
                _modes[id].Parent.Remove(id);
                _modes.Remove(id);

                if (DefaultSelection == id)
                {
                    ReselectDefaultSelection();
                }
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

        public bool CanMoveMenuItemFrom(IReadOnlyList<string> ids)
        {
            foreach (var id in ids)
            {
                if (!ContainsMode(id) && !ContainsGroup(id))
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanMoveMenuItemTo(IReadOnlyList<string> ids, string destination)
        {
            // Check null
            if (ids is null || ids.Contains(null) || destination is null)
            {
                return false;
            }
            // Check source != destination
            else if (ids.Contains(destination))
            {
                return false;
            }
            // Check free space
            else if (destination == RegisteredId)
            {
                int count = 0;
                foreach (var id in ids)
                {
                    if (!Registered.Order.Contains(id))
                    {
                        count++;
                    }
                }

                return count <= Registered.FreeSpace;
            }
            else if (destination == UnregisteredId)
            {
                return true;
            }
            else if (ContainsGroup(destination))
            {
                int count = 0;
                foreach (var id in ids)
                {
                    if (!_groups[destination].Order.Contains(id))
                    {
                        count++;
                    }
                }

                return count <= _groups[destination].FreeSpace;
            }
            // Exception
            else
            {
                return false;
            }
        }

        public void MoveMenuItem(IReadOnlyList<string> ids, string destination, int? index = null)
        {
            // Check null
            NullChecker.Check(ids, destination);
            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] is null)
                {
                    throw new FacialExpressionSwitcherException($"ids[{i}] is null.");
                }
            }

            // Select destination list
            MenuItemListBase destList;
            if (destination == RegisteredId)
            {
                destList = _registered;
            }
            else if (destination == UnregisteredId)
            {
                destList = _unregistered;
            }
            else
            {
                destList = _groups[destination];
            }

            // Clamping
            if (!index.HasValue || index.Value > destList.Count)
            {
                index = destList.Count;
            }
            else if (index.Value < 0)
            {
                index = 0;
            }

            // Calculate index after removal
            int indexAfterRemoval = index.Value;
            for (int i = 0; i < destList.Order.Count && i < index.Value; i++)
            {
                if (ids.Contains(destList.Order[i]))
                {
                    indexAfterRemoval--;
                }
            }
            index = indexAfterRemoval;

            // Remove the moving targets
            foreach (var id in ids)
            {
                if (ContainsMode(id))
                {
                    _modes[id].Parent.Remove(id);
                }
                else if (ContainsGroup(id))
                {
                    _groups[id].Parent.Remove(id);
                }
                else
                {
                    throw new FacialExpressionSwitcherException("This menu does not contain the specified MenuItem.");
                }
            }

            // Add to the specified index in reverse order
            var reversed = new List<string>(ids);
            reversed.Reverse();
            foreach (var id in reversed)
            {
                if (ContainsMode(id))
                {
                    destList.Insert(_modes[id], id, index);
                    _modes[id].ChangeParent(destList);
                }
                else if (ContainsGroup(id))
                {
                    destList.Insert(_groups[id], id, index);
                    _groups[id].ChangeParent(destList);
                }
                else
                {
                    throw new FacialExpressionSwitcherException("This menu does not contain the specified MenuItem.");
                }
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
            bool? blinkEnabled = null,
            bool? mouthMorphCancelerEnabled = null,
            bool? isLeftTriggerUsed = null,
            bool? isRightTriggerUsed = null)
        {
            _modes[modeId].ModifyBranchProperties(branchIndex, eyeTrackingControl, mouthTrackingControl, blinkEnabled, mouthMorphCancelerEnabled, isLeftTriggerUsed, isRightTriggerUsed);
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

        public void SetAnimation(Animation animation, string modeId, int? branchIndex = null, BranchAnimationType? branchAnimationType = null) => _modes[modeId].SetAnimation(animation, branchIndex, branchAnimationType);

        private string GetNewId()
        {
            var id = Guid.NewGuid().ToString("N");
            while (IsUsedId(id))
            {
                id = Guid.NewGuid().ToString("N");
            }
            return id;
        }

        private void ReselectDefaultSelection()
        {
            DefaultSelection = null;
            var queue = new Queue<string>();

            foreach (var id in Registered.Order)
            {
                queue.Enqueue(id);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (ContainsMode(current))
                {
                    DefaultSelection = current;
                    break;
                }
                else if (ContainsGroup(current))
                {
                    var group = GetGroup(current);
                    foreach (var id in group.Order)
                    {
                        queue.Enqueue(id);
                    }
                }
            }
        }
    }
}
