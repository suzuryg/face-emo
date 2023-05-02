using System;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Drawing
{
    public class ThumbnailSetting : ScriptableObject
    {
        [NonSerialized] public static readonly float MinOrthoSize = 0.01f;
        [NonSerialized] public static readonly float MaxOrthoSize = 0.2f;
        [NonSerialized] public static readonly float CameraPosXCoef = 1.1f;
        [NonSerialized] public static readonly float CameraPosYCoef = 1.1f;
        [NonSerialized] public static readonly float MaxCameraAngleH = 60;
        [NonSerialized] public static readonly float MaxCameraAngleV = 30;

        public int Main_Width = 180;
        public int Main_Height = 150;
        public float Main_OrthoSize = 0.1f;
        public float Main_CameraPosX = 0.5f;
        public float Main_CameraPosY = 0.5f;
        public float Main_CameraAngleH = 0;
        public float Main_CameraAngleV = 0;

        [NonSerialized] public static readonly int Main_MinWidth = 125;
        [NonSerialized] public static readonly int Main_MaxWidth = 300;
        [NonSerialized] public static readonly int Main_MinHeight = 125;
        [NonSerialized] public static readonly int Main_MaxHeight = 300;

        public int GestureTable_Width = 120;
        public int GestureTable_Height = 100;

        [NonSerialized] public static readonly int GestureTable_MinWidth = 70;
        [NonSerialized] public static readonly int GestureTable_MaxWidth = 250;
        [NonSerialized] public static readonly int GestureTable_MinHeight = 70;
        [NonSerialized] public static readonly int GestureTable_MaxHeight = 250;

        [NonSerialized] public static readonly int ExMenu_Width = 256;
        [NonSerialized] public static readonly int ExMenu_Height = 256;
    }
}
