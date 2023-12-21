using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    internal static class AV3Extensions
    {
        public static Vector3 GetScaledViewPosition(this VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor == null || avatarDescriptor.gameObject == null || avatarDescriptor.gameObject.transform == null)
            {
                return Vector3.zero;
            }

            var viewPosition = avatarDescriptor.ViewPosition;
            var scale = avatarDescriptor.gameObject.transform.localScale;

            if (Mathf.Approximately(scale.x, 0) || Mathf.Approximately(scale.y, 0) || Mathf.Approximately(scale.z, 0))
            {
                return viewPosition;
            }

            return new Vector3(viewPosition.x / scale.x, viewPosition.y / scale.y, viewPosition.z / scale.z);
        }
    }
}
