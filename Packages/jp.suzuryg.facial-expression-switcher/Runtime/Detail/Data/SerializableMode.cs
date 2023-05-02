using Suzuryg.FacialExpressionSwitcher.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public class SerializableMode : ScriptableObject
    {
        public string DisplayName;

        public bool UseAnimationNameAsDisplayName;
        public EyeTrackingControl EyeTrackingControl;
        public MouthTrackingControl MouthTrackingControl;
        public bool BlinkEnabled;
        public bool MouthMorphCancelerEnabled;

        public SerializableAnimation Animation;

        public List<SerializableBranch> Branches;

        public void Save(IMode mode, bool isAsset)
        {
            DisplayName = mode.DisplayName;

            UseAnimationNameAsDisplayName = mode.UseAnimationNameAsDisplayName;
            EyeTrackingControl = mode.EyeTrackingControl;
            MouthTrackingControl = mode.MouthTrackingControl;
            BlinkEnabled = mode.BlinkEnabled;
            MouthMorphCancelerEnabled = mode.MouthMorphCancelerEnabled;

            if (mode.Animation is Domain.Animation)
            {
                Animation = CreateInstance<SerializableAnimation>();
#if UNITY_EDITOR
                if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(Animation, this); }
#else
                if (isAsset) { throw new FacialExpressionSwitcherException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
                Animation.Save(mode.Animation);
                Animation.name = $"Animation_{Animation.GUID}";
            }

            Branches = new List<SerializableBranch>();
            foreach (var branch in mode.Branches)
            {
                var serializableBranch = CreateInstance<SerializableBranch>();
#if UNITY_EDITOR
                if (isAsset) { UnityEditor.AssetDatabase.AddObjectToAsset(serializableBranch, this); }
#else
                if (isAsset) { throw new FacialExpressionSwitcherException("SerializableMenu cannot be made into an asset in Play mode."); }
#endif
                serializableBranch.Save(branch, isAsset);
                serializableBranch.name = "Branch";
                Branches.Add(serializableBranch);
            }
        }

        public void Load(Domain.Menu menu, string id)
        {
            menu.ModifyModeProperties(id,
                displayName: DisplayName,
                useAnimationNameAsDisplayName: UseAnimationNameAsDisplayName,
                eyeTrackingControl: EyeTrackingControl,
                mouthTrackingControl: MouthTrackingControl,
                blinkEnabled: BlinkEnabled,
                mouthMorphCancelerEnabled: MouthMorphCancelerEnabled);

            menu.SetAnimation(Animation?.Load(), id);

            for (int i = 0; i < Branches.Count; i++)
            {
                menu.AddBranch(id);
                Branches[i].Load(menu, id, i);
            }
        }
    }
}
