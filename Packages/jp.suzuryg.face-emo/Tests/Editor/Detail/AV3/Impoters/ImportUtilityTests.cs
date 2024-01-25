using NUnit.Framework;
using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    [TestFixture]
    public class ImportUtilityTests
    {
        [Test]
        public void AreAllValuesZero()
        {
            var blendShapes = new[]
            {
                new BlendShape("body_face", "e_blink"),
                new BlendShape("body_face", "face_mabataki"),
                new BlendShape("body_face", "face_joy"),
                new BlendShape("body_face", "face_angry"),
                new BlendShape("body_face", "face_sorrow"),
                new BlendShape("body_face", "face_fun"),
                new BlendShape("body_face", "face_zito"),
                new BlendShape("body_face", "face_satori"),
                new BlendShape("body_face", "face_wink"),
                new BlendShape("body_face", "face_surprised"),
                new BlendShape("body_face", "face_dislike"),
            };

            Assert.That(ImportUtility.AreAllValuesZero(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Henge/Himiko_2022/animm/angry.anim"), blendShapes), Is.False);
            Assert.That(ImportUtility.AreAllValuesZero(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Henge/Himiko_2022/animm/close.anim"), blendShapes), Is.False);
            Assert.That(ImportUtility.AreAllValuesZero(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Henge/Himiko_2022/animm/blink_loop.anim"), blendShapes), Is.False);

            Assert.That(ImportUtility.AreAllValuesZero(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Henge/Himiko_2022/animm/idol.anim"), blendShapes), Is.True);
            Assert.That(ImportUtility.AreAllValuesZero(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Henge/Himiko_2022/animm/empty.anim"), blendShapes), Is.True);
            Assert.That(ImportUtility.AreAllValuesZero(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/TestAssets/Henge/Himiko_2022/animm/face_cancel.anim"), blendShapes), Is.True);
        }
    }
}
