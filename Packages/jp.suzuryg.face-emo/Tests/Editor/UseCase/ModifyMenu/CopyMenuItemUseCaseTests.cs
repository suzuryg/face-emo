using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using NUnit.Framework;
using System.Collections.Generic;
using System;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu
{
    public class MockCopyMenuItemPresenter : ICopyMenuItemPresenter
    {
        public CopyMenuItemResult Result { get; private set; }

        public IObservable<(CopyMenuItemResult copyMenuItemResult, string copiedItemId, IMenu menu, string errorMessage)> Observable => throw new NotImplementedException();

        public void Complete(CopyMenuItemResult copyMenuItemResult, string copiedItemId, in IMenu menu, string errorMessage = "")
        {
            Result = copyMenuItemResult;
        }
    }

    public class CopyMenuItemUseCaseTests
    {
        [Test]
        public void Test()
        {
            // Setup
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            CopyMenuItemUseCase copyMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<CopyMenuItemUseCase>();
            MockCopyMenuItemPresenter mockCopyMenuItemPresenter = useCaseTestsInstaller.Container.Resolve<ICopyMenuItemPresenter>() as MockCopyMenuItemPresenter;

            // Argument null
            copyMenuItemUseCase.Handle("hoge", null);
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.ArgumentNull));
            copyMenuItemUseCase.Handle("hoge", string.Empty);
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.ArgumentNull));
            copyMenuItemUseCase.Handle(null, "hoge");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.ArgumentNull));
            copyMenuItemUseCase.Handle(string.Empty, "hoge");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.ArgumentNull));
            copyMenuItemUseCase.Handle(null, null);
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.ArgumentNull));
            copyMenuItemUseCase.Handle(null, string.Empty);
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.ArgumentNull));
            copyMenuItemUseCase.Handle(string.Empty, null);
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.ArgumentNull));
            copyMenuItemUseCase.Handle(string.Empty, string.Empty);
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.ArgumentNull));

            // Create Menu
            var createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Add item
            var addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            var removeMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<RemoveMenuItemUseCase>();
            var modifyModePropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyModePropertiesUseCase>();
            var modifyGroupPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyGroupPropertiesUseCase>();
            var addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            var modifyBranchPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyBranchPropertiesUseCase>();
            var modifyConditionUseCase = useCaseTestsInstaller.Container.Resolve<ModifyConditionUseCase>();
            var setExistingAnimationUseCase = useCaseTestsInstaller.Container.Resolve<SetExistingAnimationUseCase>();

            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            var g0 = loadMenu().Registered.Order[0];
            var m0 = loadMenu().Registered.Order[1];
            var m1 = loadMenu().Registered.Order[2];
            var m2 = loadMenu().Registered.Order[3];
            var m3 = loadMenu().Registered.Order[4];
            var m4 = loadMenu().Registered.Order[5];
            var m5 = loadMenu().Registered.Order[6];

            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            var g1  = loadMenu().GetGroup(g0).Order[0];
            var m7  = loadMenu().GetGroup(g0).Order[1];
            var m8  = loadMenu().GetGroup(g0).Order[2];
            var m9  = loadMenu().GetGroup(g0).Order[3];
            var m10 = loadMenu().GetGroup(g0).Order[4];
            var m11 = loadMenu().GetGroup(g0).Order[5];
            var m12 = loadMenu().GetGroup(g0).Order[6];
            var m13 = loadMenu().GetGroup(g0).Order[7];

            addMenuItemUseCase.Handle(menuId, g1, AddMenuItemType.Mode);
            var m14 = loadMenu().GetGroup(g1).Order[0];

            addMenuItemUseCase.Handle(menuId, Menu.UnregisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.UnregisteredId, AddMenuItemType.Mode);
            var m15 = loadMenu().Unregistered.Order[0];
            var m16 = loadMenu().Unregistered.Order[1];

            modifyGroupPropertiesUseCase.Handle(menuId, g0, displayName: "g0");
            modifyGroupPropertiesUseCase.Handle(menuId, g1, displayName: "g1");
            modifyModePropertiesUseCase.Handle(menuId, m0, displayName:  "m0");
            modifyModePropertiesUseCase.Handle(menuId, m1, displayName:  "m1");
            modifyModePropertiesUseCase.Handle(menuId, m2, displayName:  "m2");
            modifyModePropertiesUseCase.Handle(menuId, m3, displayName:  "m3");
            modifyModePropertiesUseCase.Handle(menuId, m4, displayName:  "m4");
            modifyModePropertiesUseCase.Handle(menuId, m5, displayName:  "m5");

            modifyModePropertiesUseCase.Handle(menuId, m7, displayName:  "m7");
            modifyModePropertiesUseCase.Handle(menuId, m8, displayName:  "m8");
            modifyModePropertiesUseCase.Handle(menuId, m9, displayName:  "m9");
            modifyModePropertiesUseCase.Handle(menuId, m10, displayName: "m10");
            modifyModePropertiesUseCase.Handle(menuId, m11, displayName: "m11");
            modifyModePropertiesUseCase.Handle(menuId, m12, displayName: "m12");
            modifyModePropertiesUseCase.Handle(menuId, m13, displayName: "m13");
            modifyModePropertiesUseCase.Handle(menuId, m14, displayName: "m14");
            modifyModePropertiesUseCase.Handle(menuId, m15, displayName: "m15");
            modifyModePropertiesUseCase.Handle(menuId, m16, displayName: "m16");

            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(7));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m2"));
            Assert.That(loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m3"));
            Assert.That(loadMenu().Registered.GetModeAt(5).DisplayName, Is.EqualTo("m4"));
            Assert.That(loadMenu().Registered.GetModeAt(6).DisplayName, Is.EqualTo("m5"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).Order.Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m7"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m8"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m9"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m10"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m11"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m12"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(7).DisplayName, Is.EqualTo("m13"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).Order.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m14"));

            Assert.That(loadMenu().Unregistered.Order.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));
            Assert.That(loadMenu().Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m16"));

            // Invalid source
            copyMenuItemUseCase.Handle("hoge", "Copied");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.InvalidSource));

            // exceeds limit
            copyMenuItemUseCase.Handle(m0, "Copied");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.InvalidDestination));

            removeMenuItemUseCase.Handle(menuId, m5);
            copyMenuItemUseCase.Handle(m0, "m0_Copy");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(7));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m0_Copy"));
            Assert.That(loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m2"));
            Assert.That(loadMenu().Registered.GetModeAt(5).DisplayName, Is.EqualTo("m3"));
            Assert.That(loadMenu().Registered.GetModeAt(6).DisplayName, Is.EqualTo("m4"));

            removeMenuItemUseCase.Handle(menuId, m4);
            copyMenuItemUseCase.Handle(g0, "g0_Copy");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(7));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).DisplayName, Is.EqualTo("g0_Copy"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m0_Copy"));
            Assert.That(loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetModeAt(5).DisplayName, Is.EqualTo("m2"));
            Assert.That(loadMenu().Registered.GetModeAt(6).DisplayName, Is.EqualTo("m3"));

            // Copy mode
            var neutral = new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals);
            var fist = new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals);
            var open = new Condition(Hand.Left, HandGesture.HandOpen, ComparisonOperator.Equals);
            var point = new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals);
            var victory = new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.Equals);
            var gun = new Condition(Hand.Left, HandGesture.HandGun, ComparisonOperator.Equals);
            var rock = new Condition(Hand.Left, HandGesture.RockNRoll, ComparisonOperator.Equals);
            var thumbs = new Condition(Hand.Left, HandGesture.ThumbsUp, ComparisonOperator.Equals);

            createMenuUseCase.Handle(menuId);
            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(0));

            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Group);
            var g_0 = loadMenu().Registered.Order[0];
            modifyGroupPropertiesUseCase.Handle(menuId, g_0, displayName: "g_0");
            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(1));

            addMenuItemUseCase.Handle(menuId, g_0, AddMenuItemType.Mode);
            var m_0 = loadMenu().Registered.GetGroupAt(0).Order[0];
            modifyModePropertiesUseCase.Handle(menuId, m_0, displayName: "m_0");
            Assert.That(loadMenu().Registered.GetGroupAt(0).Order.Count, Is.EqualTo(1));

            copyMenuItemUseCase.Handle(m_0, "m_1");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetGroupAt(0).Order.Count, Is.EqualTo(2));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m_0"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).Branches.Count, Is.EqualTo(0));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m_1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches.Count, Is.EqualTo(0));

            modifyModePropertiesUseCase.Handle(menuId, m_0,
                changeDefaultFace: true,
                useAnimationNameAsDisplayName: true,
                eyeTrackingControl: EyeTrackingControl.Animation,
                mouthTrackingControl: MouthTrackingControl.Animation,
                blinkEnabled: false,
                mouthMorphCancelerEnabled: false);

            copyMenuItemUseCase.Handle(m_0, "m_2");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetGroupAt(0).Order.Count, Is.EqualTo(3));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m_0"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).Branches.Count, Is.EqualTo(0));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m_2"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches.Count, Is.EqualTo(0));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m_1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches.Count, Is.EqualTo(0));

            var m_2 = loadMenu().Registered.GetGroupAt(0).Order[1];
            modifyModePropertiesUseCase.Handle(menuId, m_2,
                changeDefaultFace: true,
                useAnimationNameAsDisplayName: false,
                eyeTrackingControl: EyeTrackingControl.Tracking,
                mouthTrackingControl: MouthTrackingControl.Animation,
                blinkEnabled: true,
                mouthMorphCancelerEnabled: false);

            addBranchUseCase.Handle(menuId, m_2, new[] { neutral, fist });
            addBranchUseCase.Handle(menuId, m_2, new[] { open, point });

            modifyBranchPropertiesUseCase.Handle(menuId, m_2, 1,
                eyeTrackingControl: EyeTrackingControl.Animation,
                mouthTrackingControl: MouthTrackingControl.Animation,
                blinkEnabled: false,
                mouthMorphCancelerEnabled: false,
                isLeftTriggerUsed: true,
                isRightTriggerUsed: true);

            setExistingAnimationUseCase.Handle(menuId, new Animation("0"), m_2, 0, BranchAnimationType.Base);
            setExistingAnimationUseCase.Handle(menuId, new Animation("1"), m_2, 0, BranchAnimationType.Left);
            setExistingAnimationUseCase.Handle(menuId, new Animation("2"), m_2, 0, BranchAnimationType.Right);
            setExistingAnimationUseCase.Handle(menuId, new Animation("3"), m_2, 0, BranchAnimationType.Both);

            copyMenuItemUseCase.Handle(m_2, "m_3");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetGroupAt(0).Order.Count, Is.EqualTo(4));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m_0"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).Branches.Count, Is.EqualTo(0));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m_2"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches.Count, Is.EqualTo(2));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].Conditions[1], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].BaseAnimation.GUID, Is.EqualTo("0"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].LeftHandAnimation.GUID, Is.EqualTo("1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].RightHandAnimation.GUID, Is.EqualTo("2"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].BothHandsAnimation.GUID, Is.EqualTo("3"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].Conditions[1], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].BothHandsAnimation, Is.Null);

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m_3"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches.Count, Is.EqualTo(2));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].Conditions[1], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].BaseAnimation.GUID, Is.EqualTo("0"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].LeftHandAnimation.GUID, Is.EqualTo("1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].RightHandAnimation.GUID, Is.EqualTo("2"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].BothHandsAnimation.GUID, Is.EqualTo("3"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].Conditions[1], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].BothHandsAnimation, Is.Null);

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m_1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).Branches.Count, Is.EqualTo(0));

            // Copy group
            copyMenuItemUseCase.Handle(g_0, "g_1");
            Assert.That(mockCopyMenuItemPresenter.Result, Is.EqualTo(CopyMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g_0"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).DisplayName, Is.EqualTo("g_1"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).Order.Count, Is.EqualTo(4));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m_0"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).Branches.Count, Is.EqualTo(0));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m_2"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches.Count, Is.EqualTo(2));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].Conditions[1], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].BaseAnimation.GUID, Is.EqualTo("0"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].LeftHandAnimation.GUID, Is.EqualTo("1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].RightHandAnimation.GUID, Is.EqualTo("2"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[0].BothHandsAnimation.GUID, Is.EqualTo("3"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].Conditions[1], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).Branches[1].BothHandsAnimation, Is.Null);

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m_3"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches.Count, Is.EqualTo(2));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].Conditions[1], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].BaseAnimation.GUID, Is.EqualTo("0"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].LeftHandAnimation.GUID, Is.EqualTo("1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].RightHandAnimation.GUID, Is.EqualTo("2"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[0].BothHandsAnimation.GUID, Is.EqualTo("3"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].Conditions[1], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).Branches[1].BothHandsAnimation, Is.Null);

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m_1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).Branches.Count, Is.EqualTo(0));

            Assert.That(loadMenu().Registered.GetGroupAt(1).Order.Count, Is.EqualTo(4));

            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(0).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(0).DisplayName, Is.EqualTo("m_0"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(0).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(0).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(0).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(0).BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(0).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(0).Branches.Count, Is.EqualTo(0));

            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).DisplayName, Is.EqualTo("m_2"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches.Count, Is.EqualTo(2));

            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].Conditions[1], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].BaseAnimation.GUID, Is.EqualTo("0"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].LeftHandAnimation.GUID, Is.EqualTo("1"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].RightHandAnimation.GUID, Is.EqualTo("2"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[0].BothHandsAnimation.GUID, Is.EqualTo("3"));

            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].Conditions[1], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(1).Branches[1].BothHandsAnimation, Is.Null);

            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).DisplayName, Is.EqualTo("m_3"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches.Count, Is.EqualTo(2));

            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].Conditions[1], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].BaseAnimation.GUID, Is.EqualTo("0"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].LeftHandAnimation.GUID, Is.EqualTo("1"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].RightHandAnimation.GUID, Is.EqualTo("2"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[0].BothHandsAnimation.GUID, Is.EqualTo("3"));

            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].Conditions[1], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].IsRightTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(2).Branches[1].BothHandsAnimation, Is.Null);

            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(3).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(3).DisplayName, Is.EqualTo("m_1"));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(3).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(3).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(3).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(3).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(3).MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetGroupAt(1).GetModeAt(3).Branches.Count, Is.EqualTo(0));
        }
    }
}
