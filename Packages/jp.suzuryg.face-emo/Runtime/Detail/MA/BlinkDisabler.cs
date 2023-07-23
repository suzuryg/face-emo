using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.MA
{
    [DisallowMultipleComponent]
    public class BlinkDisabler : RunBeforeModularAvatar
    {
        public override void OnPreProcessAvatar()
        {
            var avatarDescriptor = GetAvatarDescriptor();
            avatarDescriptor.customEyeLookSettings.eyelidType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.EyelidType.None;
        }
    }
}
