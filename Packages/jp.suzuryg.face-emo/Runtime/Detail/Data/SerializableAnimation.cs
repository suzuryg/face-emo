using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.Data
{
    public class SerializableAnimation : ScriptableObject
    {
        public string GUID;

        public void Save(Domain.Animation animation)
        {
            GUID = animation.GUID;
        }

        public Domain.Animation Load()
        {
            return new Domain.Animation(GUID);
        }
    }
}
