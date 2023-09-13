﻿using NUnit.Framework;
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
        }

        private static void AssertDefaultFaceLayer(AnimatorControllerLayer layer)
        {
            AssertNormalLayer(layer);
            Assert.That(layer.name, Is.EqualTo("[ USER EDIT ] DEFAULT FACE"));
            Assert.That(layer.defaultWeight, Is.EqualTo(1));
            Assert.That(layer.stateMachine.states.Count, Is.EqualTo(1));
            Assert.That(layer.stateMachine.stateMachines.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.entryTransitions.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.anyStateTransitions.Count, Is.EqualTo(0));
            Assert.That(layer.stateMachine.defaultState.name, Is.EqualTo("DEFAULT"));

            var defaultState = GetState(layer, "DEFAULT");
            AssertNormalState(defaultState);
            AssertDefaultFaceClip(defaultState.motion as AnimationClip);
            Assert.That(defaultState.transitions.Count, Is.EqualTo(0));
            Assert.That(defaultState.behaviours.Count, Is.EqualTo(0));
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
            Assert.That(layer.stateMachine.anyStateTransitions.Count, Is.EqualTo(3));

            // TODO: Add emote state
            Assert.That(layer.stateMachine.defaultState.name, Is.EqualTo("AFK Standby"));

            AssertNormalStateTransition(
                layer.stateMachine.anyStateTransitions.Where(x =>
                    x.destinationState.name == "in OVERRIDE" && x.duration == 0 && x.conditions.Count() == 2 &&
                    x.conditions.Any(y => y.parameter == "CN_EMOTE_OVERRIDE" && y.mode == AnimatorConditionMode.If) &&
                    x.conditions.Any(y => y.parameter == "InStation" && y.mode == AnimatorConditionMode.IfNot)));
            AssertNormalStateTransition(
                layer.stateMachine.anyStateTransitions.Where(x =>
                    x.destinationState.name == "in OVERRIDE" && x.duration == 0 && x.conditions.Count() == 2 &&
                    x.conditions.Any(y => y.parameter == "CN_EMOTE_OVERRIDE" && y.mode == AnimatorConditionMode.If) &&
                    x.conditions.Any(y => y.parameter == "SYNC_CN_DANCE_GIMMICK_ENABLE" && y.mode == AnimatorConditionMode.IfNot)));
            AssertNormalStateTransition(
                layer.stateMachine.anyStateTransitions.Where(x =>
                    x.destinationState.name == "in DANCE" && x.duration == 0 && x.conditions.Count() == 3 &&
                    x.conditions.Any(y => y.parameter == "SYNC_CN_DANCE_GIMMICK_ENABLE" && y.mode == AnimatorConditionMode.If) &&
                    x.conditions.Any(y => y.parameter == "InStation" && y.mode == AnimatorConditionMode.If) &&
                    x.conditions.Any(y => y.parameter == "Voice" && y.mode == AnimatorConditionMode.Less && y.threshold == 0.01f)));

            // TODO: check entry transitions
            // TODO: check afk state machine
            // TODO: check not afk state machine

            var overrideState = GetState(layer, "in OVERRIDE");
            AssertNormalState(overrideState);
            AssertAacDefaultClip(overrideState.motion as AnimationClip);
            Assert.That(overrideState.transitions.Count, Is.EqualTo(1));
            Assert.That(overrideState.behaviours.Count, Is.EqualTo(0));
            AssertNormalStateTransition(
                overrideState.transitions.Where(x =>
                    x.isExit && x.duration == 0 && x.conditions.Count() == 1 &&
                    x.conditions.Any(y => y.parameter == "CN_EMOTE_OVERRIDE" && y.mode == AnimatorConditionMode.IfNot)));

            var danceState = GetState(layer, "in DANCE");
            AssertNormalState(danceState);
            AssertAacDefaultClip(danceState.motion as AnimationClip);
            Assert.That(danceState.transitions.Count, Is.EqualTo(2));
            Assert.That(danceState.behaviours.Count, Is.EqualTo(0));
            AssertNormalStateTransition(
                danceState.transitions.Where(x =>
                    x.isExit && x.duration == 0 && x.conditions.Count() == 1 &&
                    x.conditions.Any(y => y.parameter == "SYNC_CN_DANCE_GIMMICK_ENABLE" && y.mode == AnimatorConditionMode.IfNot)));
            AssertNormalStateTransition(
                danceState.transitions.Where(x =>
                    x.isExit && x.duration == 0 && x.conditions.Count() == 1 &&
                    x.conditions.Any(y => y.parameter == "InStation" && y.mode == AnimatorConditionMode.IfNot)));
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

        private static void AssertNormalStateTransition(IEnumerable<AnimatorStateTransition> transitions)
        {
            Assert.That(transitions.Count, Is.EqualTo(1));
            var transition = transitions.First();

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