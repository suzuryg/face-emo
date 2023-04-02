using Suzuryg.FacialExpressionSwitcher.Domain;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    [DisallowMultipleComponent]
    public class TrackingControlDisabler : RunBeforeModularAvatar
    {
        // TODO: Is asset creation needed?
        private static readonly string DestinationPath = $"Assets/Suzuryg/{DomainConstants.SystemName}/Generated/TrackingControlDisabled.controller";

        public override void OnPreProcessAvatar()
        {
#if UNITY_EDITOR
            var avatarDescriptor = GetAvatarDescriptor();

            // Get Fx layer
            var fxExists = false;
            int fxIndex;
            for (fxIndex = 0; fxIndex < avatarDescriptor.baseAnimationLayers.Length; fxIndex++)
            {
                if (avatarDescriptor.baseAnimationLayers[fxIndex].type == VRCAvatarDescriptor.AnimLayerType.FX)
                {
                    fxExists = true;
                    break;
                }
            }

            var originalFxController = avatarDescriptor.baseAnimationLayers[fxIndex].animatorController;
            if (!fxExists || originalFxController is null)
            {
                // Pre-processing is not needed.
                return;
            }

            // Clone Fx controller
            // TODO: Is asset creation needed?
            var originalPath = AssetDatabase.GetAssetPath(originalFxController);
            if (!AssetDatabase.CopyAsset(originalPath, DestinationPath)) // TODO: Suppress warnings (missing script)
            {
                throw new FacialExpressionSwitcherException("Faild to clone Fx controller.");
            }
            var clonedFxController = AssetDatabase.LoadAssetAtPath<AnimatorController>(DestinationPath);

            // Disable tracking controls
            foreach (var layer in clonedFxController.layers)
            {
                DisableTrackingControlRecursively(layer.stateMachine);
            }

            avatarDescriptor.baseAnimationLayers[fxIndex].animatorController = clonedFxController;
#endif
        }

#if UNITY_EDITOR
        private static void DisableTrackingControlRecursively(AnimatorStateMachine animatorStateMachine)
        {
            foreach (var childStateMachine in animatorStateMachine.stateMachines)
            {
                DisableTrackingControlRecursively(childStateMachine.stateMachine);
            }

            var behaviours = new List<StateMachineBehaviour>(animatorStateMachine.behaviours);
            foreach (var state in animatorStateMachine.states)
            {
                foreach (var behaviour in state.state.behaviours)
                {
                    behaviours.Add(behaviour);
                }
            }

            foreach (var behaviour in behaviours)
            {
                if (behaviour is VRC_AnimatorTrackingControl trackingControl)
                {
                    trackingControl.trackingEyes = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    trackingControl.trackingMouth = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                }
            }
        }
#endif
    }
}
