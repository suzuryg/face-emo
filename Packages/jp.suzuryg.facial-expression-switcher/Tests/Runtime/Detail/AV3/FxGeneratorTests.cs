using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using System.Collections.Specialized;
using VRC.SDK3.Avatars.Components;
using System.Threading;
using VRC.SDKBase;
using NUnit.Framework.Constraints;
using UnityEditor.Graphs;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public class FxGeneratorTests
    {
        private static readonly string AnimationDir = "Assets/Temp/Animation";
        private static readonly string DestinationPath = "Assets/Temp/GeneratedFx.controller";
        private static readonly string AvatarPath = "/Avatar";

        private static readonly string ParamName_IsLocal = "IsLocal";
        private static readonly string ParamName_AFK = "AFK";
        private static readonly string ParamName_Voice = "Voice";
        private static readonly string ParamName_GestureLeft = "GestureLeft";
        private static readonly string ParamName_GestureLeftWeight = "GestureLeftWeight";
        private static readonly string ParamName_GestureRight = "GestureRight";
        private static readonly string ParamName_GestureRightWeight = "GestureRightWeight";
        private static readonly string ParamName_InStation = "InStation";
        private static readonly string ParamName_EM_EMOTE_SELECT_L = "EM_EMOTE_SELECT_L";
        private static readonly string ParamName_EM_EMOTE_SELECT_R = "EM_EMOTE_SELECT_R";

        private GameObject _avatar;
        private Domain.Menu _menu;

        private GameObject _animatorRoot;
        private Animator _animator;
        private List<string> _animationPaths;

        [SetUp]
        public void SetUp()
        {
            _avatar = new GameObject(Path.GetFileName(AvatarPath));
            _avatar.AddComponent<VRCAvatarDescriptor>();
            _menu = new Domain.Menu();
            _menu.Avatar = new Domain.Avatar(AvatarPath);

            _animatorRoot = new GameObject("AnimatorRoot");
            _animator = _animatorRoot.AddComponent<Animator>();
            _animationPaths = new List<string>();
            if (!AssetDatabase.IsValidFolder(AnimationDir))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(AnimationDir), Path.GetFileName(AnimationDir));
            }
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_avatar);
            Object.Destroy(_animatorRoot);
            foreach (var path in _animationPaths)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        [Test]
        public void Template()
        {
            var layerNames = new[] { AV3Constants.LayerName_FaceEmoteControl, };

            var fxGenerator = new FxGenerator();
            fxGenerator.Generate(_menu);

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(DestinationPath);
            _animator.runtimeAnimatorController = controller;
            _animator.enabled = false;

            // Check default face
            Assert.IsTrue(State(AV3Constants.LayerName_DefaultFace).IsName("DEFAULT"));

            // Initial state
            Assert.IsTrue(State(AV3Constants.LayerName_FaceEmoteControl).IsName("INIT"));
            Assert.That(_animator.GetInteger(AV3Constants.ParamName_EM_EMOTE_PRESELECT), Is.EqualTo(0));

            // Not local
            Update(layerNames);
            Assert.IsTrue(State(AV3Constants.LayerName_FaceEmoteControl).IsName("INIT"));
            Assert.That(_animator.GetInteger(AV3Constants.ParamName_EM_EMOTE_PRESELECT), Is.EqualTo(0));

            // Not loaded
            _animator.SetBool(ParamName_IsLocal, true);
            Update(layerNames);
            Assert.IsTrue(State(AV3Constants.LayerName_FaceEmoteControl).IsName("INIT"));
            Assert.That(_animator.GetInteger(AV3Constants.ParamName_EM_EMOTE_PRESELECT), Is.EqualTo(0));

            // Loaded
            _animator.SetBool(AV3Constants.ParamName_CN_EXPRESSION_PARAMETER_LOADING_COMP, true);
            Update(layerNames);
            Assert.IsTrue(State(AV3Constants.LayerName_FaceEmoteControl).IsName("L0 R0"));
            Assert.That(_animator.GetInteger(AV3Constants.ParamName_EM_EMOTE_PRESELECT), Is.EqualTo(0));

            // Mode selection
            AssertPriority(false, false, false, false, Priority.Normal, layerNames);
            AssertPriority(false, false, false, true, Priority.OnlyRight, layerNames);
            AssertPriority(false, false, true, false, Priority.OnlyLeft, layerNames);
            AssertPriority(false, false, true, true, Priority.OnlyLeft, layerNames);
            AssertPriority(false, true, false, false, Priority.PrimeRight, layerNames);
            AssertPriority(false, true, false, true, Priority.PrimeRight, layerNames);
            AssertPriority(false, true, true, false, Priority.PrimeRight, layerNames);
            AssertPriority(false, true, true, true, Priority.PrimeRight, layerNames);
            AssertPriority(true, false, false, false, Priority.PrimeLeft, layerNames);
            AssertPriority(true, false, false, true, Priority.PrimeLeft, layerNames);
            AssertPriority(true, false, true, false, Priority.PrimeLeft, layerNames);
            AssertPriority(true, false, true, true, Priority.PrimeLeft, layerNames);
            AssertPriority(true, true, false, false, Priority.PrimeLeft, layerNames);
            AssertPriority(true, true, false, true, Priority.PrimeLeft, layerNames);
            AssertPriority(true, true, true, false, Priority.PrimeLeft, layerNames);
            AssertPriority(true, true, true, true, Priority.PrimeLeft, layerNames);

            // Normal
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT, false);
            // L0
            AssertEmotePreselect(0, 0, 0, layerNames);
            AssertEmotePreselect(0, 1, 1, layerNames);
            AssertEmotePreselect(0, 2, 2, layerNames);
            AssertEmotePreselect(0, 3, 3, layerNames);
            AssertEmotePreselect(0, 4, 4, layerNames);
            AssertEmotePreselect(0, 5, 5, layerNames);
            AssertEmotePreselect(0, 6, 6, layerNames);
            AssertEmotePreselect(0, 7, 7, layerNames);
            // L1
            AssertEmotePreselect(1, 0, 8, layerNames);
            AssertEmotePreselect(1, 1, 9, layerNames);
            AssertEmotePreselect(1, 2, 10, layerNames);
            AssertEmotePreselect(1, 3, 11, layerNames);
            AssertEmotePreselect(1, 4, 12, layerNames);
            AssertEmotePreselect(1, 5, 13, layerNames);
            AssertEmotePreselect(1, 6, 14, layerNames);
            AssertEmotePreselect(1, 7, 15, layerNames);
            // L2
            AssertEmotePreselect(2, 0, 16, layerNames);
            AssertEmotePreselect(2, 1, 17, layerNames);
            AssertEmotePreselect(2, 2, 18, layerNames);
            AssertEmotePreselect(2, 3, 19, layerNames);
            AssertEmotePreselect(2, 4, 20, layerNames);
            AssertEmotePreselect(2, 5, 21, layerNames);
            AssertEmotePreselect(2, 6, 22, layerNames);
            AssertEmotePreselect(2, 7, 23, layerNames);
            // L3
            AssertEmotePreselect(3, 0, 24, layerNames);
            AssertEmotePreselect(3, 1, 25, layerNames);
            AssertEmotePreselect(3, 2, 26, layerNames);
            AssertEmotePreselect(3, 3, 27, layerNames);
            AssertEmotePreselect(3, 4, 28, layerNames);
            AssertEmotePreselect(3, 5, 29, layerNames);
            AssertEmotePreselect(3, 6, 30, layerNames);
            AssertEmotePreselect(3, 7, 31, layerNames);
            // L4
            AssertEmotePreselect(4, 0, 32, layerNames);
            AssertEmotePreselect(4, 1, 33, layerNames);
            AssertEmotePreselect(4, 2, 34, layerNames);
            AssertEmotePreselect(4, 3, 35, layerNames);
            AssertEmotePreselect(4, 4, 36, layerNames);
            AssertEmotePreselect(4, 5, 37, layerNames);
            AssertEmotePreselect(4, 6, 38, layerNames);
            AssertEmotePreselect(4, 7, 39, layerNames);
            // L5
            AssertEmotePreselect(5, 0, 40, layerNames);
            AssertEmotePreselect(5, 1, 41, layerNames);
            AssertEmotePreselect(5, 2, 42, layerNames);
            AssertEmotePreselect(5, 3, 43, layerNames);
            AssertEmotePreselect(5, 4, 44, layerNames);
            AssertEmotePreselect(5, 5, 45, layerNames);
            AssertEmotePreselect(5, 6, 46, layerNames);
            AssertEmotePreselect(5, 7, 47, layerNames);
            // L6
            AssertEmotePreselect(6, 0, 48, layerNames);
            AssertEmotePreselect(6, 1, 49, layerNames);
            AssertEmotePreselect(6, 2, 50, layerNames);
            AssertEmotePreselect(6, 3, 51, layerNames);
            AssertEmotePreselect(6, 4, 52, layerNames);
            AssertEmotePreselect(6, 5, 53, layerNames);
            AssertEmotePreselect(6, 6, 54, layerNames);
            AssertEmotePreselect(6, 7, 55, layerNames);
            // L7
            AssertEmotePreselect(7, 0, 56, layerNames);
            AssertEmotePreselect(7, 1, 57, layerNames);
            AssertEmotePreselect(7, 2, 58, layerNames);
            AssertEmotePreselect(7, 3, 59, layerNames);
            AssertEmotePreselect(7, 4, 60, layerNames);
            AssertEmotePreselect(7, 5, 61, layerNames);
            AssertEmotePreselect(7, 6, 62, layerNames);
            AssertEmotePreselect(7, 7, 63, layerNames);

            // Prime left
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT, true);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT, false);
            // L0
            AssertEmotePreselect(0, 0, 0, layerNames);
            AssertEmotePreselect(0, 1, 1, layerNames);
            AssertEmotePreselect(0, 2, 2, layerNames);
            AssertEmotePreselect(0, 3, 3, layerNames);
            AssertEmotePreselect(0, 4, 4, layerNames);
            AssertEmotePreselect(0, 5, 5, layerNames);
            AssertEmotePreselect(0, 6, 6, layerNames);
            AssertEmotePreselect(0, 7, 7, layerNames);
            // L1
            AssertEmotePreselect(1, 0, 8, layerNames);
            AssertEmotePreselect(1, 1, 8, layerNames);
            AssertEmotePreselect(1, 2, 8, layerNames);
            AssertEmotePreselect(1, 3, 8, layerNames);
            AssertEmotePreselect(1, 4, 8, layerNames);
            AssertEmotePreselect(1, 5, 8, layerNames);
            AssertEmotePreselect(1, 6, 8, layerNames);
            AssertEmotePreselect(1, 7, 8, layerNames);
            // L2
            AssertEmotePreselect(2, 0, 16, layerNames);
            AssertEmotePreselect(2, 1, 16, layerNames);
            AssertEmotePreselect(2, 2, 16, layerNames);
            AssertEmotePreselect(2, 3, 16, layerNames);
            AssertEmotePreselect(2, 4, 16, layerNames);
            AssertEmotePreselect(2, 5, 16, layerNames);
            AssertEmotePreselect(2, 6, 16, layerNames);
            AssertEmotePreselect(2, 7, 16, layerNames);
            // L3
            AssertEmotePreselect(3, 0, 24, layerNames);
            AssertEmotePreselect(3, 1, 24, layerNames);
            AssertEmotePreselect(3, 2, 24, layerNames);
            AssertEmotePreselect(3, 3, 24, layerNames);
            AssertEmotePreselect(3, 4, 24, layerNames);
            AssertEmotePreselect(3, 5, 24, layerNames);
            AssertEmotePreselect(3, 6, 24, layerNames);
            AssertEmotePreselect(3, 7, 24, layerNames);
            // L4
            AssertEmotePreselect(4, 0, 32, layerNames);
            AssertEmotePreselect(4, 1, 32, layerNames);
            AssertEmotePreselect(4, 2, 32, layerNames);
            AssertEmotePreselect(4, 3, 32, layerNames);
            AssertEmotePreselect(4, 4, 32, layerNames);
            AssertEmotePreselect(4, 5, 32, layerNames);
            AssertEmotePreselect(4, 6, 32, layerNames);
            AssertEmotePreselect(4, 7, 32, layerNames);
            // L5
            AssertEmotePreselect(5, 0, 40, layerNames);
            AssertEmotePreselect(5, 1, 40, layerNames);
            AssertEmotePreselect(5, 2, 40, layerNames);
            AssertEmotePreselect(5, 3, 40, layerNames);
            AssertEmotePreselect(5, 4, 40, layerNames);
            AssertEmotePreselect(5, 5, 40, layerNames);
            AssertEmotePreselect(5, 6, 40, layerNames);
            AssertEmotePreselect(5, 7, 40, layerNames);
            // L6
            AssertEmotePreselect(6, 0, 48, layerNames);
            AssertEmotePreselect(6, 1, 48, layerNames);
            AssertEmotePreselect(6, 2, 48, layerNames);
            AssertEmotePreselect(6, 3, 48, layerNames);
            AssertEmotePreselect(6, 4, 48, layerNames);
            AssertEmotePreselect(6, 5, 48, layerNames);
            AssertEmotePreselect(6, 6, 48, layerNames);
            AssertEmotePreselect(6, 7, 48, layerNames);
            // L7
            AssertEmotePreselect(7, 0, 56, layerNames);
            AssertEmotePreselect(7, 1, 56, layerNames);
            AssertEmotePreselect(7, 2, 56, layerNames);
            AssertEmotePreselect(7, 3, 56, layerNames);
            AssertEmotePreselect(7, 4, 56, layerNames);
            AssertEmotePreselect(7, 5, 56, layerNames);
            AssertEmotePreselect(7, 6, 56, layerNames);
            AssertEmotePreselect(7, 7, 56, layerNames);

            // Prime right
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT, true);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT, false);
            // L0
            AssertEmotePreselect(0, 0, 0, layerNames);
            AssertEmotePreselect(0, 1, 1, layerNames);
            AssertEmotePreselect(0, 2, 2, layerNames);
            AssertEmotePreselect(0, 3, 3, layerNames);
            AssertEmotePreselect(0, 4, 4, layerNames);
            AssertEmotePreselect(0, 5, 5, layerNames);
            AssertEmotePreselect(0, 6, 6, layerNames);
            AssertEmotePreselect(0, 7, 7, layerNames);
            // L1
            AssertEmotePreselect(1, 0, 8, layerNames);
            AssertEmotePreselect(1, 1, 1, layerNames);
            AssertEmotePreselect(1, 2, 2, layerNames);
            AssertEmotePreselect(1, 3, 3, layerNames);
            AssertEmotePreselect(1, 4, 4, layerNames);
            AssertEmotePreselect(1, 5, 5, layerNames);
            AssertEmotePreselect(1, 6, 6, layerNames);
            AssertEmotePreselect(1, 7, 7, layerNames);
            // L2
            AssertEmotePreselect(2, 0, 16, layerNames);
            AssertEmotePreselect(2, 1, 1, layerNames);
            AssertEmotePreselect(2, 2, 2, layerNames);
            AssertEmotePreselect(2, 3, 3, layerNames);
            AssertEmotePreselect(2, 4, 4, layerNames);
            AssertEmotePreselect(2, 5, 5, layerNames);
            AssertEmotePreselect(2, 6, 6, layerNames);
            AssertEmotePreselect(2, 7, 7, layerNames);
            // L3
            AssertEmotePreselect(3, 0, 24, layerNames);
            AssertEmotePreselect(3, 1, 1, layerNames);
            AssertEmotePreselect(3, 2, 2, layerNames);
            AssertEmotePreselect(3, 3, 3, layerNames);
            AssertEmotePreselect(3, 4, 4, layerNames);
            AssertEmotePreselect(3, 5, 5, layerNames);
            AssertEmotePreselect(3, 6, 6, layerNames);
            AssertEmotePreselect(3, 7, 7, layerNames);
            // L4
            AssertEmotePreselect(4, 0, 32, layerNames);
            AssertEmotePreselect(4, 1, 1, layerNames);
            AssertEmotePreselect(4, 2, 2, layerNames);
            AssertEmotePreselect(4, 3, 3, layerNames);
            AssertEmotePreselect(4, 4, 4, layerNames);
            AssertEmotePreselect(4, 5, 5, layerNames);
            AssertEmotePreselect(4, 6, 6, layerNames);
            AssertEmotePreselect(4, 7, 7, layerNames);
            // L5
            AssertEmotePreselect(5, 0, 40, layerNames);
            AssertEmotePreselect(5, 1, 1, layerNames);
            AssertEmotePreselect(5, 2, 2, layerNames);
            AssertEmotePreselect(5, 3, 3, layerNames);
            AssertEmotePreselect(5, 4, 4, layerNames);
            AssertEmotePreselect(5, 5, 5, layerNames);
            AssertEmotePreselect(5, 6, 6, layerNames);
            AssertEmotePreselect(5, 7, 7, layerNames);
            // L6
            AssertEmotePreselect(6, 0, 48, layerNames);
            AssertEmotePreselect(6, 1, 1, layerNames);
            AssertEmotePreselect(6, 2, 2, layerNames);
            AssertEmotePreselect(6, 3, 3, layerNames);
            AssertEmotePreselect(6, 4, 4, layerNames);
            AssertEmotePreselect(6, 5, 5, layerNames);
            AssertEmotePreselect(6, 6, 6, layerNames);
            AssertEmotePreselect(6, 7, 7, layerNames);
            // L7
            AssertEmotePreselect(7, 0, 56, layerNames);
            AssertEmotePreselect(7, 1, 1, layerNames);
            AssertEmotePreselect(7, 2, 2, layerNames);
            AssertEmotePreselect(7, 3, 3, layerNames);
            AssertEmotePreselect(7, 4, 4, layerNames);
            AssertEmotePreselect(7, 5, 5, layerNames);
            AssertEmotePreselect(7, 6, 6, layerNames);
            AssertEmotePreselect(7, 7, 7, layerNames);

            // Only left
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT, true);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT, false);
            // L0
            AssertEmotePreselect(0, 0, 0, layerNames);
            AssertEmotePreselect(0, 1, 0, layerNames);
            AssertEmotePreselect(0, 2, 0, layerNames);
            AssertEmotePreselect(0, 3, 0, layerNames);
            AssertEmotePreselect(0, 4, 0, layerNames);
            AssertEmotePreselect(0, 5, 0, layerNames);
            AssertEmotePreselect(0, 6, 0, layerNames);
            AssertEmotePreselect(0, 7, 0, layerNames);
            // L1
            AssertEmotePreselect(1, 0, 8, layerNames);
            AssertEmotePreselect(1, 1, 8, layerNames);
            AssertEmotePreselect(1, 2, 8, layerNames);
            AssertEmotePreselect(1, 3, 8, layerNames);
            AssertEmotePreselect(1, 4, 8, layerNames);
            AssertEmotePreselect(1, 5, 8, layerNames);
            AssertEmotePreselect(1, 6, 8, layerNames);
            AssertEmotePreselect(1, 7, 8, layerNames);
            // L2
            AssertEmotePreselect(2, 0, 16, layerNames);
            AssertEmotePreselect(2, 1, 16, layerNames);
            AssertEmotePreselect(2, 2, 16, layerNames);
            AssertEmotePreselect(2, 3, 16, layerNames);
            AssertEmotePreselect(2, 4, 16, layerNames);
            AssertEmotePreselect(2, 5, 16, layerNames);
            AssertEmotePreselect(2, 6, 16, layerNames);
            AssertEmotePreselect(2, 7, 16, layerNames);
            // L3
            AssertEmotePreselect(3, 0, 24, layerNames);
            AssertEmotePreselect(3, 1, 24, layerNames);
            AssertEmotePreselect(3, 2, 24, layerNames);
            AssertEmotePreselect(3, 3, 24, layerNames);
            AssertEmotePreselect(3, 4, 24, layerNames);
            AssertEmotePreselect(3, 5, 24, layerNames);
            AssertEmotePreselect(3, 6, 24, layerNames);
            AssertEmotePreselect(3, 7, 24, layerNames);
            // L4
            AssertEmotePreselect(4, 0, 32, layerNames);
            AssertEmotePreselect(4, 1, 32, layerNames);
            AssertEmotePreselect(4, 2, 32, layerNames);
            AssertEmotePreselect(4, 3, 32, layerNames);
            AssertEmotePreselect(4, 4, 32, layerNames);
            AssertEmotePreselect(4, 5, 32, layerNames);
            AssertEmotePreselect(4, 6, 32, layerNames);
            AssertEmotePreselect(4, 7, 32, layerNames);
            // L5
            AssertEmotePreselect(5, 0, 40, layerNames);
            AssertEmotePreselect(5, 1, 40, layerNames);
            AssertEmotePreselect(5, 2, 40, layerNames);
            AssertEmotePreselect(5, 3, 40, layerNames);
            AssertEmotePreselect(5, 4, 40, layerNames);
            AssertEmotePreselect(5, 5, 40, layerNames);
            AssertEmotePreselect(5, 6, 40, layerNames);
            AssertEmotePreselect(5, 7, 40, layerNames);
            // L6
            AssertEmotePreselect(6, 0, 48, layerNames);
            AssertEmotePreselect(6, 1, 48, layerNames);
            AssertEmotePreselect(6, 2, 48, layerNames);
            AssertEmotePreselect(6, 3, 48, layerNames);
            AssertEmotePreselect(6, 4, 48, layerNames);
            AssertEmotePreselect(6, 5, 48, layerNames);
            AssertEmotePreselect(6, 6, 48, layerNames);
            AssertEmotePreselect(6, 7, 48, layerNames);
            // L7
            AssertEmotePreselect(7, 0, 56, layerNames);
            AssertEmotePreselect(7, 1, 56, layerNames);
            AssertEmotePreselect(7, 2, 56, layerNames);
            AssertEmotePreselect(7, 3, 56, layerNames);
            AssertEmotePreselect(7, 4, 56, layerNames);
            AssertEmotePreselect(7, 5, 56, layerNames);
            AssertEmotePreselect(7, 6, 56, layerNames);
            AssertEmotePreselect(7, 7, 56, layerNames);

            // Only right
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT, false);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT, true);
            // L0
            AssertEmotePreselect(0, 0, 0, layerNames);
            AssertEmotePreselect(0, 1, 1, layerNames);
            AssertEmotePreselect(0, 2, 2, layerNames);
            AssertEmotePreselect(0, 3, 3, layerNames);
            AssertEmotePreselect(0, 4, 4, layerNames);
            AssertEmotePreselect(0, 5, 5, layerNames);
            AssertEmotePreselect(0, 6, 6, layerNames);
            AssertEmotePreselect(0, 7, 7, layerNames);
            // L1
            AssertEmotePreselect(1, 0, 0, layerNames);
            AssertEmotePreselect(1, 1, 1, layerNames);
            AssertEmotePreselect(1, 2, 2, layerNames);
            AssertEmotePreselect(1, 3, 3, layerNames);
            AssertEmotePreselect(1, 4, 4, layerNames);
            AssertEmotePreselect(1, 5, 5, layerNames);
            AssertEmotePreselect(1, 6, 6, layerNames);
            AssertEmotePreselect(1, 7, 7, layerNames);
            // L2
            AssertEmotePreselect(2, 0, 0, layerNames);
            AssertEmotePreselect(2, 1, 1, layerNames);
            AssertEmotePreselect(2, 2, 2, layerNames);
            AssertEmotePreselect(2, 3, 3, layerNames);
            AssertEmotePreselect(2, 4, 4, layerNames);
            AssertEmotePreselect(2, 5, 5, layerNames);
            AssertEmotePreselect(2, 6, 6, layerNames);
            AssertEmotePreselect(2, 7, 7, layerNames);
            // L3
            AssertEmotePreselect(3, 0, 0, layerNames);
            AssertEmotePreselect(3, 1, 1, layerNames);
            AssertEmotePreselect(3, 2, 2, layerNames);
            AssertEmotePreselect(3, 3, 3, layerNames);
            AssertEmotePreselect(3, 4, 4, layerNames);
            AssertEmotePreselect(3, 5, 5, layerNames);
            AssertEmotePreselect(3, 6, 6, layerNames);
            AssertEmotePreselect(3, 7, 7, layerNames);
            // L4
            AssertEmotePreselect(4, 0, 0, layerNames);
            AssertEmotePreselect(4, 1, 1, layerNames);
            AssertEmotePreselect(4, 2, 2, layerNames);
            AssertEmotePreselect(4, 3, 3, layerNames);
            AssertEmotePreselect(4, 4, 4, layerNames);
            AssertEmotePreselect(4, 5, 5, layerNames);
            AssertEmotePreselect(4, 6, 6, layerNames);
            AssertEmotePreselect(4, 7, 7, layerNames);
            // L5
            AssertEmotePreselect(5, 0, 0, layerNames);
            AssertEmotePreselect(5, 1, 1, layerNames);
            AssertEmotePreselect(5, 2, 2, layerNames);
            AssertEmotePreselect(5, 3, 3, layerNames);
            AssertEmotePreselect(5, 4, 4, layerNames);
            AssertEmotePreselect(5, 5, 5, layerNames);
            AssertEmotePreselect(5, 6, 6, layerNames);
            AssertEmotePreselect(5, 7, 7, layerNames);
            // L6
            AssertEmotePreselect(6, 0, 0, layerNames);
            AssertEmotePreselect(6, 1, 1, layerNames);
            AssertEmotePreselect(6, 2, 2, layerNames);
            AssertEmotePreselect(6, 3, 3, layerNames);
            AssertEmotePreselect(6, 4, 4, layerNames);
            AssertEmotePreselect(6, 5, 5, layerNames);
            AssertEmotePreselect(6, 6, 6, layerNames);
            AssertEmotePreselect(6, 7, 7, layerNames);
            // L7
            AssertEmotePreselect(7, 0, 0, layerNames);
            AssertEmotePreselect(7, 1, 1, layerNames);
            AssertEmotePreselect(7, 2, 2, layerNames);
            AssertEmotePreselect(7, 3, 3, layerNames);
            AssertEmotePreselect(7, 4, 4, layerNames);
            AssertEmotePreselect(7, 5, 5, layerNames);
            AssertEmotePreselect(7, 6, 6, layerNames);
            AssertEmotePreselect(7, 7, 7, layerNames);
        }

        private enum Priority
        {
            Normal,
            PrimeLeft,
            PrimeRight,
            OnlyLeft,
            OnlyRight,
        }

        private void AssertPriority(bool priorityLeft, bool priorityRight, bool onlyLeft, bool onlyRight, Priority priority, IReadOnlyList<string> layerNames)
        {
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT, priorityLeft);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT, priorityRight);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT, onlyLeft);
            _animator.SetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT, onlyRight);

            // No transition to other modes unless the gesture changes.
            _animator.SetInteger(ParamName_EM_EMOTE_SELECT_L, 7);
            _animator.SetInteger(ParamName_EM_EMOTE_SELECT_R, 7);
            Update(layerNames);

            _animator.SetInteger(ParamName_EM_EMOTE_SELECT_L, 0);
            _animator.SetInteger(ParamName_EM_EMOTE_SELECT_R, 0);
            Update(layerNames);
            Assert.IsTrue(State(AV3Constants.LayerName_FaceEmoteControl).IsName($"{AV3Constants.LayerName_FaceEmoteControl}.{priority}.L0.L0 R0"));

            switch (priority)
            {
                case Priority.Normal:
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT), Is.EqualTo(false));
                    break;
                case Priority.PrimeLeft:
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT), Is.EqualTo(true));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT), Is.EqualTo(false));
                    break;
                case Priority.PrimeRight:
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT), Is.EqualTo(true));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT), Is.EqualTo(false));
                    break;
                case Priority.OnlyLeft:
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT), Is.EqualTo(true));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT), Is.EqualTo(false));
                    break;
                case Priority.OnlyRight:
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_LEFT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_PRIORITY_RIGHT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_LEFT), Is.EqualTo(false));
                    Assert.That(_animator.GetBool(AV3Constants.ParamName_CN_EMOTE_SELECT_ONLY_RIGHT), Is.EqualTo(true));
                    break;
            }
        }

        private void AssertEmotePreselect(int left, int right, int preselect, IReadOnlyList<string> layerNames)
        {
            // No transition to other modes unless the gesture changes.
            _animator.SetInteger(ParamName_EM_EMOTE_SELECT_L, left + 1);
            _animator.SetInteger(ParamName_EM_EMOTE_SELECT_R, right + 1);
            Update(layerNames);

            _animator.SetInteger(ParamName_EM_EMOTE_SELECT_L, left);
            _animator.SetInteger(ParamName_EM_EMOTE_SELECT_R, right);
            Update(layerNames);
            Assert.IsTrue(State(AV3Constants.LayerName_FaceEmoteControl).IsName($"L{left} R{right}"));
            Assert.That(_animator.GetInteger(AV3Constants.ParamName_EM_EMOTE_PRESELECT), Is.EqualTo(preselect));
        }

        [Test]
        public void PlayExpression()
        {
            var normal = _menu.AddMode(Domain.Menu.RegisteredId);
            _menu.ModifyModeProperties(normal, "Normal");
            _menu.AddBranch(normal, new[] { new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals ) });
            _menu.SetAnimation(NewAnimation("Expression"), normal, 0, BranchAnimationType.Base);

            //var fxGenerator = new FxGenerator();
            //fxGenerator.Generate(_menu);

            //var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(DestinationPath);
            //_animator.runtimeAnimatorController = controller;
            //_animator.enabled = false;

            //Assert.IsTrue(State(AV3Constants.LayerName_FaceEmoteSetControl).IsName("INIT"));
            //Assert.IsTrue(State(AV3Constants.LayerName_FaceEmotePlayer).IsName("Empty"));

            //_animator.Update(1 / 60f);
            //Assert.IsTrue(State(AV3Constants.LayerName_FaceEmoteControl).IsName("Expression"));
            //Assert.IsTrue(Clip(AV3Constants.LayerName_FaceEmoteControl).IsName("Expression"));
        }

        private Domain.Animation NewAnimation(string clipName)
        {
            var clip = new AnimationClip();
            var path = Path.Combine(AnimationDir, clipName + ".anim");
            AssetDatabase.CreateAsset(clip, path);
            _animationPaths.Add(path);
            return new Domain.Animation(AssetDatabase.AssetPathToGUID(path));
        }

        private int GetLayerIndex(string layerName)
        {
            var animatorController = _animator.runtimeAnimatorController as AnimatorController;
            return animatorController.layers.ToList().FindIndex(x => x.name == layerName);
        }

        private AnimatorStateInfo State(string layerName)
        {
            return _animator.GetCurrentAnimatorStateInfo(GetLayerIndex(layerName));
        }

        private AnimatorStateInfo StateByIndex(int layerIndex)
        {
            return _animator.GetCurrentAnimatorStateInfo(layerIndex);
        }

        private AnimatorClipInfo Clip(string layerName)
        {
            var clips = _animator.GetCurrentAnimatorClipInfo(GetLayerIndex(layerName));
            Assert.That(clips.Count, Is.EqualTo(1));
            return clips[0];
        }

        private void Update(IReadOnlyList<string> layerNames)
        {
            const int maxLoop = 100;

            var layerIndices = layerNames.Select(x => GetLayerIndex(x)).ToList();
            var prevStates = layerIndices.Select(x => StateByIndex(x)).ToList();
            _animator.Update(1 / 60f);
            var nowStates = layerIndices.Select(x => StateByIndex(x)).ToList();

            // Update until all states will not be changed.
            int numOfLoop = 0;
            while (!AreStatesSame(prevStates, nowStates))
            {
                // Evaluate VRCAvatarParameterDriver
                for (int i = 0; i < layerIndices.Count; i++)
                {
                    var state = nowStates[i];
                    var behaviours = _animator.GetBehaviours(state.fullPathHash, layerIndices[i]);
                    foreach (var behaviour in behaviours)
                    {
                        if (behaviour is VRC_AvatarParameterDriver parameterDriver)
                        {
                            foreach (var driverParameter in parameterDriver.parameters)
                            {
                                if (driverParameter.type != VRC_AvatarParameterDriver.ChangeType.Set)
                                {
                                    throw new FacialExpressionSwitcherException("Set type can only be emulated.");
                                }
                                foreach (var animatorParameter in _animator.parameters)
                                {
                                    if (animatorParameter.name == driverParameter.name)
                                    {
                                        switch (animatorParameter.type)
                                        {
                                            case AnimatorControllerParameterType.Float:
                                                _animator.SetFloat(driverParameter.name, driverParameter.value);
                                                break;
                                            case AnimatorControllerParameterType.Int:
                                                _animator.SetInteger(driverParameter.name, (int)driverParameter.value);
                                                break;
                                            case AnimatorControllerParameterType.Bool:
                                                _animator.SetBool(driverParameter.name, driverParameter.value > 0);
                                                break;
                                            case AnimatorControllerParameterType.Trigger:
                                                _animator.SetTrigger(driverParameter.name);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Update
                _animator.Update(1 / 60f);
                prevStates = nowStates;
                nowStates = layerIndices.Select(x => StateByIndex(x)).ToList();
                numOfLoop++;
                if (numOfLoop > maxLoop)
                {
                    throw new FacialExpressionSwitcherException("Update can't be stopped.");
                }
            }
        }

        private bool AreStatesSame(IReadOnlyList<AnimatorStateInfo> prev, IReadOnlyList<AnimatorStateInfo> now)
        {
            if (prev.Count != now.Count)
            {
                throw new FacialExpressionSwitcherException("Mismatched num of states.");
            }
            for (int i = 0; i < prev.Count; i++)
            {
                if (prev[i].fullPathHash != now[i].fullPathHash)
                {
                    return false;
                }
            }
            return true;
        }
    }

    internal static class TestExtensions
    {
        public static string Name(this AnimatorClipInfo animatorClipInfo)
        {
            var clip = animatorClipInfo.clip;
            var path = AssetDatabase.GetAssetPath(clip);
            return Path.GetFileNameWithoutExtension(path);
        }
    }
}
