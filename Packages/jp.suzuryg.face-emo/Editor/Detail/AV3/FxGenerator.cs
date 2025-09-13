#if VALID_VRCSDK3_AVATARS
using Suzuryg.FaceEmo.NDMF;
#endif

#if USE_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
using Sync = nadena.dev.modular_avatar.core.ParameterSyncType;
#endif

using AnimatorAsCode.V0;
using AnimatorAsCode.V0.Extensions.VRChat;
using Hai.ComboGesture.Scripts.Editor.Internal;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UniRx;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public class FxGenerator : IFxGenerator
    {
        private IReadOnlyLocalizationSetting _localizationSetting;
        private ModeNameProvider _modeNameProvider;
        private ExMenuThumbnailDrawer _exMenuThumbnailDrawer;
        private AV3Setting _aV3Setting;

        public FxGenerator(IReadOnlyLocalizationSetting localizationSetting, ModeNameProvider modeNameProvider, ExMenuThumbnailDrawer exMenuThumbnailDrawer, AV3Setting aV3Setting)
        {
            _localizationSetting = localizationSetting;
            _modeNameProvider = modeNameProvider;
            _exMenuThumbnailDrawer = exMenuThumbnailDrawer;
            _aV3Setting = aV3Setting;
        }

        public void Generate(IMenu menu, IEnumerable<string> editablePrefabPaths) => Generate(menu, editablePrefabPaths, false);

        public void Generate(IMenu menu, IEnumerable<string> editablePrefabPaths, bool forceOverLimitMode = false)
        {
            try
            {
                if (editablePrefabPaths is null) { editablePrefabPaths = new HashSet<string>(); }

                //UnityEngine.Profiling.Profiler.BeginSample("FxGenerator");
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, "Start FX controller generation.", 0);

                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Creating folder...", 0);
                var generatedDir = AV3Constants.Path_GeneratedDir + DateTime.Now.ToString("/yyyyMMdd_HHmmss");
                AV3Utility.CreateFolderRecursively(generatedDir);

                var fxPath = generatedDir + "/FaceEmo_FX.controller";
                var exMenuPath = generatedDir + "/FaceEmo_ExMenu.asset";

                // Copy template FX controller
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Creating fx controller...", 0);
                var templatePath = _aV3Setting.SmoothAnalogFist ? AV3Constants.Path_FxTemplate_WithIntegrator : AV3Constants.Path_FxTemplate_Basic;
                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(templatePath) == null)
                {
                    throw new FaceEmoException("FX template was not found.");
                }
                else if (!AssetDatabase.CopyAsset(templatePath, fxPath))
                {
                    Debug.LogError(fxPath);
                    throw new FaceEmoException("Failed to copy FX template.");
                }
                var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(fxPath);

                // Generate layers
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating layers...", 0);
                var avatarDescriptor = _aV3Setting.TargetAvatar as VRCAvatarDescriptor;
                if (avatarDescriptor == null)
                {
                    throw new FaceEmoException("AvatarDescriptor was not found.");
                }
                var aac = AacV0.Create(GetConfiguration(avatarDescriptor, animatorController, writeDefaults: false));
                var modes = AV3Utility.FlattenMenuItemList(menu.Registered, _modeNameProvider);
                var defaultModeIndex = GetDefaultModeIndex(modes, menu);
                var emoteCount = GetEmoteCount(modes);
                var useOverLimitMode = forceOverLimitMode || emoteCount > AV3Constants.MaxEmoteNum;
                GenerateFaceEmoteSetControlLayer(modes, aac, animatorController, useOverLimitMode);
                GenerateDefaultFaceLayer(aac, avatarDescriptor, animatorController);
                GenerateFaceEmotePlayerLayer(modes, _aV3Setting, aac, animatorController, useOverLimitMode);
                ModifyBlinkLayer(aac, avatarDescriptor, animatorController);
                ModifyMouthMorphCancelerLayer(_aV3Setting, aac, avatarDescriptor, animatorController);
                AddBypassLayer(_aV3Setting, aac, animatorController);

                if (!_aV3Setting.DisableFxDuringDancing && _aV3Setting.MatchAvatarWriteDefaults)
                {
                    AV3Utility.RemoveLayer(animatorController, AV3Constants.LayerName_DanceGimickControl);
                }

                // Generate MA Object
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating ExMenu...", 0);
                var exMenu = GenerateExMenu(modes, menu, exMenuPath, useOverLimitMode);

                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating MA objects...", 0);
                var rootObject = GetMARootObject(avatarDescriptor);
                var parentPrefabPath = GetParentPrefabPathOfMARootObject(rootObject);
                if (!string.IsNullOrEmpty(parentPrefabPath) && editablePrefabPaths.Contains(parentPrefabPath))
                {
                    var parentPrefabInstance = PrefabUtility.LoadPrefabContents(parentPrefabPath);
                    try
                    {
                        rootObject = parentPrefabInstance.transform.Find(AV3Constants.MARootObjectName)?.gameObject;
                        GenerateMAObject(rootObject, animatorController, exMenu, defaultModeIndex);
                        PrefabUtility.SaveAsPrefabAsset(parentPrefabInstance, parentPrefabPath);
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(parentPrefabInstance);
                    }
                }
                else
                {
                    GenerateMAObject(rootObject, animatorController, exMenu, defaultModeIndex);
                }

                // Replace sub-avatar's MA objects
                foreach (var item in _aV3Setting.SubTargetAvatars)
                {
                    var subAvatar = item as VRCAvatarDescriptor;
                    if (subAvatar == null ||
                        subAvatar.gameObject == null ||
                        ReferenceEquals(_aV3Setting.TargetAvatar, subAvatar))
                    {
                        continue;
                    }

                    var subRoot = GetMARootObject(subAvatar);
                    var subParentPath = GetParentPrefabPathOfMARootObject(subRoot);
                    if (!string.IsNullOrEmpty(subParentPath) && editablePrefabPaths.Contains(subParentPath))
                    {
                        var subParentInstance = PrefabUtility.LoadPrefabContents(subParentPath);
                        try
                        {
                            subRoot = subParentInstance.transform.Find(AV3Constants.MARootObjectName)?.gameObject;
                            ReplaceMAObject(subRoot);
                            PrefabUtility.SaveAsPrefabAsset(subParentInstance, subParentPath);
                        }
                        finally
                        {
                            PrefabUtility.UnloadPrefabContents(subParentInstance);
                        }
                    }
                    else
                    {
                        ReplaceMAObject(subRoot);
                    }
                }

                // Clean assets
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Cleaning assets...", 0);
                CleanAssets();

                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, "Done!", 1);
            }
            finally
            {
                //UnityEngine.Profiling.Profiler.EndSample();
                EditorUtility.ClearProgressBar();
            }
        }

        private static void GenerateFaceEmoteSetControlLayer(IReadOnlyList<ModeEx> modes, AacFlBase aac, AnimatorController animatorController, bool useOverLimitMode)
        {
            // Replace existing layer
            var layerName = AV3Constants.LayerName_FaceEmoteSetControl;
            if (!animatorController.layers.Any(x => x.name == layerName))
            {
                throw new FaceEmoException($"The layer \"{layerName}\" was not found in FX template.");
            }
            var layer = aac.CreateSupportingArbitraryControllerLayer(animatorController, layerName);
            AV3Utility.SetLayerWeight(animatorController, layerName, 0);
            layer.StateMachine.WithEntryPosition(0, -1).WithAnyStatePosition(0, -2).WithExitPosition(3, 0);

            // Create initializing states
            var init = layer.NewState("INIT", 0, 0);
            var gate = layer.NewState("GATE", 1, 0);
            init.TransitionsTo(gate).
                When(layer.Av3().IsLocal.IsEqualTo(true));

            // Create each mode's sub-state machine
            for (int modeIndex = 0; modeIndex < modes.Count; modeIndex++)
            {
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...",
                    1f / modes.Count * modeIndex);

                var mode = modes[modeIndex];
                var modeStateMachine = layer.NewSubStateMachine(mode.PathToMode.Replace('.', '_'), 2, modeIndex)
                    .WithEntryPosition(0, 0).WithAnyStatePosition(0, -1).WithParentStateMachinePosition(0, -2).WithExitPosition(2, 0);
                gate.TransitionsTo(modeStateMachine)
                    .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsEqualTo(modeIndex));
                modeStateMachine.TransitionsTo(modeStateMachine)
                    .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsEqualTo(modeIndex));
                modeStateMachine.Exits()
                    .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsNotEqualTo(modeIndex));

                // Check if the mode has branches
                var hasBranches = modes[modeIndex].Mode.Branches.Count > 0;

                if (hasBranches)
                {
                    // Create each gesture's state
                    for (int leftIndex = 0; leftIndex < AV3Constants.EmoteSelectToGesture.Count; leftIndex++)
                    {
                        for (int rightIndex = 0; rightIndex < AV3Constants.EmoteSelectToGesture.Count; rightIndex++)
                        {
                            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...",
                                1f / modes.Count * modeIndex 
                                + 1f / modes.Count / AV3Constants.EmoteSelectToGesture.Count * leftIndex 
                                + 1f / modes.Count / AV3Constants.EmoteSelectToGesture.Count / AV3Constants.EmoteSelectToGesture.Count * rightIndex);

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
                else
                {
                    // Create single state
                    var state = modeStateMachine.NewState($"Any Gestures", 1, 0);
                    state.TransitionsFromEntry();
                    state.Exits()
                        .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsNotEqualTo(modeIndex));

                    // Add parameter driver
                    var emoteIndex = GetEmoteIndex(branchIndex: -1, mode, useOverLimitMode);
                    state.Drives(layer.IntParameter(AV3Constants.ParamName_SYNC_EM_EMOTE), emoteIndex).DrivingLocally();
                }
            }

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 1);
        }

        private void GenerateDefaultFaceLayer(AacFlBase aac, VRCAvatarDescriptor avatarDescriptor, AnimatorController animatorController)
        {
            var layerName = AV3Constants.LayerName_DefaultFace;

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 0);

            // Replace existing layer
            if (!animatorController.layers.Any(x => x.name == layerName))
            {
                throw new FaceEmoException($"The layer \"{layerName}\" was not found in FX template.");
            }
            var layer = aac.CreateSupportingArbitraryControllerLayer(animatorController, layerName);
            layer.StateMachine.WithEntryPosition(0, -1).WithAnyStatePosition(0, -2).WithExitPosition(0, -3);

            // Create default face state
            var defaultFace = GetDefaultFaceAnimation(aac, avatarDescriptor);
            var defaultState = layer.NewState("DEFAULT", 0, 0).WithAnimation(defaultFace);

            // Create bypass state
            var bypassState = layer.NewState("BYPASS", 1, 0);
            defaultState.TransitionsTo(bypassState).
                When(layer.BoolParameter(AV3Constants.ParamName_CN_BYPASS).IsTrue());
            bypassState.TransitionsTo(defaultState).
                When(layer.BoolParameter(AV3Constants.ParamName_CN_BYPASS).IsFalse());

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 1);
        }

        private static void GenerateFaceEmotePlayerLayer(IReadOnlyList<ModeEx> modes, AV3Setting aV3Setting, AacFlBase aac, AnimatorController animatorController, bool useOverLimitMode)
        {
            var layerName = AV3Constants.LayerName_FaceEmotePlayer;

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 0);

            // Replace existing layer
            if (!animatorController.layers.Any(x => x.name == layerName))
            {
                throw new FaceEmoException($"The layer \"{layerName}\" was not found in FX template.");
            }
            var layer = aac.CreateSupportingArbitraryControllerLayer(animatorController, layerName);
            layer.StateMachine.WithEntryPosition(0, 0).WithAnyStatePosition(2, -2).WithExitPosition(2, 0);

            // Create not-afk sub-state machine
            var notAfkStateMachine = layer.NewSubStateMachine("Not AFK", 1, 0)
                .WithEntryPosition(0, 0).WithAnyStatePosition(0, -1).WithParentStateMachinePosition(0, -2).WithExitPosition(2, 0);
            notAfkStateMachine.TransitionsFromEntry()
                .When(layer.Av3().AFK.IsFalse());
            notAfkStateMachine.Exits();

            // Create face emote playing states
            var emptyClip = aac.NewClip();
            var emptyName = "Empty";
            for (int modeIndex = 0; modeIndex < modes.Count; modeIndex++)
            {
                EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...",
                    1f / modes.Count * modeIndex);

                var mode = modes[modeIndex];
                var stateMachine = notAfkStateMachine;

                // If use over-limit mode, create sub-state machines
                if (useOverLimitMode)
                {
                    var modeStateMachine = stateMachine.NewSubStateMachine(mode.PathToMode, 1, modeIndex)
                        .WithEntryPosition(0, 0).WithAnyStatePosition(0, -1).WithParentStateMachinePosition(0, -2).WithExitPosition(2, 0);
                    modeStateMachine.TransitionsFromEntry()
                        .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsEqualTo(modeIndex));
                    modeStateMachine.Exits();
                    stateMachine = modeStateMachine;
                }

                for (int branchIndex = -1; branchIndex < mode.Mode.Branches.Count; branchIndex++)
                {
                    EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...",
                        1f / modes.Count * modeIndex
                        + 1f / modes.Count / (mode.Mode.Branches.Count + 1) * (branchIndex + 1));

                    var emoteIndex = GetEmoteIndex(branchIndex, mode, useOverLimitMode);
                    AacFlState emoteState;
                    // Mode
                    if (branchIndex < 0)
                    {
                        var animation = AV3Utility.GetAnimationClipWithName(mode.Mode.Animation);
                        emoteState = stateMachine.NewState(animation.name?.Replace('.', '_') ?? emptyName, 1, emoteIndex)
                            .WithAnimation(animation.clip ?? emptyClip.Clip);

                        emoteState
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), mode.Mode.BlinkEnabled)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), mode.Mode.MouthMorphCancelerEnabled && mode.Mode.MouthTrackingControl == MouthTrackingControl.Tracking)
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
                        var leftWeight = aV3Setting.SmoothAnalogFist ? CgeSharedLayerUtils.HaiGestureComboLeftWeightSmoothing : AV3Constants.ParamName_GestureLeftWeight;
                        var rightWeight = aV3Setting.SmoothAnalogFist ? CgeSharedLayerUtils.HaiGestureComboRightWeightSmoothing : AV3Constants.ParamName_GestureRightWeight;

                        // Both triggers used
                        if (branch.CanLeftTriggerUsed && branch.IsLeftTriggerUsed && branch.CanRightTriggerUsed && branch.IsRightTriggerUsed)
                        {
                            var blendTree = aac.NewBlendTreeAsRaw();
                            blendTree.blendType = BlendTreeType.FreeformCartesian2D;
                            blendTree.useAutomaticThresholds = false;
                            blendTree.blendParameter = leftWeight;
                            blendTree.blendParameterY = rightWeight;
                            blendTree.AddChild(baseAnimation.clip ?? emptyClip.Clip, new Vector2(0, 0));
                            blendTree.AddChild(leftAnimation.clip ?? emptyClip.Clip, new Vector2(1, 0));
                            blendTree.AddChild(rightAnimation.clip ?? emptyClip.Clip, new Vector2(0, 1));
                            blendTree.AddChild(bothAnimation.clip ?? emptyClip.Clip, new Vector2(1, 1));
                            motion = (blendTree, $"{baseAnimation.name ?? emptyName}_{leftAnimation.name ?? emptyName}_{rightAnimation.name ?? emptyName}_{bothAnimation.name ?? emptyName}");
                        }
                        // Left trigger used
                        else if (branch.CanLeftTriggerUsed && branch.IsLeftTriggerUsed)
                        {
                            var blendTree = aac.NewBlendTreeAsRaw();
                            blendTree.blendType = BlendTreeType.Simple1D;
                            blendTree.useAutomaticThresholds = false;
                            blendTree.blendParameter = leftWeight;
                            blendTree.AddChild(baseAnimation.clip ?? emptyClip.Clip, 0);
                            blendTree.AddChild(leftAnimation.clip ?? emptyClip.Clip, 1);
                            motion = (blendTree, $"{baseAnimation.name ?? emptyName}_{leftAnimation.name ?? emptyName}");
                        }
                        // Right trigger used
                        else if (branch.CanRightTriggerUsed && branch.IsRightTriggerUsed)
                        {
                            var blendTree = aac.NewBlendTreeAsRaw();
                            blendTree.blendType = BlendTreeType.Simple1D;
                            blendTree.useAutomaticThresholds = false;
                            blendTree.blendParameter = rightWeight;
                            blendTree.AddChild(baseAnimation.clip ?? emptyClip.Clip, 0);
                            blendTree.AddChild(rightAnimation.clip ?? emptyClip.Clip, 1);
                            motion = (blendTree, $"{baseAnimation.name ?? emptyName}_{rightAnimation.name ?? emptyName}");
                        }
                        // No triggers used
                        else
                        {
                            motion = (baseAnimation.clip ?? emptyClip.Clip, baseAnimation.name ?? emptyName);
                        }

                        emoteState = stateMachine.NewState(motion.name?.Replace('.', '_'), 1, emoteIndex)
                            .WithAnimation(motion.motion)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), branch.BlinkEnabled)
                            .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), branch.MouthMorphCancelerEnabled && branch.MouthTrackingControl == MouthTrackingControl.Tracking)
                            .TrackingSets(TrackingElement.Eyes, AV3Utility.ConvertEyeTrackingType(branch.EyeTrackingControl))
                            .TrackingSets(TrackingElement.Mouth, AV3Utility.ConvertMouthTrackingType(branch.MouthTrackingControl));
                    }

                    emoteState.TransitionsFromEntry()
                        .When(layer.IntParameter(AV3Constants.ParamName_SYNC_EM_EMOTE).IsEqualTo(emoteIndex));

                    var exitEmote = emoteState.Exits()
                        .WithTransitionDurationSeconds((float)aV3Setting.TransitionDurationSeconds)
                        .When(layer.Av3().AFK.IsTrue())
                        .Or()
                        .When(layer.IntParameter(AV3Constants.ParamName_SYNC_EM_EMOTE).IsNotEqualTo(emoteIndex))
                        .And(layer.BoolParameter(AV3Constants.ParamName_SYNC_CN_WAIT_FACE_EMOTE_BY_VOICE).IsFalse())
                        .Or()
                        .When(layer.IntParameter(AV3Constants.ParamName_SYNC_EM_EMOTE).IsNotEqualTo(emoteIndex))
                        .And(layer.BoolParameter(AV3Constants.ParamName_SYNC_CN_WAIT_FACE_EMOTE_BY_VOICE).IsTrue())
                        .And(layer.Av3().Voice.IsLessThan(AV3Constants.VoiceThreshold));

                    // If use over-limit mode, extra-exit-transition is needed
                    if (useOverLimitMode)
                    {
                        var exitEmoteOverlimit = emoteState.Exits()
                            .WithTransitionDurationSeconds((float)aV3Setting.TransitionDurationSeconds)
                            .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsNotEqualTo(modeIndex))
                            .And(layer.BoolParameter(AV3Constants.ParamName_SYNC_CN_WAIT_FACE_EMOTE_BY_VOICE).IsFalse())
                            .Or()
                            .When(layer.IntParameter(AV3Constants.ParamName_EM_EMOTE_PATTERN).IsNotEqualTo(modeIndex))
                            .And(layer.BoolParameter(AV3Constants.ParamName_SYNC_CN_WAIT_FACE_EMOTE_BY_VOICE).IsTrue())
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
                        .When(layer.BoolParameter(AV3Constants.ParamName_DUMMY_CONSTANT_FALSE).IsFalse());
                }
            }

            // Create AFK sub-state machine
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
                .When(layer.BoolParameter(AV3Constants.ParamName_DUMMY_CONSTANT_FALSE).IsFalse());
            if (aV3Setting.AfkEnterFace != null && aV3Setting.AfkEnterFace != null) { afkEnterState.WithAnimation(aV3Setting.AfkEnterFace); }

            var afkState = afkStateMachine.NewState("AFK", 0, 3)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), false)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), false)
                .TrackingSets(TrackingElement.Eyes, VRC_AnimatorTrackingControl.TrackingType.Animation)
                .TrackingSets(TrackingElement.Mouth, VRC_AnimatorTrackingControl.TrackingType.Tracking);
            afkEnterState.TransitionsTo(afkState)
                .WithTransitionDurationSeconds(0.75f)
                .AfterAnimationFinishes();
            if (aV3Setting.AfkFace != null && aV3Setting.AfkFace != null) { afkState.WithAnimation(aV3Setting.AfkFace); }

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
            if (aV3Setting.AfkExitFace != null && aV3Setting.AfkExitFace != null) { afkExitState.WithAnimation(aV3Setting.AfkExitFace); }

            // Create override state
            var overrideState = layer.NewState("in OVERRIDE", 2, -1);
            overrideState.TransitionsFromAny()
                .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_OVERRIDE).IsTrue())
                .And(layer.BoolParameter(AV3Constants.ParamName_CN_BYPASS).IsFalse());
            overrideState.Exits()
                .WithTransitionDurationSeconds((float)aV3Setting.TransitionDurationSeconds)
                .When(layer.BoolParameter(AV3Constants.ParamName_CN_EMOTE_OVERRIDE).IsFalse());

            // Create bypass state
            var bypassState = layer.NewState("BYPASS", 3, -1)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BLINK_ENABLE), false)
                .Drives(layer.BoolParameter(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE), false)
                .TrackingSets(TrackingElement.Eyes, VRC_AnimatorTrackingControl.TrackingType.Animation)
                .TrackingSets(TrackingElement.Mouth, VRC_AnimatorTrackingControl.TrackingType.Tracking);
            bypassState.TransitionsFromAny()
                .When(layer.BoolParameter(AV3Constants.ParamName_CN_BYPASS).IsTrue());
            bypassState.Exits()
                .WithTransitionDurationSeconds((float)aV3Setting.TransitionDurationSeconds)
                .When(layer.BoolParameter(AV3Constants.ParamName_CN_BYPASS).IsFalse());

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating \"{layerName}\" layer...", 1);
        }

        private void ModifyBlinkLayer(AacFlBase aac, VRCAvatarDescriptor avatarDescriptor, AnimatorController animatorController)
        {
            var layerName = AV3Constants.LayerName_Blink;

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Modifying \"{layerName}\" layer...", 0);

            AacFlClip motion;
            if (_aV3Setting.ReplaceBlink)
            {
                if (_aV3Setting.UseBlinkClip)
                {
                    if (_aV3Setting.BlinkClip != null)
                    {
                        motion = aac.CopyClip(_aV3Setting.BlinkClip).Looping();
                    }
                    else
                    {
                        motion = aac.NewClip();
                    }
                }
                else
                {
                    motion = GetBlinkAnimation(aac, avatarDescriptor);
                }
            }
            else
            {
                motion = aac.NewClip();
            }
            AV3Utility.SetMotion(animatorController, layerName, AV3Constants.StateName_BlinkEnabled, motion.Clip);

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Modifying \"{layerName}\" layer...", 1);
        }

        private void ModifyMouthMorphCancelerLayer(AV3Setting aV3Setting, AacFlBase aac, VRCAvatarDescriptor avatarDescriptor, AnimatorController animatorController)
        {
            var layerName = AV3Constants.LayerName_MouthMorphCanceler;

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Modifying \"{layerName}\" layer...", 0);

            AacFlClip motion;
            if (_aV3Setting.UseMouthMorphCancelClip)
            {
                if (_aV3Setting.MouthMorphCancelClip != null)
                {
                    motion = aac.CopyClip(_aV3Setting.MouthMorphCancelClip);
                }
                else
                {
                    motion = aac.NewClip();
                }
            }
            else
            {
                motion = GetMouthMorphCancelerAnimation(aV3Setting, aac, avatarDescriptor);
            }
            AV3Utility.SetMotion(animatorController, layerName, AV3Constants.StateName_MouthMorphCancelerEnabled, motion.Clip);

            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Modifying \"{layerName}\" layer...", 1);
        }

        private void AddBypassLayer(AV3Setting aV3Setting, AacFlBase aac, AnimatorController animatorController)
        {
            // Create or replace layer
            var layerName = AV3Constants.LayerName_Bypass;
            var layer = aac.CreateSupportingArbitraryControllerLayer(animatorController, layerName);
            AV3Utility.SetLayerWeight(animatorController, layerName, 0);
            layer.StateMachine.WithEntryPosition(0, -1).WithAnyStatePosition(0, -2).WithExitPosition(0, -3);

            // Create states
            var disable = layer.NewState("DISABLE", 0, 0).Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BYPASS), false);
            var enable = layer.NewState("ENABLE", 0, 1).Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BYPASS), true);
            var dance = layer.NewState("in DANCE", 1, 1).Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BYPASS), true);

            disable.TransitionsTo(enable).
                When(layer.BoolParameter(AV3Constants.ParamName_CN_FORCE_BYPASS_ENABLE).IsTrue());
            var enableToDisable = enable.TransitionsTo(disable).
                When(layer.BoolParameter(AV3Constants.ParamName_CN_FORCE_BYPASS_ENABLE).IsFalse());

            if (!aV3Setting.ChangeAfkFace)
            {
                var afk = layer.NewState("in AFK", -1, 0).Drives(layer.BoolParameter(AV3Constants.ParamName_CN_BYPASS), true);
                var afkExit = layer.NewState("AFK Exit", -1, 1);
                disable.TransitionsTo(afk).
                    When(layer.Av3().AFK.IsTrue());
                afk.TransitionsTo(afkExit).
                    WithTransitionDurationSeconds(aV3Setting.AfkExitDurationSeconds).
                    When(layer.Av3().AFK.IsFalse());
                afkExit.TransitionsTo(disable).Automatically();
            }

            disable.TransitionsTo(dance).
                When(layer.BoolParameter(AV3Constants.ParamName_SYNC_CN_DANCE_GIMMICK_ENABLE).IsTrue()).
                And(layer.Av3().InStation.IsEqualTo(true));
            dance.TransitionsTo(disable).
                When(layer.BoolParameter(AV3Constants.ParamName_SYNC_CN_DANCE_GIMMICK_ENABLE).IsFalse()).
                Or().
                When(layer.Av3().InStation.IsEqualTo(false));

            foreach (var item in aV3Setting.ContactReceivers)
            {
                if (item is ContactReceiver contactReceiver && !string.IsNullOrEmpty(contactReceiver.parameter))
                {
                    switch (contactReceiver.receiverType)
                    {
                        case ContactReceiver.ReceiverType.Constant:
                            disable.TransitionsTo(enable).
                                When(layer.BoolParameter(contactReceiver.parameter).IsTrue());
                            enableToDisable.
                                And(layer.BoolParameter(contactReceiver.parameter).IsFalse());
                            break;
                        case ContactReceiver.ReceiverType.OnEnter:
                            disable.TransitionsTo(enable).
                                When(layer.BoolParameter(contactReceiver.parameter).IsTrue());
                            enableToDisable.
                                And(layer.BoolParameter(contactReceiver.parameter).IsFalse());
                            break;
                        case ContactReceiver.ReceiverType.Proximity:
                            disable.TransitionsTo(enable).
                                When(layer.FloatParameter(contactReceiver.parameter).IsGreaterThan(aV3Setting.ProximityThreshold));
                            enableToDisable.
                                And(layer.FloatParameter(contactReceiver.parameter).IsLessThan(aV3Setting.ProximityThreshold));
                            break;
                    }
                }
            }
        }

        private VRCExpressionsMenu GenerateExMenu(IReadOnlyList<ModeEx> modes, IMenu menu, string exMenuPath, bool useOverLimitMode)
        {
            var loc = _localizationSetting.GetCurrentLocaleTable();

            AssetDatabase.DeleteAsset(exMenuPath);
            var container = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            AssetDatabase.CreateAsset(container, exMenuPath);

            var idToModeIndex = new Dictionary<string, int>();
            var idToModeEx = new Dictionary<string, ModeEx>();
            for (int modeIndex = 0; modeIndex < modes.Count; modeIndex++)
            {
                var mode = modes[modeIndex];
                var id = mode.Mode.GetId();

                idToModeIndex[id] = modeIndex;
                idToModeEx[id] = mode;
            }

            var rootMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            rootMenu.name = AV3Constants.RootMenuName;

            // Re-generate thumbnails
            if (_aV3Setting.GenerateExMenuThumbnails)
            {
                _exMenuThumbnailDrawer.RequestUpdateAll();
                foreach (var mode in modes)
                {
                    _exMenuThumbnailDrawer.GetThumbnail(mode.Mode.Animation);
                    if (_aV3Setting.AddConfig_EmoteSelect)
                    {
                        foreach (var branch in mode.Mode.Branches)
                        {
                            _exMenuThumbnailDrawer.GetThumbnail(branch.BaseAnimation);
                        }
                    }
                }
                _exMenuThumbnailDrawer.Update();
            }

            // Get icons
            var folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("a06282136d558c54aa15d533f163ff59")); // item folder
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("617fecc28d6cb5a459d1297801b9213e")); // logo
            var lockIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/Lock.png");

            // Mode select
            GenerateSubMenuRecursive(rootMenu, menu.Registered, idToModeIndex, container);

            // Emote select
            if (_aV3Setting.AddConfig_EmoteSelect)
            {
                var emoteSelectMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                emoteSelectMenu.name = loc.ExMenu_EmoteSelect;

                emoteSelectMenu.controls.Add(CreateBoolToggleControl(loc.ExMenu_EmoteLock, AV3Constants.ParamName_CN_EMOTE_LOCK_ENABLE, lockIcon));

                GenerateEmoteSelectMenuRecursive(emoteSelectMenu, menu.Registered, idToModeIndex, container, idToModeEx, useOverLimitMode);

                var emoteSelectControl = CreateSubMenuControl(loc.ExMenu_EmoteSelect, emoteSelectMenu, folderIcon);
                emoteSelectControl.parameter = new VRCExpressionsMenu.Control.Parameter() { name = AV3Constants.ParamName_CN_EMOTE_PRELOCK_ENABLE };
                emoteSelectControl.value = 1;
                rootMenu.controls.Add(emoteSelectControl);

                AssetDatabase.AddObjectToAsset(emoteSelectMenu, container);
            }

            // Setting
            GenerateSettingMenu(rootMenu, container);

            container.controls.Add(CreateSubMenuControl(AV3Constants.RootMenuName, rootMenu, logo));
            AssetDatabase.AddObjectToAsset(rootMenu, container);

            EditorUtility.SetDirty(container);

            return container;
        }

        private void GenerateSubMenuRecursive(VRCExpressionsMenu parent, IMenuItemList menuItemList, Dictionary<string, int> idToModeIndex, VRCExpressionsMenu container)
        {
            foreach (var id in menuItemList.Order)
            {
                var type = menuItemList.GetType(id);
                if (type == MenuItemType.Mode)
                {
                    EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating pattern selection controls...", (float)idToModeIndex[id] / idToModeIndex.Count);

                    var mode = new ModeExInner(menuItemList.GetMode(id));
                    var control = CreateIntToggleControl(_modeNameProvider.Provide(mode), AV3Constants.ParamName_EM_EMOTE_PATTERN, idToModeIndex[id], icon: null);

                    Texture2D icon = null;
                    if (_aV3Setting.GenerateExMenuThumbnails && mode.ChangeDefaultFace)
                    {
                        icon = GetExpressionThumbnail(mode.Animation, container);
                    }
                    else
                    {
                        icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("af1ba8919b0ccb94a99caf43ac36f97d")); // face smile
                    }
                    control.icon = icon;

                    parent.controls.Add(control);
                }
                else
                {
                    var group = menuItemList.GetGroup(id);

                    var subMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                    subMenu.name = group.DisplayName;
                    var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("a06282136d558c54aa15d533f163ff59")); // item folder
                    parent.controls.Add(CreateSubMenuControl(group.DisplayName, subMenu, icon));

                    GenerateSubMenuRecursive(subMenu, group, idToModeIndex, container);
                    AssetDatabase.AddObjectToAsset(subMenu, container);
                }
            }
        }

        private Texture2D GetExpressionThumbnail(Domain.Animation animation, VRCExpressionsMenu container)
        {
            var icon = _exMenuThumbnailDrawer.GetThumbnail(animation);
            if (!AssetDatabase.IsMainAsset(icon) && !AssetDatabase.IsSubAsset(icon)) // Do not save icons that have already been generated and error icons
            {
                AssetDatabase.AddObjectToAsset(icon, container);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(container));
            }
            return icon;
        }

        private void GenerateEmoteSelectMenuRecursive(VRCExpressionsMenu parent, IMenuItemList menuItemList, Dictionary<string, int> idToModeIndex, VRCExpressionsMenu container,
            Dictionary<string, ModeEx> idToModeEx, bool useOverLimitMode)
        {
            var loc = _localizationSetting.GetCurrentLocaleTable();

            foreach (var id in menuItemList.Order)
            {
                var type = menuItemList.GetType(id);
                if (type == MenuItemType.Mode)
                {
                    EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating emote selection controls...", (float)idToModeIndex[id] / idToModeIndex.Count);

                    // Get branches
                    var mode = new ModeExInner(menuItemList.GetMode(id));
                    var numOfBranches = mode.Branches.Count(b => b.ShowInEmoteSelect);
                    if (mode.ChangeDefaultFace) { numOfBranches++; }
                    if (numOfBranches <= 0) { continue; }

                    // Create mode folder
                    var modeFolder = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                    modeFolder.name = _modeNameProvider.Provide(mode);
                    var folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("a06282136d558c54aa15d533f163ff59")); // item folder
                    var modeControl = CreateSubMenuControl(_modeNameProvider.Provide(mode), modeFolder, folderIcon);
                    if (useOverLimitMode)
                    {
                        modeControl.parameter = new VRCExpressionsMenu.Control.Parameter() { name = AV3Constants.ParamName_EM_EMOTE_PATTERN };
                        modeControl.value = idToModeIndex[id];
                    }
                    parent.controls.Add(modeControl);

                    // Calculate num of branch folders
                    const int itemLimit = 8;
                    var numOfBranchFolders = numOfBranches / itemLimit;
                    if (numOfBranches % itemLimit != 0) { numOfBranchFolders++; }
                    numOfBranchFolders = Math.Min(numOfBranchFolders, itemLimit);

                    for (int folderIndex = 0; folderIndex < numOfBranchFolders; folderIndex++)
                    {
                        var startBranchIndex = folderIndex * itemLimit;
                        var endBranchIndex = (folderIndex + 1) * itemLimit - 1;

                        // Create branch folder
                        var branchFolder = modeFolder;
                        var createFolder = numOfBranchFolders > 1;
                        if (createFolder)
                        {
                            var branchFolderName = $"{startBranchIndex + 1} - {Math.Min(endBranchIndex + 1, numOfBranches)}";
                            branchFolder = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                            branchFolder.name = branchFolderName;
                            modeFolder.controls.Add(CreateSubMenuControl(branchFolderName, branchFolder, folderIcon));
                        }

                        // Create branch controls
                        if (mode.ChangeDefaultFace)
                        {
                            startBranchIndex--;
                            endBranchIndex--;
                        }
                        for (int branchIndex = startBranchIndex; branchIndex <= endBranchIndex; branchIndex++)
                        {
                            Domain.Animation guid;
                            // Mode
                            if (branchIndex < 0)
                            {
                                guid = mode.Animation;
                            }
                            // Branch
                            else
                            {
                                if (branchIndex >= mode.Branches.Count) { continue; }
                                
                                // Skip branches that shouldn't show in emote select menu
                                if (!mode.Branches[branchIndex].ShowInEmoteSelect) continue;
                                
                                guid = mode.Branches[branchIndex].BaseAnimation;
                            }
                            var emoteIndex = GetEmoteIndex(branchIndex, idToModeEx[id], useOverLimitMode);
                            var animation = AV3Utility.GetAnimationClipWithName(guid);
                            var animationName = !string.IsNullOrEmpty(animation.name) ? animation.name : loc.ModeNameProvider_NoExpression;

                            Texture2D emoteIcon = null;
                            if (_aV3Setting.GenerateExMenuThumbnails)
                            {
                                emoteIcon = GetExpressionThumbnail(guid, container);
                            }
                            else
                            {
                                emoteIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("af1ba8919b0ccb94a99caf43ac36f97d")); // face smile
                            }

                            var control = CreateIntToggleControl(animationName, AV3Constants.ParamName_SYNC_EM_EMOTE, emoteIndex, icon: emoteIcon);
                            branchFolder.controls.Add(control);
                        }

                        if (createFolder) { AssetDatabase.AddObjectToAsset(branchFolder, container); }
                    }
                    AssetDatabase.AddObjectToAsset(modeFolder, container);
                }
                else
                {
                    var group = menuItemList.GetGroup(id);

                    var subMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                    subMenu.name = group.DisplayName;
                    var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("a06282136d558c54aa15d533f163ff59")); // item folder
                    parent.controls.Add(CreateSubMenuControl(group.DisplayName, subMenu, icon));

                    GenerateEmoteSelectMenuRecursive(subMenu, group, idToModeIndex, container, idToModeEx, useOverLimitMode);
                    AssetDatabase.AddObjectToAsset(subMenu, container);
                }
            }
        }

        private void GenerateSettingMenu(VRCExpressionsMenu parent, VRCExpressionsMenu container)
        {
            EditorUtility.DisplayProgressBar(DomainConstants.SystemName, $"Generating setting menu...", 0);

            var loc = _localizationSetting.GetCurrentLocaleTable();

            var settingRoot = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            settingRoot.name = loc.ExMenu_Setting;

            // Emote lock setting
            var lockIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/Lock.png");
            settingRoot.controls.Add(CreateBoolToggleControl(loc.ExMenu_EmoteLock, AV3Constants.ParamName_CN_EMOTE_LOCK_ENABLE, lockIcon));

            // Blink off setting
            if (_aV3Setting.AddConfig_BlinkOff && _aV3Setting.ReplaceBlink)
            {
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/BlinkOff.png");
                settingRoot.controls.Add(CreateBoolToggleControl(loc.ExMenu_BlinkOff,  AV3Constants.ParamName_SYNC_CN_FORCE_BLINK_DISABLE, icon));
            }

            // Dance gimmick setting
            if (_aV3Setting.AddConfig_DanceGimmick)
            {
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("9a20b3a6641e1af4e95e058f361790cb")); // person dance
                settingRoot.controls.Add(CreateBoolToggleControl(loc.ExMenu_DanceGimmick, AV3Constants.ParamName_SYNC_CN_DANCE_GIMMICK_ENABLE, icon));
            }

            // Contact emote lock setting
            if (_aV3Setting.AddConfig_ContactLock)
            {
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/ContactLock.png");
                settingRoot.controls.Add(CreateBoolToggleControl(loc.ExMenu_ContactLock, AV3Constants.ParamName_CN_CONTACT_EMOTE_LOCK_ENABLE, icon));
            }

            // Emote override setting
            if (_aV3Setting.AddConfig_Override)
            {
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("5acca5d9b1a37724880f1a1dc1bc54d3")); // hand waving
                settingRoot.controls.Add(CreateBoolToggleControl(loc.ExMenu_Override, AV3Constants.ParamName_SYNC_CN_EMOTE_OVERRIDE_ENABLE, icon));
            }

            // Wait emote by voice setting
            if (_aV3Setting.AddConfig_Voice)
            {
                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("50865644fb00f2b4d88bf8a8186039f5")); // face gasp
                settingRoot.controls.Add(CreateBoolToggleControl(loc.ExMenu_Voice, AV3Constants.ParamName_SYNC_CN_WAIT_FACE_EMOTE_BY_VOICE, icon));
            }

            // Hand pattern setting
            if (_aV3Setting.AddConfig_HandPattern_Swap || _aV3Setting.AddConfig_HandPattern_DisableLeft || _aV3Setting.AddConfig_HandPattern_DisableRight)
            {
                var swapIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/HandRL.png");
                var leftDisableIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/Additional/HandL_disable.png");
                var rightDisableIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/Additional/HandR_disable.png");

                var handPatternSetting = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                handPatternSetting.name = loc.ExMenu_HandPattern;
                handPatternSetting.controls = new List<VRCExpressionsMenu.Control>();
                if (_aV3Setting.AddConfig_HandPattern_Swap) { handPatternSetting.controls.Add(CreateBoolToggleControl(loc.ExMenu_HandPattern_SwapLR, AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR, swapIcon)); }
                if (_aV3Setting.AddConfig_HandPattern_DisableLeft) { handPatternSetting.controls.Add(CreateBoolToggleControl(loc.ExMenu_HandPattern_DisableLeft, AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT, leftDisableIcon)); }
                if (_aV3Setting.AddConfig_HandPattern_DisableRight) { handPatternSetting.controls.Add(CreateBoolToggleControl(loc.ExMenu_HandPattern_DisableRight, AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT, rightDisableIcon)); }

                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/FaceSelect.png");
                settingRoot.controls.Add(CreateSubMenuControl(loc.ExMenu_HandPattern, handPatternSetting, icon));
                AssetDatabase.AddObjectToAsset(handPatternSetting, container);
            }

            // Controller setting
            if (_aV3Setting.AddConfig_Controller_Quest || _aV3Setting.AddConfig_Controller_Index)
            {
                var questIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/QuestController.png");
                var indexIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/IndexController.png");

                var controllerSetting = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                controllerSetting.name = loc.ExMenu_Controller;
                controllerSetting.controls = new List<VRCExpressionsMenu.Control>();
                if (_aV3Setting.AddConfig_Controller_Quest) { controllerSetting.controls.Add(CreateBoolToggleControl(loc.ExMenu_Controller_Quest, AV3Constants.ParamName_CN_CONTROLLER_TYPE_QUEST, questIcon)); }
                if (_aV3Setting.AddConfig_Controller_Index) { controllerSetting.controls.Add(CreateBoolToggleControl(loc.ExMenu_Controller_Index, AV3Constants.ParamName_CN_CONTROLLER_TYPE_INDEX, indexIcon)); }

                var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/Controller.png");
                settingRoot.controls.Add(CreateSubMenuControl(loc.ExMenu_Controller, controllerSetting, icon));
                AssetDatabase.AddObjectToAsset(controllerSetting, container);
            }

            var settingIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AV3Constants.Path_BearsDenIcons + "/Settings.png");
            parent.controls.Add(CreateSubMenuControl(loc.ExMenu_Setting, settingRoot, settingIcon));
            AssetDatabase.AddObjectToAsset(settingRoot, container);
        }

        private static VRCExpressionsMenu.Control CreateBoolToggleControl(string name, string parameterName, Texture2D icon)
        {
            var control = new VRCExpressionsMenu.Control();
            control.name = name;
            control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
            control.parameter = new VRCExpressionsMenu.Control.Parameter() { name = parameterName };
            control.subParameters = new VRCExpressionsMenu.Control.Parameter[0];
            control.icon = icon;
            control.labels = new VRCExpressionsMenu.Control.Label[0];
            return control;
        }

        private static VRCExpressionsMenu.Control CreateIntToggleControl(string name, string parameterName, int value, Texture2D icon)
        {
            var control = CreateBoolToggleControl(name, parameterName, icon);
            control.value = value;
            return control;
        }

        private static VRCExpressionsMenu.Control CreateSubMenuControl(string name, VRCExpressionsMenu subMenu, Texture2D icon)
        {
            var control = new VRCExpressionsMenu.Control();
            control.name = name;
            control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            control.parameter = new VRCExpressionsMenu.Control.Parameter() { name = string.Empty };
            control.subParameters = new VRCExpressionsMenu.Control.Parameter[0];
            control.subMenu = subMenu;
            control.icon = icon;
            control.labels = new VRCExpressionsMenu.Control.Label[0];
            return control;
        }

        private static GameObject GetMARootObject(VRCAvatarDescriptor avatarDescriptor)
        {
            var avatarRoot = avatarDescriptor.gameObject;
            if (avatarRoot == null)
            {
                return null;
            }

            var rootObject = avatarRoot.transform.Find(AV3Constants.MARootObjectName)?.gameObject;
            if (rootObject == null)
            {
                rootObject = new GameObject(AV3Constants.MARootObjectName);
                rootObject.transform.parent = avatarRoot.transform;
            }

            return rootObject;
        }

        public List<string> GetParentPrefabPathOfMARootObjects()
        {
            return GetExistingMARootObjects()
                .Select(x => GetParentPrefabPathOfMARootObject(x))
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();
        }

        private List<GameObject> GetExistingMARootObjects()
        {
            var maRootObjects = new List<GameObject>();

            var targets = new List<Transform>();
            if (_aV3Setting.TargetAvatar != null)
            {
                targets.Add(_aV3Setting.TargetAvatar.gameObject.transform);
            }
            foreach (var subAvatar in _aV3Setting.SubTargetAvatars)
            {
                if (subAvatar != null)
                {
                    targets.Add(subAvatar.gameObject.transform);
                }
            }

            foreach (var transform in targets)
            {
                var found = transform.Find(AV3Constants.MARootObjectName);
                if (found != null && !maRootObjects.Any(x => ReferenceEquals(x, found.gameObject)))
                {
                    maRootObjects.Add(found.gameObject);
                }
            }

            return maRootObjects;
        }

        private static string GetParentPrefabPathOfMARootObject(GameObject rootObject)
        {
            if (rootObject == null ||
                rootObject.transform.parent == null ||
                !PrefabUtility.IsPartOfAnyPrefab(rootObject.transform.parent.gameObject))
            {
                return string.Empty;
            }

            var parentPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(rootObject.transform.parent.gameObject);
            if (string.IsNullOrEmpty(parentPath) ||
                !parentPath.ToLower().EndsWith(".prefab"))
            {
                return string.Empty;
            }

            var parent = PrefabUtility.LoadPrefabContents(parentPath);
            try
            {
                var found = parent.transform.Find(AV3Constants.MARootObjectName);
                if (found == null ||
                    found.gameObject.transform.parent == null)
                {
                    return string.Empty;
                }

                if (PrefabUtility.IsPartOfAnyPrefab(found.gameObject.transform.parent.gameObject))
                {
                    var deeperParentPath = GetParentPrefabPathOfMARootObject(found.gameObject);
                    if (!string.IsNullOrEmpty(deeperParentPath))
                    {
                        return deeperParentPath;
                    }
                    else
                    {
                        return parentPath;
                    }
                }
                else
                {
                    return parentPath;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(parent);
            }
        }

        private void GenerateMAObject(GameObject rootObject, AnimatorController animatorController, VRCExpressionsMenu exMenu, int defaultModeIndex)
        {
            if (rootObject == null)
            {
                throw new FaceEmoException("Failed to get MA root object.");
            }

            if (PrefabUtility.IsOutermostPrefabInstanceRoot(rootObject))
            {
                PrefabUtility.UnpackPrefabInstance(rootObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            // Add MA components
            AddMergeAnimatorComponent(rootObject, animatorController);
            AddMenuInstallerComponent(rootObject, exMenu);
            AddParameterComponent(rootObject, defaultModeIndex);

            AddBlinkDisablerComponent(rootObject);
            AddTrackingControlDisablerComponent(rootObject);

            // Instantiate prefabs
            ModifyExistingSubPrefabs(rootObject);
            InstantiatePrefabs(rootObject);

            // Update MA object prefab
            string rootObjectPrefabPath = AssetDatabase.GetAssetPath(_aV3Setting.MARootObjectPrefab);
            if (string.IsNullOrEmpty(rootObjectPrefabPath))
            {
                var parent = AV3Constants.Path_PrefabDir + "/" + _aV3Setting.TargetAvatar.name + "_";

                var folderPath = parent + Guid.NewGuid().ToString("N");
                while (AssetDatabase.IsValidFolder(folderPath))
                {
                    folderPath = parent + Guid.NewGuid().ToString("N");
                }
                AV3Utility.CreateFolderRecursively(folderPath);

                rootObjectPrefabPath = folderPath + "/" + AV3Constants.MARootObjectName + ".prefab";
            }
            _aV3Setting.MARootObjectPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(rootObject, rootObjectPrefabPath, InteractionMode.AutomatedAction);
        }

        private void ReplaceMAObject(GameObject rootObject)
        {
            if (rootObject == null)
            {
                throw new FaceEmoException("Failed to get MA root object.");
            }

            var avatarRoot = rootObject.transform.parent.gameObject;
            if (avatarRoot == null)
            {
                throw new FaceEmoException("Failed to get avatar root.");
            }

            try
            {
                UnityEngine.Object.DestroyImmediate(rootObject);
            }
            catch (InvalidOperationException ioe)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, GetPrefabErrorMessage() + "\n\n" + rootObject.GetFullPath(), "OK");
                throw new FaceEmoException("Failed to delete existing instance.", ioe);
            }

            var instantiated = PrefabUtility.InstantiatePrefab(_aV3Setting.MARootObjectPrefab) as GameObject;
            instantiated.transform.parent = avatarRoot.transform;
        }

        private void InstantiatePrefabs(GameObject rootObject)
        {
            var paths = new[] { AV3Constants.Path_EmoteLocker, AV3Constants.Path_IndicatorSound, };
            foreach (var path in paths)
            {
                var prefabName = Path.GetFileNameWithoutExtension(path);

                // Delete existing instance
                var existing = rootObject.transform.Find(prefabName)?.gameObject;
                if (existing != null)
                {
                    try
                    {
                        UnityEngine.Object.DestroyImmediate(existing);
                    }
                    catch (InvalidOperationException ioe)
                    {
                        EditorUtility.DisplayDialog(DomainConstants.SystemName, GetPrefabErrorMessage() + "\n\n" + existing.GetFullPath(), "OK");
                        throw new FaceEmoException("Failed to delete existing instance.", ioe);
                    }
                }

                // Check config
                if (!_aV3Setting.AddConfig_ContactLock && path == AV3Constants.Path_EmoteLocker) { continue; }
                if (!_aV3Setting.AddConfig_ContactLock && path == AV3Constants.Path_IndicatorSound) { continue; }

                // Load prefab
                var loaded = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (loaded == null)
                {
                    throw new FaceEmoException($"Failed to load prefab: {path}");
                }

                // Instantiate prefab
                var instantiated = PrefabUtility.InstantiatePrefab(loaded) as GameObject;
                instantiated.transform.parent = rootObject.transform;
                instantiated.transform.SetAsFirstSibling();

#if USE_MODULAR_AVATAR
                var modularAvatarMergeAnimator = instantiated.GetComponent<ModularAvatarMergeAnimator>();
                if (modularAvatarMergeAnimator != null)
                {
                    modularAvatarMergeAnimator.matchAvatarWriteDefaults = _aV3Setting.MatchAvatarWriteDefaults;
                    EditorUtility.SetDirty(modularAvatarMergeAnimator);
                }
#else
                Debug.LogError("Please install Modular Avatar!");
#endif
            }
        }

        private void ModifyExistingSubPrefabs(GameObject rootObject)
        {
#if USE_MODULAR_AVATAR
            if (rootObject != null && rootObject.transform != null)
            {
                for (int i = 0; i < rootObject.transform.childCount; i++)
                {
                    var child = rootObject.transform.GetChild(i);
                    if (child != null && child.gameObject != null)
                    {
                        var modularAvatarMergeAnimator = child.gameObject.GetComponent<ModularAvatarMergeAnimator>();
                        if (modularAvatarMergeAnimator != null)
                        {
                            modularAvatarMergeAnimator.matchAvatarWriteDefaults = _aV3Setting.MatchAvatarWriteDefaults;
                            EditorUtility.SetDirty(modularAvatarMergeAnimator);
                        }
                    }
                }
            }
#else
            Debug.LogError("Please install Modular Avatar!");
#endif
        }

        private void AddMergeAnimatorComponent(GameObject rootObject, AnimatorController animatorController)
        {
#if USE_MODULAR_AVATAR
            foreach (var component in rootObject.GetComponents<ModularAvatarMergeAnimator>())
            {
                UnityEngine.Object.DestroyImmediate(component);
            }
            var modularAvatarMergeAnimator = rootObject.AddComponent<ModularAvatarMergeAnimator>();

            modularAvatarMergeAnimator.animator = animatorController;
            modularAvatarMergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            modularAvatarMergeAnimator.deleteAttachedAnimator = true;
            modularAvatarMergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            modularAvatarMergeAnimator.matchAvatarWriteDefaults = _aV3Setting.MatchAvatarWriteDefaults;

            EditorUtility.SetDirty(modularAvatarMergeAnimator);
#else
            Debug.LogError("Please install Modular Avatar!");
#endif
        }

        private static void AddMenuInstallerComponent(GameObject rootObject, VRCExpressionsMenu expressionsMenu)
        {
#if USE_MODULAR_AVATAR
            foreach (var component in rootObject.GetComponents<ModularAvatarMenuInstaller>())
            {
                UnityEngine.Object.DestroyImmediate(component);
            }
            var modularAvatarMenuInstaller = rootObject.AddComponent<ModularAvatarMenuInstaller>();

            modularAvatarMenuInstaller.menuToAppend = expressionsMenu;

            EditorUtility.SetDirty(modularAvatarMenuInstaller);
#else
            Debug.LogError("Please install Modular Avatar!");
#endif
        }

        private void AddParameterComponent(GameObject rootObject, int defaultModeIndex)
        {
#if USE_MODULAR_AVATAR
            foreach (var component in rootObject.GetComponents<ModularAvatarParameters>())
            {
                UnityEngine.Object.DestroyImmediate(component);
            }
            var modularAvatarParameters = rootObject.AddComponent<ModularAvatarParameters>();

            // Config (Saved) (Bool)
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_CN_CONTROLLER_TYPE_QUEST,         _aV3Setting.AddConfig_Controller_Quest ? Sync.Bool : Sync.NotSynced,            defaultValue: _aV3Setting.DefaultValue_Controller_Quest ? 1 : 0,            saved: true, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_CN_CONTROLLER_TYPE_INDEX,         _aV3Setting.AddConfig_Controller_Index ? Sync.Bool : Sync.NotSynced,            defaultValue: _aV3Setting.DefaultValue_Controller_Index ? 1 : 0,            saved: true, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_CN_EMOTE_SELECT_SWAP_LR,          _aV3Setting.AddConfig_HandPattern_Swap ? Sync.Bool : Sync.NotSynced,            defaultValue: _aV3Setting.DefaultValue_HandPattern_Swap ? 1 : 0,            saved: true, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_LEFT,     _aV3Setting.AddConfig_HandPattern_DisableLeft ? Sync.Bool : Sync.NotSynced,     defaultValue: _aV3Setting.DefaultValue_HandPattern_DisableLeft ? 1 : 0,     saved: true, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_CN_EMOTE_SELECT_DISABLE_RIGHT,    _aV3Setting.AddConfig_HandPattern_DisableRight ? Sync.Bool : Sync.NotSynced,    defaultValue: _aV3Setting.DefaultValue_HandPattern_DisableRight ? 1 : 0,    saved: true, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_CN_CONTACT_EMOTE_LOCK_ENABLE,     _aV3Setting.AddConfig_ContactLock ? Sync.Bool : Sync.NotSynced,                                defaultValue: _aV3Setting.DefaultValue_ContactLock ? 1 : 0,                 saved: true, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_SYNC_CN_EMOTE_OVERRIDE_ENABLE,    _aV3Setting.AddConfig_Override ? Sync.Bool : Sync.NotSynced,                    defaultValue: _aV3Setting.DefaultValue_Override ? 1 : 0,                    saved: true, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_SYNC_CN_WAIT_FACE_EMOTE_BY_VOICE, _aV3Setting.AddConfig_Voice ? Sync.Bool : Sync.NotSynced,                       defaultValue: _aV3Setting.DefaultValue_Voice ? 1 : 0,                       saved: true, addPrefix: _aV3Setting.AddParameterPrefix));

            // Config (Saved) (Int)
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_EM_EMOTE_PATTERN, Sync.Int, defaultValue: defaultModeIndex, saved: true, addPrefix: _aV3Setting.AddParameterPrefix));

            // Config (Not saved) (Bool)
            var blinkOffEnabled = _aV3Setting.AddConfig_BlinkOff && _aV3Setting.ReplaceBlink;
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_CN_EMOTE_LOCK_ENABLE,         Sync.Bool,       defaultValue: 0, saved: false, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_CN_EMOTE_PRELOCK_ENABLE,      _aV3Setting.AddConfig_EmoteSelect ? Sync.Bool : Sync.NotSynced,     defaultValue: 0, saved: false, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_SYNC_CN_FORCE_BLINK_DISABLE,  blinkOffEnabled ? Sync.Bool : Sync.NotSynced,                       defaultValue: 0, saved: false, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_SYNC_CN_DANCE_GIMMICK_ENABLE, _aV3Setting.AddConfig_DanceGimmick ? Sync.Bool : Sync.NotSynced,    defaultValue: 0, saved: false, addPrefix: _aV3Setting.AddParameterPrefix));

            // Synced (Int)
            modularAvatarParameters.parameters.Add(MAParam(AV3Constants.ParamName_SYNC_EM_EMOTE, Sync.Int, defaultValue: 0, saved: false, addPrefix: _aV3Setting.AddParameterPrefix));

            // Not synced
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_DUMMY_CONSTANT_FALSE, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_EM_EMOTE_SELECT_L, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_EM_EMOTE_SELECT_R, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_EM_EMOTE_PRESELECT, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_CN_BLINK_ENABLE, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_CN_MOUTH_MORPH_CANCEL_ENABLE, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_CN_EMOTE_OVERRIDE, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_CN_BYPASS, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_EV_PLAY_INDICATOR_SOUND, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_CNST_TOUCH_NADENADE_POINT, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_CNST_TOUCH_EMOTE_LOCK_TRIGGER_L, addPrefix: _aV3Setting.AddParameterPrefix));
            modularAvatarParameters.parameters.Add(NotSyncedMAParam(AV3Constants.ParamName_CNST_TOUCH_EMOTE_LOCK_TRIGGER_R, addPrefix: _aV3Setting.AddParameterPrefix));

            EditorUtility.SetDirty(modularAvatarParameters);
