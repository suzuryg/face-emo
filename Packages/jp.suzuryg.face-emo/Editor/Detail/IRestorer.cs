namespace Suzuryg.FaceEmo.Detail
{
    public interface IRestorer
    {
        bool CanRestore();
        void Restore();
        (string current, string backup) GetNames();
    }
}

