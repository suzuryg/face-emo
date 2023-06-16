using System;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.Drawing
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

        public int Main_Width = 180;
        public int Main_Height = 150;
        public float Main_FOV = DefaultFOV;
        public float Main_Distance = DefaultDistance;
        public float Main_CameraPosX = DefaultCameraPosX;
        public float Main_CameraPosY = DefaultCameraPosY;
        public float Main_CameraAngleH = DefaultCameraAngleH;
        public float Main_CameraAngleV = DefaultCameraAngleV;

        [NonSerialized] public static readonly int Main_MinWidth = 125;
        [NonSerialized] public static readonly int Main_MaxWidth = 300;
        [NonSerialized] public static readonly int Main_MinHeight = 125;
        [NonSerialized] public static readonly int Main_MaxHeight = 300;

        public int GestureTable_Width = 110;
        public int GestureTable_Height = 85;

        [NonSerialized] public static readonly int GestureTable_MinWidth = 70;
        [NonSerialized] public static readonly int GestureTable_MaxWidth = 250;
        [NonSerialized] public static readonly int GestureTable_MinHeight = 70;
        [NonSerialized] public static readonly int GestureTable_MaxHeight = 250;

        [NonSerialized] public static readonly int ExMenu_InnerWidth = 208;
        [NonSerialized] public static readonly int ExMenu_InnerHeight = 208;
        [NonSerialized] public static readonly int ExMenu_OuterWidth = 256;
        [NonSerialized] public static readonly int ExMenu_OuterHeight = 256;

        public int Inspector_Width = 256;
        public int Inspector_Height = 256;
    }
}
