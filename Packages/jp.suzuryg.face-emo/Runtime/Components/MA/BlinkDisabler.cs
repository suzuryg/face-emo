using UnityEngine;

namespace Suzuryg.FaceEmo.Components.MA
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
