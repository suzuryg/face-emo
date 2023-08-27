using System.Collections.Generic;
using UnityEngine;

namespace Suzuryg.FaceEmo.Components.Settings
{
    public class AV3Setting : ScriptableObject
    {
        // Independent of VRCSDK to prevent ScriptableObject references breaking in the event of a compilation error.
        // Cast to VRCAvatarDescriptor when in use.
        public MonoBehaviour TargetAvatar;
        public List<MonoBehaviour> SubTargetAvatars = new List<MonoBehaviour>();

        public GameObject MARootObjectPrefab;

        public List<string> MouthMorphBlendShapes = new List<string>();
        public List<SkinnedMeshRenderer> AdditionalSkinnedMeshes = new List<SkinnedMeshRenderer>();
        public List<GameObject> AdditionalToggleObjects = new List<GameObject>();
        public List<GameObject> AdditionalTransformObjects = new List<GameObject>();

        public string TargetAvatarPath;
        public List<string> SubTargetAvatarPaths = new List<string>();
        public List<string> AdditionalSkinnedMeshPaths = new List<string>();
        public List<string> AdditionalToggleObjectPaths = new List<string>();
        public List<string> AdditionalTransformObjectPaths = new List<string>();

        public AnimationClip AfkEnterFace;
        public AnimationClip AfkFace;
        public AnimationClip AfkExitFace;

        public bool SmoothAnalogFist = true;
        public double TransitionDurationSeconds = 0.1;
        public bool GenerateExMenuThumbnails = true;
        public float GammaCorrectionValueForExMenuThumbnails = 1.35f;
        public bool ReplaceBlink = true;
        public bool DisableTrackingControls = true;
        public bool AddParameterPrefix = true;
        public bool MatchAvatarWriteDefaults = true;

        public bool AddConfig_EmoteSelect = true;
        public bool AddConfig_BlinkOff = true;
        public bool AddConfig_DanceGimmick = true;
        public bool AddConfig_ContactLock = true;
        public bool AddConfig_Override = true;
        public bool AddConfig_Voice = true;
        public bool AddConfig_HandPattern_Swap = true;
        public bool AddConfig_HandPattern_DisableLeft = true;
        public bool AddConfig_HandPattern_DisableRight = true;
        public bool AddConfig_Controller_Quest = true;
        public bool AddConfig_Controller_Index = true;

        public bool DefaultValue_ContactLock = false;
        public bool DefaultValue_Override = true;
        public bool DefaultValue_Voice = false;
        public bool DefaultValue_HandPattern_Swap = false;
        public bool DefaultValue_HandPattern_DisableLeft = false;
        public bool DefaultValue_HandPattern_DisableRight = false;
        public bool DefaultValue_Controller_Quest = false;
        public bool DefaultValue_Controller_Index = false;

        public bool ExpressionDefaults_ChangeDefaultFace = false;
        public bool ExpressionDefaults_UseAnimationNameAsDisplayName = false;
        public bool ExpressionDefaults_EyeTrackingEnabled = true;
        public bool ExpressionDefaults_MouthTrackingEnabled = true;
        public bool ExpressionDefaults_BlinkEnabled = true;
        public bool ExpressionDefaults_MouthMorphCancelerEnabled = true;

        public string LastOpenedAnimationPath;
        public string LastSavedAnimationPath;
    }
}
