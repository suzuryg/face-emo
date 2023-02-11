using AnimatorAsCode.V0;
using AnimatorAsCode.V0.Extensions.VRChat;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.External.Hai.ComboGestureIntegrator;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UIElements;
using UnityEditor.IMGUI.Controls;
using UniRx;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class FxGenerator : IFxGenerator
    {
        private static readonly string TemplatePath = $"{DetailConstants.DetailDirectory}/AV3/BearsDen/FES_FX.controller"; // test
        private static readonly string BlinkTemplatePath = $"{DetailConstants.DetailDirectory}/AV3/BearsDen/Blink_Enable.anim"; // test
        private static readonly string DestinationPath = "Assets/Temp/GeneratedFx.controller"; // test

        private class ModeEx
        {
            public string PathToMode { get; set; }
            public int DefaultEmoteIndex { get; set; }
            public IMode Mode { get; set; }
        }

        public void Generate(IMenu menu) => Generate(menu, false);

        public void Generate(IMenu menu, bool forceOverLimitMode = false)
        {
            try
            {
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, "Start generation FX controller.", 0);

                // Copy template FX controller
                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(TemplatePath) is null)
                {
                    throw new FacialExpressionSwitcherException("FX template was not found.");
                }
                else if (!AssetDatabase.CopyAsset(TemplatePath, DestinationPath))
                {
                    throw new FacialExpressionSwitcherException("Failed to copy FX template.");
                }
                var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(DestinationPath);

                // Generate layers
                var avatarDescriptor = AV3Utility.GetAvatarDescriptor(menu.Avatar);
                var aac = AacV0.Create(GetConfiguration(avatarDescriptor, animatorController, menu.WriteDefaults));
                var modes = FlattenMenuItemList(menu.Registered);
                var emoteCount = GetEmoteCount(modes);
                var useOverLimitMode = forceOverLimitMode || emoteCount > AV3Constants.MaxEmoteNum;
                GenerateFaceEmoteSetControlLayer(modes, aac, animatorController, useOverLimitMode);
                GenerateDefaultFaceLayer(aac, avatarDescriptor, animatorController);
                GenerateFaceEmotePlayerLayer(modes, menu, aac, animatorController, useOverLimitMode);
                ModifyBlinkLayer(aac, avatarDescriptor, animatorController);
                ModifyMouthMorphCancelerLayer(menu, aac, avatarDescriptor, animatorController);
                if (menu.SmoothAnalogFist)
                {
                    ComboGestureIntegratorProxy.DoGenerate(animatorController, Path.GetDirectoryName(DestinationPath), menu.WriteDefaults);
                }

                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, "Done!", 1);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void GenerateFaceEmoteSetControlLayer(IReadOnlyList<ModeEx> modes, AacFlBase aac, AnimatorController animatorController, bool useOverLimitMode)
        {
            // Replace existing layer
            var layerName = AV3Constants.LayerName_FaceEmoteSetControl;
            if (!animatorController.layers.Any(x => x.name == layerName))
            {
                throw new FacialExpressionSwitcherException($"The layer \"{layerName}\" was not found in FX template.");
            }
            var layer = aac.CreateSupportingArbitraryControllerLayer(animatorController, layerName);
            AV3Utility.SetLayerWeight(animatorController, layerName, 0);
            layer.StateMachine.WithEntryPosition(0, -1).WithAnyStatePosition(0, -2).WithExitPosition(3, 0);

            // Create initializing states
            var init = layer.NewState("INIT", 0, 0);
            var gate = layer.NewState("GATE", 1, 0);
            init.TransitionsTo(gate).
                When(layer.Av3().IsLocal.IsEqualTo(true)).
                And(layer.BoolParameter(AV3Constants.ParamName_CN_EXPRESSION_PARAMETER_LOADING_COMP).IsTrue());

            // Create each mode's sub-state machine
            for (int modeIndex = 0; modeIndex < modes.Count; modeIndex++)
            {
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{AV3Constants.LayerName_FaceEmoteSetControl}\" layer...",
                    (float)modeIndex / modes.Count);

                var mode = modes[modeIndex];
                var modeStateMachine = layer.NewSubStateMachine(mode.PathToMode, 2, modeIndex)
                    .WithEntryPosition(0, 0).WithAnyStatePosition(0, -1).WithParentStateMachinePosition(0, -2).WithExitPosition(2, 0);
                gate.TransitionsTo(modeStateMachine)
                    .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsEqualTo(modeIndex));
                modeStateMachine.TransitionsTo(modeStateMachine)
                    .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsEqualTo(modeIndex));
                modeStateMachine.Exits()
                    .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsNotEqualTo(modeIndex));

                // Create each gesture's state
                for (int leftIndex = 0; leftIndex < AV3Constants.EmoteSelectToGesture.Count; leftIndex++)
                {
                    for (int rightIndex = 0; rightIndex < AV3Constants.EmoteSelectToGesture.Count; rightIndex++)
                    {
                        EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{AV3Constants.LayerName_FaceEmoteSetControl}\" layer...",
                            (float)modeIndex / modes.Count
                            + 1f / modes.Count * leftIndex / AV3Constants.EmoteSelectToGesture.Count
                            + 1f / modes.Count / AV3Constants.EmoteSelectToGesture.Count * rightIndex / AV3Constants.EmoteSelectToGesture.Count);

                        var preSelectEmoteIndex = AV3Utility.GetPreselectEmoteIndex(AV3Constants.EmoteSelectToGesture[leftIndex], AV3Constants.EmoteSelectToGesture[rightIndex]);
                        var gestureState = modeStateMachine.NewState($"L{leftIndex} R{rightIndex}", 1, preSelectEmoteIndex);
                        gestureState.TransitionsFromEntry()
                            .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PRESELECT).IsEqualTo(preSelectEmoteIndex));
                        gestureState.Exits()
                            .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PRESELECT).IsNotEqualTo(preSelectEmoteIndex))
                            .Or()
                            .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsNotEqualTo(modeIndex));

                        // Add parameter driver
                        var branchIndex = GetBranchIndex(AV3Constants.EmoteSelectToGesture[leftIndex], AV3Constants.EmoteSelectToGesture[rightIndex], mode.Mode);
                        var emoteIndex = GetEmoteIndex(branchIndex, mode, useOverLimitMode);
                        gestureState.Drives(layer.IntParameter(AV3Constants.ParamName_SYNC_EM_EMOTE), emoteIndex).DrivingLocally();
                    }
                }
            }

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 1);
        }

        private static void GenerateDefaultFaceLayer(AacFlBase aac, VRCAvatarDescriptor avatarDescriptor, AnimatorController animatorController)
        {
            var layerName = AV3Constants.LayerName_DefaultFace;

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 0);

            // Replace existing layer
            if (!animatorController.layers.Any(x => x.name == layerName))
            {
                throw new FacialExpressionSwitcherException($"The layer \"{layerName}\" was not found in FX template.");
            }
            var layer = aac.CreateSupportingArbitraryControllerLayer(animatorController, layerName);
            layer.StateMachine.WithEntryPosition(0, -1).WithAnyStatePosition(0, -2).WithExitPosition(0, -3);

            // Create default face state
            var defaultFace = GetDefaultFaceAnimation(aac, avatarDescriptor);
            layer.NewState("DEFAULT", 0, 0).WithAnimation(defaultFace);

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 1);
        }

        private static void GenerateFaceEmotePlayerLayer(IReadOnlyList<ModeEx> modes, IMenu menu, AacFlBase aac, AnimatorController animatorController, bool useOverLimitMode)
        {
            var layerName = AV3Constants.LayerName_FaceEmotePlayer;

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 0);

            // Replace existing layer
            if (!animatorController.layers.Any(x => x.name == layerName))
            {
                throw new FacialExpressionSwitcherException($"The layer \"{layerName}\" was not found in FX template.");
            }
            var layer = aac.CreateSupportingArbitraryControllerLayer(animatorController, layerName);
            layer.StateMachine.WithEntryPosition(0, 0).WithAnyStatePosition(2, -2).WithExitPosition(2, 0);

            // Create face emote playing states
            for (int modeIndex = 0; modeIndex < modes.Count; modeIndex++)
            {
                var mode = modes[modeIndex];
                AacFlStateMachine stateMachine = layer.StateMachine;

                // If use over-limit mode, create sub-state machines 
                if (useOverLimitMode)
                {
                    var modeStateMachine = layer.NewSubStateMachine(mode.PathToMode, 1, modeIndex)
                        .WithEntryPosition(0, 0).WithAnyStatePosition(0, -1).WithParentStateMachinePosition(0, -2).WithExitPosition(2, 0);
                    modeStateMachine.TransitionsFromEntry()
                        .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsEqualTo(modeIndex));
                    modeStateMachine.Exits();
                    stateMachine = modeStateMachine;
                }

                for (int branchIndex = -1; branchIndex < mode.Mode.Branches.Count; branchIndex++)
                {
                    var emoteIndex = GetEmoteIndex(branchIndex, mode, useOverLimitMode);
                    AacFlState emoteState;
                    // Mode
                    if (branchIndex < 0)
                    {
                        var animation = AV3Utility.GetAnimationClipWithName(mode.Mode.Animation);
                        if (animation.clip is AnimationClip)
                        {
                            emoteState = stateMachine.NewState(animation.name, 1, emoteIndex)
                                .WithAnimation(animation.clip);
                        }
                        else
                        {
                            emoteState = stateMachine.NewState("Empty", 1, emoteIndex);
                        }

                        emoteState
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), mode.Mode.BlinkEnabled)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), mode.Mode.MouthMorphCancelerEnabled)
                            .TrackingSets(TrackingElement.Eyes, AV3Utility.ConvertEyeTrackingType(mode.Mode.EyeTrackingControl))
                            .TrackingSets(TrackingElement.Mouth, AV3Utility.ConvertMouthTrackingType(mode.Mode.MouthTrackingControl));
                    }
                    // Branches
                    else
                    {
                        (Motion motion, string name) motion = (null, null);
                        var branch = mode.Mode.Branches[branchIndex];
                        var baseAnimation = AV3Utility.GetAnimationClipWithName(branch.BaseAnimation);
                        var leftAnimation = AV3Utility.GetAnimationClipWithName(branch.LeftHandAnimation);
                        var rightAnimation = AV3Utility.GetAnimationClipWithName(branch.RightHandAnimation);
                        var bothAnimation = AV3Utility.GetAnimationClipWithName(branch.BothHandsAnimation);
                        var leftWeight = menu.SmoothAnalogFist ? AV3Constants.ParamName_GestureLWSmoothing : AV3Constants.ParamName_GestureLeftWeight;
                        var rightWeight = menu.SmoothAnalogFist ? AV3Constants.ParamName_GestureRWSmoothing : AV3Constants.ParamName_GestureRightWeight;

                        // Both triggers used
                        if (branch.CanLeftTriggerUsed && branch.IsLeftTriggerUsed && branch.CanRightTriggerUsed && branch.IsRightTriggerUsed)
                        {
                            var blendTree = aac.NewBlendTreeAsRaw();
                            blendTree.blendType = BlendTreeType.FreeformCartesian2D;
                            blendTree.useAutomaticThresholds = false;
                            blendTree.blendParameter = leftWeight;
                            blendTree.blendParameterY = rightWeight;
                            blendTree.AddChild(baseAnimation.clip, new Vector2(0, 0));
                            blendTree.AddChild(leftAnimation.clip, new Vector2(1, 0));
                            blendTree.AddChild(rightAnimation.clip, new Vector2(0, 1));
                            blendTree.AddChild(bothAnimation.clip, new Vector2(1, 1));
                            motion = (blendTree, $"{baseAnimation.name}_{leftAnimation.name}_{rightAnimation.name}_{bothAnimation.name}");
                        }
                        // Left trigger used
                        else if (branch.CanLeftTriggerUsed && branch.IsLeftTriggerUsed)
                        {
                            var blendTree = aac.NewBlendTreeAsRaw();
                            blendTree.blendType = BlendTreeType.Simple1D;
                            blendTree.useAutomaticThresholds = false;
                            blendTree.blendParameter = leftWeight;
                            blendTree.AddChild(baseAnimation.clip, 0);
                            blendTree.AddChild(leftAnimation.clip, 1);
                            motion = (blendTree, $"{baseAnimation.name}_{leftAnimation.name}");
                        }
                        // Right trigger used
                        else if (branch.CanRightTriggerUsed && branch.IsRightTriggerUsed)
                        {
                            var blendTree = aac.NewBlendTreeAsRaw();
                            blendTree.blendType = BlendTreeType.Simple1D;
                            blendTree.useAutomaticThresholds = false;
                            blendTree.blendParameter = rightWeight;
                            blendTree.AddChild(baseAnimation.clip, 0);
                            blendTree.AddChild(rightAnimation.clip, 1);
                            motion = (blendTree, $"{baseAnimation.name}_{rightAnimation.name}");
                        }
                        // No triggers used
                        else
                        {
                            motion = (baseAnimation.clip, baseAnimation.name);
                        }

                        if (motion.motion is Motion)
                        {
                            emoteState = stateMachine.NewState(motion.name, 1, emoteIndex)
                                .WithAnimation(motion.motion);
                        }
                        else
                        {
                            emoteState = stateMachine.NewState("Empty", 1, emoteIndex);
                        }

                        emoteState
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), branch.BlinkEnabled)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), branch.MouthMorphCancelerEnabled)
                            .TrackingSets(TrackingElement.Eyes, AV3Utility.ConvertEyeTrackingType(branch.EyeTrackingControl))
                            .TrackingSets(TrackingElement.Mouth, AV3Utility.ConvertMouthTrackingType(branch.MouthTrackingControl));
                    }

                    emoteState.TransitionsFromEntry()
                        .When(layer.IntParameter(AV3Constants.ParamName_SYNC_EM_EMOTE).IsEqualTo(emoteIndex));
                    emoteState.Exits()
                        .WithTransitionDurationSeconds((float)menu.TransitionDurationSeconds)
                        .When(layer.IntParameter(AV3Constants.ParamName_SYNC_EM_EMOTE).IsNotEqualTo(emoteIndex))
                        .And(layer.Av3().Voice.IsLessThan(AV3Constants.VoiceThreshold))
                        .Or()
                        .When(layer.Av3().AFK.IsTrue());

                    // If use over-limit mode, extra-exit-transition is needed
                    if (useOverLimitMode)
                    {
                        emoteState.Exits()
                        .WithTransitionDurationSeconds((float)menu.TransitionDurationSeconds)
                        .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsNotEqualTo(modeIndex))
                        .And(layer.Av3().Voice.IsLessThan(AV3Constants.VoiceThreshold));
                    }
                }

                // If use over-limit mode, extra-exit-state is needed
                if (useOverLimitMode)
                {
                    var exitState = stateMachine.NewState("MODE CHANGE", 1, -1);
                    exitState.TransitionsFromEntry()
                        .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsNotEqualTo(modeIndex));
                    exitState.Exits()
                        .When(layer.BoolParameter("Dummy").IsFalse());
                }
            }

            // Create AFK sub-state machine
            // TODO: Play AFK emotes (idle, afk start, afk, afk end)
            var afkStateMachine = layer.NewSubStateMachine("AFK", 1, -1)
                .WithEntryPosition(0, 0).WithAnyStatePosition(0, -1).WithParentStateMachinePosition(0, -2).WithExitPosition(0, 5);
            layer.EntryTransitionsTo(afkStateMachine)
                .When(layer.Av3().AFK.IsTrue());
            afkStateMachine.Exits();

            var afkStandbyState = afkStateMachine.NewState("AFK Standby", 0, 1)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), true)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), true)
                .TrackingSets(TrackingElement.Eyes, VRC_AnimatorTrackingControl.TrackingType.Tracking)
                .TrackingSets(TrackingElement.Mouth, VRC_AnimatorTrackingControl.TrackingType.Tracking);

            var afkEnterState = afkStateMachine.NewState("AFK Enter", 0, 2)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), true)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), true)
                .TrackingSets(TrackingElement.Eyes, VRC_AnimatorTrackingControl.TrackingType.Tracking)
                .TrackingSets(TrackingElement.Mouth, VRC_AnimatorTrackingControl.TrackingType.Tracking);
            afkStandbyState.TransitionsTo(afkEnterState)
                .WithTransitionDurationSeconds(0.1f)
                .When(layer.BoolParameter("Dummy").IsFalse());

            var afkState = afkStateMachine.NewState("AFK", 0, 3)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), false)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), false)
                .TrackingSets(TrackingElement.Eyes, VRC_AnimatorTrackingControl.TrackingType.Animation)
                .TrackingSets(TrackingElement.Mouth, VRC_AnimatorTrackingControl.TrackingType.Animation);
            afkEnterState.TransitionsTo(afkState)
                .WithTransitionDurationSeconds(0.75f)
                .AfterAnimationFinishes();

            var afkExitState = afkStateMachine.NewState("AFK Exit", 0, 4)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), true)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), true)
                .TrackingSets(TrackingElement.Eyes, VRC_AnimatorTrackingControl.TrackingType.Tracking)
                .TrackingSets(TrackingElement.Mouth, VRC_AnimatorTrackingControl.TrackingType.Tracking);
            afkState.TransitionsTo(afkExitState)
                .WithTransitionDurationSeconds(0.75f)
                .When(layer.Av3().AFK.IsFalse());
            afkExitState.Exits()
                .WithTransitionDurationSeconds(0.1f)
                .AfterAnimationFinishes();

            // Create override state
            var overrideState = layer.NewState("in OVERRIDE", 2, -1);
            overrideState.TransitionsFromAny()
                .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_OVERRIDE).IsTrue());
            overrideState.Exits()
                .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_OVERRIDE).IsFalse());

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 1);
        }

        private static void ModifyBlinkLayer(AacFlBase aac, VRCAvatarDescriptor avatarDescriptor, AnimatorController animatorController)
        {
            var layerName = AV3Constants.LayerName_Blink;

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Modifying \"{layerName}\" layer...", 0);

            var motion = GetBlinkAnimation(aac, avatarDescriptor);
            AV3Utility.SetMotion(animatorController, layerName, AV3Constants.StateName_BlinkEnabled, motion.Clip);

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Modifying \"{layerName}\" layer...", 1);
        }

        private static void ModifyMouthMorphCancelerLayer(IMenu menu, AacFlBase aac, VRCAvatarDescriptor avatarDescriptor, AnimatorController animatorController)
        {
            var layerName = AV3Constants.LayerName_MouthMorphCanceler;

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Modifying \"{layerName}\" layer...", 0);

            var motion = GetMouthMorphCancelerAnimation(menu, aac, avatarDescriptor);
            AV3Utility.SetMotion(animatorController, layerName, AV3Constants.StateName_MouthMorphCancelerEnabled, motion.Clip);

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Modifying \"{layerName}\" layer...", 1);
        }

        private static AacFlClip GetDefaultFaceAnimation(AacFlBase aac, VRCAvatarDescriptor avatarDescriptor)
        {
            var clip = aac.NewClip();

            var faceMesh = AV3Utility.GetFaceMesh(avatarDescriptor);
            if (faceMesh is null)
            {
                return clip;
            }

            var toBeExcluded = AV3Utility.GetBlendShapeNamesToBeExcluded(avatarDescriptor);

            // Get blendshape names and weights
            var blendShapes = new List<(string name, float weight)>();
            if (faceMesh.sharedMesh is Mesh)
            {
                for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
                {
                    var name = faceMesh.sharedMesh.GetBlendShapeName(i);
                    if (!AV3Utility.IsExcluded(name, toBeExcluded))
                    {
                        var weight = faceMesh.GetBlendShapeWeight(i);
                        blendShapes.Add((name, weight));
                    }
                }
            }

            // Generate clip
            foreach (var blendshape in blendShapes)
            {
                clip = clip.BlendShape(faceMesh, blendshape.name, blendshape.weight);
            }

            return clip;
        }

        private static AacFlClip GetBlinkAnimation(AacFlBase aac, VRCAvatarDescriptor avatarDescriptor)
        {
            const string blendShapePrefix = "blendShape.";

            var template = AssetDatabase.LoadAssetAtPath<AnimationClip>(BlinkTemplatePath);
            if (template is null)
            {
                throw new FacialExpressionSwitcherException("Blink template was not found.");
            }
            var clip = aac.CopyClip(template);

            var bindings = AnimationUtility.GetCurveBindings(clip.Clip);
            if (bindings.Count() == 1 && bindings.First().propertyName == blendShapePrefix + "blink")
            {
                var binding = bindings.First();
                var curve = AnimationUtility.GetEditorCurve(clip.Clip, binding);

                binding.propertyName = blendShapePrefix + "hoge";
                AnimationUtility.SetEditorCurve(clip.Clip, binding, curve);
            }
            else
            {
                throw new FacialExpressionSwitcherException("Invalid blink template count.");
            }

            return clip;
        }

        private static AacFlClip GetMouthMorphCancelerAnimation(IMenu menu, AacFlBase aac, VRCAvatarDescriptor avatarDescriptor)
        {
            var clip = aac.NewClip();

            var faceMesh = AV3Utility.GetFaceMesh(avatarDescriptor);
            if (faceMesh is null)
            {
                return clip;
            }

            var mouthMorphBlendShapes = new HashSet<string>(menu.MouthMorphBlendShapes);
            var toBeExcluded = AV3Utility.GetBlendShapeNamesToBeExcluded(avatarDescriptor);

            // Get blendshape names and weights
            var blendShapes = new List<(string name, float weight)>();
            if (faceMesh.sharedMesh is Mesh)
            {
                for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
                {
                    var name = faceMesh.sharedMesh.GetBlendShapeName(i);
                    if (mouthMorphBlendShapes.Contains(name) && !AV3Utility.IsExcluded(name, toBeExcluded))
                    {
                        var weight = faceMesh.GetBlendShapeWeight(i);
                        blendShapes.Add((name, weight));
                    }
                }
            }

            // Generate clip
            foreach (var blendshape in blendShapes)
            {
                clip = clip.BlendShape(faceMesh, blendshape.name, blendshape.weight);
            }

            return clip;
        }

        private static AacConfiguration GetConfiguration(VRCAvatarDescriptor avatarDescriptor, AnimatorController animatorController, bool writeDefaults)
        {
            return new AacConfiguration() {
                SystemName = string.Empty,
                AvatarDescriptor = null,
                AnimatorRoot = avatarDescriptor.transform,
                DefaultValueRoot = avatarDescriptor.transform,
                AssetContainer = animatorController,
                AssetKey = "FES",
                DefaultsProvider = new FesAacDefaultsProvider(writeDefaults),
            };
        }

        private static List<ModeEx> FlattenMenuItemList(IMenuItemList menuItemList)
        {
            var ret = new List<ModeEx>();
            FlattenMenuItemListSub(menuItemList, ret, string.Empty);

            var branchCount = 0;
            foreach (var mode in ret)
            {
                mode.DefaultEmoteIndex = branchCount;
                branchCount += mode.Mode.Branches.Count + 1;
            }
            return ret;
        }

        private static void FlattenMenuItemListSub(IMenuItemList menuItemList, List<ModeEx> flattened, string pathToParent)
        {
            foreach (var id in menuItemList.Order)
            {
                if (menuItemList.GetType(id) == MenuItemType.Mode)
                {
                    var mode = menuItemList.GetMode(id);
                    flattened.Add(new ModeEx() { PathToMode = pathToParent + mode.DisplayName, Mode = mode });
                }
                else
                {
                    var group = menuItemList.GetGroup(id);
                    FlattenMenuItemListSub(group, flattened, pathToParent + group.DisplayName + "/");
                }
            }
        }

        private static int GetEmoteCount(IReadOnlyList<ModeEx> modes)
        {
            if (modes is null || modes.Count == 0)
            {
                return 0;
            }
            else
            {
                var last = modes.Last();
                return last.DefaultEmoteIndex + last.Mode.Branches.Count + 1;
            }
        }

        // TODO: Refactor domain method
        // Should invalid value -1?
        private static int GetBranchIndex(HandGesture left, HandGesture right, IMode mode)
        {
            var branch = mode.GetGestureCell(left, right);
            if (branch is null)
            {
                return -1;
            }

            for (int i = 0; i < mode.Branches.Count; i++)
            {
                if (ReferenceEquals(branch, mode.Branches[i]))
                {
                    return i;
                }
            }

            return - 1;
        }

        private static int GetEmoteIndex(int branchIndex, ModeEx mode, bool useOverLimitMode)
        {
            if (useOverLimitMode)
            {
                if (branchIndex < 0) { return 0; }
                else { return branchIndex + 1; }
            }
            else
            {
                if (branchIndex < 0) { return mode.DefaultEmoteIndex; }
                else { return mode.DefaultEmoteIndex + branchIndex + 1; }
            }
        }
    }
}
