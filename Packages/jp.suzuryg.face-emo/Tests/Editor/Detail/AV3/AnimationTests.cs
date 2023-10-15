using NUnit.Framework;
using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    [TestFixture]
    internal class AnimationTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }


        [Test]
        public void CombineExpressions()
        {
            // load
            var mouth1 = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Suzuryg/TestAnimations/mouth1.anim");
            var mouth2 = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Suzuryg/TestAnimations/mouth2.anim");
            var cube1 = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Suzuryg/TestAnimations/cube1.anim");
            var cube2 = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Suzuryg/TestAnimations/cube2.anim");
            var ear1 = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Suzuryg/TestAnimations/ear1.anim");
            var ear2 = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Suzuryg/TestAnimations/ear2.anim");

            Assert.That(mouth1.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(mouth1).Length, Is.EqualTo(1));
            Assert.That(AV3TestUtility.GetBlendShapeValue(mouth1, new BlendShape("body_face", "m_aa")), Is.EqualTo(100));

            Assert.That(mouth2.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(mouth2).Length, Is.EqualTo(1));
            Assert.That(AV3TestUtility.GetBlendShapeValue(mouth2, new BlendShape("body_face", "m_aa")), Is.EqualTo(50));

            Assert.That(cube1.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(cube1).Length, Is.EqualTo(1));
            Assert.That(AV3TestUtility.GetToggleValue(cube1, "Cube"), Is.EqualTo(true));

            Assert.That(cube2.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(cube2).Length, Is.EqualTo(1));
            Assert.That(AV3TestUtility.GetToggleValue(cube2, "Cube"), Is.EqualTo(false));

            Assert.That(ear1.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(ear1).Length, Is.EqualTo(18));
            Assert.That(AV3TestUtility.GetPositionX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.3).Within(0.1));
            Assert.That(AV3TestUtility.GetPositionX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.3).Within(0.1));

            Assert.That(ear2.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(ear2).Length, Is.EqualTo(18));
            Assert.That(AV3TestUtility.GetPositionX(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(30).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(10).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.3).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetPositionX(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-30).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-10).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.3).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear2, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.1).Within(0.1));

            // combine
            var mouth12 = new AnimationClip();
            AV3Utility.CombineExpressions(mouth1, mouth2, mouth12);
            Assert.That(mouth12.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(mouth12).Length, Is.EqualTo(1));
            Assert.That(AV3TestUtility.GetBlendShapeValue(mouth12, new BlendShape("body_face", "m_aa")), Is.EqualTo(100));

            var cube12 = new AnimationClip();
            AV3Utility.CombineExpressions(cube1, cube2, cube12);
            Assert.That(cube12.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(cube12).Length, Is.EqualTo(1));
            Assert.That(AV3TestUtility.GetToggleValue(cube12, "Cube"), Is.EqualTo(true));

            var ear12 = new AnimationClip();
            AV3Utility.CombineExpressions(ear1, ear2, ear12);
            Assert.That(ear12.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(ear12).Length, Is.EqualTo(18));
            Assert.That(AV3TestUtility.GetPositionX(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(30).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(10).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.3).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetPositionX(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-30).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-10).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.3).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear12, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.1).Within(0.1));

            var ear21 = new AnimationClip();
            AV3Utility.CombineExpressions(ear2, ear1, ear21);
            Assert.That(ear21.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(ear21).Length, Is.EqualTo(18));
            Assert.That(AV3TestUtility.GetPositionX(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.3).Within(0.1));
            Assert.That(AV3TestUtility.GetPositionX(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear21, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.3).Within(0.1));

            var mouth_cube = new AnimationClip();
            AV3Utility.CombineExpressions(mouth1, cube1, mouth_cube);
            Assert.That(mouth_cube.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(mouth_cube).Length, Is.EqualTo(2));
            Assert.That(AV3TestUtility.GetBlendShapeValue(mouth_cube, new BlendShape("body_face", "m_aa")), Is.EqualTo(100));
            Assert.That(AV3TestUtility.GetToggleValue(mouth_cube, "Cube"), Is.EqualTo(true));

            var cube_ear = new AnimationClip();
            AV3Utility.CombineExpressions(cube1, ear1, cube_ear);
            Assert.That(cube_ear.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(cube_ear).Length, Is.EqualTo(19));
            Assert.That(AV3TestUtility.GetToggleValue(cube_ear, "Cube"), Is.EqualTo(true));
            Assert.That(AV3TestUtility.GetPositionX(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.3).Within(0.1));
            Assert.That(AV3TestUtility.GetPositionX(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(cube_ear, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.3).Within(0.1));

            var ear_mouth = new AnimationClip();
            AV3Utility.CombineExpressions(ear1, mouth1, ear_mouth);
            Assert.That(ear_mouth.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(ear_mouth).Length, Is.EqualTo(19));
            Assert.That(AV3TestUtility.GetPositionX(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.3).Within(0.1));
            Assert.That(AV3TestUtility.GetPositionX(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear_mouth, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.3).Within(0.1));
            Assert.That(ear_mouth.isLooping, Is.EqualTo(false));

            // original clips are not changed
            Assert.That(mouth1.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(mouth1).Length, Is.EqualTo(1));
            Assert.That(AV3TestUtility.GetBlendShapeValue(mouth1, new BlendShape("body_face", "m_aa")), Is.EqualTo(100));

            Assert.That(cube1.isLooping, Is.EqualTo(false));
            Assert.That(AnimationUtility.GetCurveBindings(cube1).Length, Is.EqualTo(1));
            Assert.That(AV3TestUtility.GetToggleValue(cube1, "Cube"), Is.EqualTo(true));

            Assert.That(ear1.isLooping, Is.EqualTo(false));
            Assert.That(AV3TestUtility.GetPositionX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(-0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_L.001"), Is.EqualTo(1.3).Within(0.1));
            Assert.That(AV3TestUtility.GetPositionX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.1).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.2).Within(0.01));
            Assert.That(AV3TestUtility.GetPositionZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(0.3).Within(0.01));
            Assert.That(AV3TestUtility.GetRotationX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-10).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-20).Within(0.1));
            Assert.That(AV3TestUtility.GetRotationZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-30).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleX(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.1).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleY(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.2).Within(0.1));
            Assert.That(AV3TestUtility.GetScaleZ(ear1, "Armature/Hips/Spine/Chest/Neck/Head/Hair.000/Ear_R.001"), Is.EqualTo(-1.3).Within(0.1));
        }
    }
}
