using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public class GestureCellTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;

            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            //ModifyBranchPropertiesUseCase modifyBranchPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyBranchPropertiesUseCase>();
            ChangeBranchOrderUseCase changeBranchOrderUseCase = useCaseTestsInstaller.Container.Resolve<ChangeBranchOrderUseCase>();
            RemoveBranchUseCase removeBranchUseCase = useCaseTestsInstaller.Container.Resolve<RemoveBranchUseCase>();
            AddConditionUseCase addConditionUseCase = useCaseTestsInstaller.Container.Resolve<AddConditionUseCase>();
            ChangeConditionOrderUseCase changeConditionOrderUseCase = useCaseTestsInstaller.Container.Resolve<ChangeConditionOrderUseCase>();
            ModifyConditionUseCase modifyConditionUseCase = useCaseTestsInstaller.Container.Resolve<ModifyConditionUseCase>();
            RemoveConditionUseCase removeConditionUseCase = useCaseTestsInstaller.Container.Resolve<RemoveConditionUseCase>();

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Case1 ----------------------------------------------------------------------------------------------------------------------------------------
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            var menuCache = menuRepository.Load(menuId);
            var branchCache = menuCache.Registered.GetModeAt(0).Branches;
            var mode0Id = menuCache.Registered.Order[0];

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            // Case2 ----------------------------------------------------------------------------------------------------------------------------------------
            addBranchUseCase.Handle(menuId, mode0Id);
            addBranchUseCase.Handle(menuId, mode0Id);
            addBranchUseCase.Handle(menuId, mode0Id);
            addBranchUseCase.Handle(menuId, mode0Id);
            addBranchUseCase.Handle(menuId, mode0Id);
            addBranchUseCase.Handle(menuId, mode0Id);
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].IsReachable, Is.False);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].CanRightTriggerUsed, Is.False);

            // Case3 ----------------------------------------------------------------------------------------------------------------------------------------
            addConditionUseCase.Handle(menuId, mode0Id, 0, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)); 
            addConditionUseCase.Handle(menuId, mode0Id, 0, new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)); 
            addConditionUseCase.Handle(menuId, mode0Id, 1, new Condition(Hand.OneSide, HandGesture.Fist, ComparisonOperator.Equals)); 
            addConditionUseCase.Handle(menuId, mode0Id, 2, new Condition(Hand.Either, HandGesture.Fist, ComparisonOperator.Equals)); 
            addConditionUseCase.Handle(menuId, mode0Id, 3, new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.Equals)); 
            addConditionUseCase.Handle(menuId, mode0Id, 4, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)); 
            addConditionUseCase.Handle(menuId, mode0Id, 5, new Condition(Hand.Right, HandGesture.HandOpen, ComparisonOperator.Equals)); 
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[4]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[1]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].CanRightTriggerUsed, Is.False);

            // Case4 ----------------------------------------------------------------------------------------------------------------------------------------
            changeBranchOrderUseCase.Handle(menuId, mode0Id, 3, 0);
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].Conditions[0], Is.EqualTo(new Condition(Hand.OneSide, HandGesture.Fist, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.Fist, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.HandOpen, ComparisonOperator.Equals)));
            changeBranchOrderUseCase.Handle(menuId, mode0Id, 3, 1);
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.Fist, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].Conditions[0], Is.EqualTo(new Condition(Hand.OneSide, HandGesture.Fist, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.HandOpen, ComparisonOperator.Equals)));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[4]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[1]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[5]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[5].CanRightTriggerUsed, Is.False);

            // Case5 ----------------------------------------------------------------------------------------------------------------------------------------
            removeBranchUseCase.Handle(menuId, mode0Id, 1);
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].Conditions[0], Is.EqualTo(new Condition(Hand.OneSide, HandGesture.Fist, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.HandOpen, ComparisonOperator.Equals)));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[2]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanRightTriggerUsed, Is.False);

            // Case6 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.Both, HandGesture.Victory, ComparisonOperator.Equals));
            modifyConditionUseCase.Handle(menuId, mode0Id, 1, 1, new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.Equals));
            modifyConditionUseCase.Handle(menuId, mode0Id, 2, 0, new Condition(Hand.Either, HandGesture.RockNRoll, ComparisonOperator.Equals));
            modifyConditionUseCase.Handle(menuId, mode0Id, 3, 0, new Condition(Hand.Either, HandGesture.HandGun, ComparisonOperator.NotEqual));
            modifyConditionUseCase.Handle(menuId, mode0Id, 4, 0, new Condition(Hand.Either, HandGesture.ThumbsUp, ComparisonOperator.NotEqual));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.Victory, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.RockNRoll, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.HandGun, ComparisonOperator.NotEqual))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.ThumbsUp, ComparisonOperator.NotEqual)));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(branchCache[2]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanRightTriggerUsed, Is.False);

            // Case7 ----------------------------------------------------------------------------------------------------------------------------------------
            changeConditionOrderUseCase.Handle(menuId, mode0Id, 1, 1, -1);
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.Victory, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.RockNRoll, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.HandGun, ComparisonOperator.NotEqual))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.ThumbsUp, ComparisonOperator.NotEqual)));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(branchCache[2]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].IsReachable, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanRightTriggerUsed, Is.False);

            // Case8 ----------------------------------------------------------------------------------------------------------------------------------------
            removeConditionUseCase.Handle(menuId, mode0Id, 1, 1);
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.Victory, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.Equals)));
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.RockNRoll, ComparisonOperator.Equals))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.HandGun, ComparisonOperator.NotEqual))); 
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.ThumbsUp, ComparisonOperator.NotEqual)));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(branchCache[1]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(branchCache[1]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(branchCache[2]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(branchCache[4]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(branchCache[2]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(branchCache[3]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(branchCache[3]));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].IsReachable, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[1].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[2].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[3].CanRightTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[4].CanRightTriggerUsed, Is.False);

            // Case9 ----------------------------------------------------------------------------------------------------------------------------------------
            removeBranchUseCase.Handle(menuId, mode0Id, 1);
            removeBranchUseCase.Handle(menuId, mode0Id, 1);
            removeBranchUseCase.Handle(menuId, mode0Id, 1);
            removeBranchUseCase.Handle(menuId, mode0Id, 1);
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);

            // Case10 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.Equals));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);

            // Case11 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.OneSide, HandGesture.HandOpen, ComparisonOperator.Equals));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);

            // Case12 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.Either, HandGesture.Fingerpoint, ComparisonOperator.Equals));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);

            // Case13 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.Both, HandGesture.Victory, ComparisonOperator.Equals));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.False);

            // Case14 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.Left, HandGesture.RockNRoll, ComparisonOperator.NotEqual));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);

            // Case15 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.Right, HandGesture.HandGun, ComparisonOperator.NotEqual));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);

            // Case16 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.OneSide, HandGesture.ThumbsUp, ComparisonOperator.NotEqual));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);

            // Case17 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.Either, HandGesture.Neutral, ComparisonOperator.NotEqual));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.True);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.True);

            // Case18 ----------------------------------------------------------------------------------------------------------------------------------------
            modifyConditionUseCase.Handle(menuId, mode0Id, 0, 0, new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual));
            menuCache = menuRepository.Load(menuId);
            branchCache = menuCache.Registered.GetModeAt(0).Branches;

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Neutral, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Neutral), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandOpen), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Fingerpoint), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.Victory), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.RockNRoll), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.HandGun), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fist, HandGesture.ThumbsUp), Is.SameAs(null));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandOpen, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Fingerpoint, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.Victory, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.RockNRoll, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.HandGun, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Neutral), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fist), Is.SameAs(null));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandOpen), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Fingerpoint), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.Victory), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.RockNRoll), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.HandGun), Is.SameAs(branchCache[0]));
            Assert.That(menuCache.Registered.GetModeAt(0).GetGestureCell(HandGesture.ThumbsUp, HandGesture.ThumbsUp), Is.SameAs(branchCache[0]));

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].IsReachable, Is.True);

            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanLeftTriggerUsed, Is.False);
            Assert.That(menuCache.Registered.GetModeAt(0).Branches[0].CanRightTriggerUsed, Is.False);
        }
    }
}
