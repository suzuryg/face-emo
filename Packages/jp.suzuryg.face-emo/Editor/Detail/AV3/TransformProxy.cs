using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public sealed class TransformProxy
    {
        public GameObject GameObject { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }
        public Vector3 Scale { get; private set; }

        public TransformProxy(GameObject gameObject, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            GameObject = gameObject;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public static TransformProxy FromGameObject(GameObject gameObject)
        {
            return new TransformProxy(
                gameObject,
                new Vector3(
                    gameObject.transform.localPosition.x,
                    gameObject.transform.localPosition.y,
                    gameObject.transform.localPosition.z),
                new Vector3(
                    WrapAngle(gameObject.transform.localEulerAngles.x),
                    WrapAngle(gameObject.transform.localEulerAngles.y),
                    WrapAngle(gameObject.transform.localEulerAngles.z)),
                new Vector3(
                    gameObject.transform.localScale.x,
                    gameObject.transform.localScale.y,
                    gameObject.transform.localScale.z)
            );
        }

        private static float WrapAngle(float angle)
        {
            angle %= 360;
            if (angle > 180) { angle -= 360;}
            return angle;
        }
    }
}
