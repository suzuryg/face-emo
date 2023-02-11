using Suzuryg.FacialExpressionSwitcher.Domain;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class AV3Constants
    {
        public static readonly string LayerName_FaceEmoteControl = "[ FES ] FACE EMOTE CONTROL";
        public static readonly string LayerName_FaceEmoteSetControl = "[ FES ] FACE EMOTE SET CONTROL";
        public static readonly string LayerName_DefaultFace = "[ FES ] DEFAULT FACE";
        public static readonly string LayerName_FaceEmotePlayer = "[ FES ] FACE EMOTE PLAYER";
        public static readonly string LayerName_Blink = "BLINK";
        public static readonly string LayerName_MouthMorphCanceler = "MOUTH MORPH CANCELLER";

        public static readonly string StateName_BlinkEnabled = "ENABLE";
        public static readonly string StateName_MouthMorphCancelerEnabled = "ENABLE";

        public static readonly string ParamName_CN_EXPRESSION_PARAMETER_LOADING_COMP = "CN_EXPRESSION_PARAMETER_LOADING_COMP";
        public static readonly string ParamName_EM_EMOTE_SELECT_L = "EM_EMOTE_SELECT_L";
        public static readonly string ParamName_EM_EMOTE_SELECT_R = "EM_EMOTE_SELECT_R";
        public static readonly string ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT = "CN_EMOTE_SELECT_PRIORITY_LEFT";
        public static readonly string ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT = "CN_EMOTE_SELECT_PRIORITY_RIGHT";
        public static readonly string ParamName_CN_EMOTE_SELECT_ONLY_LEFT = "CN_EMOTE_SELECT_ONLY_LEFT";
        public static readonly string ParamName_CN_EMOTE_SELECT_ONLY_RIGHT = "CN_EMOTE_SELECT_ONLY_RIGHT";
        public static readonly string ParamName_EM_EMOTE_PRESELECT = "EM_EMOTE_PRESELECT";
        public static readonly string ParamName_CN_EMOTE_LOCK_ENABLE = "CN_EMOTE_LOCK_ENABLE";
        public static readonly string ParamName_CN_EMOTE_PRELOCK_ENABLE = "CN_EMOTE_PRELOCK_ENABLE";
        public static readonly string ParamName_EM_EMOTE_PATTERN = "EM_EMOTE_PATTERN";
        public static readonly string ParamName_SYNC_EM_EMOTE = "SYNC_EM_EMOTE";
        public static readonly string ParamName_CN_BLINK_ENABLE = "CN_BLINK_ENABLE";
        public static readonly string ParamName_SYNC_CN_FORCE_BLINK_DISABLE = "SYNC_CN_FORCE_BLINK_DISABLE";
        public static readonly string ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE = "CN_MOUTH_MORPH_CANCEL_ENABLE";
        public static readonly string ParamName_CN_EMOTE_OVERRIDE = "CN_EMOTE_OVERRIDE";
        public static readonly string ParamName_SYNC_CN_EMOTE_OVERRIDE_ENABLE = "SYNC_CN_EMOTE_OVERRIDE_ENABLE";
        public static readonly string ParamName_SYNC_CN_DANCE_GIMMICK_ENABLE = "SYNC_CN_DANCE_GIMMICK_ENABLE";
        public static readonly string ParamName_GestureLeftWeight = "GestureLeftWeight";
        public static readonly string ParamName_GestureRightWeight = "GestureRightWeight";
        public static readonly string ParamName_GestureLWSmoothing = "_Hai_GestureLWSmoothing";
        public static readonly string ParamName_GestureRWSmoothing = "_Hai_GestureRWSmoothing";

        public static readonly int MaxEmoteNum = 256;
        public static readonly float VoiceThreshold = 0.01f;
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
