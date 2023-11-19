using NUnit.Framework;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    [TestFixture]
    public class EditPrefabTests
    {
        private static readonly string TestAvatarPath = "Assets/TestAssets/Henge/Himiko_2022/Himiko_2022_Test.prefab";
        private static readonly string PrefabTestDirPath = "Assets/TestAssets/Henge/Himiko_2022/prefab_tests";

        private AV3Setting _av3Setting;
        private FxGenerator _fxGenerator;
        private Domain.Menu _menu;

        [SetUp]
        public void Setup()
        {
            _av3Setting = ScriptableObject.CreateInstance<AV3Setting>();

            var localizationSetting = new LocalizationSetting();
            var modeNameProvider = new ModeNameProvider(localizationSetting);
            var thumbnailSetting = ScriptableObject.CreateInstance<ThumbnailSetting>();
            var exMenuThumbnailDrawer = new ExMenuThumbnailDrawer(_av3Setting, thumbnailSetting);
            _fxGenerator = new FxGenerator(localizationSetting, modeNameProvider, exMenuThumbnailDrawer, _av3Setting);

            _menu = new Domain.Menu();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void DoNotEditPrefabs()
        {
            var main = InstantiatePrefabAtPath(TestAvatarPath);
            var sub0 = InstantiatePrefabAtPath(TestAvatarPath);
            var sub1 = InstantiatePrefabAtPath(TestAvatarPath);
            var indicator = InstantiatePrefabAtPath(AV3Constants.Path_IndicatorSound);
            var emoteLocker = InstantiatePrefabAtPath(AV3Constants.Path_EmoteLocker);
            var emoteOverride = InstantiatePrefabAtPath(AV3Constants.Path_EmoteOverridePrefab);

            _av3Setting.TargetAvatar = main.GetComponent<VRCAvatarDescriptor>();
            _av3Setting.SubTargetAvatars.Add(sub0.GetComponent<VRCAvatarDescriptor>());
            _av3Setting.SubTargetAvatars.Add(sub1.GetComponent<VRCAvatarDescriptor>());

            Assert.That(main.transform.Find(AV3Constants.MARootObjectName), Is.Null);
            Assert.That(sub0.transform.Find(AV3Constants.MARootObjectName), Is.Null);
            Assert.That(sub1.transform.Find(AV3Constants.MARootObjectName), Is.Null);

            var emptyRoot = new GameObject(AV3Constants.MARootObjectName);
            emptyRoot.transform.parent = main.transform;
            emoteOverride.transform.parent = emptyRoot.transform;

            var toBeDeleted = new GameObject("toBeDeleted");
            toBeDeleted.transform.parent = sub0.transform;

            _fxGenerator.Generate(_menu, null);

            var mainRoot = main.transform.Find(AV3Constants.MARootObjectName).gameObject;
            var sub0Root = sub0.transform.Find(AV3Constants.MARootObjectName).gameObject;
            var sub1Root = sub1.transform.Find(AV3Constants.MARootObjectName).gameObject;

            Assert.That(mainRoot.transform.childCount, Is.EqualTo(3));
            Assert.That(sub0Root.transform.childCount, Is.EqualTo(3));
            Assert.That(sub1Root.transform.childCount, Is.EqualTo(3));

            Assert.That(HasPrefabInstance(mainRoot, indicator), Is.True);
            Assert.That(HasPrefabInstance(sub0Root, indicator), Is.True);
            Assert.That(HasPrefabInstance(sub1Root, indicator), Is.True);

            Assert.That(HasPrefabInstance(mainRoot, emoteLocker), Is.True);
            Assert.That(HasPrefabInstance(sub0Root, emoteLocker), Is.True);
            Assert.That(HasPrefabInstance(sub1Root, emoteLocker), Is.True);

            Assert.That(HasPrefabInstance(mainRoot, emoteOverride), Is.True);
            Assert.That(HasPrefabInstance(sub0Root, emoteOverride), Is.True);
            Assert.That(HasPrefabInstance(sub1Root, emoteOverride), Is.True);

            GameObject.DestroyImmediate(main);
            GameObject.DestroyImmediate(sub0);
            GameObject.DestroyImmediate(sub1);
            GameObject.DestroyImmediate(indicator);
            GameObject.DestroyImmediate(emoteLocker);
            GameObject.DestroyImmediate(emoteOverride);
            GameObject.DestroyImmediate(emptyRoot);
            GameObject.DestroyImmediate(toBeDeleted);
        }

        [Test]
        public void EdirPrefabs()
        {
            var originalPrefab = InstantiatePrefabAtPath(PrefabTestDirPath + "/original_prefab.prefab");
            var originalPrefabVariant = InstantiatePrefabAtPath(PrefabTestDirPath + "/original_prefab_variant.prefab");
            var prefabVariant = InstantiatePrefabAtPath(PrefabTestDirPath + "/prefab_variant.prefab");
            var prefabVariantVariant = InstantiatePrefabAtPath(PrefabTestDirPath + "/prefab_variant_variant.prefab");
            var indicator = InstantiatePrefabAtPath(AV3Constants.Path_IndicatorSound);
            var emoteLocker = InstantiatePrefabAtPath(AV3Constants.Path_EmoteLocker);

            _av3Setting.TargetAvatar = originalPrefab.GetComponent<VRCAvatarDescriptor>();
            _av3Setting.SubTargetAvatars = new List<MonoBehaviour>()
            {
                originalPrefabVariant.GetComponent<VRCAvatarDescriptor>(),
                prefabVariant.GetComponent<VRCAvatarDescriptor>(),
                prefabVariantVariant.GetComponent<VRCAvatarDescriptor>(),
            };

            var editablePrefabs = new HashSet<string>()
            {
                PrefabTestDirPath + "/original_prefab.prefab",
                PrefabTestDirPath + "/prefab_variant.prefab",
            };

            _fxGenerator.Generate(_menu, editablePrefabs);

            var rootObjects = new List<GameObject>()
            {
                originalPrefab.transform.Find(AV3Constants.MARootObjectName).gameObject,
                originalPrefabVariant.transform.Find(AV3Constants.MARootObjectName).gameObject,
                prefabVariant.transform.Find(AV3Constants.MARootObjectName).gameObject,
                prefabVariantVariant.transform.Find(AV3Constants.MARootObjectName).gameObject,
            };

            foreach (var rootObject in rootObjects)
            {
                Assert.That(rootObject.transform.childCount, Is.EqualTo(2));
                Assert.That(HasPrefabInstance(rootObject, indicator), Is.True);
                Assert.That(HasPrefabInstance(rootObject, emoteLocker), Is.True);
            }

            GameObject.DestroyImmediate(originalPrefab);
            GameObject.DestroyImmediate(originalPrefabVariant);
            GameObject.DestroyImmediate(prefabVariant);
            GameObject.DestroyImmediate(prefabVariantVariant);
            GameObject.DestroyImmediate(indicator);
            GameObject.DestroyImmediate(emoteLocker);
        }

        [Test]
        public void GetParentPrefabPaths()
        {
            var notPrefab = InstantiatePrefabAtPath(TestAvatarPath);
            var originalPrefab = InstantiatePrefabAtPath(PrefabTestDirPath + "/original_prefab.prefab");
            var originalPrefabVariant = InstantiatePrefabAtPath(PrefabTestDirPath + "/original_prefab_variant.prefab");
            var prefabVariant = InstantiatePrefabAtPath(PrefabTestDirPath + "/prefab_variant.prefab");
            var prefabVariantVariant = InstantiatePrefabAtPath(PrefabTestDirPath + "/prefab_variant_variant.prefab");

            var unpackedOriginalPrefab = InstantiatePrefabAtPath(PrefabTestDirPath + "/original_prefab.prefab");
            var unpackedOriginalPrefabVariant = InstantiatePrefabAtPath(PrefabTestDirPath + "/original_prefab_variant.prefab");
            var unpackedPrefabVariant = InstantiatePrefabAtPath(PrefabTestDirPath + "/prefab_variant.prefab");
            var unpackedPrefabVariantVariant = InstantiatePrefabAtPath(PrefabTestDirPath + "/prefab_variant_variant.prefab");
            PrefabUtility.UnpackPrefabInstance(unpackedOriginalPrefab, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            PrefabUtility.UnpackPrefabInstance(unpackedOriginalPrefabVariant, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            PrefabUtility.UnpackPrefabInstance(unpackedPrefabVariant, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            PrefabUtility.UnpackPrefabInstance(unpackedPrefabVariantVariant, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

            _av3Setting.TargetAvatar = notPrefab.GetComponent<VRCAvatarDescriptor>();
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>() { });

            _av3Setting.TargetAvatar = originalPrefab.GetComponent<VRCAvatarDescriptor>();
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>() { PrefabTestDirPath + "/original_prefab.prefab" });

            _av3Setting.TargetAvatar = originalPrefabVariant.GetComponent<VRCAvatarDescriptor>();
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>() { PrefabTestDirPath + "/original_prefab.prefab" });

            _av3Setting.TargetAvatar = prefabVariant.GetComponent<VRCAvatarDescriptor>();
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>() { PrefabTestDirPath + "/prefab_variant.prefab" });

            _av3Setting.TargetAvatar = prefabVariantVariant.GetComponent<VRCAvatarDescriptor>();
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>() { PrefabTestDirPath + "/prefab_variant.prefab" });

            _av3Setting.TargetAvatar = unpackedOriginalPrefab.GetComponent<VRCAvatarDescriptor>();
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>() { });

            _av3Setting.TargetAvatar = unpackedOriginalPrefabVariant.GetComponent<VRCAvatarDescriptor>();
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>() { PrefabTestDirPath + "/original_prefab.prefab" });

            _av3Setting.TargetAvatar = unpackedPrefabVariant.GetComponent<VRCAvatarDescriptor>();
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>() { });

            _av3Setting.TargetAvatar = unpackedPrefabVariantVariant.GetComponent<VRCAvatarDescriptor>();
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>() { PrefabTestDirPath + "/prefab_variant.prefab" });

            _av3Setting.TargetAvatar = notPrefab.GetComponent<VRCAvatarDescriptor>();
            _av3Setting.SubTargetAvatars = new List<MonoBehaviour>()
            {
                originalPrefab.GetComponent<VRCAvatarDescriptor>(),
                originalPrefabVariant.GetComponent<VRCAvatarDescriptor>(),
                prefabVariant.GetComponent<VRCAvatarDescriptor>(),
                prefabVariantVariant.GetComponent<VRCAvatarDescriptor>(),
                unpackedOriginalPrefab.GetComponent<VRCAvatarDescriptor>(),
                unpackedOriginalPrefabVariant.GetComponent<VRCAvatarDescriptor>(),
                unpackedPrefabVariant.GetComponent<VRCAvatarDescriptor>(),
                unpackedPrefabVariantVariant.GetComponent<VRCAvatarDescriptor>(),
            };
            CollectionAssert.AreEquivalent(_fxGenerator.GetParentPrefabPathOfMARootObjects(),
                new List<string>()
                {
                    PrefabTestDirPath + "/original_prefab.prefab",
                    PrefabTestDirPath + "/prefab_variant.prefab",
                });

            GameObject.DestroyImmediate(notPrefab);
            GameObject.DestroyImmediate(originalPrefab);
            GameObject.DestroyImmediate(originalPrefabVariant);
            GameObject.DestroyImmediate(prefabVariant);
            GameObject.DestroyImmediate(prefabVariantVariant);
            GameObject.DestroyImmediate(unpackedOriginalPrefab);
            GameObject.DestroyImmediate(unpackedOriginalPrefabVariant);
            GameObject.DestroyImmediate(unpackedPrefabVariant);
            GameObject.DestroyImmediate(unpackedPrefabVariantVariant);
        }

        private static GameObject InstantiatePrefabAtPath(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instantiated = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            return instantiated;
        }

        private static bool HasPrefabInstance(GameObject parent, GameObject prefabInstance)
        {
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabInstance);
            Assert.That(string.IsNullOrEmpty(path), Is.False);

            foreach (Transform child in parent.transform)
            {
                if (child.gameObject.name == prefabInstance.name &&
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child.gameObject) == path)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
