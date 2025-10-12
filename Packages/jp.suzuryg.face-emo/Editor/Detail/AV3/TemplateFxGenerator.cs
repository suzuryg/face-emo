using AnimatorAsCode.V0;
using AnimatorAsCode.V0.Extensions.VRChat;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using Suzuryg.FaceEmo.Domain;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public class TemplateFxGenerator
    {
        private static readonly IReadOnlyList<HandPattern> Patterns = Enum.GetValues(typeof(HandPattern)).Cast<HandPattern>().ToList();

        private enum HandPattern
        {
            Normal_Left_Enabled_Right_Enabled,
            Normal_Left_Enabled_Right_Disabled,
            Normal_Left_Disabled_Right_Enabled,
            Normal_Left_Disabled_Right_Disabled,
            Swap_Left_Enabled_Right_Enabled,
            Swap_Left_Enabled_Right_Disabled,
            Swap_Left_Disabled_Right_Enabled,
            Swap_Left_Disabled_Right_Disabled,
        }

        // After importing CustomAnimatorControllers, deep copy with CopyAssetsWithDependency (to change GUID).
        [MenuItem("FaceEmo/Debug/GenerateTemplateFx", false, 200)]
        public static void Generate()
        {
            try
            {
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, "Start generation Template FX controller.", 0);

                // Copy template FX controller
                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(AV3Constants.Path_BearsDenFx) == null)
                {
                    throw new FaceEmoException("Original template was not found.");
                }
                else if (!AssetDatabase.CopyAsset(AV3Constants.Path_BearsDenFx, AV3Constants.Path_FxTemplate))
                {
                    throw new FaceEmoException("Failed to copy FX template (basic).");
                }
                var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AV3Constants.Path_FxTemplate);

                // Create container
                var templateContainer = AssetDatabase.LoadAssetAtPath<AnimatorController>(AV3Constants.Path_TemplateContainer);
                if (templateContainer != null)
                {
                    AssetDatabase.DeleteAsset(AV3Constants.Path_TemplateContainer);
                }
                templateContainer = new AnimatorController();
                AssetDatabase.CreateAsset(templateContainer, AV3Constants.Path_TemplateContainer);

                var aac = AacV0.Create(GetConfiguration(templateContainer, writeDefaults: false));

                AV3Utility.RemoveLayer(animatorController, AV3Constants.LayerName_Base);
                AV3Utility.RemoveLayer(animatorController, AV3Constants.LayerName_FaceEmoteOverride);
                AV3Utility.RemoveLayer(animatorController, AV3Constants.LayerName_LocalIndicatorSound);
                AV3Utility.MoveLayer(animatorController, AV3Constants.LayerName_DefaultFace, 0);

                // If not initialized, layer replacement process will be slower.
                aac.CreateSupportingArbitraryControllerLayer(animatorController, AV3Constants.LayerName_FaceEmoteSetControl);
                aac.CreateSupportingArbitraryControllerLayer(animatorController, AV3Constants.LayerName_DefaultFace);
                aac.CreateSupportingArbitraryControllerLayer(animatorController, AV3Constants.LayerName_FaceEmotePlayer);

                GenerateFaceEmoteControlLayer(aac, animatorController);

                // Remove unused parameters
                RemoveParameters(animatorController);

                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, "Done!", 1);
                EditorUtility.DisplayDialog(DomainConstants.SystemName, "Generation Succeeded!", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void GenerateFaceEmoteControlLayer(AacFlBase aac, AnimatorController templateFx)
        {
            // Create or replace layer
            var layerName = AV3Constants.LayerName_FaceEmoteControl;
            var layer = aac.CreateSupportingArbitraryControllerLayer(templateFx, layerName);
            AV3Utility.SetLayerWeight(templateFx, layerName, 0);
            layer.StateMachine.WithEntryPosition(0, -1).WithAnyStatePosition(0, -2).WithExitPosition(3, 0);

            // Create initializing states
            var init = layer.NewState("INIT", 0, 0);
            var gate = layer.NewState("GATE", 1, 0);
            init.TransitionsTo(gate).
                When(layer.Av3().IsLocal.IsEqualTo(true));

            // Create Mode states machines
            foreach (var pattern in Patterns)
            {
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{AV3Constants.LayerName_FaceEmoteControl}\" layer...",
                    (float)pattern / Patterns.Count);

                var priorityStateMachine = layer.NewSubStateMachine(pattern.ToString(), 2, (int)pattern)
                    .WithEntryPosition(0, 0).WithAnyStatePosition(0, -1).WithParentStateMachinePosition(0, -2).WithExitPosition(2, 0);

                switch (pattern)
                {
                    case HandPattern.Normal_Left_Enabled_Right_Enabled:
                        //  Mode select
                        gate.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsFalse());
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsFalse());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)  .IsTrue()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT) .IsTrue());
                        break;
                    case HandPattern.Normal_Left_Enabled_Right_Disabled:
                        //  Mode select
                        gate.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsTrue());
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsTrue());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)  .IsTrue()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT) .IsFalse());
                        break;
                    case HandPattern.Normal_Left_Disabled_Right_Enabled:
                        //  Mode select
                        gate.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsFalse());
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsFalse());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)  .IsFalse()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT) .IsTrue());
                        break;
                    case HandPattern.Normal_Left_Disabled_Right_Disabled:
                        //  Mode select
                        gate.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsTrue());
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsTrue());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)  .IsFalse()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT) .IsFalse());
                        break;
                    case HandPattern.Swap_Left_Enabled_Right_Enabled:
                        //  Mode select
                        gate.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsFalse());
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsFalse());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)  .IsTrue()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT) .IsTrue());
                        break;
                    case HandPattern.Swap_Left_Enabled_Right_Disabled:
                        //  Mode select
                        gate.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsTrue());
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsTrue());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)  .IsTrue()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT) .IsFalse());
                        break;
                    case HandPattern.Swap_Left_Disabled_Right_Enabled:
                        //  Mode select
                        gate.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsFalse());
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsFalse());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)  .IsFalse()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT) .IsTrue());
                        break;
                    case HandPattern.Swap_Left_Disabled_Right_Disabled:
                        //  Mode select
                        gate.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsTrue());
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)   .IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT)  .IsTrue());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR)       .IsFalse()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT)  .IsFalse()).Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT) .IsFalse());
                        break;
                    default:
                        throw new InvalidOperationException("Unknown pattern.");
                }

                // Create each gesture's state
                for (int leftIndex = 0; leftIndex < AV3Constants.EmoteSelectToGesture.Count; leftIndex++)
                {
                    var leftStateMachine = priorityStateMachine.NewSubStateMachine($"L{leftIndex}", 1, leftIndex)
                        .WithEntryPosition(0, 0).WithAnyStatePosition(0, -1).WithParentStateMachinePosition(0, -2).WithExitPosition(2, 0);
                    leftStateMachine.TransitionsFromEntry()
                        .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_SELECT_L).IsEqualTo(leftIndex));
                    leftStateMachine.Exits();

                    for (int rightIndex = 0; rightIndex < AV3Constants.EmoteSelectToGesture.Count; rightIndex++)
                    {
                        var rightState = leftStateMachine.NewState($"L{leftIndex} R{rightIndex}", 1, rightIndex);
                        rightState.TransitionsFromEntry()
                            .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_SELECT_R).IsEqualTo(rightIndex));
                        rightState.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_LOCK_ENABLE).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_PRELOCK_ENABLE).IsFalse())
                            .And(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_SELECT_L).IsNotEqualTo(leftIndex))
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_LOCK_ENABLE).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_PRELOCK_ENABLE).IsFalse())
                            .And(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_SELECT_R).IsNotEqualTo(rightIndex));

                        // Add parameter driver
                        var converted = ConvertGeture(AV3Constants.EmoteSelectToGesture[leftIndex], AV3Constants.EmoteSelectToGesture[rightIndex], pattern);
                        var preSelectEmoteIndex = AV3Utility.GetPreselectEmoteIndex(converted.left, converted.right);
                        rightState.Drives(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PRESELECT), preSelectEmoteIndex).DrivingLocally();
                    }
                }
            }
        }

        private static void RemoveParameters(AnimatorController controller)
        {
            var paramNames = new[]
            {
                "CN_EXPRESSION_PARAMETER_LOADING_COMP",
                "CN_EMOTE_SELECT_PRIORITY_RIGHT",
                "CN_EMOTE_SELECT_ONLY_LEFT",
                "CN_EMOTE_SELECT_ONLY_RIGHT",
                "Dummy",
            };

            foreach (var parameName in paramNames) { RemoveParameter(controller, parameName); }
        }

        private static void RemoveParameter(AnimatorController controller, string paramName)
        {
            if (controller != null)
            {
                // Find the index of the parameter to remove
                int parameterIndex = -1;
                for (int i = 0; i < controller.parameters.Length; i++)
                {
                    if (controller.parameters[i].name == paramName)
                    {
                        parameterIndex = i;
                        break;
                    }
                }

                // Remove the parameter if it's found
                if (parameterIndex >= 0)
                {
                    controller.RemoveParameter(parameterIndex);
                }
                else
                {
                    Debug.LogError($"Parameter '{paramName}' was not found");
                }
            }
        }

        private static AacConfiguration GetConfiguration(AnimatorController dummyContainer, bool writeDefaults)
        {
            return new AacConfiguration() {
                SystemName = string.Empty,
                AvatarDescriptor = null,
                AnimatorRoot = null,
                DefaultValueRoot = null,
                AssetContainer = dummyContainer,
                AssetKey = "FaceEmo",
                DefaultsProvider = new FaceEmoAacDefaultsProvider(writeDefaults),
            };
        }

        private static (HandGesture left, HandGesture right) ConvertGeture(HandGesture left, HandGesture right, HandPattern priority)
        {
            switch (priority)
            {
                case HandPattern.Normal_Left_Enabled_Right_Enabled:
                    return (left, right);
                case HandPattern.Normal_Left_Enabled_Right_Disabled:
                    return (left, HandGesture.Neutral);
                case HandPattern.Normal_Left_Disabled_Right_Enabled:
                    return (HandGesture.Neutral, right);
                case HandPattern.Normal_Left_Disabled_Right_Disabled:
                    return (HandGesture.Neutral, HandGesture.Neutral);
                case HandPattern.Swap_Left_Enabled_Right_Enabled:
                    return (right, left);
                case HandPattern.Swap_Left_Enabled_Right_Disabled:
                    return (HandGesture.Neutral, left);
                case HandPattern.Swap_Left_Disabled_Right_Enabled:
                    return (right, HandGesture.Neutral);
                case HandPattern.Swap_Left_Disabled_Right_Disabled:
                    return (HandGesture.Neutral, HandGesture.Neutral);
                default:
                    throw new InvalidOperationException("Unknown pattern.");
            }
        }
    }
}
