using UnityEngine;

namespace Suzuryg.FaceEmo.Components
{
    [DisallowMultipleComponent]
    public class RestorationCheckpoint : MonoBehaviour
    {
        public MonoBehaviour TargetAvatar;
        public ScriptableObject LatestBackup;
    }
}
