using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public class SerializableBranch : ScriptableObject
    {
        public EyeTrackingControl EyeTrackingControl;
        public MouthTrackingControl MouthTrackingControl;
        public bool IsLeftTriggerUsed;
        public bool IsRightTriggerUsed;

        public SerializableAnimation BaseAnimation;
        public SerializableAnimation LeftHandAnimation;
        public SerializableAnimation RightHandAnimation;
        public SerializableAnimation BothHandsAnimation;

        public List<SerializableCondition> Conditions;

        public void Save(IBranch branch)
        {
            EyeTrackingControl = branch.EyeTrackingControl;
            MouthTrackingControl = branch.MouthTrackingControl;
            IsLeftTriggerUsed = branch.IsLeftTriggerUsed;
            IsRightTriggerUsed = branch.IsRightTriggerUsed;

            if (branch.BaseAnimation is Domain.Animation)
            {
                BaseAnimation = CreateInstance<SerializableAnimation>();
                BaseAnimation.Save(branch.BaseAnimation);
            }
            if (branch.LeftHandAnimation is Domain.Animation)
            {
                LeftHandAnimation = CreateInstance<SerializableAnimation>();
                LeftHandAnimation.Save(branch.LeftHandAnimation);
            }
            if (branch.RightHandAnimation is Domain.Animation)
            {
                RightHandAnimation = CreateInstance<SerializableAnimation>();
                RightHandAnimation.Save(branch.RightHandAnimation);
            }
            if (branch.BothHandsAnimation is Domain.Animation)
            {
                BothHandsAnimation = CreateInstance<SerializableAnimation>();
                BothHandsAnimation.Save(branch.BothHandsAnimation);
            }

            Conditions = new List<SerializableCondition>();
            foreach (var condition in branch.Conditions)
            {
                var serializableCondition = CreateInstance<SerializableCondition>();
                serializableCondition.Save(condition);
                Conditions.Add(serializableCondition);
            }
        }

        public void Load(Menu menu, string id, int index)
        {
            menu.ModifyBranchProperties(id, index,
                eyeTrackingControl: EyeTrackingControl,
                mouthTrackingControl: MouthTrackingControl,
                isLeftTriggerUsed: IsLeftTriggerUsed,
                isRightTriggerUsed: IsRightTriggerUsed);

            menu.SetAnimation(BaseAnimation?.Load(), id, index, BranchAnimationType.Base);
            menu.SetAnimation(LeftHandAnimation?.Load(), id, index, BranchAnimationType.Left);
            menu.SetAnimation(RightHandAnimation?.Load(), id, index, BranchAnimationType.Right);
            menu.SetAnimation(BothHandsAnimation?.Load(), id, index, BranchAnimationType.Both);

            for (int i = 0; i < Conditions.Count; i++)
            {
                var condition = Conditions[i].Load();
                menu.AddCondition(id, index, condition);
            }
        }
    }
}
