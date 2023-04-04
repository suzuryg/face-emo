using AnimatorAsCode.V0;
using AnimatorAsCode.V0.Extensions.VRChat;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using Suzuryg.FacialExpressionSwitcher.Domain;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class TemplateFxGenerator
    {
        private static readonly IReadOnlyList<PatternLRPriority> PatternsLRPriority = Enum.GetValues(typeof(PatternLRPriority)).Cast<PatternLRPriority>().ToList();

        private enum PatternLRPriority
        {
            Normal,
            PrimeLeft,
            PrimeRight,
            OnlyLeft,
            OnlyRight,
        }

        // After importing CustomAnimatorControllers, deep copy with CopyAssetsWithDependency (to change GUID).
        [MenuItem( "Tools/Suzuryg/FacialExpressionSwitcher/GenerateTemplateFx" )]
        public static void Generate()
        {
            try
            {
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, "Start generation Template FX controller.", 0);

                // Copy template FX controller
                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(AV3Constants.Path_BearsDenFx) is null)
                {
                    throw new FacialExpressionSwitcherException("Original template was not found.");
                }
                else if (!AssetDatabase.CopyAsset(AV3Constants.Path_BearsDenFx, AV3Constants.Path_FxTemplate))
                {
                    throw new FacialExpressionSwitcherException("Failed to copy FX template.");
                }
                var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AV3Constants.Path_FxTemplate);


                // Create container
                var dummyContainer = AssetDatabase.LoadAssetAtPath<AnimatorController>(AV3Constants.Path_TemplateContainer);
                if (dummyContainer is AnimatorController)
                {
                    AssetDatabase.DeleteAsset(AV3Constants.Path_TemplateContainer);
                }
                dummyContainer = new AnimatorController();
                AssetDatabase.CreateAsset(dummyContainer, AV3Constants.Path_TemplateContainer);

                var aac = AacV0.Create(GetConfiguration(dummyContainer, writeDefaults: false));

                AV3Utility.RemoveLayer(animatorController, AV3Constants.LayerName_Base);
                AV3Utility.RemoveLayer(animatorController, AV3Constants.LayerName_FaceEmoteOverride);
                AV3Utility.RemoveLayer(animatorController, AV3Constants.LayerName_LocalIndicatorSound);
                AV3Utility.MoveLayer(animatorController, AV3Constants.LayerName_DefaultFace, 0);

                // If not initialized, layer replacement process will be slower.
                aac.CreateSupportingArbitraryControllerLayer(animatorController, AV3Constants.LayerName_FaceEmoteSetControl);
                aac.CreateSupportingArbitraryControllerLayer(animatorController, AV3Constants.LayerName_DefaultFace);
                aac.CreateSupportingArbitraryControllerLayer(animatorController, AV3Constants.LayerName_FaceEmotePlayer);

                GenerateFaceEmoteControlLayer(aac, animatorController);

                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, "Done!", 1);
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
            layer.StateMachine.WithEntryPosition(0, -1).WithAnyStatePosition(0, -2).WithExitPosition(4, 0);

            // Create initializing states
            var init = layer.NewState("INIT", 0, 0);
            var gate = layer.NewState("GATE", 1, 0);
            init.TransitionsTo(gate).
                When(layer.Av3().IsLocal.IsEqualTo(true)).
                And(layer.BoolParameter(AV3Constants.ParamName_CN_EXPRESSION_PARAMETER_LOADING_COMP).IsTrue());

            // Create Mode states machines
            foreach (var priority in PatternsLRPriority)
            {
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{AV3Constants.LayerName_FaceEmoteControl}\" layer...",
                    (float)priority / PatternsLRPriority.Count);

                var priorityGate = layer.NewState(priority.ToString() + "GATE", 2, (int)priority);
                var priorityStateMachine = layer.NewSubStateMachine(priority.ToString(), 3, (int)priority)
                    .WithEntryPosition(0, 0).WithAnyStatePosition(0, -1).WithParentStateMachinePosition(0, -2).WithExitPosition(2, 0);
                priorityGate.TransitionsTo(priorityStateMachine)
                    .When(layer.BoolParameter("Dummy").IsFalse());

                switch (priority)
                {
                    case PatternLRPriority.Normal:
                        //  Mode select
                        gate.TransitionsTo(priorityGate)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsFalse());
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsFalse());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsTrue());
                        break;
                    case PatternLRPriority.PrimeLeft:
                        //  Mode select
                        gate.TransitionsTo(priorityGate)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsTrue());
                        // Drive mode parameters
                        priorityGate
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT), false)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT), false)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT), false)
                            .DrivingLocally();
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsFalse());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsFalse())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsTrue());
                        break;
                    case PatternLRPriority.PrimeRight:
                        //  Mode select
                        gate.TransitionsTo(priorityGate)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsTrue());
                        // Drive mode parameters
                        priorityGate
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT), false)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT), false)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT), false)
                            .DrivingLocally();
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsFalse());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsFalse())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsTrue());
                        break;
                    case PatternLRPriority.OnlyLeft:
                        //  Mode select
                        gate.TransitionsTo(priorityGate)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsTrue());
                        // Drive mode parameters
                        priorityGate
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT), false)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT), false)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT), false)
                            .DrivingLocally();
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsTrue())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsFalse());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsFalse())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsTrue());
                        break;
                    case PatternLRPriority.OnlyRight:
                        //  Mode select
                        gate.TransitionsTo(priorityGate)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsTrue());
                        // Drive mode parameters
                        priorityGate
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT), false)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT), false)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT), false)
                            .DrivingLocally();
                        // Self transition & exit
                        priorityStateMachine.TransitionsTo(priorityStateMachine)
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsFalse())
                            .And(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsTrue());
                        priorityStateMachine.Exits()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT).IsTrue())
                            .Or()
                            .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT).IsFalse());
                        break;
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
                            .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_SELECT_L).IsNotEqualTo(leftIndex))
                            .Or()
                            .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_SELECT_R).IsNotEqualTo(rightIndex));

                        // Add parameter driver
                        var converted = ConvertGeture(AV3Constants.EmoteSelectToGesture[leftIndex], AV3Constants.EmoteSelectToGesture[rightIndex], priority);
                        var preSelectEmoteIndex = AV3Utility.GetPreselectEmoteIndex(converted.left, converted.right);
                        rightState.Drives(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PRESELECT), preSelectEmoteIndex).DrivingLocally();
                    }
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
                AssetKey = "FES",
                DefaultsProvider = new FesAacDefaultsProvider(writeDefaults),
            };
        }

        private static (HandGesture left, HandGesture right) ConvertGeture(HandGesture left, HandGesture right, PatternLRPriority priority)
        {
            switch (priority)
            {
                case PatternLRPriority.Normal:
                    // NOP
                    break;
                case PatternLRPriority.PrimeLeft:
                    if (left != HandGesture.Neutral)
                    {
                        right = HandGesture.Neutral;
                    }
                    break;
                case PatternLRPriority.PrimeRight:
                    if (right != HandGesture.Neutral)
                    {
                        left = HandGesture.Neutral;
                    }
                    break;
                case PatternLRPriority.OnlyLeft:
                    right = HandGesture.Neutral;
                    break;
                case PatternLRPriority.OnlyRight:
                    left = HandGesture.Neutral;
                    break;
            }
            return (left, right);
        }
    }
}
