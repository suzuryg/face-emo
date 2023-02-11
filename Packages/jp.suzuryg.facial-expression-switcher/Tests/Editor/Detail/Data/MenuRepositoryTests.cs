using Suzuryg.FacialExpressionSwitcher.Domain;
using System.Collections.Generic;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    [UnityEditor.CustomEditor(typeof(MenuRepositoryTestComponent))]
    public class MenuRepositoryTests : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (UnityEngine.GUILayout.Button("Clear"))
            {
                try
                {
                    if (UnpackPrefab())
                    {
                        Clear();
                        UnityEditor.EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "Test passed!", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEditor.EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), $"Test failed.\n{ex.ToString()}", "OK");
                }
            }

            if (UnityEngine.GUILayout.Button("Save"))
            {
                try
                {
                    if (UnpackPrefab())
                    {
                        Save();
                        UnityEditor.EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "Test passed!", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEditor.EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), $"Test failed.\n{ex.ToString()}", "OK");
                }
            }

            if (UnityEngine.GUILayout.Button("Load"))
            {
                try
                {
                    if (UnpackPrefab())
                    {
                        Load();
                        UnityEditor.EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), "Test passed!", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEditor.EditorUtility.DisplayDialog(nameof(MenuRepositoryTests), $"Test failed.\n{ex.ToString()}", "OK");
                }
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

        public void Clear()
        {
            var menuRepositoryTestComponent = target as MenuRepositoryTestComponent;
            var serializableMenu = menuRepositoryTestComponent.gameObject.GetComponent<SerializableMenu>();
            var menuRepository = new MenuRepository(serializableMenu);

            var menu = new Menu();

            menuRepository.Save(null, menu, "MenuRepositoryTests(Clear)");

            menu = menuRepository.Load(null);
            Assert.That(menu.WriteDefaults, Is.EqualTo(false));
            Assert.That(menu.TransitionDurationSeconds, Is.EqualTo(0.1));
            Assert.That(menu.InsertIndices.Count, Is.EqualTo(0));
            Assert.That(menu.Registered.Count, Is.EqualTo(0));
            Assert.That(menu.Unregistered.Count, Is.EqualTo(0));
        }

        public void Save()
        {
            var menuRepositoryTestComponent = target as MenuRepositoryTestComponent;
            var serializableMenu = menuRepositoryTestComponent.gameObject.GetComponent<SerializableMenu>();
            var menuRepository = new MenuRepository(serializableMenu);

            var menu = new Menu();

            menu.WriteDefaults = true;
            menu.TransitionDurationSeconds = 5.0;
            menu.SetInsertIndices(new List<int>() { 0, 2, 5 });

            var r0 = menu.AddMode(Menu.RegisteredId);
            var r1 = menu.AddGroup(Menu.RegisteredId);
            var r2 = menu.AddMode(Menu.RegisteredId);
            var r3 = menu.AddMode(Menu.RegisteredId);
            var r4 = menu.AddMode(r1);
            var r5 = menu.AddMode(r1);

            var u0 = menu.AddGroup(Menu.UnregisteredId);
            var u1 = menu.AddMode(Menu.UnregisteredId);
            var u2 = menu.AddMode(Menu.UnregisteredId);
            var u3 = menu.AddGroup(Menu.UnregisteredId);
            var u4 = menu.AddMode(u0);
            var u5 = menu.AddMode(u3);

            menu.ModifyModeProperties(r0, "r0", true, EyeTrackingControl.Tracking, MouthTrackingControl.Tracking);
            menu.ModifyGroupProperties(r1, "r1");
            menu.ModifyModeProperties(r2, "r2", false, EyeTrackingControl.Animation, MouthTrackingControl.Animation);
            menu.ModifyModeProperties(r3, "r3", true, EyeTrackingControl.Animation, MouthTrackingControl.Tracking);
            menu.ModifyModeProperties(r4, "r4", false, EyeTrackingControl.Tracking, MouthTrackingControl.Animation);
            menu.ModifyModeProperties(r5, "r5", true, EyeTrackingControl.Tracking, MouthTrackingControl.Tracking);

            menu.ModifyGroupProperties(u0, "u0");
            menu.ModifyModeProperties(u1, "u1", true, EyeTrackingControl.Tracking, MouthTrackingControl.Animation);
            menu.ModifyModeProperties(u2, "u2", false, EyeTrackingControl.Animation, MouthTrackingControl.Tracking);
            menu.ModifyGroupProperties(u3, "u3");
            menu.ModifyModeProperties(u4, "u4", true, EyeTrackingControl.Tracking, MouthTrackingControl.Tracking);
            menu.ModifyModeProperties(u5, "u5", false, EyeTrackingControl.Tracking, MouthTrackingControl.Animation);

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

            menu.SetAnimation(new Animation("anim00"), r0);
            menu.SetAnimation(new Animation("anim01"), r0, 0, BranchAnimationType.Base);
            menu.SetAnimation(new Animation("anim02"), r0, 0, BranchAnimationType.Left);
            menu.SetAnimation(new Animation("anim03"), r0, 0, BranchAnimationType.Right);
            menu.SetAnimation(new Animation("anim04"), r0, 0, BranchAnimationType.Both);
            menu.SetAnimation(new Animation("anim05"), r2);

            menu.SetAnimation(new Animation("anim06"),  u1);
            menu.SetAnimation(new Animation("anim07"),  u1, 0, BranchAnimationType.Base);
            menu.SetAnimation(new Animation("anim08"),  u1, 0, BranchAnimationType.Left);
            menu.SetAnimation(new Animation("anim09"),  u1, 0, BranchAnimationType.Right);
            menu.SetAnimation(new Animation("anim10"), u1, 0, BranchAnimationType.Both);
            menu.SetAnimation(new Animation("anim11"), u4);

            menuRepository.Save(null, menu, "MenuRepositoryTests(Save)");

            menu = menuRepository.Load(null);

            Assert.That(menu.GetMode(r0).DisplayName, Is.EqualTo("r0"));
            Assert.That(menu.GetGroup(r1).DisplayName, Is.EqualTo("r1"));
            Assert.That(menu.GetMode(r2).DisplayName, Is.EqualTo("r2"));
            Assert.That(menu.GetMode(r3).DisplayName, Is.EqualTo("r3"));
            Assert.That(menu.GetMode(r4).DisplayName, Is.EqualTo("r4"));
            Assert.That(menu.GetMode(r5).DisplayName, Is.EqualTo("r5"));

            Assert.That(menu.GetGroup(u0).DisplayName, Is.EqualTo("u0"));
            Assert.That(menu.GetMode(u1).DisplayName, Is.EqualTo("u1"));
            Assert.That(menu.GetMode(u2).DisplayName, Is.EqualTo("u2"));
            Assert.That(menu.GetGroup(u3).DisplayName, Is.EqualTo("u3"));
            Assert.That(menu.GetMode(u4).DisplayName, Is.EqualTo("u4"));
            Assert.That(menu.GetMode(u5).DisplayName, Is.EqualTo("u5"));
        }

        public void Load()
        {
            var menuRepositoryTestComponent = target as MenuRepositoryTestComponent;
            var serializableMenu = menuRepositoryTestComponent.gameObject.GetComponent<SerializableMenu>();
            var menuRepository = new MenuRepository(serializableMenu);

            var menu = menuRepository.Load(null);

            Assert.That(menu.WriteDefaults, Is.EqualTo(true));
            Assert.That(menu.TransitionDurationSeconds, Is.EqualTo(5.0));
            CollectionAssert.AreEqual(menu.InsertIndices, new List<int>() { 0, 2, 5 });

            Assert.That(menu.Registered.GetModeAt(0).DisplayName, Is.EqualTo("r0"));
            Assert.That(menu.Registered.GetGroupAt(1).DisplayName, Is.EqualTo("r1"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("r2"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("r3"));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(0).DisplayName, Is.EqualTo("r4"));
            Assert.That(menu.Registered.GetGroupAt(1).GetModeAt(1).DisplayName, Is.EqualTo("r5"));

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
