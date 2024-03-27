using System.Collections.Generic;
using System.Linq;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    public static class FxParameterChecker
    {
        public static bool CheckPrefixNeeds(VRCAvatarDescriptor avatarDescriptor)
        {
            var fx = ImportUtility.GetFxLayer(avatarDescriptor);
            if (fx == null || fx.parameters == null)
            {
                return false;
            }

            var parameterNames = fx.parameters.Where(x => x != null).Select(x => x.name);
            if (!parameterNames.Any())
            {
                return false;
            }

            var hashSet = new HashSet<string>(parameterNames);
            return
                hashSet.Contains(AV3Constants.ParamName_CN_CONTROLLER_TYPE_QUEST) ||
                hashSet.Contains(AV3Constants.ParamName_CN_CONTROLLER_TYPE_INDEX) ||
                hashSet.Contains(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR) ||
                hashSet.Contains(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT) ||
                hashSet.Contains(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT) ||
                hashSet.Contains(AV3Constants.ParamName_CN_CONTACT_EMOTE_LOCK_ENABLE) ||
                hashSet.Contains(AV3Constants.ParamName_SYNC_CN_EMOTE_OVERRIDE_ENABLE) ||
                hashSet.Contains(AV3Constants.ParamName_SYNC_CN_WAIT_FACE_EMOTE_BY_VOICE) ||

                hashSet.Contains(AV3Constants.ParamName_EM_EMOTE_PATTERN) ||

                hashSet.Contains(AV3Constants.ParamName_CN_EMOTE_LOCK_ENABLE) ||
                hashSet.Contains(AV3Constants.ParamName_CN_EMOTE_PRELOCK_ENABLE) ||
                hashSet.Contains(AV3Constants.ParamName_SYNC_CN_FORCE_BLINK_DISABLE) ||
                hashSet.Contains(AV3Constants.ParamName_SYNC_CN_DANCE_GIMMICK_ENABLE) ||

                hashSet.Contains(AV3Constants.ParamName_SYNC_EM_EMOTE) ||

                hashSet.Contains(AV3Constants.ParamName_DUMMY_CONSTANT_FALSE) ||
                hashSet.Contains(AV3Constants.ParamName_EM_EMOTE_SELECT_L) ||
                hashSet.Contains(AV3Constants.ParamName_EM_EMOTE_SELECT_R) ||
                hashSet.Contains(AV3Constants.ParamName_EM_EMOTE_PRESELECT) ||
                hashSet.Contains(AV3Constants.ParamName_CN_BLINK_ENABLE) ||
                hashSet.Contains(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE) ||
                hashSet.Contains(AV3Constants.ParamName_CN_EMOTE_OVERRIDE) ||
                hashSet.Contains(AV3Constants.ParamName_CN_BYPASS) ||
                hashSet.Contains(AV3Constants.ParamName_EV_PLAY_INDICATOR_SOUND) ||
                hashSet.Contains(AV3Constants.ParamName_CNST_TOUCH_NADENADE_POINT) ||
                hashSet.Contains(AV3Constants.ParamName_CNST_TOUCH_EMOTE_LOCK_TRIGGER_L) ||
                hashSet.Contains(AV3Constants.ParamName_CNST_TOUCH_EMOTE_LOCK_TRIGGER_R);
        }
    }
}

