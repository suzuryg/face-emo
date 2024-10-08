﻿using NUnit.Framework;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public class MockModifyConditionPresenter : IModifyConditionPresenter
    {
        public ModifyConditionResult Result { get; private set; }

        public System.IObservable<(ModifyConditionResult, IMenu, string)> Observable => throw new System.NotImplementedException();

        void IModifyConditionPresenter.Complete(ModifyConditionResult modifyConditionResult, in IMenu menu, string errorMessage)
        {
            Result = modifyConditionResult;
        }
    }

    public class ModifyConditionUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            ModifyConditionUseCase modifyConditionUseCase = useCaseTestsInstaller.Container.Resolve<ModifyConditionUseCase>();
            MockModifyConditionPresenter mockModifyConditionPresenter = useCaseTestsInstaller.Container.Resolve<IModifyConditionPresenter>() as MockModifyConditionPresenter;

            // null
            modifyConditionUseCase.Handle(null, "", 0, 0, null);
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.ArgumentNull));
            modifyConditionUseCase.Handle("", null, 0, 0, null);
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                modifyConditionUseCase.Handle(menuId, "", 0, 0, null);
                Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.MenuDoesNotExist));
            }

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid branch
            modifyConditionUseCase.Handle(menuId, "", 0, 0, null);
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.InvalidCondition));

            modifyConditionUseCase.Handle(menuId, Menu.RegisteredId, 0, 0, null);
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.InvalidCondition));

            modifyConditionUseCase.Handle(menuId, Menu.UnregisteredId, 0, 0, null);
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.InvalidCondition));

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);

            // Add branch
            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            System.Func<IMode> mode0 = () => loadMenu().Registered.GetModeAt(0);

            // Add condition
            AddConditionUseCase addConditionUseCase = useCaseTestsInstaller.Container.Resolve<AddConditionUseCase>();
            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals));
            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual));
            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals));
            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            // Modify condition
            // Invalid
            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], -1, 0, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 3, 0, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, -1, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 3, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            // Success
            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 0, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 1, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 2, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, 1, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, 2, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));

            modifyConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, 0, new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals));
            Assert.That(mockModifyConditionPresenter.Result, Is.EqualTo(ModifyConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.ThumbsUp, ComparisonOperator.Equals)));
        }
    }
}
