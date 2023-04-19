using System;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Drawing
{
    public class ThumbnailSetting : MonoBehaviour
    {
        public int Main_Width = 180;
        public int Main_Height = 150;
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
