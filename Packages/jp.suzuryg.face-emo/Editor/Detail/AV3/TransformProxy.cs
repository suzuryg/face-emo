using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Detail.View;
using Suzuryg.FaceEmo.Detail.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UniRx;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public class TransformProxy
    {
        public GameObject GameObject { get; set; }

        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }

        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }

        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }

        public TransformProxy Copy()
        {
            var copied = new TransformProxy();

            copied.GameObject = GameObject;

            copied.PositionX = PositionX;
            copied.PositionY = PositionY;
            copied.PositionZ = PositionZ;

            copied.RotationX = RotationX;
            copied.RotationY = RotationY;
            copied.RotationZ = RotationZ;

            copied.ScaleX = ScaleX;
            copied.ScaleY = ScaleY;
            copied.ScaleZ = ScaleZ;

            return copied;
        }

        public static bool IsUpdated(TransformProxy oldValue, TransformProxy newValue)
        {
            if (!Mathf.Approximately(oldValue.PositionX, newValue.PositionX) ||
                !Mathf.Approximately(oldValue.PositionY, newValue.PositionY) ||
                !Mathf.Approximately(oldValue.PositionZ, newValue.PositionZ) ||
                !Mathf.Approximately(oldValue.RotationX, newValue.RotationX) ||
                !Mathf.Approximately(oldValue.RotationY, newValue.RotationY) ||
                !Mathf.Approximately(oldValue.RotationZ, newValue.RotationZ) ||
                !Mathf.Approximately(oldValue.ScaleX, newValue.ScaleX) ||
                !Mathf.Approximately(oldValue.ScaleY, newValue.ScaleY) ||
                !Mathf.Approximately(oldValue.ScaleZ, newValue.ScaleZ))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static TransformProxy FromGameObject(GameObject gameObject)
        {
            var proxy = new TransformProxy();

            proxy.GameObject = gameObject;

            proxy.PositionX = gameObject.transform.localPosition.x;
            proxy.PositionY = gameObject.transform.localPosition.y;
            proxy.PositionZ = gameObject.transform.localPosition.z;

            proxy.RotationX = WrapAngle(gameObject.transform.localEulerAngles.x);
            proxy.RotationY = WrapAngle(gameObject.transform.localEulerAngles.y);
            proxy.RotationZ = WrapAngle(gameObject.transform.localEulerAngles.z);

            proxy.ScaleX = gameObject.transform.localScale.x;
            proxy.ScaleY = gameObject.transform.localScale.y;
            proxy.ScaleZ = gameObject.transform.localScale.z;

            return proxy;
        }

        public static float WrapAngle(float angle)
        {
            angle %= 360;
            if (angle > 180) { angle -= 360;}
            return angle;
        }
    }
}
