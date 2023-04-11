using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class AV3Setting : MonoBehaviour
    {
        public VRCAvatarDescriptor TargetAvatar;
        public List<string> MouthMorphBlendShapes = new List<string>();

        public bool SmoothAnalogFist = true;
        public double TransitionDurationSeconds = 0.1;
        public bool ReplaceBlink = true;
        public bool DisableTrackingControls = true;
        public bool DoNotTransitionWhenSpeaking = false;

        public bool AddConfig_EmoteLock = true;
        public bool AddConfig_BlinkOff = true;
        public bool AddConfig_DanceGimmick = true;
        public bool AddConfig_ContactLock = true;
        public bool AddConfig_Override = true;
        public bool AddConfig_HandPriority = true;
        public bool AddConfig_Controller = true;
    }
}
