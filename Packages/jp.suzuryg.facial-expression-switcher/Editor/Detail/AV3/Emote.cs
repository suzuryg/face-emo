using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class Emote
    {
        public string Name { get; set; }
        public int ModeIndex { get; set; }
        public int EmoteIndex { get; set; }
        AnimationClip Clip { get; set; }
    }
}