#else
            Debug.LogError("Please install Modular Avatar!");
#endif
        }

        private int GetDefaultModeIndex(IReadOnlyList<ModeEx> modes, IMenu menu)
        {
            int defaultModeIndex = 0;
            for (int modeIndex = 0; modeIndex < modes.Count; modeIndex++)
            {
                var mode = modes[modeIndex];
                var id = mode.Mode.GetId();
                if (id == menu.DefaultSelection)
                {
                    defaultModeIndex = modeIndex;
                    break;
                }
            }
            return defaultModeIndex;
        }

#if USE_MODULAR_AVATAR
        private static ParameterConfig MAParam(string name, ParameterSyncType type, float defaultValue, bool saved, bool addPrefix) 
        {
            var parameterConfig = new ParameterConfig();

            parameterConfig.nameOrPrefix = name;
            parameterConfig.syncType = type;
            if (addPrefix) { parameterConfig.remapTo = AV3Constants.ParameterPrefix + name; }
            if (type != Sync.NotSynced)
            {
                parameterConfig.defaultValue = defaultValue;
                parameterConfig.saved = saved;
            }

            return parameterConfig;
        }

        private static ParameterConfig NotSyncedMAParam(string name, bool addPrefix) => MAParam(name, Sync.NotSynced, defaultValue: 0, saved: false, addPrefix: addPrefix);
