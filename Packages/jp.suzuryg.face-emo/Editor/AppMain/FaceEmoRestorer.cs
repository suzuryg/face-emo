using Suzuryg.FaceEmo.AppMain;
using Suzuryg.FaceEmo.Components;

namespace Suzuryg.FaceEmo.Detail
{
    public class FaceEmoRestorer : IRestorer
    {
        private RestorationCheckpoint _restorationCheckpoint;

        public FaceEmoRestorer(RestorationCheckpoint restorationCheckpoint)
        {
            _restorationCheckpoint = restorationCheckpoint;
        }

        public bool CanRestore() => FaceEmoLauncher.CanRestore(_restorationCheckpoint);
        public void Restore() => FaceEmoLauncher.Restore(_restorationCheckpoint);

        public (string current, string backup) GetNames()
        {
            var current = _restorationCheckpoint.gameObject != null ? _restorationCheckpoint.gameObject.name : string.Empty;
            var backup = _restorationCheckpoint.LatestBackup != null ? _restorationCheckpoint.LatestBackup.name : string.Empty;
            return (current, backup);
        }
    }
}

