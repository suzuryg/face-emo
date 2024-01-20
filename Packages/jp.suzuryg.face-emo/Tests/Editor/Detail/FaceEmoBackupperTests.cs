using NUnit.Framework;
using Suzuryg.FaceEmo.Components;
using Suzuryg.FaceEmo.Components.Data;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Components.States;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Data;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Detail.View;
using Suzuryg.FaceEmo.UseCase;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;

namespace Suzuryg.FaceEmo.Detail
{
    [TestFixture]
    public class FaceEmoBackupperTests
    {
        private static readonly string TempDirPath = "Assets/Temp/FaceEmoBackupperTests";
        private static readonly string BackupPath = TempDirPath + "/BackupTest.asset";

        private GameObject _avatarRoot;
        private AV3Setting _av3Setting;
        private ExpressionEditorSetting _expressionEditorSetting;
        private ThumbnailSetting _thumbnailSetting;
        private FaceEmoBackupper _backupper;

        [SetUp]
        public void Setup()
        {
            var launcher = new GameObject();
            var menuRepositoryComponent = launcher.AddComponent<MenuRepositoryComponent>();
            var menuRepository = new MenuRepository(menuRepositoryComponent);
            menuRepository.Save(string.Empty, new Domain.Menu(), string.Empty);

            var updateMenuSubject = new UpdateMenuSubject();
            var viewSelection = ScriptableObject.CreateInstance<ViewSelection>();
            var selectionSynchronizer = new SelectionSynchronizer(menuRepository, updateMenuSubject, viewSelection);

            var restorationCheckpoint = launcher.AddComponent<RestorationCheckpoint>();

            _av3Setting = ScriptableObject.CreateInstance<AV3Setting>();
            _expressionEditorSetting = ScriptableObject.CreateInstance<ExpressionEditorSetting>();
            _thumbnailSetting = ScriptableObject.CreateInstance<ThumbnailSetting>();
            var localizationSetting = new LocalizationSetting();

            _backupper = new FaceEmoBackupper(menuRepository, menuRepository, selectionSynchronizer, _av3Setting, _expressionEditorSetting, _thumbnailSetting, restorationCheckpoint, localizationSetting);

            if (!AssetDatabase.IsValidFolder(TempDirPath))
            {
                AV3Utility.CreateFolderRecursively(TempDirPath);
            }

            var avatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestAssets/Henge/Himiko_2022/Himiko_2022_Test.prefab");
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
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(BackupPath);
        }

        [Test]
        public void Export_Import()
        {
            _backupper.Export(BackupPath);
            _backupper.Import(BackupPath);

            Assert.That(_av3Setting.TargetAvatarPath, Is.EqualTo("/Himiko_2022_Test"));
            Assert.That(_av3Setting.TargetAvatar.gameObject.name, Is.EqualTo(_avatarRoot.name));

            Assert.That(_av3Setting.ContactReceiverPaths.Count, Is.EqualTo(5));
            Assert.That(_av3Setting.ContactReceiverPaths[0], Is.EqualTo("/Himiko_2022_Test/Armature/Hips/Spine/Chest/Neck/Head/Contact_Constant"));
            Assert.That(_av3Setting.ContactReceiverPaths[1], Is.EqualTo("/Himiko_2022_Test/Armature/Hips/Spine/Chest/Neck/Head/Contact_OnEnter"));
            Assert.That(_av3Setting.ContactReceiverPaths[2], Is.EqualTo("/Himiko_2022_Test/Armature/Hips/Spine/Chest/Neck/Head/Contact_Proximity"));
            Assert.That(_av3Setting.ContactReceiverPaths[3], Is.EqualTo("/Himiko_2022_Test/Armature/Hips/Spine/Chest/Neck/Head/Contact_Double"));
            Assert.That(_av3Setting.ContactReceiverPaths[4], Is.EqualTo("/Himiko_2022_Test/Armature/Hips/Spine/Chest/Neck/Head/Contact_Double"));
            Assert.That(_av3Setting.ContactReceiverParameterNames.Count, Is.EqualTo(5));
            Assert.That(_av3Setting.ContactReceiverParameterNames[0], Is.EqualTo("Contact_Constant"));
            Assert.That(_av3Setting.ContactReceiverParameterNames[1], Is.EqualTo("Contact_OnEnter"));
            Assert.That(_av3Setting.ContactReceiverParameterNames[2], Is.EqualTo("Contact_Proximity"));
            Assert.That(_av3Setting.ContactReceiverParameterNames[3], Is.EqualTo("Contact_Double_Constant"));
            Assert.That(_av3Setting.ContactReceiverParameterNames[4], Is.EqualTo("Contact_Double_Proximity"));
            Assert.That(_av3Setting.ContactReceivers.Count, Is.EqualTo(5));
            Assert.That((_av3Setting.ContactReceivers[0] as ContactReceiver).parameter, Is.EqualTo("Contact_Constant"));
            Assert.That((_av3Setting.ContactReceivers[1] as ContactReceiver).parameter, Is.EqualTo("Contact_OnEnter"));
            Assert.That((_av3Setting.ContactReceivers[2] as ContactReceiver).parameter, Is.EqualTo("Contact_Proximity"));
            Assert.That((_av3Setting.ContactReceivers[3] as ContactReceiver).parameter, Is.EqualTo("Contact_Double_Constant"));
            Assert.That((_av3Setting.ContactReceivers[4] as ContactReceiver).parameter, Is.EqualTo("Contact_Double_Proximity"));

            // TODO: Check other properties
        }
    }
}
