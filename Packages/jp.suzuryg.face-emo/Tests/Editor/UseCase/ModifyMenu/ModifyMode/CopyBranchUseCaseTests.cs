using NUnit.Framework;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.UseCase.ModifyMenu;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using System.Collections.Generic;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode
{
    public class MockCopyBranchPresenter : ICopyBranchPresenter
    {
        public CopyBranchResult Result { get; private set; }

        public System.IObservable<(CopyBranchResult, IMenu, string)> Observable => throw new System.NotImplementedException();

        void ICopyBranchPresenter.Complete(CopyBranchResult CopyBranchResult, in IMenu menu, string errorMessage)
        {
            Result = CopyBranchResult;
        }
    }

    public class CopyBranchUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            CopyBranchUseCase copyBranchUseCase = useCaseTestsInstaller.Container.Resolve<CopyBranchUseCase>();
            MockCopyBranchPresenter mockCopyBranchPresenter = useCaseTestsInstaller.Container.Resolve<ICopyBranchPresenter>() as MockCopyBranchPresenter;

            // null
            copyBranchUseCase.Handle(null, "", 0, null, null, null, null);
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.ArgumentNull));
            copyBranchUseCase.Handle("", null, 0, null, null, null, null);
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                copyBranchUseCase.Handle(menuId, "", 0, null, null, null, null);
                Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.MenuDoesNotExist));
            }

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid destination
            copyBranchUseCase.Handle(menuId, "", 0, null, null, null, null);
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.InvalidDestination));

            copyBranchUseCase.Handle(menuId, Menu.RegisteredId, 0, null, null, null, null);
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.InvalidDestination));

            copyBranchUseCase.Handle(menuId, Menu.UnregisteredId, 0, null, null, null, null);
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.InvalidDestination));

            // Prepare
            var neutral = new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals);
            var fist = new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals);
            var open = new Condition(Hand.Left, HandGesture.HandOpen, ComparisonOperator.Equals);
            var point = new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals);
            var victory = new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.Equals);
            var gun = new Condition(Hand.Left, HandGesture.HandGun, ComparisonOperator.Equals);
            var rock = new Condition(Hand.Left, HandGesture.RockNRoll, ComparisonOperator.Equals);
            var thumbs = new Condition(Hand.Left, HandGesture.ThumbsUp, ComparisonOperator.Equals);

            var addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            var addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            var modifyBranchPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyBranchPropertiesUseCase>();
            var modifyConditionUseCase = useCaseTestsInstaller.Container.Resolve<ModifyConditionUseCase>();
            var setExistingAnimationUseCase = useCaseTestsInstaller.Container.Resolve<SetExistingAnimationUseCase>();

            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            var m0 = loadMenu().Registered.Order[0];
            addBranchUseCase.Handle(menuId, m0, new[] { neutral });

            // Copy branch
            copyBranchUseCase.Handle(string.Empty, m0, 0, null, null, null, null);
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));

            modifyConditionUseCase.Handle(menuId, m0, 0, 0, fist);
            modifyBranchPropertiesUseCase.Handle(menuId, m0, 0,
                eyeTrackingControl: EyeTrackingControl.Animation,
                mouthTrackingControl: MouthTrackingControl.Animation,
                blinkEnabled: false,
                mouthMorphCancelerEnabled: false,
                isLeftTriggerUsed: true,
                isRightTriggerUsed: true);

            copyBranchUseCase.Handle(string.Empty, m0, 0, null, null, null, null);
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(3));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(true));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(true));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsRightTriggerUsed, Is.EqualTo(false));

            modifyConditionUseCase.Handle(menuId, m0, 1, 0, open);
            modifyBranchPropertiesUseCase.Handle(menuId, m0, 1,
                eyeTrackingControl: EyeTrackingControl.Tracking,
                mouthTrackingControl: MouthTrackingControl.Animation,
                blinkEnabled: true,
                mouthMorphCancelerEnabled: false,
                isLeftTriggerUsed: true,
                isRightTriggerUsed: false);

            copyBranchUseCase.Handle(string.Empty, m0, 1, null, null, null, null);
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(4));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(true));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsRightTriggerUsed, Is.EqualTo(false));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[3].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[3].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[3].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[3].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[3].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[3].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[3].IsRightTriggerUsed, Is.EqualTo(false));

            // Set animations
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            var m1 = loadMenu().Registered.Order[1];
            addBranchUseCase.Handle(menuId, m1);
            setExistingAnimationUseCase.Handle(menuId, new Animation("0"), m1, 0, BranchAnimationType.Base);
            setExistingAnimationUseCase.Handle(menuId, new Animation("1"), m1, 0, BranchAnimationType.Left);
            setExistingAnimationUseCase.Handle(menuId, new Animation("2"), m1, 0, BranchAnimationType.Right);
            setExistingAnimationUseCase.Handle(menuId, new Animation("3"), m1, 0, BranchAnimationType.Both);

            copyBranchUseCase.Handle(string.Empty, m1, 0, null, null, null, null);
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches.Count, Is.EqualTo(2));

            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].BothHandsAnimation, Is.Null);

            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[1].BaseAnimation.GUID, Is.EqualTo("0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[1].LeftHandAnimation.GUID, Is.EqualTo("1"));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[1].RightHandAnimation.GUID, Is.EqualTo("2"));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[1].BothHandsAnimation.GUID, Is.EqualTo("3"));

            copyBranchUseCase.Handle(string.Empty, m1, 1, new Animation("4"), new Animation("5"), new Animation("6"), new Animation("7"));
            Assert.That(mockCopyBranchPresenter.Result, Is.EqualTo(CopyBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches.Count, Is.EqualTo(3));

            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].BothHandsAnimation, Is.Null);

            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[1].BaseAnimation.GUID, Is.EqualTo("4"));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[1].LeftHandAnimation.GUID, Is.EqualTo("5"));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[1].RightHandAnimation.GUID, Is.EqualTo("6"));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[1].BothHandsAnimation.GUID, Is.EqualTo("7"));

            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[2].BaseAnimation.GUID, Is.EqualTo("0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[2].LeftHandAnimation.GUID, Is.EqualTo("1"));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[2].RightHandAnimation.GUID, Is.EqualTo("2"));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[2].BothHandsAnimation.GUID, Is.EqualTo("3"));
        }
    }
}
