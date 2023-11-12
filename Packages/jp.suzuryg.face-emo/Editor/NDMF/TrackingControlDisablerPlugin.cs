#if USE_NDMF
using nadena.dev.ndmf;
using Suzuryg.FaceEmo.NDMF;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

[assembly: ExportsPlugin(typeof(TrackingControlDisablerPlugin))]

namespace Suzuryg.FaceEmo.NDMF
{
    public class TrackingControlDisablerPlugin : Plugin<TrackingControlDisablerPlugin>
    {
        public override string QualifiedName => "jp.suzuryg.face-emo.tracking-control-disabler";

        public override string DisplayName => "FaceEmo TrackingControlDisabler";

        protected override void Configure()
        {
            // Run after the controller has been cloned in the "Resolving" phase
            InPhase(BuildPhase.Generating).Run("Disable tracking controls", ctx =>
            {
                var obj = ctx.AvatarRootObject.GetComponentInChildren<TrackingControlDisabler>();
                if (obj != null)
                {
                    try
                    {
#if USE_MODULAR_AVATAR
                        var fx = GetFxLayer(ctx.AvatarDescriptor);
                        if (fx != null)
                        {
                            foreach (var layer in fx.layers)
                            {
                                DisableTrackingControlRecursively(layer.stateMachine);
                            }
                        }
                        MonoBehaviour.DestroyImmediate(obj);
                        Debug.Log($"[FaceEmo] Succeeded to disable tracking controls.");
#else
                        throw new InvalidOperationException("[Error]\n" +
                            "Please install Modular Avatar 1.8.0 or later.\n" +
                            "Modular Avatar 1.8.0 以降をインストールしてください。");
#endif
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[FaceEmo] Failed to disable tracking controls. {ex?.ToString()}");
                        ShowErrorMessage(ex);
                    }
                }
            });
        }

        private static AnimatorController GetFxLayer(VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor != null)
            {
                foreach (var layer in avatarDescriptor.baseAnimationLayers)
                {
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.FX)
                    {
                        return layer.animatorController as AnimatorController;
                    }
                }
            }
            return null;
        }

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
                if (behaviour is VRC_AnimatorTrackingControl trackingControl && trackingControl != null)
                {
                    trackingControl.trackingEyes = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    trackingControl.trackingMouth = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                }
            }
        }

        private static void ShowErrorMessage(Exception ex)
        {
            string errorMessage = string.Empty;
            var currentCulture = CultureInfo.CurrentUICulture;
            if (currentCulture.Name == "ja-JP")
            {
                errorMessage += "VRC Animator Tracing Controlの無効化に失敗しました。\n" +
                    "表情が正しく動かない場合、元々のアバターのFXレイヤーから表情操作のレイヤーを削除してください。\n" +
                    "レイヤーを削除する際は、作業の前に必ずバックアップを作成してください。";
            }
            else
            {
                errorMessage += "Failed to disable VRC Animator Tracing Control.\n" +
                    "If the facial expressions do not work correctly, please delete the facial expression control layers from the original avatar's FX layer.\n" +
                    "Be sure to make a backup before deleting the layer.";
            }

            errorMessage += $"\n\n{ex?.Message}";

            EditorUtility.DisplayDialog("FaceEmo", errorMessage, "OK");
        }
    }
}
#endif

