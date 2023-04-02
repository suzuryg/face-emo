using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class AV3Setting : MonoBehaviour
    {
        [HideInInspector] public bool WriteDefaults = false;
        [HideInInspector] public bool SmoothAnalogFist = true;
        [HideInInspector] public double TransitionDurationSeconds = 0.1;
        [HideInInspector] public bool ReplaceBlink = true;
        [HideInInspector] public bool DisableTrackingControls = true;
        [HideInInspector] public bool DoNotTransitionWhenSpeaking = false;
    }
}
