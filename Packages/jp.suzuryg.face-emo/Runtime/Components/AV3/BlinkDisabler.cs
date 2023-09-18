using UnityEngine;

namespace Suzuryg.FaceEmo.Components.AV3
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
