using NUnit.Framework;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    [TestFixture]
    public class ExpressionImporterTests
    {
        public static readonly string AssetDirPath = "Assets/Temp/ExpressionImporterTests";

        private ExpressionImporter _importer;

        private Domain.Menu _menu;
        private AV3Setting _av3Setting;

        private Locale _locale;

        [SetUp]
        public void Setup()
        {
            _menu = new Domain.Menu();
            _av3Setting = ScriptableObject.CreateInstance<AV3Setting>();

            AssetDatabase.DeleteAsset(AssetDirPath);
            _importer = new ExpressionImporter(_menu, _av3Setting, AssetDirPath);

            _locale = LocalizationSetting.GetLocale();
            new LocalizationSetting().SetLocale(Locale.en_US);
        }

        [TearDown]
        public void TearDown()
        {
            new LocalizationSetting().SetLocale(_locale);
        }

        [Test]
        public void Import_HandsLayer()
        {
            var avatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Henge/Himiko_2022/Himiko_2022_Test.prefab");
            var avatarRoot = PrefabUtility.InstantiatePrefab(avatarPrefab) as GameObject;
            var avartarDescriptor = avatarRoot.GetComponent<VRCAvatarDescriptor>();

            _importer.Import(avartarDescriptor);

            // animations
            var idle = AssertIdle(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/idol.anim"));
            var dislike = AssertDislike(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/dislike.anim"));
            var fun = AssertFun(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/fun.anim"));
            var joy = AssertJoy(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/joy.anim"));
            var sorrow = AssertSorrow(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/sorrow.anim"));
            var surprised = AssertSurprized(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/surprised.anim"));
            var wink = AssertWink(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/wink.anim"));
            var zito = AssertZito(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/zito.anim"));
            var zito2 = AssertZito(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/zito_(2).anim"));
            var close = AssertClose(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/close.anim"));
            var closeBase = AssertCloseBase(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/close_Base.anim"));

            // menu
            Assert.That(_menu.Registered.Count, Is.EqualTo(1));
            Assert.That(_menu.Unregistered.Count, Is.EqualTo(0));

            // mode
            var id = _menu.Registered.Order.First();
            var mode = _menu.Registered.GetMode(id);
            Assert.That(_menu.DefaultSelection, Is.EqualTo(id));
            Assert.That(mode.DisplayName, Is.EqualTo("Imported"));
            Assert.That(mode.ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(mode.UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(mode.EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(mode.MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode.BlinkEnabled, Is.EqualTo(true));
            Assert.That(mode.MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode.Branches.Count, Is.EqualTo(12));

            // branches
            Assert.That(mode.Branches[0].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode.Branches[0].Conditions.First(), Is.EqualTo(new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(mode.Branches[0].BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(idle))));
            Assert.That(mode.Branches[0].RightHandAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(wink))));
            Assert.That(mode.Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(mode.Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode.Branches[0].BlinkEnabled, Is.EqualTo(false));
            Assert.That(mode.Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode.Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(mode.Branches[0].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(mode.Branches[0].IsReachable, Is.EqualTo(true));
            AssertNormalBranch(mode.Branches[1], Hand.Right, HandGesture.HandOpen, idle, true, true);
            AssertNormalBranch(mode.Branches[2], Hand.Right, HandGesture.Fingerpoint, surprised, false, false);
            AssertNormalBranch(mode.Branches[3], Hand.Right, HandGesture.RockNRoll, zito2, false, false);
            AssertNormalBranch(mode.Branches[4], Hand.Right, HandGesture.HandGun, sorrow, false, false);

            Assert.That(mode.Branches[5].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode.Branches[5].Conditions.First(), Is.EqualTo(new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(mode.Branches[5].BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(closeBase))));
            Assert.That(mode.Branches[5].LeftHandAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(close))));
            Assert.That(mode.Branches[5].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(mode.Branches[5].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode.Branches[5].BlinkEnabled, Is.EqualTo(false));
            Assert.That(mode.Branches[5].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode.Branches[5].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(mode.Branches[5].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(mode.Branches[5].IsReachable, Is.EqualTo(true));
            AssertNormalBranch(mode.Branches[6], Hand.Left, HandGesture.HandOpen, fun, true, true);
            AssertNormalBranch(mode.Branches[7], Hand.Left, HandGesture.Fingerpoint, surprised, false, false);
            AssertNormalBranch(mode.Branches[8], Hand.Left, HandGesture.Victory, joy, false, false);
            AssertNormalBranch(mode.Branches[9], Hand.Left, HandGesture.RockNRoll, zito, false, false);
            AssertNormalBranch(mode.Branches[10], Hand.Left, HandGesture.HandGun, dislike, false, false);
            AssertNormalBranch(mode.Branches[11], Hand.Left, HandGesture.ThumbsUp, wink, false, false);

            GameObject.DestroyImmediate(avatarRoot);
        }

        [Test]
        public void Import_MimyLabBasic()
        {
            var avatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Henge/Himiko_2022/Himiko_2022_Test.prefab");
            var avatarRoot = PrefabUtility.InstantiatePrefab(avatarPrefab) as GameObject;
            var avartarDescriptor = avatarRoot.GetComponent<VRCAvatarDescriptor>();
            var fx = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/TestAssets/MimyLab/Common/AV3Templates/FX/ExpressionImporterTests/Custom_FX_Basic.controller");

            for (int i = 0; i < avartarDescriptor.baseAnimationLayers.Length; i++)
            {
                if (avartarDescriptor.baseAnimationLayers[i].type == VRCAvatarDescriptor.AnimLayerType.FX)
                {
                    avartarDescriptor.baseAnimationLayers[i].animatorController = fx;
                    break;
                }
            }

            _importer.Import(avartarDescriptor);

            // animations
            var angry = AssertAngry(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/angry.anim"));
            var dislike = AssertDislike(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/dislike.anim"));
            var fun = AssertFun(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/fun.anim"));
            var joy = AssertJoy(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/joy.anim"));
            var sorrow = AssertSorrow(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/sorrow.anim"));
            var surprised = AssertSurprized(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/surprised.anim"));
            var wink = AssertWink(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/wink.anim"));
            var zito = AssertZito(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/zito.anim"));
            var close = AssertClose(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/close.anim"));
            var closeBase = AssertCloseBase(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/close_Base.anim"));

            // menu
            Assert.That(_menu.Registered.Count, Is.EqualTo(1));
            Assert.That(_menu.Unregistered.Count, Is.EqualTo(0));

            // mode
            var id = _menu.Registered.Order.First();
            var mode = _menu.Registered.GetMode(id);
            Assert.That(_menu.DefaultSelection, Is.EqualTo(id));
            Assert.That(mode.DisplayName, Is.EqualTo("Imported"));
            Assert.That(mode.ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(mode.UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(mode.EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(mode.MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode.BlinkEnabled, Is.EqualTo(true));
            Assert.That(mode.MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode.Branches.Count, Is.EqualTo(14));

            // branches
            Assert.That(mode.Branches[0].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode.Branches[0].Conditions.First(), Is.EqualTo(new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(mode.Branches[0].BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(closeBase))));
            Assert.That(mode.Branches[0].RightHandAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(close))));
            Assert.That(mode.Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(mode.Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode.Branches[0].BlinkEnabled, Is.EqualTo(false));
            Assert.That(mode.Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode.Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(mode.Branches[0].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(mode.Branches[0].IsReachable, Is.EqualTo(true));
            AssertNormalBranch(mode.Branches[1], Hand.Right, HandGesture.HandOpen, zito, false, true);
            AssertNormalBranch(mode.Branches[2], Hand.Right, HandGesture.Fingerpoint, wink, false, true);
            AssertNormalBranch(mode.Branches[3], Hand.Right, HandGesture.Victory, surprised, false, true);
            AssertNormalBranch(mode.Branches[4], Hand.Right, HandGesture.RockNRoll, sorrow, false, true);
            AssertNormalBranch(mode.Branches[5], Hand.Right, HandGesture.HandGun, joy, false, true);
            AssertNormalBranch(mode.Branches[6], Hand.Right, HandGesture.ThumbsUp, fun, false, true);

            Assert.That(mode.Branches[7].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode.Branches[7].Conditions.First(), Is.EqualTo(new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(mode.Branches[7].BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(closeBase))));
            Assert.That(mode.Branches[7].LeftHandAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(close))));
            Assert.That(mode.Branches[7].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(mode.Branches[7].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode.Branches[7].BlinkEnabled, Is.EqualTo(false));
            Assert.That(mode.Branches[7].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode.Branches[7].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(mode.Branches[7].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(mode.Branches[7].IsReachable, Is.EqualTo(true));
            AssertNormalBranch(mode.Branches[8], Hand.Left, HandGesture.HandOpen, angry, false, true);
            AssertNormalBranch(mode.Branches[9], Hand.Left, HandGesture.Fingerpoint, dislike, false, true);
            AssertNormalBranch(mode.Branches[10], Hand.Left, HandGesture.Victory, fun, false, true);
            AssertNormalBranch(mode.Branches[11], Hand.Left, HandGesture.RockNRoll, joy, false, true);
            AssertNormalBranch(mode.Branches[12], Hand.Left, HandGesture.HandGun, sorrow, false, true);
            AssertNormalBranch(mode.Branches[13], Hand.Left, HandGesture.ThumbsUp, surprised, false, true);

            GameObject.DestroyImmediate(avatarRoot);
        }

        [Test]
        public void Import_CAC()
        {
            var avatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Henge/Himiko_2022_Plus/hp_plus_test.prefab");
            var avatarRoot = PrefabUtility.InstantiatePrefab(avatarPrefab) as GameObject;
            var avartarDescriptor = avatarRoot.GetComponent<VRCAvatarDescriptor>();

            _importer.Import(avartarDescriptor);

            // animations
            var angry = AssertAngry(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/angry.anim"));
            var dislike = AssertDislike(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/dislike.anim"));
            var fun = AssertFun(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/fun.anim"));
            var joy = AssertJoy(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/joy.anim"));
            var sorrow = AssertSorrow(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/sorrow.anim"));
            var surprised = AssertSurprized(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/surprised.anim"));
            var wink = AssertWink(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/wink.anim"));
            var close = AssertClose(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/close.anim"));
            var closeBase = AssertCloseBase(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDirPath + "/close_Base.anim"));

            // menu
            Assert.That(_menu.Registered.Count, Is.EqualTo(3));
            Assert.That(_menu.Unregistered.Count, Is.EqualTo(0));

            // mode
            var id1 = _menu.Registered.Order[0];
            var id2 = _menu.Registered.Order[1];
            var id3 = _menu.Registered.Order[2];
            Assert.That(_menu.DefaultSelection, Is.EqualTo(id1));

            var mode1 = _menu.Registered.GetMode(id1);
            Assert.That(mode1.DisplayName, Is.EqualTo("Imported_1"));
            Assert.That(mode1.ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(mode1.UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(mode1.EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(mode1.MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode1.BlinkEnabled, Is.EqualTo(true));
            Assert.That(mode1.MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode1.Branches.Count, Is.EqualTo(14));

            var mode2 = _menu.Registered.GetMode(id2);
            Assert.That(mode2.DisplayName, Is.EqualTo("Imported_2"));
            Assert.That(mode2.ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(mode2.UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(mode2.EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(mode2.MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode2.BlinkEnabled, Is.EqualTo(true));
            Assert.That(mode2.MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode2.Branches.Count, Is.EqualTo(4));

            var mode3 = _menu.Registered.GetMode(id3);
            Assert.That(mode3.DisplayName, Is.EqualTo("Imported_3"));
            Assert.That(mode3.ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(mode3.UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(mode3.EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(mode3.MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode3.BlinkEnabled, Is.EqualTo(true));
            Assert.That(mode3.MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode3.Branches.Count, Is.EqualTo(4));

            // branches (1)
            Assert.That(mode1.Branches[0].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode1.Branches[0].Conditions.First(), Is.EqualTo(new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(mode1.Branches[0].BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(closeBase))));
            Assert.That(mode1.Branches[0].RightHandAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(close))));
            Assert.That(mode1.Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(mode1.Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode1.Branches[0].BlinkEnabled, Is.EqualTo(false));
            Assert.That(mode1.Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode1.Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(mode1.Branches[0].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(mode1.Branches[0].IsReachable, Is.EqualTo(true));
            AssertCacBranch(mode1.Branches[1], Hand.Right, HandGesture.HandOpen, angry, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode1.Branches[2], Hand.Right, HandGesture.Fingerpoint, dislike, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode1.Branches[3], Hand.Right, HandGesture.Victory, fun, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode1.Branches[4], Hand.Right, HandGesture.RockNRoll, joy, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode1.Branches[5], Hand.Right, HandGesture.HandGun, sorrow, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode1.Branches[6], Hand.Right, HandGesture.ThumbsUp, wink, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);

            Assert.That(mode1.Branches[7].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode1.Branches[7].Conditions.First(), Is.EqualTo(new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(mode1.Branches[7].BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(closeBase))));
            Assert.That(mode1.Branches[7].LeftHandAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(close))));
            Assert.That(mode1.Branches[7].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(mode1.Branches[7].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(mode1.Branches[7].BlinkEnabled, Is.EqualTo(false));
            Assert.That(mode1.Branches[7].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(mode1.Branches[7].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(mode1.Branches[7].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(mode1.Branches[7].IsReachable, Is.EqualTo(true));
            AssertCacBranch(mode1.Branches[8], Hand.Left, HandGesture.HandOpen, wink, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode1.Branches[9], Hand.Left, HandGesture.Fingerpoint, surprised, EyeTrackingControl.Tracking, MouthTrackingControl.Animation, true, false);
            AssertCacBranch(mode1.Branches[10], Hand.Left, HandGesture.Victory, sorrow, EyeTrackingControl.Tracking, MouthTrackingControl.Animation, false, true);
            AssertCacBranch(mode1.Branches[11], Hand.Left, HandGesture.RockNRoll, fun, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode1.Branches[12], Hand.Left, HandGesture.HandGun, dislike, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode1.Branches[13], Hand.Left, HandGesture.ThumbsUp, joy, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);

            // branches (2)
            AssertCacBranch(mode2.Branches[0], Hand.Right, HandGesture.HandOpen, fun, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode2.Branches[1], Hand.Right, HandGesture.HandGun, wink, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);

            Assert.That(mode2.Branches[2].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode2.Branches[2].Conditions.First(), Is.EqualTo(new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(mode2.Branches[2].BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(closeBase))));
            Assert.That(mode2.Branches[2].LeftHandAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(close))));
            Assert.That(mode2.Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(mode2.Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode2.Branches[2].BlinkEnabled, Is.EqualTo(false));
            Assert.That(mode2.Branches[2].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode2.Branches[2].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(mode2.Branches[2].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(mode2.Branches[2].IsReachable, Is.EqualTo(true));
            AssertCacBranch(mode2.Branches[3], Hand.Left, HandGesture.Victory, joy, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);

            // branches (3)
            Assert.That(mode3.Branches[0].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode3.Branches[0].Conditions.First(), Is.EqualTo(new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(mode3.Branches[0].BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(closeBase))));
            Assert.That(mode3.Branches[0].RightHandAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(close))));
            Assert.That(mode3.Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(mode3.Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(mode3.Branches[0].BlinkEnabled, Is.EqualTo(false));
            Assert.That(mode3.Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(mode3.Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(mode3.Branches[0].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(mode3.Branches[0].IsReachable, Is.EqualTo(true));
            AssertCacBranch(mode3.Branches[1], Hand.Right, HandGesture.RockNRoll, sorrow, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);

            AssertCacBranch(mode3.Branches[2], Hand.Left, HandGesture.HandOpen, surprised, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);
            AssertCacBranch(mode3.Branches[3], Hand.Left, HandGesture.RockNRoll, angry, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, false, true);

            GameObject.DestroyImmediate(avatarRoot);
        }

        private static AnimationClip AssertIdle(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(8));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static AnimationClip AssertJoy(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(8));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(100));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static AnimationClip AssertAngry(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(8));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(100));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static AnimationClip AssertSorrow(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(8));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(100));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static AnimationClip AssertFun(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(8));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(100));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static AnimationClip AssertZito(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(8));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(100));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static AnimationClip AssertWink(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(8));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(100));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static AnimationClip AssertSurprized(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(8));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(100));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static AnimationClip AssertDislike(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(8));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(100));

            return clip;
        }

        private static AnimationClip AssertCloseBase(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(9));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_mabataki")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static AnimationClip AssertClose(AnimationClip clip)
        {
            Assert.That(clip.isLooping, Is.EqualTo(false));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Length, Is.EqualTo(9));

            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_mabataki")), Is.EqualTo(100));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_joy")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_angry")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_sorrow")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_fun")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_zito")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_wink")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_surprised")), Is.EqualTo(0));
            Assert.That(AV3TestUtility.GetBlendShapeValue(clip, new BlendShape("body_face", "face_dislike")), Is.EqualTo(0));

            return clip;
        }

        private static void AssertNormalBranch(IBranch branch, Hand hand, HandGesture handGesture, AnimationClip animation, bool eyeTracking, bool mouthTracking)
        {
            Assert.That(branch.Conditions.Count, Is.EqualTo(1));
            Assert.That(branch.Conditions.First(), Is.EqualTo(new Condition(hand, handGesture, ComparisonOperator.Equals)));
            Assert.That(branch.BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animation))));
            Assert.That(branch.EyeTrackingControl, Is.EqualTo(eyeTracking ? EyeTrackingControl.Tracking : EyeTrackingControl.Animation));
            Assert.That(branch.MouthTrackingControl, Is.EqualTo(mouthTracking ? MouthTrackingControl.Tracking : MouthTrackingControl.Animation));
            Assert.That(branch.BlinkEnabled, Is.EqualTo(eyeTracking));
            Assert.That(branch.MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(branch.IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(branch.IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(branch.IsReachable, Is.EqualTo(true));
        }

        private static void AssertCacBranch(IBranch branch, Hand hand, HandGesture handGesture, AnimationClip animation,
            EyeTrackingControl eyeTracking, MouthTrackingControl mouthTracking, bool blinkEnabled, bool mouthMorphCancelerEnabled)
        {
            Assert.That(branch.Conditions.Count, Is.EqualTo(1));
            Assert.That(branch.Conditions.First(), Is.EqualTo(new Condition(hand, handGesture, ComparisonOperator.Equals)));
            Assert.That(branch.BaseAnimation.GUID, Is.EqualTo(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animation))));
            Assert.That(branch.EyeTrackingControl, Is.EqualTo(eyeTracking));
            Assert.That(branch.MouthTrackingControl, Is.EqualTo(mouthTracking));
            Assert.That(branch.BlinkEnabled, Is.EqualTo(blinkEnabled));
            Assert.That(branch.MouthMorphCancelerEnabled, Is.EqualTo(mouthMorphCancelerEnabled));
            Assert.That(branch.IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(branch.IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(branch.IsReachable, Is.EqualTo(true));
        }
    }
}
