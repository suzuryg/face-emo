using Suzuryg.FaceEmo.Domain;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace Suzuryg.FaceEmo.Components.MA
{
    [DisallowMultipleComponent]
    public class TrackingControlDisabler : RunBeforeModularAvatar
    {
        // TODO: Is asset creation needed?
        private static readonly string DestinationDir = $"Assets/Suzuryg/{DomainConstants.SystemName}/Generated";
        private static readonly string DestinationPath = DestinationDir + "/TrackingControlDisabled.controller";

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
            if (!fxExists || originalFxController == null)
            {
                // Pre-processing is not needed.
                return;
            }

            // Clone Fx controller
            // TODO: Is asset creation needed?
            // VRC_AnimatorTrackingControl is not references to the AnimatorController but values held in the AnimatorController,
            // so if the AnimatorController is copied, changing the destination asset will not affect the original AnimatorController.
            if (!AssetDatabase.IsValidFolder(DestinationDir))
            { CreateFolder(); }

            var originalPath = AssetDatabase.GetAssetPath(originalFxController);
            if (!AssetDatabase.CopyAsset(originalPath, DestinationPath)) // TODO: Suppress warnings (missing script)
            { ShowErrorMessage(originalPath); }
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
        private static void CreateFolder()
        {
            CreateFolderRecursively(DestinationDir);
            if (AssetDatabase.IsValidFolder(DestinationDir))
            {
                Debug.Log("Folder created: " + DestinationDir);
            }
            else
            {
                Debug.LogError("Failed to create folder: " + DestinationDir);
            }
        }

        private static void ShowErrorMessage(string originalPath)
        {
            string errorMessage = string.Empty;
            var currentCulture = CultureInfo.CurrentCulture;
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
            errorMessage += $"\n\n{originalPath} -> {DestinationPath}";
            EditorUtility.DisplayDialog(DomainConstants.SystemName, errorMessage, "OK");
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

        // https://hacchi-man.hatenablog.com/entry/2020/08/23/220000
        private static void CreateFolderRecursively(string path)
        {
            // If it doesn't start with Assets, it can't be processed.
            if (!path.StartsWith("Assets/"))
            {
                return;
            }

            // AssetDatabase, so the delimiter is /.
            var dirs = path.Split('/');
            var combinePath = dirs[0];

            // Skip the Assets part.
            foreach (var dir in dirs.Skip(1))
            {
                // Check existence of directory
                if (!AssetDatabase.IsValidFolder(combinePath + '/' + dir))
                {
                    AssetDatabase.CreateFolder(combinePath, dir);
                }
                combinePath += '/' + dir;
            }
        }
#endif
    }
}
