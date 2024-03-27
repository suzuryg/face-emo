using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    [TestFixture]
    internal class FxParameterCheckerTests
    {
        [Test]
        public void CheckPrefixNeeds()
        {
            Assert.That(FxParameterChecker.CheckPrefixNeeds(null), Is.False);
            Assert.That(FxParameterChecker.CheckPrefixNeeds(new GameObject().AddComponent<VRCAvatarDescriptor>()), Is.False);

            var handsLayerAvatar = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Henge/Himiko_2022/Himiko_2022_Test.prefab");
            Assert.That(FxParameterChecker.CheckPrefixNeeds(handsLayerAvatar.GetComponent<VRCAvatarDescriptor>()), Is.False);

            var cacAvatar = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Henge/Himiko_2022_Plus/hp_plus_test.prefab");
            Assert.That(FxParameterChecker.CheckPrefixNeeds(cacAvatar.GetComponent<VRCAvatarDescriptor>()), Is.True);
        }
    }
}

