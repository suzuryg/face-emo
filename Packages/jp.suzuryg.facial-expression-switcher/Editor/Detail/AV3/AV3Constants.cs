using Suzuryg.FacialExpressionSwitcher.Domain;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class AV3Constants
    {
        public static readonly string LayerName_Base = "Base";
        public static readonly string LayerName_DanceGimickControl = "DANCE GIMICK CONTROL";
        public static readonly string LayerName_InputConverterL = "INPUT CONVERTER L";
        public static readonly string LayerName_InputConverterR = "INPUT CONVERTER R";
        public static readonly string LayerName_FaceEmoteLock = "FACE EMOTE LOCK";
        public static readonly string LayerName_FaceEmoteControl = "FACE EMOTE CONTROL";
        public static readonly string LayerName_FaceEmoteSetControl = "FACE EMOTE SET CONTROL";
        public static readonly string LayerName_DefaultFace = "[ USER EDIT ] DEFAULT FACE";
        public static readonly string LayerName_FaceEmotePlayer = "[ USER EDIT ] FACE EMOTE PLAYER";
        public static readonly string LayerName_FaceEmoteOverride = "[ USER EDIT ] FACE EMOTE OVERRIDE";
        public static readonly string LayerName_Blink = "BLINK";
        public static readonly string LayerName_MouthMorphCanceler = "MOUTH MORPH CANCELLER";
        public static readonly string LayerName_LocalIndicatorSound = "LOCAL INDICATOR SOUND";

        public static readonly string StateName_BlinkEnabled = "ENABLE";
        public static readonly string StateName_MouthMorphCancelerEnabled = "ENABLE";

        // Reserved
        public static readonly string ParamName_GestureLeftWeight = "GestureLeftWeight";
        public static readonly string ParamName_GestureRightWeight = "GestureRightWeight";

        // Config (Saved) (Bool)
        public static readonly string ParamName_CN_CONTROLLER_TYPE_QUEST = "CN_CONTROLLER_TYPE_QUEST";
        public static readonly string ParamName_CN_CONTROLLER_TYPE_INDEX = "CN_CONTROLLER_TYPE_INDEX";
        public static readonly string ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT = "CN_EMOTE_SELECT_PRIORITY_LEFT";
        public static readonly string ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT = "CN_EMOTE_SELECT_PRIORITY_RIGHT";
        public static readonly string ParamName_CN_EMOTE_SELECT_ONLY_LEFT = "CN_EMOTE_SELECT_ONLY_LEFT";
        public static readonly string ParamName_CN_EMOTE_SELECT_ONLY_RIGHT = "CN_EMOTE_SELECT_ONLY_RIGHT";
        public static readonly string ParamName_CN_CONTACT_EMOTE_LOCK_ENABLE = "CN_CONTACT_EMOTE_LOCK_ENABLE";
        public static readonly string ParamName_SYNC_CN_EMOTE_OVERRIDE_ENABLE = "SYNC_CN_EMOTE_OVERRIDE_ENABLE";
        public static readonly string ParamName_SYNC_CN_WAIT_FACE_EMOTE_BY_VOICE = "SYNC_CN_WAIT_FACE_EMOTE_BY_VOICE";


        // Config (Saved) (Int)
        public static readonly string ParamName_EM_EMOTE_PATTERN = "EM_EMOTE_PATTERN";

        // Config (Not saved) (Bool)
        public static readonly string ParamName_CN_EMOTE_LOCK_ENABLE = "CN_EMOTE_LOCK_ENABLE";
        public static readonly string ParamName_SYNC_CN_FORCE_BLINK_DISABLE = "SYNC_CN_FORCE_BLINK_DISABLE";
        public static readonly string ParamName_SYNC_CN_DANCE_GIMMICK_ENABLE = "SYNC_CN_DANCE_GIMMICK_ENABLE";

        // Synced (Int)
        public static readonly string ParamName_SYNC_EM_EMOTE = "SYNC_EM_EMOTE";

        // Not synced
        public static readonly string ParamName_EM_EMOTE_SELECT_L = "EM_EMOTE_SELECT_L";
        public static readonly string ParamName_EM_EMOTE_SELECT_R = "EM_EMOTE_SELECT_R";
        public static readonly string ParamName_EM_EMOTE_PRESELECT = "EM_EMOTE_PRESELECT";
        public static readonly string ParamName_CN_BLINK_ENABLE = "CN_BLINK_ENABLE";
        public static readonly string ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE = "CN_MOUTH_MORPH_CANCEL_ENABLE";
        public static readonly string ParamName_CN_EMOTE_OVERRIDE = "CN_EMOTE_OVERRIDE";
        public static readonly string ParamName_EV_PLAY_INDICATOR_SOUND = "EV_PLAY_INDICATOR_SOUND";
        public static readonly string ParamName_CNST_TOUCH_NADENADE_POINT = "CNST_TOUCH_NADENADE_POINT";
        public static readonly string ParamName_CNST_TOUCH_EMOTE_LOCK_TRIGGER_L = "CNST_TOUCH_EMOTE_LOCK_TRIGGER_L";
        public static readonly string ParamName_CNST_TOUCH_EMOTE_LOCK_TRIGGER_R = "CNST_TOUCH_EMOTE_LOCK_TRIGGER_R";
        // public static readonly string ParamName_CN_IS_ACTION_ACTIVE = "CN_IS_ACTION_ACTIVE"; // Do not rename (to work with action layer)
        public static readonly string ParamName_GestureLWSmoothing = "_Hai_GestureLWSmoothing"; // Do not rename (to work with blend tree)
        public static readonly string ParamName_GestureRWSmoothing = "_Hai_GestureRWSmoothing"; // Do not rename (to work with blend tree)

        public static readonly string ParameterPrefix = "FES_";

        public static readonly string RootMenuName = DomainConstants.SystemName;
        public static readonly string MARootObjectName = DomainConstants.SystemName + "Object";

        public static readonly int MaxEmoteNum = 256;
        public static readonly float VoiceThreshold = 0.01f;

        public static readonly string Path_BlinkTemplate = $"{DetailConstants.ExternalDirectory}/BearsDen/CustomAnimatorControllers/__SetupFiles/Animator/Animation/FX/Blink/Blink_Enable.anim";
        public static readonly string Path_BearsDenFx = $"{DetailConstants.ExternalDirectory}/BearsDen/CustomAnimatorControllers/__SetupFiles/Animator/Controller/FX.controller";
        public static readonly string Path_FxTemplate = $"{DetailConstants.DetailDirectory}/AV3/Template/FxTemplate.controller";
        public static readonly string Path_TemplateContainer = $"{DetailConstants.DetailDirectory}/AV3/Template/FxTemplateContainer.controller";
        public static readonly string Path_EmoteLocker = $"{DetailConstants.DetailDirectory}/AV3/Template/FES_EmoteLocker.prefab";
        public static readonly string Path_IndicatorSound = $"{DetailConstants.DetailDirectory}/AV3/Template/FES_IndicatorSound.prefab";
        public static readonly string Path_EmoteOverridePrefab = $"{DetailConstants.DetailDirectory}/AV3/Template/FES_EmoteOverrideExample.prefab";
        public static readonly string Path_EmoteOverrideController = $"{DetailConstants.DetailDirectory}/AV3/Template/FES_EmoteOverrideExample.controller";
        public static readonly string Path_GeneratedDir = $"Assets/Suzuryg/{DomainConstants.SystemName}/Generated";

        public static readonly IReadOnlyList<HandGesture> EmoteSelectToGesture = new List<HandGesture>()
        {
            HandGesture.Neutral,
            HandGesture.Fist,
            HandGesture.HandOpen,
            HandGesture.Fingerpoint,
            HandGesture.Victory,
            HandGesture.RockNRoll,
            HandGesture.HandGun,
            HandGesture.ThumbsUp,
        };
    }
}
