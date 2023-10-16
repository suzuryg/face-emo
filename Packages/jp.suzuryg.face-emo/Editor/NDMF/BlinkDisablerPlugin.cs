#if USE_NDMF
using nadena.dev.ndmf;
using Suzuryg.FaceEmo.NDMF;
using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

[assembly: ExportsPlugin(typeof(BlinkDisablerPlugin))]

namespace Suzuryg.FaceEmo.NDMF
{
    public class BlinkDisablerPlugin : Plugin<BlinkDisablerPlugin>
    {
        public override string QualifiedName => "jp.suzuryg.face-emo.blink-disabler";

        public override string DisplayName => "FaceEmo BlinkDisabler";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming).Run("Disable blink", ctx =>
            {
                var obj = ctx.AvatarRootObject.GetComponentInChildren<BlinkDisabler>();
                if (obj != null)
                {
                    try
                    {
                        ctx.AvatarDescriptor.customEyeLookSettings.eyelidType = VRCAvatarDescriptor.EyelidType.None;
                        Debug.Log($"[FaceEmo] Succeeded to disable blink.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[FaceEmo] Failed to disable blink. {ex?.ToString()}");
                        ShowErrorMessage(ex);
                    }
                }
            });
        }

        private static void ShowErrorMessage(Exception ex)
        {
            string errorMessage = string.Empty;
            var currentCulture = CultureInfo.CurrentCulture;
            if (currentCulture.Name == "ja-JP")
            {
                errorMessage += "まばたきの無効化に失敗しました。\n" +
                    "表情が正しく動かない場合、AvatarDescriptorのEyelid TypeをNoneに変更してください。";
            }
            else
            {
                errorMessage += "Failed to disable blink.\n" +
                    "If the facial expressions do not work correctly, please change the Eyelid Type in the AvatarDescriptor to None.";
            }

            errorMessage += $"\n\n{ex?.Message}";

            EditorUtility.DisplayDialog("FaceEmo", errorMessage, "OK");
        }
    }
}
#endif

