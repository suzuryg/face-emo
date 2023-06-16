using Suzuryg.FaceEmo.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.Data
{
    public class SerializableBranch : ScriptableObject
    {
        public EyeTrackingControl EyeTrackingControl;
        public MouthTrackingControl MouthTrackingControl;
        public bool BlinkEnabled;
        public bool MouthMorphCancelerEnabled;
        public bool IsLeftTriggerUsed;
        public bool IsRightTriggerUsed;

        public SerializableAnimation BaseAnimation;
        public SerializableAnimation LeftHandAnimation;
        public SerializableAnimation RightHandAnimation;
        public SerializableAnimation BothHandsAnimation;

        public List<SerializableCondition> Conditions;

        public void Save(IBranch branch, bool isAsset)
        {
            EyeTrackingControl = branch.EyeTrackingControl;
            MouthTrackingControl = branch.MouthTrackingControl;
            BlinkEnabled = branch.BlinkEnabled;
            MouthMorphCancelerEnabled = branch.MouthMorphCancelerEnabled;
            IsLeftTriggerUsed = branch.IsLeftTriggerUsed;
            IsRightTriggerUsed = branch.IsRightTriggerUsed;

            if (branch.BaseAnimation is Domain.Animation)
            {
                BaseAnimation = CreateInstance<SerializableAnimation>();
#if UNITY_EDITOR
                if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(BaseAnimation, this); }
#else
                if (isAsset) { throw new FaceEmoException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
                BaseAnimation.Save(branch.BaseAnimation);
                BaseAnimation.name = $"Animation_{BaseAnimation.GUID}";
            }
            if (branch.LeftHandAnimation is Domain.Animation)
            {
                LeftHandAnimation = CreateInstance<SerializableAnimation>();
#if UNITY_EDITOR
                if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(LeftHandAnimation, this); }
#else
                if (isAsset) { throw new FaceEmoException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
                LeftHandAnimation.Save(branch.LeftHandAnimation);
                LeftHandAnimation.name = $"Animation_{LeftHandAnimation.GUID}";
            }
            if (branch.RightHandAnimation is Domain.Animation)
            {
                RightHandAnimation = CreateInstance<SerializableAnimation>();
#if UNITY_EDITOR
                if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(RightHandAnimation, this); }
#else
                if (isAsset) { throw new FaceEmoException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
                RightHandAnimation.Save(branch.RightHandAnimation);
                RightHandAnimation.name = $"Animation_{RightHandAnimation.GUID}";
            }
            if (branch.BothHandsAnimation is Domain.Animation)
            {
                BothHandsAnimation = CreateInstance<SerializableAnimation>();
#if UNITY_EDITOR
                if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(BothHandsAnimation, this); }
#else
                if (isAsset) { throw new FaceEmoException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
                BothHandsAnimation.Save(branch.BothHandsAnimation);
                BothHandsAnimation.name = $"Animation_{BothHandsAnimation.GUID}";
            }

            Conditions = new List<SerializableCondition>();
            foreach (var condition in branch.Conditions)
            {
                var serializableCondition = CreateInstance<SerializableCondition>();
#if UNITY_EDITOR
                if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(serializableCondition, this); }
#else
                if (isAsset) { throw new FaceEmoException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
                serializableCondition.Save(condition);
                serializableCondition.name = "Condition";
                Conditions.Add(serializableCondition);
            }
        }

        public void Load(Domain.Menu menu, string id, int index)
        {
            menu.ModifyBranchProperties(id, index,
                eyeTrackingControl: EyeTrackingControl,
                mouthTrackingControl: MouthTrackingControl,
                blinkEnabled: BlinkEnabled,
                mouthMorphCancelerEnabled: MouthMorphCancelerEnabled,
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
