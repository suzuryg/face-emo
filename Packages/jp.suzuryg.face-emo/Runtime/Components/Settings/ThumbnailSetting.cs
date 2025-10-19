using System;
using UnityEngine;

namespace Suzuryg.FaceEmo.Components.Settings
{
    public class ThumbnailSetting : ScriptableObject
    {
        [NonSerialized] public static readonly float DefaultFOV = 20;
        [NonSerialized] public static readonly float MinFOV = 0.1f;
        [NonSerialized] public static readonly float MaxFOV = 60;

        [NonSerialized] public static readonly float DefaultDistance = 0.6f;
        [NonSerialized] public static readonly float MinDistance = 0.01f;
        [NonSerialized] public static readonly float MaxDistance = 1.0f;

        [NonSerialized] public static readonly float DefaultCameraPosX = 0.5f;
        [NonSerialized] public static readonly float CameraPosXCoef = 1.1f;

        [NonSerialized] public static readonly float DefaultCameraPosY = 0.55f;
        [NonSerialized] public static readonly float CameraPosYCoef = 1.1f;

        [NonSerialized] public static readonly float DefaultCameraAngleH = 0;
        [NonSerialized] public static readonly float MaxCameraAngleH = 60;

        [NonSerialized] public static readonly float DefaultCameraAngleV = -5;
        [NonSerialized] public static readonly float MaxCameraAngleV = 30;

        [NonSerialized] public static readonly float DefaultAnimationProgress = 1f;
        [NonSerialized] public static readonly float MinAnimationProgress = 0f;
        [NonSerialized] public static readonly float MaxAnimationProgress = 1f;

        public float Main_FOV = DefaultFOV;
        public float Main_Distance = DefaultDistance;
        public float Main_CameraPosX = DefaultCameraPosX;
        public float Main_CameraPosY = DefaultCameraPosY;
        public float Main_CameraAngleH = DefaultCameraAngleH;
        public float Main_CameraAngleV = DefaultCameraAngleV;
        public float Main_AnimationProgress = DefaultAnimationProgress;

        public int Inspector_Width = 256;
        public int Inspector_Height = 256;

        [Obsolete] public int Main_Width;
        [Obsolete] public int Main_Height;
        [Obsolete] public int GestureTable_Width;
        [Obsolete] public int GestureTable_Height;
    }
}
