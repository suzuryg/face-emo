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
    internal class ContactSettingImporterTests
    {
        private ContactSettingImporter _importer;
        private AV3Setting _av3Setting;

        [SetUp]
        public void Setup()
        {
            _av3Setting = ScriptableObject.CreateInstance<AV3Setting>();
            _importer = new ContactSettingImporter(_av3Setting);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Import()
        {
            var avatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Henge/Himiko_2022/Himiko_2022_Test.prefab");
            var avatarRoot = PrefabUtility.InstantiatePrefab(avatarPrefab) as GameObject;
            var avartarDescriptor = avatarRoot.GetComponent<VRCAvatarDescriptor>();

            var externalFacePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Suzuryg/ExternalContacts/ExternalContact_Face.prefab");
            var externalFace = PrefabUtility.InstantiatePrefab(externalFacePrefab) as GameObject;
            externalFace.transform.parent = avatarRoot.transform;

            var externalNotFacePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Suzuryg/ExternalContacts/ExternalContact_NotFace.prefab");
            var externalNotFace = PrefabUtility.InstantiatePrefab(externalNotFacePrefab) as GameObject;
            externalNotFace.transform.parent = avatarRoot.transform;

            var wrongTypePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Suzuryg/ExternalContacts/ExternalContact_WrongType.prefab");
            var wrongType = PrefabUtility.InstantiatePrefab(wrongTypePrefab) as GameObject;
            wrongType.transform.parent = avatarRoot.transform;

            var imported = _importer.Import(avartarDescriptor);
            CollectionAssert.AreEqual(imported, _av3Setting.ContactReceivers);

            Assert.That(_av3Setting.ContactReceivers.Count, Is.EqualTo(2));
            Assert.That(_av3Setting.ContactReceivers[0].gameObject.GetFullPath(), Is.EqualTo("/Himiko_2022_Test/Armature/Hips/Spine/Chest/Neck/Head/Contact_Proximity"));
            Assert.That(_av3Setting.ContactReceivers[1].gameObject.GetFullPath(), Is.EqualTo("/Himiko_2022_Test/ExternalContact_Face/ExternalContact_Face"));
        }
    }
}
