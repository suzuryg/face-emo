using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Components.Data;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FaceEmo.Detail.Data
{
    [UnityEditor.CustomEditor(typeof(MenuRepositoryTestComponent))]
    public class MenuRepositoryTests : Editor
    {
        private static readonly string OutputDir = "Assets/Temp";
        private static readonly string OutputPath = OutputDir + "/MenuRepositoryTests.asset";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!UnpackPrefab()) { Debug.LogError("Failed to unpack prefab."); }

            if (GUILayout.Button("Clear (Scene)"))
            {
                try
                {
                    Clear(isAsset: false);
                    EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "Clear (Scene) Test passed!", "OK");
                }
                catch (System.Exception ex) { EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), $"Test failed.\n{ex.ToString()}", "OK"); }
            }

            if (GUILayout.Button("Save (Scene)"))
            {
                try
                {
                    Save(isAsset: false);
                    EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "Save (Scene) Test passed!", "OK");
                }
                catch (System.Exception ex) { EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), $"Test failed.\n{ex.ToString()}", "OK"); }
            }

            if (GUILayout.Button("Load (Scene)"))
            {
                try
                {
                    Load(isAsset: false);
                    EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "Load (Scene) Test passed!", "OK");
                }
                catch (System.Exception ex) { EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), $"Test failed.\n{ex.ToString()}", "OK"); }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Clear (Asset)"))
            {
                try
                {
                    Clear(isAsset: true);
                    EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "Clear (Asset) Test passed!", "OK");
                }
                catch (System.Exception ex) { EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), $"Test failed.\n{ex.ToString()}", "OK"); }
            }

            if (GUILayout.Button("Save (Asset)"))
            {
                try
                {
                    Save(isAsset: true);
                    EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "Save (Asset) Test passed!", "OK");
                }
                catch (System.Exception ex) { EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), $"Test failed.\n{ex.ToString()}", "OK"); }
            }

            if (GUILayout.Button("Load (Asset)"))
            {
                try
                {
                    Load(isAsset: true);
                    EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "Load (Asset) Test passed!", "OK");
                }
                catch (System.Exception ex) { EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), $"Test failed.\n{ex.ToString()}", "OK"); }
            }
        }

        public bool UnpackPrefab()
        {
            var menuRepositoryTestComponent = target as MenuRepositoryTestComponent;
            var rootObject = menuRepositoryTestComponent.gameObject;

            if (UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(rootObject))
            {
                if (UnityEditor.EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "You need to unpack prefab. Continue?", "OK", "Cancel"))
                {
                    UnityEditor.PrefabUtility.UnpackPrefabInstance(rootObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.UserAction);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public void Clear(bool isAsset)
        {
            if (isAsset)
            {
                if (AssetDatabase.LoadAssetAtPath<SerializableMenu>(OutputPath) is SerializableMenu)
                {
                    AssetDatabase.DeleteAsset(OutputPath);
                }
            }
            else
            {
                var menuRepositoryTestComponent = target as MenuRepositoryTestComponent;
                var menuRepository = new MenuRepository(menuRepositoryTestComponent.gameObject.GetComponent<MenuRepositoryComponent>());

                var menu = new Domain.Menu();

                menuRepository.Save(null, menu, "MenuRepositoryTests(Clear)");

                menu = menuRepository.Load(null);
                Assert.That(menu.Registered.Count, Is.EqualTo(0));
                Assert.That(menu.Unregistered.Count, Is.EqualTo(0));
            }
        }

        public void Save(bool isAsset)
        {
            var menu = new Domain.Menu();

            var r0 = menu.AddMode(Domain.Menu.RegisteredId);
            var r1 = menu.AddGroup(Domain.Menu.RegisteredId);
            var r2 = menu.AddMode(Domain.Menu.RegisteredId);
            var r3 = menu.AddMode(Domain.Menu.RegisteredId);
            var r4 = menu.AddMode(r1);
            var r5 = menu.AddMode(r1);

            var u0 = menu.AddGroup(Domain.Menu.UnregisteredId);
            var u1 = menu.AddMode(Domain.Menu.UnregisteredId);
            var u2 = menu.AddMode(Domain.Menu.UnregisteredId);
            var u3 = menu.AddGroup(Domain.Menu.UnregisteredId);
            var u4 = menu.AddMode(u0);
            var u5 = menu.AddMode(u3);

            menu.ModifyModeProperties(r0, true, "r0", true, EyeTrackingControl.Tracking, MouthTrackingControl.Tracking);
            menu.ModifyGroupProperties(r1, "r1");
            menu.ModifyModeProperties(r2, true, "r2", false, EyeTrackingControl.Animation, MouthTrackingControl.Animation);
            menu.ModifyModeProperties(r3, true, "r3", true, EyeTrackingControl.Animation, MouthTrackingControl.Tracking);
            menu.ModifyModeProperties(r4, false, "r4", false, EyeTrackingControl.Tracking, MouthTrackingControl.Animation);
            menu.ModifyModeProperties(r5, false, "r5", true, EyeTrackingControl.Tracking, MouthTrackingControl.Tracking);

            menu.ModifyGroupProperties(u0, "u0");
            menu.ModifyModeProperties(u1, false, "u1", true, EyeTrackingControl.Tracking, MouthTrackingControl.Animation);
            menu.ModifyModeProperties(u2, true, "u2", false, EyeTrackingControl.Animation, MouthTrackingControl.Tracking);
            menu.ModifyGroupProperties(u3, "u3");
            menu.ModifyModeProperties(u4, false, "u4", true, EyeTrackingControl.Tracking, MouthTrackingControl.Tracking);
            menu.ModifyModeProperties(u5, true, "u5", false, EyeTrackingControl.Tracking, MouthTrackingControl.Animation);

            menu.AddBranch(r0, new List<Condition>() { new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals) });
            menu.AddBranch(r0, new List<Condition>() { new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.NotEqual) });
            menu.AddBranch(r4, new List<Condition>() { new Condition(Hand.Both, HandGesture.HandGun, ComparisonOperator.Equals),  new Condition(Hand.Either, HandGesture.HandOpen, ComparisonOperator.NotEqual)});

            menu.ModifyBranchProperties(r0, 0, EyeTrackingControl.Animation, MouthTrackingControl.Animation, true, false, true, true);
            menu.ModifyBranchProperties(r0, 1, EyeTrackingControl.Animation, MouthTrackingControl.Tracking, true, false, false, true);
            menu.ModifyBranchProperties(r4, 0, EyeTrackingControl.Tracking, MouthTrackingControl.Tracking, true, false, false, false);

            menu.AddBranch(u1, new List<Condition>() { new Condition(Hand.OneSide, HandGesture.Neutral, ComparisonOperator.Equals) });
            menu.AddBranch(u5, new List<Condition>() { new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.NotEqual) });
            menu.AddBranch(u5, new List<Condition>() { new Condition(Hand.Either, HandGesture.ThumbsUp, ComparisonOperator.Equals),  new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.NotEqual)});

            menu.ModifyBranchProperties(u1, 0, EyeTrackingControl.Tracking, MouthTrackingControl.Tracking, true, false, true, true);
            menu.ModifyBranchProperties(u5, 0, EyeTrackingControl.Animation, MouthTrackingControl.Animation, true, false, false, true);
            menu.ModifyBranchProperties(u5, 1, EyeTrackingControl.Tracking, MouthTrackingControl.Animation, true, false, false, false);

            menu.SetAnimation(new Domain.Animation("anim00"), r0);
            menu.SetAnimation(new Domain.Animation("anim01"), r0, 0, BranchAnimationType.Base);
            menu.SetAnimation(new Domain.Animation("anim02"), r0, 0, BranchAnimationType.Left);
            menu.SetAnimation(new Domain.Animation("anim03"), r0, 0, BranchAnimationType.Right);
            menu.SetAnimation(new Domain.Animation("anim04"), r0, 0, BranchAnimationType.Both);
            menu.SetAnimation(new Domain.Animation("anim05"), r2);

            menu.SetAnimation(new Domain.Animation("anim06"),  u1);
            menu.SetAnimation(new Domain.Animation("anim07"),  u1, 0, BranchAnimationType.Base);
            menu.SetAnimation(new Domain.Animation("anim08"),  u1, 0, BranchAnimationType.Left);
            menu.SetAnimation(new Domain.Animation("anim09"),  u1, 0, BranchAnimationType.Right);
            menu.SetAnimation(new Domain.Animation("anim10"), u1, 0, BranchAnimationType.Both);
            menu.SetAnimation(new Domain.Animation("anim11"), u4);

            Domain.Menu loaded = null;
            if (isAsset)
            {
                if (!AssetDatabase.IsValidFolder(OutputDir))
                {
                    AV3Utility.CreateFolderRecursively(OutputDir);
                }

                var exporterRoot = new GameObject();
                var importerRoot = new GameObject();
                try
                {
                    var exporter = new MenuRepository(exporterRoot.AddComponent<MenuRepositoryComponent>());
                    exporter.Save(string.Empty, menu, "MenuRepositoryTests(Save)");
                    var expoted = CreateInstance<SerializableMenu>();
                    AssetDatabase.CreateAsset(expoted, OutputPath);
                    exporter.Export(expoted);

                    var importer = new MenuRepository(importerRoot.AddComponent<MenuRepositoryComponent>());
                    var imported = AssetDatabase.LoadAssetAtPath<SerializableMenu>(OutputPath);
                    importer.Import(imported);
                    loaded = importer.Load(string.Empty);
                }
                finally
                {
                    if (exporterRoot != null) { DestroyImmediate(exporterRoot); }
                    if (importerRoot != null) { DestroyImmediate(importerRoot); }
                }
            }
            else
            {
                var menuRepositoryTestComponent = target as MenuRepositoryTestComponent;
                var menuRepository = new MenuRepository(menuRepositoryTestComponent.gameObject.GetComponent<MenuRepositoryComponent>());
                menuRepository.Save(null, menu, "MenuRepositoryTests(Save)");
                loaded = menuRepository.Load(null);
            }

            Assert.That(loaded.GetMode(r0).DisplayName, Is.EqualTo("r0"));
            Assert.That(loaded.GetGroup(r1).DisplayName, Is.EqualTo("r1"));
            Assert.That(loaded.GetMode(r2).DisplayName, Is.EqualTo("r2"));
            Assert.That(loaded.GetMode(r3).DisplayName, Is.EqualTo("r3"));
            Assert.That(loaded.GetMode(r4).DisplayName, Is.EqualTo("r4"));
            Assert.That(loaded.GetMode(r5).DisplayName, Is.EqualTo("r5"));

            Assert.That(loaded.GetGroup(u0).DisplayName, Is.EqualTo("u0"));
            Assert.That(loaded.GetMode(u1).DisplayName, Is.EqualTo("u1"));
            Assert.That(loaded.GetMode(u2).DisplayName, Is.EqualTo("u2"));
            Assert.That(loaded.GetGroup(u3).DisplayName, Is.EqualTo("u3"));
            Assert.That(loaded.GetMode(u4).DisplayName, Is.EqualTo("u4"));
            Assert.That(loaded.GetMode(u5).DisplayName, Is.EqualTo("u5"));
        }

        public void Load(bool isAsset)
        {
            Domain.Menu menu = null;
            if (isAsset)
            {
                var importerRoot = new GameObject();
                try
                {
                    var importer = new MenuRepository(importerRoot.AddComponent<MenuRepositoryComponent>());
                    var imported = AssetDatabase.LoadAssetAtPath<SerializableMenu>(OutputPath);
                    importer.Import(imported);
                    menu = importer.Load(string.Empty);
                }
                finally
                {
                    if (importerRoot != null) { DestroyImmediate(importerRoot); }
                }
            }
            else
            {
                var menuRepositoryTestComponent = target as MenuRepositoryTestComponent;
                var menuRepository = new MenuRepository(menuRepositoryTestComponent.gameObject.GetComponent<MenuRepositoryComponent>());
                menu = menuRepository.Load(null);
            }

            Assert.That(menu.Registered.GetModeAt(0).DisplayName, Is.EqualTo("r0"));
            Assert.That(menu.Registered.GetGroupAt(1).DisplayName, Is.EqualTo("r1"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("r2"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("r3"));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).DisplayName, Is.EqualTo("r4"));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(1).DisplayName, Is.EqualTo("r5"));

            Assert.That(menu.Registered.GetModeAt(0).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(menu.Registered.GetModeAt(2).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(menu.Registered.GetModeAt(3).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(1).ChangeDefaultFace, Is.EqualTo(false));

            Assert.That(menu.Registered.GetModeAt(0).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(menu.Registered.GetModeAt(2).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(3).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(1).UseAnimationNameAsDisplayName, Is.EqualTo(true));

            Assert.That(menu.Registered.GetModeAt(0).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(2).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(menu.Registered.GetModeAt(3).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(1).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));

            Assert.That(menu.Registered.GetModeAt(0).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(2).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(menu.Registered.GetModeAt(3).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(1).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));

            Assert.That(menu.Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(2).Animation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(3).Animation, Is.Null);
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Animation, Is.Null);
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(1).Animation, Is.Null);

            Assert.That(menu.Registered.GetModeAt(0).Animation.GUID, Is.EqualTo("anim00"));
            Assert.That(menu.Registered.GetModeAt(2).Animation.GUID, Is.EqualTo("anim05"));

            Assert.That(menu.Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetModeAt(2).Branches.Count, Is.EqualTo(0));
            Assert.That(menu.Registered.GetModeAt(3).Branches.Count, Is.EqualTo(0));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches.Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(1).Branches.Count, Is.EqualTo(0));

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].BaseAnimation, Is.Null);
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].BaseAnimation, Is.Null);

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].LeftHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].RightHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].BothHandsAnimation, Is.Null);
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID, Is.EqualTo("anim01"));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation.GUID, Is.EqualTo("anim02"));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation.GUID, Is.EqualTo("anim03"));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation.GUID, Is.EqualTo("anim04"));

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].Conditions.Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(2));

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals)));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.HandGun, ComparisonOperator.Equals)));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Either, HandGesture.HandOpen, ComparisonOperator.NotEqual)));

            Assert.That(menu.Unregistered.GetGroupAt(0).DisplayName, Is.EqualTo("u0"));
            Assert.That(menu.Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("u1"));
            Assert.That(menu.Unregistered.GetModeAt(2).DisplayName, Is.EqualTo("u2"));
            Assert.That(menu.Unregistered.GetGroupAt(3).DisplayName, Is.EqualTo("u3"));
            Assert.That(menu.Unregistered.GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("u4"));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).DisplayName, Is.EqualTo("u5"));

            Assert.That(menu.Unregistered.GetModeAt(1).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(menu.Unregistered.GetModeAt(2).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(menu.Unregistered.GetGroupAt(0).GetModeAt(0).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).ChangeDefaultFace, Is.EqualTo(true));

            Assert.That(menu.Unregistered.GetModeAt(1).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(menu.Unregistered.GetModeAt(2).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(menu.Unregistered.GetGroupAt(0).GetModeAt(0).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).UseAnimationNameAsDisplayName, Is.EqualTo(false));

            Assert.That(menu.Unregistered.GetModeAt(1).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Unregistered.GetModeAt(2).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(menu.Unregistered.GetGroupAt(0).GetModeAt(0).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));

            Assert.That(menu.Unregistered.GetModeAt(1).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(menu.Unregistered.GetModeAt(2).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Unregistered.GetGroupAt(0).GetModeAt(0).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));

            Assert.That(menu.Unregistered.GetModeAt(1).Animation, Is.Not.Null);
            Assert.That(menu.Unregistered.GetModeAt(2).Animation, Is.Null);
            Assert.That(menu.Unregistered.GetGroupAt(0).GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Animation, Is.Null);

            Assert.That(menu.Unregistered.GetModeAt(1).Animation.GUID, Is.EqualTo("anim06"));
            Assert.That(menu.Unregistered.GetGroupAt(0).GetModeAt(0).Animation.GUID, Is.EqualTo("anim11"));

            Assert.That(menu.Unregistered.GetModeAt(1).Branches.Count, Is.EqualTo(1));
            Assert.That(menu.Unregistered.GetModeAt(2).Branches.Count, Is.EqualTo(0));
            Assert.That(menu.Unregistered.GetGroupAt(0).GetModeAt(0).Branches.Count, Is.EqualTo(0));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches.Count, Is.EqualTo(2));

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].BaseAnimation, Is.Null);
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].BaseAnimation, Is.Null);

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].LeftHandAnimation, Is.Not.Null);
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].LeftHandAnimation, Is.Null);

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].RightHandAnimation, Is.Not.Null);
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].RightHandAnimation, Is.Null);

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].BothHandsAnimation, Is.Not.Null);
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].BothHandsAnimation, Is.Null);

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].BaseAnimation.GUID, Is.EqualTo("anim07"));
            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].LeftHandAnimation.GUID, Is.EqualTo("anim08"));
            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].RightHandAnimation.GUID, Is.EqualTo("anim09"));
            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].BothHandsAnimation.GUID, Is.EqualTo("anim10"));

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(false));

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].Conditions.Count, Is.EqualTo(1));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(1));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].Conditions.Count, Is.EqualTo(2));

            Assert.That(menu.Unregistered.GetModeAt(1).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.OneSide, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.NotEqual)));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(menu.Unregistered.GetGroupAt(3).GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.NotEqual)));
        }
    }
}
