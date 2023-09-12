using NUnit.Framework;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDKBase;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    [TestFixture]
    public class FxGeneratorTests
    {
        private static readonly string TempDirPath = "Assets/Temp/FxGeneratorTests";
        private static readonly string ProxyPrefabPath = TempDirPath + "/ProxyFaceEmoPrefab.prefab";
        private static readonly string TestAvatarPath = "Assets/TestAssets/Henge/Himiko_2022/Himiko_2022_Test.prefab";

        private AV3Setting _av3Setting;
        private FxGenerator _fxGenerator;
        private Domain.Menu _menu;
        private GameObject _avatarRoot;

        [SetUp]
        public void Setup()
        {
            _av3Setting = ScriptableObject.CreateInstance<AV3Setting>();

            var localizationSetting = new LocalizationSetting();
            var modeNameProvider = new ModeNameProvider(localizationSetting);
            var thumbnailSetting = ScriptableObject.CreateInstance<ThumbnailSetting>();
            var exMenuThumbnailDrawer = new ExMenuThumbnailDrawer(_av3Setting, thumbnailSetting);
            _fxGenerator = new FxGenerator(localizationSetting, modeNameProvider, exMenuThumbnailDrawer, _av3Setting);

            var avatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TestAvatarPath);
            _avatarRoot = PrefabUtility.InstantiatePrefab(avatarPrefab) as GameObject;
            _av3Setting.TargetAvatar = _avatarRoot.GetComponent<VRCAvatarDescriptor>();

            var components = _avatarRoot.GetComponentsInChildren(typeof(VRCContactReceiver), includeInactive: true);
            foreach (var item in components)
            {
                if (item is VRCContactReceiver contactReceiver)
                {
                    _av3Setting.ContactReceivers.Add(contactReceiver);
                }
            }

            if (!AssetDatabase.IsValidFolder(TempDirPath))
            {
                AV3Utility.CreateFolderRecursively(TempDirPath);
            }
            var proxyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProxyPrefabPath);
            if (proxyPrefab == null)
            {
                proxyPrefab = new GameObject();
                PrefabUtility.SaveAsPrefabAssetAndConnect(proxyPrefab, ProxyPrefabPath, InteractionMode.AutomatedAction);
            }
            _av3Setting.MARootObjectPrefab = proxyPrefab;

            _menu = new Domain.Menu();
        }

        [TearDown]
        public void TearDown()
        {
            var generated = GetGeneratedAssets();
            var fxPath = AssetDatabase.GetAssetPath(generated.fx);
            var exMenuPath = AssetDatabase.GetAssetPath(generated.exMenu);

            _fxGenerator.Generate(_menu, forceOverLimitMode: false);
            Assert.That(AssetDatabase.LoadAssetAtPath<AnimatorController>(fxPath) == null, Is.True);
            Assert.That(AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(exMenuPath) == null, Is.True);
            Assert.That(AssetDatabase.IsValidFolder(generated.generatedDir), Is.False);

            GameObject.DestroyImmediate(_avatarRoot);
        }

        [Test]
        public void Generate()
        {
            _fxGenerator.Generate(_menu);
            var generated = GetGeneratedAssets();
            AssertDefaultFaceLayer(generated.fx.layers[0]);
            AssertLeftInputConverterLayer(generated.fx.layers[1]);
            AssertRightInputConverterLayer(generated.fx.layers[2]);
            AssertFaceEmoteLockLayer(generated.fx.layers[3]);
            AssertFaceEmoteControlLayer(generated.fx.layers[4]);
            AssertFaceEmoteSetControlLayer(generated.fx.layers[5]);
            AssertFaceEmotePlayerLayer(generated.fx.layers[6]);
            AssertBlinkLayer(generated.fx.layers[7]);
            AssertMouthMorphCancellerLayer(generated.fx.layers[8]);
            AssertLeftGestureWeightLayer(generated.fx.layers[9]);
            AssertRightGestureWeightLayer(generated.fx.layers[10]);
            AssertLeftGestureSmoothingLayer(generated.fx.layers[11]);
            AssertRightGestureSmoothingLayer(generated.fx.layers[12]);
            AssertBypassLayer(generated.fx.layers[13]);
        }

        [Test]
        public void Generate_ChangeAfkFace()
        {
            _av3Setting.ChangeAfkFace = true;

            _fxGenerator.Generate(_menu);
            var generated = GetGeneratedAssets();
            AssertDefaultFaceLayer(generated.fx.layers[0]);
            AssertLeftInputConverterLayer(generated.fx.layers[1]);
            AssertRightInputConverterLayer(generated.fx.layers[2]);
            AssertFaceEmoteLockLayer(generated.fx.layers[3]);
            AssertFaceEmoteControlLayer(generated.fx.layers[4]);
            AssertFaceEmoteSetControlLayer(generated.fx.layers[5]);
            AssertFaceEmotePlayerLayer(generated.fx.layers[6]);
            AssertBlinkLayer(generated.fx.layers[7]);
            AssertMouthMorphCancellerLayer(generated.fx.layers[8]);
            AssertLeftGestureWeightLayer(generated.fx.layers[9]);
            AssertRightGestureWeightLayer(generated.fx.layers[10]);
            AssertLeftGestureSmoothingLayer(generated.fx.layers[11]);
            AssertRightGestureSmoothingLayer(generated.fx.layers[12]);
            AssertBypassLayer_ChangeAfkFace(generated.fx.layers[13]);
        }

        [Test]
        public void Generate_DisableFxDuringDancing()
        {
            _av3Setting.DisableFxDuringDancing = true;

            _fxGenerator.Generate(_menu);
            var generated = GetGeneratedAssets();
            AssertDefaultFaceLayer(generated.fx.layers[0]);
            AssertDanceGimmickControlLayer(generated.fx.layers[1]);
            AssertLeftInputConverterLayer(generated.fx.layers[2]);
            AssertRightInputConverterLayer(generated.fx.layers[3]);
            AssertFaceEmoteLockLayer(generated.fx.layers[4]);
            AssertFaceEmoteControlLayer(generated.fx.layers[5]);
            AssertFaceEmoteSetControlLayer(generated.fx.layers[6]);
            AssertFaceEmotePlayerLayer(generated.fx.layers[7]);
            AssertBlinkLayer(generated.fx.layers[8]);
            AssertMouthMorphCancellerLayer(generated.fx.layers[9]);
            AssertLeftGestureWeightLayer(generated.fx.layers[10]);
            AssertRightGestureWeightLayer(generated.fx.layers[11]);
            AssertLeftGestureSmoothingLayer(generated.fx.layers[12]);
            AssertRightGestureSmoothingLayer(generated.fx.layers[13]);
            AssertBypassLayer(generated.fx.layers[14]);
        }

        [Test]
        public void Generate_ForceWriteDefaultsOff()
        {
            _av3Setting.DisableFxDuringDancing = false;
            _av3Setting.MatchAvatarWriteDefaults = false;

            _fxGenerator.Generate(_menu);
            var generated = GetGeneratedAssets();
            AssertDefaultFaceLayer(generated.fx.layers[0]);
            AssertDanceGimmickControlLayer(generated.fx.layers[1]);
            AssertLeftInputConverterLayer(generated.fx.layers[2]);
            AssertRightInputConverterLayer(generated.fx.layers[3]);
            AssertFaceEmoteLockLayer(generated.fx.layers[4]);
            AssertFaceEmoteControlLayer(generated.fx.layers[5]);
            AssertFaceEmoteSetControlLayer(generated.fx.layers[6]);
            AssertFaceEmotePlayerLayer(generated.fx.layers[7]);
            AssertBlinkLayer(generated.fx.layers[8]);
            AssertMouthMorphCancellerLayer(generated.fx.layers[9]);
            AssertLeftGestureWeightLayer(generated.fx.layers[10]);
            AssertRightGestureWeightLayer(generated.fx.layers[11]);
            AssertLeftGestureSmoothingLayer(generated.fx.layers[12]);
            AssertRightGestureSmoothingLayer(generated.fx.layers[13]);
            AssertBypassLayer(generated.fx.layers[14]);
        }

        private static void AssertDefaultFaceLayer(AnimatorControllerLayer layer)
        {
            AssertNormalLayer(layer);
            Assert.That(layer.name, Is.EqualTo("[ USER EDIT ] DEFAULT FACE"));
            Assert.That(layer.defaultWeight, Is.EqualTo(1));
            Assert.That(layer.stateMachine.states.Count, Is.EqualTo(2));
            Assert.That(layer.stateMachine.stateMachines.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.entryTransitions.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.anyStateTransitions.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.defaultState.name, Is.EqualTo("DEFAULT"));

            var defaultState = GetState(layer, "DEFAULT");
            AssertNormalState(defaultState);
            AssertDefaultFaceClip(defaultState.motion as AnimationClip);
            Assert.That(defaultState.transitions.Count, Is.EqualTo(1));
            Assert.That(defaultState.behaviours.Count, Is.EqualTo(0));
            AssertNormalStateTransition(defaultState.transitions[0]);
            Assert.That(defaultState.transitions[0].destinationState.name, Is.EqualTo("BYPASS"));
            Assert.That(defaultState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(defaultState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(defaultState.transitions[0].conditions[0].parameter, Is.EqualTo("CN_BYPASS"));
            Assert.That(defaultState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));

            var bypassState = GetState(layer, "BYPASS");
            AssertNormalState(bypassState);
            AssertAacDefaultClip(bypassState.motion as AnimationClip);
            Assert.That(bypassState.transitions.Count, Is.EqualTo(1));
            Assert.That(bypassState.behaviours.Count, Is.EqualTo(0));
            AssertNormalStateTransition(bypassState.transitions[0]);
            Assert.That(bypassState.transitions[0].destinationState.name, Is.EqualTo("DEFAULT"));
            Assert.That(bypassState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(bypassState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(bypassState.transitions[0].conditions[0].parameter, Is.EqualTo("CN_BYPASS"));
            Assert.That(bypassState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
        }

        private static void AssertDanceGimmickControlLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("DANCE GIMICK CONTROL"));
            // TODO: Check other properties
        }

        private static void AssertLeftInputConverterLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("INPUT CONVERTER L"));
            // TODO: Check other properties
        }

        private static void AssertRightInputConverterLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("INPUT CONVERTER R"));
            // TODO: Check other properties
        }

        private static void AssertFaceEmoteLockLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("FACE EMOTE LOCK"));
            // TODO: Check other properties
        }

        private static void AssertFaceEmoteControlLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("FACE EMOTE CONTROL"));
            // TODO: Check other properties
        }

        private static void AssertFaceEmoteSetControlLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("FACE EMOTE SET CONTROL"));
            // TODO: Check other properties
        }

        private static void AssertFaceEmotePlayerLayer(AnimatorControllerLayer layer)
        {
            AssertNormalLayer(layer);
            Assert.That(layer.name, Is.EqualTo("[ USER EDIT ] FACE EMOTE PLAYER"));
            Assert.That(layer.defaultWeight, Is.EqualTo(1));
            Assert.That(layer.stateMachine.states.Count, Is.EqualTo(2));
            Assert.That(layer.stateMachine.stateMachines.Count, Is.EqualTo(2));
            Assert.That(layer.stateMachine.entryTransitions.Count, Is.EqualTo(2));
            Assert.That(layer.stateMachine.anyStateTransitions.Count, Is.EqualTo(2));

            // TODO: Add emote state
            Assert.That(layer.stateMachine.defaultState.name, Is.EqualTo("AFK Standby"));

            AssertNormalStateTransition(layer.stateMachine.anyStateTransitions[0]);
            Assert.That(layer.stateMachine.anyStateTransitions[0].destinationState.name, Is.EqualTo("in OVERRIDE"));
            Assert.That(layer.stateMachine.anyStateTransitions[0].duration, Is.EqualTo(0));
            Assert.That(layer.stateMachine.anyStateTransitions[0].conditions.Count, Is.EqualTo(2));
            Assert.That(layer.stateMachine.anyStateTransitions[0].conditions[0].parameter, Is.EqualTo("CN_EMOTE_OVERRIDE"));
            Assert.That(layer.stateMachine.anyStateTransitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            Assert.That(layer.stateMachine.anyStateTransitions[0].conditions[1].parameter, Is.EqualTo("CN_BYPASS"));
            Assert.That(layer.stateMachine.anyStateTransitions[0].conditions[1].mode, Is.EqualTo(AnimatorConditionMode.IfNot));

            AssertNormalStateTransition(layer.stateMachine.anyStateTransitions[1]);
            Assert.That(layer.stateMachine.anyStateTransitions[1].destinationState.name, Is.EqualTo("BYPASS"));
            Assert.That(layer.stateMachine.anyStateTransitions[1].duration, Is.EqualTo(0));
            Assert.That(layer.stateMachine.anyStateTransitions[1].conditions.Count, Is.EqualTo(1));
            Assert.That(layer.stateMachine.anyStateTransitions[1].conditions[0].parameter, Is.EqualTo("CN_BYPASS"));
            Assert.That(layer.stateMachine.anyStateTransitions[1].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));

            // TODO: check entry transitions
            // TODO: check afk state machine
            // TODO: check not afk state machine

            var overrideState = GetState(layer, "in OVERRIDE");
            AssertNormalState(overrideState);
            AssertAacDefaultClip(overrideState.motion as AnimationClip);
            Assert.That(overrideState.transitions.Count, Is.EqualTo(1));
            Assert.That(overrideState.behaviours.Count, Is.EqualTo(0));
            AssertNormalStateTransition(overrideState.transitions[0]);
            Assert.That(overrideState.transitions[0].isExit, Is.EqualTo(true));
            Assert.That(overrideState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(overrideState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(overrideState.transitions[0].conditions[0].parameter, Is.EqualTo("CN_EMOTE_OVERRIDE"));
            Assert.That(overrideState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.IfNot));

            var bypassState = GetState(layer, "BYPASS");
            AssertNormalState(bypassState);
            AssertAacDefaultClip(bypassState.motion as AnimationClip);
            Assert.That(bypassState.transitions.Count, Is.EqualTo(1));
            Assert.That(bypassState.behaviours.Count, Is.EqualTo(2));
            Assert.That((bypassState.behaviours[0] as VRC_AvatarParameterDriver).localOnly, Is.EqualTo(false));
            Assert.That((bypassState.behaviours[0] as VRC_AvatarParameterDriver).debugString, Is.Null);
            Assert.That((bypassState.behaviours[0] as VRC_AvatarParameterDriver).parameters.Count, Is.EqualTo(2));
            Assert.That((bypassState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Set));
            Assert.That((bypassState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].name, Is.EqualTo("CN_BLINK_ENABLE"));
            Assert.That((bypassState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].value, Is.EqualTo(0));
            Assert.That((bypassState.behaviours[0] as VRC_AvatarParameterDriver).parameters[1].type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Set));
            Assert.That((bypassState.behaviours[0] as VRC_AvatarParameterDriver).parameters[1].name, Is.EqualTo("CN_MOUTH_MORPH_CANCEL_ENABLE"));
            Assert.That((bypassState.behaviours[0] as VRC_AvatarParameterDriver).parameters[1].value, Is.EqualTo(0));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).debugString, Is.Null);
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingHead, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.NoChange));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingLeftHand, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.NoChange));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingRightHand, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.NoChange));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingHip, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.NoChange));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingLeftFoot, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.NoChange));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingRightFoot, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.NoChange));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingLeftFingers, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.NoChange));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingRightFingers, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.NoChange));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingEyes, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Animation));
            Assert.That((bypassState.behaviours[1] as VRC_AnimatorTrackingControl).trackingMouth, Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Tracking));
            AssertNormalStateTransition(bypassState.transitions[0]);
            Assert.That(bypassState.transitions[0].isExit, Is.EqualTo(true));
            Assert.That(bypassState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(bypassState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(bypassState.transitions[0].conditions[0].parameter, Is.EqualTo("CN_BYPASS"));
            Assert.That(bypassState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
        }

        private static void AssertBlinkLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("BLINK"));
            // TODO: Check other properties
        }

        private static void AssertMouthMorphCancellerLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("MOUTH MORPH CANCELLER"));
            // TODO: Check other properties
        }

        private static void AssertLeftGestureWeightLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("Hai_GestureWeightLeft"));
            // TODO: Check other properties
        }

        private static void AssertRightGestureWeightLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("Hai_GestureWeightRight"));
            // TODO: Check other properties
        }

        private static void AssertLeftGestureSmoothingLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("Hai_GestureSmoothingLeft"));
            // TODO: Check other properties
        }

        private static void AssertRightGestureSmoothingLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.name, Is.EqualTo("Hai_GestureSmoothingRight"));
            // TODO: Check other properties
        }

        private static void AssertBypassLayer(AnimatorControllerLayer layer)
        {
            AssertNormalLayer(layer);
            Assert.That(layer.name, Is.EqualTo("BYPASS"));
            Assert.That(layer.defaultWeight, Is.EqualTo(0));
            Assert.That(layer.stateMachine.states.Count, Is.EqualTo(4));
            Assert.That(layer.stateMachine.stateMachines.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.entryTransitions.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.anyStateTransitions.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.defaultState.name, Is.EqualTo("LOCAL GATE"));

            var gateState = GetState(layer, "LOCAL GATE");
            AssertNormalState(gateState);
            AssertAacDefaultClip(gateState.motion as AnimationClip);
            Assert.That(gateState.transitions.Count, Is.EqualTo(1));
            Assert.That(gateState.behaviours.Count, Is.EqualTo(0));
            AssertNormalStateTransition(gateState.transitions[0]);
            Assert.That(gateState.transitions[0].destinationState.name, Is.EqualTo("DISABLE"));
            Assert.That(gateState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(gateState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(gateState.transitions[0].conditions[0].parameter, Is.EqualTo("IsLocal"));
            Assert.That(gateState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));

            var disableState = GetState(layer, "DISABLE");
            AssertNormalState(disableState);
            AssertAacDefaultClip(disableState.motion as AnimationClip);
            Assert.That(disableState.transitions.Count, Is.EqualTo(8));
            Assert.That(disableState.behaviours.Count, Is.EqualTo(1));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).localOnly, Is.EqualTo(true));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).debugString, Is.Null);
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).parameters.Count, Is.EqualTo(1));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Set));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].name, Is.EqualTo("CN_BYPASS"));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].value, Is.EqualTo(0));
            AssertNormalStateTransition(disableState.transitions[0]);
            Assert.That(disableState.transitions[0].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[0].conditions[0].parameter, Is.EqualTo("CN_FORCE_BYPASS_ENABLE"));
            Assert.That(disableState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[1]);
            Assert.That(disableState.transitions[1].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[1].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[1].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[1].conditions[0].parameter, Is.EqualTo("AFK"));
            Assert.That(disableState.transitions[1].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[2]);
            Assert.That(disableState.transitions[2].destinationState.name, Is.EqualTo("in DANCE"));
            Assert.That(disableState.transitions[2].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[2].conditions.Count, Is.EqualTo(2));
            Assert.That(disableState.transitions[2].conditions[0].parameter, Is.EqualTo("SYNC_CN_DANCE_GIMMICK_ENABLE"));
            Assert.That(disableState.transitions[2].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            Assert.That(disableState.transitions[2].conditions[1].parameter, Is.EqualTo("InStation"));
            Assert.That(disableState.transitions[2].conditions[1].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[3]);
            Assert.That(disableState.transitions[3].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[3].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[3].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[3].conditions[0].parameter, Is.EqualTo("Contact_Constant"));
            Assert.That(disableState.transitions[3].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[4]);
            Assert.That(disableState.transitions[4].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[4].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[4].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[4].conditions[0].parameter, Is.EqualTo("Contact_OnEnter"));
            Assert.That(disableState.transitions[4].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[5]);
            Assert.That(disableState.transitions[5].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[5].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[5].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[5].conditions[0].parameter, Is.EqualTo("Contact_Proximity"));
            Assert.That(disableState.transitions[5].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.Greater));
            Assert.That(disableState.transitions[5].conditions[0].threshold, Is.EqualTo(0.1).Within(0.01));
            AssertNormalStateTransition(disableState.transitions[6]);
            Assert.That(disableState.transitions[6].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[6].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[6].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[6].conditions[0].parameter, Is.EqualTo("Contact_Double_Constant"));
            Assert.That(disableState.transitions[6].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[7]);
            Assert.That(disableState.transitions[7].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[7].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[7].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[7].conditions[0].parameter, Is.EqualTo("Contact_Double_Proximity"));
            Assert.That(disableState.transitions[7].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.Greater));
            Assert.That(disableState.transitions[7].conditions[0].threshold, Is.EqualTo(0.1).Within(0.01));

            var enableState = GetState(layer, "ENABLE");
            AssertNormalState(enableState);
            AssertAacDefaultClip(enableState.motion as AnimationClip);
            Assert.That(enableState.transitions.Count, Is.EqualTo(1));
            Assert.That(enableState.behaviours.Count, Is.EqualTo(1));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).localOnly, Is.EqualTo(true));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).debugString, Is.Null);
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).parameters.Count, Is.EqualTo(1));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Set));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].name, Is.EqualTo("CN_BYPASS"));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].value, Is.EqualTo(1));
            AssertNormalStateTransition(enableState.transitions[0]);
            Assert.That(enableState.transitions[0].destinationState.name, Is.EqualTo("DISABLE"));
            Assert.That(enableState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(enableState.transitions[0].conditions.Count, Is.EqualTo(7));
            Assert.That(enableState.transitions[0].conditions[0].parameter, Is.EqualTo("CN_FORCE_BYPASS_ENABLE"));
            Assert.That(enableState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            Assert.That(enableState.transitions[0].conditions[1].parameter, Is.EqualTo("AFK"));
            Assert.That(enableState.transitions[0].conditions[1].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            Assert.That(enableState.transitions[0].conditions[2].parameter, Is.EqualTo("Contact_Constant"));
            Assert.That(enableState.transitions[0].conditions[2].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            Assert.That(enableState.transitions[0].conditions[3].parameter, Is.EqualTo("Contact_OnEnter"));
            Assert.That(enableState.transitions[0].conditions[3].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            Assert.That(enableState.transitions[0].conditions[4].parameter, Is.EqualTo("Contact_Proximity"));
            Assert.That(enableState.transitions[0].conditions[4].mode, Is.EqualTo(AnimatorConditionMode.Less));
            Assert.That(enableState.transitions[0].conditions[4].threshold, Is.EqualTo(0.1).Within(0.01));
            Assert.That(enableState.transitions[0].conditions[5].parameter, Is.EqualTo("Contact_Double_Constant"));
            Assert.That(enableState.transitions[0].conditions[5].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            Assert.That(enableState.transitions[0].conditions[6].parameter, Is.EqualTo("Contact_Double_Proximity"));
            Assert.That(enableState.transitions[0].conditions[6].mode, Is.EqualTo(AnimatorConditionMode.Less));
            Assert.That(enableState.transitions[0].conditions[6].threshold, Is.EqualTo(0.1).Within(0.01));

            var danceState = GetState(layer, "in DANCE");
            AssertNormalState(danceState);
            AssertAacDefaultClip(danceState.motion as AnimationClip);
            Assert.That(danceState.transitions.Count, Is.EqualTo(2));
            Assert.That(danceState.behaviours.Count, Is.EqualTo(1));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).localOnly, Is.EqualTo(true));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).debugString, Is.Null);
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).parameters.Count, Is.EqualTo(1));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Set));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].name, Is.EqualTo("CN_BYPASS"));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].value, Is.EqualTo(1));
            AssertNormalStateTransition(danceState.transitions[0]);
            Assert.That(danceState.transitions[0].destinationState.name, Is.EqualTo("DISABLE"));
            Assert.That(danceState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(danceState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(danceState.transitions[0].conditions[0].parameter, Is.EqualTo("SYNC_CN_DANCE_GIMMICK_ENABLE"));
            Assert.That(danceState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            AssertNormalStateTransition(danceState.transitions[1]);
            Assert.That(danceState.transitions[1].destinationState.name, Is.EqualTo("DISABLE"));
            Assert.That(danceState.transitions[1].duration, Is.EqualTo(0));
            Assert.That(danceState.transitions[1].conditions.Count, Is.EqualTo(1));
            Assert.That(danceState.transitions[1].conditions[0].parameter, Is.EqualTo("InStation"));
            Assert.That(danceState.transitions[1].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
        }

        private static void AssertBypassLayer_ChangeAfkFace(AnimatorControllerLayer layer)
        {
            AssertNormalLayer(layer);
            Assert.That(layer.name, Is.EqualTo("BYPASS"));
            Assert.That(layer.defaultWeight, Is.EqualTo(0));
            Assert.That(layer.stateMachine.states.Count, Is.EqualTo(4));
            Assert.That(layer.stateMachine.stateMachines.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.entryTransitions.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.anyStateTransitions.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.defaultState.name, Is.EqualTo("LOCAL GATE"));

            var gateState = GetState(layer, "LOCAL GATE");
            AssertNormalState(gateState);
            AssertAacDefaultClip(gateState.motion as AnimationClip);
            Assert.That(gateState.transitions.Count, Is.EqualTo(1));
            Assert.That(gateState.behaviours.Count, Is.EqualTo(0));
            AssertNormalStateTransition(gateState.transitions[0]);
            Assert.That(gateState.transitions[0].destinationState.name, Is.EqualTo("DISABLE"));
            Assert.That(gateState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(gateState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(gateState.transitions[0].conditions[0].parameter, Is.EqualTo("IsLocal"));
            Assert.That(gateState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));

            var disableState = GetState(layer, "DISABLE");
            AssertNormalState(disableState);
            AssertAacDefaultClip(disableState.motion as AnimationClip);
            Assert.That(disableState.transitions.Count, Is.EqualTo(7));
            Assert.That(disableState.behaviours.Count, Is.EqualTo(1));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).localOnly, Is.EqualTo(true));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).debugString, Is.Null);
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).parameters.Count, Is.EqualTo(1));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Set));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].name, Is.EqualTo("CN_BYPASS"));
            Assert.That((disableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].value, Is.EqualTo(0));
            AssertNormalStateTransition(disableState.transitions[0]);
            Assert.That(disableState.transitions[0].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[0].conditions[0].parameter, Is.EqualTo("CN_FORCE_BYPASS_ENABLE"));
            Assert.That(disableState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[1]);
            Assert.That(disableState.transitions[1].destinationState.name, Is.EqualTo("in DANCE"));
            Assert.That(disableState.transitions[1].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[1].conditions.Count, Is.EqualTo(2));
            Assert.That(disableState.transitions[1].conditions[0].parameter, Is.EqualTo("SYNC_CN_DANCE_GIMMICK_ENABLE"));
            Assert.That(disableState.transitions[1].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            Assert.That(disableState.transitions[1].conditions[1].parameter, Is.EqualTo("InStation"));
            Assert.That(disableState.transitions[1].conditions[1].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[2]);
            Assert.That(disableState.transitions[2].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[2].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[2].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[2].conditions[0].parameter, Is.EqualTo("Contact_Constant"));
            Assert.That(disableState.transitions[2].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[3]);
            Assert.That(disableState.transitions[3].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[3].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[3].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[3].conditions[0].parameter, Is.EqualTo("Contact_OnEnter"));
            Assert.That(disableState.transitions[3].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[4]);
            Assert.That(disableState.transitions[4].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[4].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[4].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[4].conditions[0].parameter, Is.EqualTo("Contact_Proximity"));
            Assert.That(disableState.transitions[4].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.Greater));
            Assert.That(disableState.transitions[4].conditions[0].threshold, Is.EqualTo(0.1).Within(0.01));
            AssertNormalStateTransition(disableState.transitions[5]);
            Assert.That(disableState.transitions[5].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[5].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[5].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[5].conditions[0].parameter, Is.EqualTo("Contact_Double_Constant"));
            Assert.That(disableState.transitions[5].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.If));
            AssertNormalStateTransition(disableState.transitions[6]);
            Assert.That(disableState.transitions[6].destinationState.name, Is.EqualTo("ENABLE"));
            Assert.That(disableState.transitions[6].duration, Is.EqualTo(0));
            Assert.That(disableState.transitions[6].conditions.Count, Is.EqualTo(1));
            Assert.That(disableState.transitions[6].conditions[0].parameter, Is.EqualTo("Contact_Double_Proximity"));
            Assert.That(disableState.transitions[6].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.Greater));
            Assert.That(disableState.transitions[6].conditions[0].threshold, Is.EqualTo(0.1).Within(0.01));

            var enableState = GetState(layer, "ENABLE");
            AssertNormalState(enableState);
            AssertAacDefaultClip(enableState.motion as AnimationClip);
            Assert.That(enableState.transitions.Count, Is.EqualTo(1));
            Assert.That(enableState.behaviours.Count, Is.EqualTo(1));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).localOnly, Is.EqualTo(true));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).debugString, Is.Null);
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).parameters.Count, Is.EqualTo(1));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Set));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].name, Is.EqualTo("CN_BYPASS"));
            Assert.That((enableState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].value, Is.EqualTo(1));
            AssertNormalStateTransition(enableState.transitions[0]);
            Assert.That(enableState.transitions[0].destinationState.name, Is.EqualTo("DISABLE"));
            Assert.That(enableState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(enableState.transitions[0].conditions.Count, Is.EqualTo(6));
            Assert.That(enableState.transitions[0].conditions[0].parameter, Is.EqualTo("CN_FORCE_BYPASS_ENABLE"));
            Assert.That(enableState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            Assert.That(enableState.transitions[0].conditions[1].parameter, Is.EqualTo("Contact_Constant"));
            Assert.That(enableState.transitions[0].conditions[1].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            Assert.That(enableState.transitions[0].conditions[2].parameter, Is.EqualTo("Contact_OnEnter"));
            Assert.That(enableState.transitions[0].conditions[2].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            Assert.That(enableState.transitions[0].conditions[3].parameter, Is.EqualTo("Contact_Proximity"));
            Assert.That(enableState.transitions[0].conditions[3].mode, Is.EqualTo(AnimatorConditionMode.Less));
            Assert.That(enableState.transitions[0].conditions[3].threshold, Is.EqualTo(0.1).Within(0.01));
            Assert.That(enableState.transitions[0].conditions[4].parameter, Is.EqualTo("Contact_Double_Constant"));
            Assert.That(enableState.transitions[0].conditions[4].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            Assert.That(enableState.transitions[0].conditions[5].parameter, Is.EqualTo("Contact_Double_Proximity"));
            Assert.That(enableState.transitions[0].conditions[5].mode, Is.EqualTo(AnimatorConditionMode.Less));
            Assert.That(enableState.transitions[0].conditions[5].threshold, Is.EqualTo(0.1).Within(0.01));

            var danceState = GetState(layer, "in DANCE");
            AssertNormalState(danceState);
            AssertAacDefaultClip(danceState.motion as AnimationClip);
            Assert.That(danceState.transitions.Count, Is.EqualTo(2));
            Assert.That(danceState.behaviours.Count, Is.EqualTo(1));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).localOnly, Is.EqualTo(true));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).debugString, Is.Null);
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).parameters.Count, Is.EqualTo(1));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Set));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].name, Is.EqualTo("CN_BYPASS"));
            Assert.That((danceState.behaviours[0] as VRC_AvatarParameterDriver).parameters[0].value, Is.EqualTo(1));
            AssertNormalStateTransition(danceState.transitions[0]);
            Assert.That(danceState.transitions[0].destinationState.name, Is.EqualTo("DISABLE"));
            Assert.That(danceState.transitions[0].duration, Is.EqualTo(0));
            Assert.That(danceState.transitions[0].conditions.Count, Is.EqualTo(1));
            Assert.That(danceState.transitions[0].conditions[0].parameter, Is.EqualTo("SYNC_CN_DANCE_GIMMICK_ENABLE"));
            Assert.That(danceState.transitions[0].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
            AssertNormalStateTransition(danceState.transitions[1]);
            Assert.That(danceState.transitions[1].destinationState.name, Is.EqualTo("DISABLE"));
            Assert.That(danceState.transitions[1].duration, Is.EqualTo(0));
            Assert.That(danceState.transitions[1].conditions.Count, Is.EqualTo(1));
            Assert.That(danceState.transitions[1].conditions[0].parameter, Is.EqualTo("InStation"));
            Assert.That(danceState.transitions[1].conditions[0].mode, Is.EqualTo(AnimatorConditionMode.IfNot));
        }

        private static void AssertDefaultFaceClip(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(46));

            // blink
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_blink")), Is.EqualTo(0));

            // lip sync
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_aa")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_ch")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_dd")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_e")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_ff")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_ih")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_kk")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_nn")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_oh")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_ou")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_pp")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_rr")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_sil")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_ss")), Is.Null);
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "vrc.v_th")), Is.Null);

            // others
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_aa")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_ch")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_ou")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_oh")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_niko")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_mu")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_heart")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_wa")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_bero")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "m_aaaa")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_satori_L")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_satori_R")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_niko_L")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_niko_R")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_mabataki_L")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_mabataki_R")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_zito_L")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_zito_R")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_guda")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_turi")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_tare")), Is.EqualTo(100));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_off")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_small")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_pattern1")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "e_pattern2")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "eb_turi_L")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "eb_turi_R")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "eb_tare_L")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "eb_tare_R")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "eb_komari_L")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "eb_komari_R")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "eb_up")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "eb_down")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "eb_front")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "c_puku")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_mabataki")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_satori")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));
        }

        private static void AssertNormalLayer(AnimatorControllerLayer layer)
        {
            Assert.That(layer.avatarMask == null, Is.True);
            Assert.That(layer.blendingMode, Is.EqualTo(AnimatorLayerBlendingMode.Override));
            Assert.That(layer.syncedLayerIndex, Is.EqualTo(-1));
            Assert.That(layer.iKPass, Is.EqualTo(false));
            Assert.That(layer.syncedLayerAffectsTiming, Is.EqualTo(false));
        }

        private static void AssertNormalState(AnimatorState state)
        {
            Assert.That(state.speed, Is.EqualTo(1));
            Assert.That(state.cycleOffset, Is.EqualTo(0));
            Assert.That(state.mirror, Is.EqualTo(false));
            Assert.That(state.iKOnFeet, Is.EqualTo(false));
            Assert.That(state.writeDefaultValues, Is.EqualTo(false));
            Assert.That(state.tag, Is.EqualTo(string.Empty));
            Assert.That(state.speedParameterActive, Is.EqualTo(false));
            Assert.That(state.cycleOffsetParameterActive, Is.EqualTo(false));
            Assert.That(state.mirrorParameterActive, Is.EqualTo(false));
            Assert.That(state.timeParameterActive, Is.EqualTo(false));
        }

        private static void AssertAacDefaultClip(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(1));
            Assert.That(bindings[0].path, Is.EqualTo("_ignored"));
        }

        private static void AssertNormalStateTransition(AnimatorStateTransition transition)
        {
            Assert.That(transition.offset, Is.EqualTo(0));
            Assert.That(transition.interruptionSource, Is.EqualTo(TransitionInterruptionSource.None));
            Assert.That(transition.hasExitTime, Is.EqualTo(false));
            Assert.That(transition.hasFixedDuration, Is.EqualTo(true));
            Assert.That(transition.canTransitionToSelf, Is.EqualTo(false));
            AssertNormalBaseTransition(transition);
        }

        private static void AssertNormalBaseTransition(AnimatorTransitionBase transition)
        {
            Assert.That(transition.solo, Is.EqualTo(false));
            Assert.That(transition.mute, Is.EqualTo(false));
        }

        private static (string generatedDir, AnimatorController fx, VRCExpressionsMenu exMenu) GetGeneratedAssets()
        {
            var generatedDir = AssetDatabase.GetSubFolders("Assets/Suzuryg/FaceEmo/Generated").OrderBy(x => x).Last();
            var fxPath = generatedDir + "/FaceEmo_Fx.controller";
            var exMenuPath = generatedDir + "/FaceEmo_ExMenu.asset";
            var fx = AssetDatabase.LoadAssetAtPath<AnimatorController>(fxPath);
            var exMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(exMenuPath);
            Assert.That(fx != null, Is.True);
            Assert.That(exMenu != null, Is.True);
            return (generatedDir, fx, exMenu);
        }

        private static AnimatorState GetState(AnimatorControllerLayer layer, string stateName)
        {
            var states = layer.stateMachine.states.Where(x => x.state.name == stateName);
            Assert.That(states.Count, Is.EqualTo(1));
            return states.First().state;
        }

        private static float? GetBlendShapeValue(AnimationClip clip, BlendShape blendShape)
        {
            var binding = new EditorCurveBinding { path = blendShape.Path, propertyName = $"blendShape.{blendShape.Name}", type = typeof(SkinnedMeshRenderer) };
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve is null)
            {
                return null;
            }
            else if (curve.keys.Length == 1)
            {
                return curve.keys[0].value;
            }
            else if (curve.keys.Length == 2)
            {
                Assert.That(curve.keys[0].value == curve.keys[1].value, Is.True);
                return curve.keys[0].value;
            }
            else
            {
                return null;
            }
        }
    }
}
