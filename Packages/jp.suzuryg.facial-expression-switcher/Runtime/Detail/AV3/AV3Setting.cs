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
        [HideInInspector] public VRCAvatarDescriptor TargetAvatar;

        [HideInInspector] public bool WriteDefaults = false;
        [HideInInspector] public bool SmoothAnalogFist = true;
        [HideInInspector] public double TransitionDurationSeconds = 0.1;
        [HideInInspector] public bool ReplaceBlink = true;
        [HideInInspector] public bool DisableTrackingControls = true;
        [HideInInspector] public bool DoNotTransitionWhenSpeaking = false;

        [HideInInspector] public bool AddConfig_EmoteLock = true;
        [HideInInspector] public bool AddConfig_BlinkOff = true;
        [HideInInspector] public bool AddConfig_DanceGimmick = true;
        [HideInInspector] public bool AddConfig_ContactLock = true;
        [HideInInspector] public bool AddConfig_Override = true;
        [HideInInspector] public bool AddConfig_HandPriority = true;
        [HideInInspector] public bool AddConfig_Controller = true;
    }
}
