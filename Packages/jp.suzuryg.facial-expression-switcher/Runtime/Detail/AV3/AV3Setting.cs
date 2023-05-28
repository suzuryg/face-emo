using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class AV3Setting : ScriptableObject
    {
        public VRCAvatarDescriptor TargetAvatar;
        public List<string> MouthMorphBlendShapes = new List<string>();
        public List<GameObject> AdditionalToggleObjects = new List<GameObject>();
        public List<GameObject> AdditionalTransformObjects = new List<GameObject>();

        public string TargetAvatarPath;
        public List<string> AdditionalToggleObjectPaths = new List<string>();
        public List<string> AdditionalTransformObjectPaths = new List<string>();

        public AnimationClip AfkEnterFace;
        public AnimationClip AfkFace;
        public AnimationClip AfkExitFace;

        public bool SmoothAnalogFist = true;
        public double TransitionDurationSeconds = 0.1;
        public bool GenerateModeThumbnails = true;
        public bool ReplaceBlink = true;
        public bool DisableTrackingControls = true;
        public bool AddParameterPrefix = true;

        public bool AddConfig_EmoteLock = true;
        public bool AddConfig_BlinkOff = true;
        public bool AddConfig_DanceGimmick = true;
        public bool AddConfig_ContactLock = true;
        public bool AddConfig_Override = true;
        public bool AddConfig_Voice = true;
        public bool AddConfig_HandPattern = true;
        public bool AddConfig_Controller = true;

        public bool ExpressionDefaults_ChangeDefaultFace = false;
        public bool ExpressionDefaults_UseAnimationNameAsDisplayName = false;
        public bool ExpressionDefaults_EyeTrackingEnabled = true;
        public bool ExpressionDefaults_MouthTrackingEnabled = true;
        public bool ExpressionDefaults_BlinkEnabled = true;
        public bool ExpressionDefaults_MouthMorphCancelerEnabled = true;

        public string LastOpendOrSavedAnimationPath;
    }
}