#endif

        private void AddBlinkDisablerComponent(GameObject rootObject)
        {
#if VALID_VRCSDK3_AVATARS
            foreach (var component in rootObject.GetComponents<BlinkDisabler>())
            {
                UnityEngine.Object.DestroyImmediate(component);
            }

            if (_aV3Setting.ReplaceBlink)
            {
                rootObject.AddComponent<BlinkDisabler>();
            }
#else
            throw new FaceEmoException("Please install latest VRCSDK!");
#endif
        }

        private void AddTrackingControlDisablerComponent(GameObject rootObject)
        {
#if VALID_VRCSDK3_AVATARS
            foreach (var component in rootObject.GetComponents<TrackingControlDisabler>())
            {
                UnityEngine.Object.DestroyImmediate(component);
            }

            if (_aV3Setting.DisableTrackingControls)
            {
                rootObject.AddComponent<TrackingControlDisabler>();
            }
#else
            throw new FaceEmoException("Please install latest VRCSDK!");
#endif
        }

        private AacFlClip GetDefaultFaceAnimation(AacFlBase aac, VRCAvatarDescriptor avatarDescriptor)
        {
            var loc = _localizationSetting.GetCurrentLocaleTable();

            var clip = aac.NewClip();

            // Generate face mesh blendshape animation
            var excludeBlink = !_aV3Setting.ReplaceBlink; // If blinking is not replaced by animation, do not reset the blend shapes for blinking
            var excludeLipSync = true;

            var pathToMesh = new Dictionary<string, SkinnedMeshRenderer>();
            var faceBlendShapes = AV3Utility.GetFaceMeshBlendShapeValues(avatarDescriptor, excludeBlink, excludeLipSync);
            foreach (var mesh in _aV3Setting.AdditionalSkinnedMeshes)
            {
                var blendShapes = AV3Utility.GetBlendShapeValues(mesh, _aV3Setting.TargetAvatar as VRCAvatarDescriptor, excludeBlink, excludeLipSync);
                foreach (var item in blendShapes) { faceBlendShapes[item.Key] = item.Value; }
                if (blendShapes.Any()) { pathToMesh[blendShapes.First().Key.Path] = mesh; }
            }
            foreach (var excluded in _aV3Setting.ExcludedBlendShapes)
            {
                while (faceBlendShapes.ContainsKey(excluded)) { faceBlendShapes.Remove(excluded); }
            }

            foreach (var blendShape in faceBlendShapes)
            {
                SkinnedMeshRenderer mesh;
                if (pathToMesh.ContainsKey(blendShape.Key.Path))
                {
                    mesh = pathToMesh[blendShape.Key.Path];
                }
                else
                {
                    mesh = AV3Utility.GetMeshByPath(blendShape.Key.Path, avatarDescriptor);
                    pathToMesh[blendShape.Key.Path] = mesh;
                }

                if (mesh != null)
                {
                    clip = clip.BlendShape(mesh, blendShape.Key.Name, blendShape.Value);
                }
                else
                {
                    Debug.LogError(loc.FxGenerator_Message_FaceMeshNotFound);
                }
            }

            // Generate additional expresstion objects animation
            foreach (var gameObject in _aV3Setting.AdditionalToggleObjects)
            {
                if (gameObject != null)
                {
                    clip = clip.Toggling(new[] { gameObject }, gameObject.activeSelf);
                }
            }

            foreach (var gameObject in _aV3Setting.AdditionalTransformObjects)
            {
                if (gameObject != null)
                {
                    clip = clip.Positioning(new[] { gameObject }, gameObject.transform.localPosition);
                    clip = clip.Rotationing(new[] { gameObject }, gameObject.transform.localEulerAngles);
                    clip = clip.Scaling(new[] { gameObject }, gameObject.transform.localScale);
                }
            }

            return clip;
        }

        private AacFlClip GetBlinkAnimation(AacFlBase aac, VRCAvatarDescriptor avatarDescriptor)
        {
            var loc = _localizationSetting.GetCurrentLocaleTable();

            var clip = aac.NewClip().Looping();

            var faceMesh = AV3Utility.GetFaceMesh(avatarDescriptor);
            if (faceMesh == null)
            {
                Debug.LogError(loc.FxGenerator_Message_FaceMeshNotFound);
                return clip;
            }

            var blink = AV3Utility.GetBlinkBlendShape(avatarDescriptor);
            if (blink is BlendShape)
            {
                clip = clip.BlendShape(faceMesh, blink.Name, GetBlinkCurve());
            }
            else
            {
                Debug.LogError(loc.FxGenerator_Message_BlinkBlendShapeNotFound);
            }

            return clip;
        }

        private static AnimationCurve GetBlinkCurve()
        {
            const string blendShapePrefix = "blendShape.";

            var template = AssetDatabase.LoadAssetAtPath<AnimationClip>(AV3Constants.Path_BlinkTemplate);
            if (template == null)
            {
                throw new FaceEmoException("Blink template was not found.");
            }

            var bindings = AnimationUtility.GetCurveBindings(template);
            if (bindings.Count() == 1 && bindings.First().propertyName == blendShapePrefix + "blink")
            {
                var binding = bindings.First();
                return AnimationUtility.GetEditorCurve(template, binding);
            }
            else
            {
                throw new FaceEmoException("Invalid blink template count.");
            }
        }

        private AacFlClip GetMouthMorphCancelerAnimation(AV3Setting aV3Setting, AacFlBase aac, VRCAvatarDescriptor avatarDescriptor)
        {
            var clip = aac.NewClip();

            #pragma warning disable CS0612
            var obsolete = aV3Setting.MouthMorphBlendShapes;
            #pragma warning restore CS0612
            if (obsolete.Any())
            {
                var faceMesh = AV3Utility.GetFaceMesh(avatarDescriptor);
                if (faceMesh != null)
                {
                    var faceMeshPath = AV3Utility.GetPathFromAvatarRoot(faceMesh.transform, avatarDescriptor);
                    foreach (var name in obsolete)
                    {
                        var blendShape = new BlendShape(path: faceMeshPath, name: name);
                        if (!aV3Setting.MouthMorphs.Contains(blendShape)) { aV3Setting.MouthMorphs.Add(blendShape); }
                    }
                    obsolete.Clear();
                    EditorUtility.SetDirty(aV3Setting);
                }
            }

            var excludeBlink = false;
            var excludeLipSync = true;
            var blendShapeValues = AV3Utility.GetFaceMeshBlendShapeValues(avatarDescriptor, excludeBlink, excludeLipSync);
            foreach (var mesh in aV3Setting.AdditionalSkinnedMeshes)
            {
                var blendShapes = AV3Utility.GetBlendShapeValues(mesh, avatarDescriptor, excludeBlink, excludeLipSync);
                foreach (var item in blendShapes) { blendShapeValues[item.Key] = item.Value; }
            }

            var mouthMorphBlendShapes = new HashSet<BlendShape>(aV3Setting.MouthMorphs);

            var cachedMeshes = new Dictionary<string, SkinnedMeshRenderer>();
            foreach (var blendShape in blendShapeValues.Keys)
            {
                var weight = blendShapeValues[blendShape];
                if (mouthMorphBlendShapes.Contains(blendShape))
                {
                    var mesh = cachedMeshes.ContainsKey(blendShape.Path) ? cachedMeshes[blendShape.Path] : AV3Utility.GetMeshByPath(blendShape.Path, avatarDescriptor);
                    clip = clip.BlendShape(mesh, blendShape.Name, weight);
                }
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
                AssetKey = "FaceEmo",
                DefaultsProvider = new FaceEmoAacDefaultsProvider(writeDefaults),
            };
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

        private static void CleanAssets()
        {
#if USE_MODULAR_AVATAR
            // To include inactive objects, Resources.FindObjectsOfTypeAll<T>() must be used in Unity 2019.
            var referencedFxGUIDs = new HashSet<string>();
            foreach (var anim in Resources.FindObjectsOfTypeAll<ModularAvatarMergeAnimator>())
            {
                referencedFxGUIDs.Add(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(anim.animator)));
            }

            // Delete dateDir which is not referenced.
            foreach (var dateDir in AssetDatabase.GetSubFolders(AV3Constants.Path_GeneratedDir))
            {
                var referenced = false;
                foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(AnimatorController)}", new[] { dateDir }))
                {
                    if (referencedFxGUIDs.Contains(guid))
                    {
                        referenced = true;
                        break;
                    }
                }

                if (!referenced)
                {
                    if (!AssetDatabase.DeleteAsset(dateDir))
                    {
                        throw new FaceEmoException($"Failed to clean assets in {dateDir}");
                    }
                }
            }
#else
            Debug.LogError("Please install Modular Avatar!");
#endif
        }

        // workaround (to be deleted)
        private static string GetPrefabErrorMessage()
        {
            var locale = LocalizationSetting.GetLocale();
            switch (locale)
            {
                case Locale.ja_JP:
                    return "下記のオブジェクトがPrefab内にあるため、表情メニューをアバターに適用できません。\nFaceEmoPrefabを削除してから再度適用してください。";
                default:
                    return "The facial expression menu cannot be applied to the avatar because the following object is in the Prefab.\nPlease delete FaceEmoPrefab and apply the menu again.";
            }
        }
    }
}
