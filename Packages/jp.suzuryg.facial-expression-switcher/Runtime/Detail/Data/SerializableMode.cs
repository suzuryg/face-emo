using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public class SerializableMode : ScriptableObject
    {
        public string DisplayName;

        public bool UseAnimationNameAsDisplayName;
        public EyeTrackingControl EyeTrackingControl;
        public MouthTrackingControl MouthTrackingControl;

        public SerializableAnimation Animation;

        public List<SerializableBranch> Branches;

        public void Save(IMode mode)
        {
            DisplayName = mode.DisplayName;

            UseAnimationNameAsDisplayName = mode.UseAnimationNameAsDisplayName;
            EyeTrackingControl = mode.EyeTrackingControl;
            MouthTrackingControl = mode.MouthTrackingControl;

            if (mode.Animation is Domain.Animation)
            {
                Animation = CreateInstance<SerializableAnimation>();
                Animation.Save(mode.Animation);
            }

            Branches = new List<SerializableBranch>();
            foreach (var branch in mode.Branches)
            {
                var serializableBranch = CreateInstance<SerializableBranch>();
                serializableBranch.Save(branch);
                Branches.Add(serializableBranch);
            }
        }

        public void Load(Menu menu, string id)
        {
            menu.ModifyModeProperties(id,
                displayName: DisplayName,
                useAnimationNameAsDisplayName: UseAnimationNameAsDisplayName,
                eyeTrackingControl: EyeTrackingControl,
                mouthTrackingControl: MouthTrackingControl);

            menu.SetAnimation(Animation?.Load(), id);

            for (int i = 0; i < Branches.Count; i++)
            {
                menu.AddBranch(id);
                Branches[i].Load(menu, id, i);
            }
        }
    }
}
