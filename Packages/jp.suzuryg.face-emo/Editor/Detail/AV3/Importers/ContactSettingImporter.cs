#if USE_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Domain;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    public class ContactSettingImporter
    {
        private AV3Setting _av3Setting;

        private List<BlendShape> _faceBlendShapes = new List<BlendShape>();

        public ContactSettingImporter(AV3Setting av3Setting)
        {
            _av3Setting = av3Setting;
        }

        public List<VRCContactReceiver> Import(VRCAvatarDescriptor avatarDescriptor)
        {
            _faceBlendShapes = ImportUtility.GetAllFaceBlendShapeValues(avatarDescriptor, _av3Setting, excludeBlink: false, excludeLipSync: true).Select(x => x.Key).ToList();

            var found = avatarDescriptor.gameObject.GetComponentsInChildren(typeof(VRCContactReceiver), includeInactive: true);
            var paramToContacts = new Dictionary<string, List<VRCContactReceiver>>();
            foreach (var item in found)
            {
                if (item is VRCContactReceiver contactReceiver && contactReceiver != null &&
                    contactReceiver.parameter != AV3Constants.ParamName_CNST_TOUCH_NADENADE_POINT &&
                    contactReceiver.parameter != AV3Constants.ParamName_CNST_TOUCH_EMOTE_LOCK_TRIGGER_L &&
                    contactReceiver.parameter != AV3Constants.ParamName_CNST_TOUCH_EMOTE_LOCK_TRIGGER_R)
                {
                    if (!paramToContacts.ContainsKey(contactReceiver.parameter)) { paramToContacts[contactReceiver.parameter] = new List<VRCContactReceiver>(); }
                    paramToContacts[contactReceiver.parameter].Add(contactReceiver);
                }
            }

            var controllers = new List<AnimatorController>();
            var fx = ImportUtility.GetFxLayer(avatarDescriptor);
            if (fx != null) { controllers.Add(fx); }

#if USE_MODULAR_AVATAR
            var mergeAnimatorComponents = avatarDescriptor.gameObject.GetComponentsInChildren(typeof(ModularAvatarMergeAnimator), includeInactive: true);
            foreach (var item in mergeAnimatorComponents)
            {
                if (item is ModularAvatarMergeAnimator mergeAnimator && mergeAnimator != null &&
                    mergeAnimator.animator is AnimatorController animatorController && animatorController != null)
                {
                    controllers.Add(animatorController);
                }
            }
#endif

            IEnumerable<string> usedParams = new List<string>();
            foreach (var controller in controllers)
            {
                if (!controller.parameters.Any(param =>
                    paramToContacts.ContainsKey(param.name) &&
                    paramToContacts[param.name].Any(contcact => IsValidType(param, contcact))))
                {
                    continue;
                }

                foreach (var layer in controller.layers)
                {
                    usedParams = usedParams.Concat(GetUsedContactParams(layer.stateMachine, paramToContacts.Keys));
                }
            }
            usedParams = usedParams.Distinct();

            var validContacts = new List<VRCContactReceiver>();
            var controllerParams = controllers.SelectMany(x => x.parameters);
            foreach (var param in usedParams)
            {
                var matched = controllerParams.Where(x => x.name == param);

                if (paramToContacts.ContainsKey(param))
                {
                    foreach (var contact in paramToContacts[param])
                    {
                        if (matched.Any(x => IsValidType(x, contact)) &&
                            !matched.Any(x => !IsValidType(x, contact)))
                        {
                            validContacts.Add(contact);
                        }
                    }
                }
            }

            foreach (var item in validContacts)
            {
                if (!_av3Setting.ContactReceivers.Contains(item))
                {
                    _av3Setting.ContactReceivers.Add(item);
                }
            }

            return validContacts;
        }

        private List<string> GetUsedContactParams(AnimatorStateMachine stateMachine, IEnumerable<string> contactParams)
        {
            IEnumerable<string> usedParams = new List<string>();
            if (stateMachine == null) { return usedParams.ToList(); }

            foreach (var transition in stateMachine.entryTransitions)
            {
                usedParams = usedParams.Concat(GetUsedContactParamsSub(transition, contactParams));
            }

            foreach (var transition in stateMachine.anyStateTransitions)
            {
                usedParams = usedParams.Concat(GetUsedContactParamsSub(transition, contactParams));
            }

            foreach (var state in stateMachine.states)
            {
                if (state.state == null) { continue; }

                foreach (var transition in state.state.transitions)
                {
                    usedParams = usedParams.Concat(GetUsedContactParamsSub(transition, contactParams));
                }
            }

            foreach (var subMachine in stateMachine.stateMachines)
            {
                usedParams = usedParams.Concat(GetUsedContactParams(subMachine.stateMachine, contactParams));
            }

            return usedParams.Distinct().ToList();
        }

        private List<string> GetUsedContactParamsSub(AnimatorTransitionBase transition, IEnumerable<string> contactParams)
        {
            var usedParams = new List<string>();

            if (transition.destinationState == null) { return usedParams; }

            foreach (var condition in transition.conditions)
            {
                if (contactParams.Contains(condition.parameter) &&
                    ImportUtility.IsFaceMotion(transition.destinationState.motion, _faceBlendShapes))
                {
                    usedParams.Add(condition.parameter);
                }
            }

            return usedParams;
        }

        private static bool IsValidType(AnimatorControllerParameter parameter, VRCContactReceiver receiver)
        {
            if (parameter == null || receiver == null) { return false; }

            // Match the parameter type used by FxGenerator
            switch (receiver.receiverType)
            {
                case ContactReceiver.ReceiverType.Constant:
                    return parameter.type == AnimatorControllerParameterType.Bool;
                case ContactReceiver.ReceiverType.OnEnter:
                    return parameter.type == AnimatorControllerParameterType.Bool;
                case ContactReceiver.ReceiverType.Proximity:
                    return parameter.type == AnimatorControllerParameterType.Float;
                default:
                    return false;
            }
        }
    }
}
