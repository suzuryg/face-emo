using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public class MockRemoveConditionPresenter : IRemoveConditionPresenter
    {
        public RemoveConditionResult Result { get; private set; }

        public event System.Action<RemoveConditionResult, IMenu, string> OnCompleted;

        void IRemoveConditionPresenter.Complete(RemoveConditionResult removeConditionResult, in IMenu menu, string errorMessage)
        {
            Result = removeConditionResult;
        }
    }

    public class RemoveConditionUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            RemoveConditionUseCase removeConditionUseCase = useCaseTestsInstaller.Container.Resolve<RemoveConditionUseCase>();
            MockRemoveConditionPresenter mockRemoveConditionPresenter = useCaseTestsInstaller.Container.Resolve<IRemoveConditionPresenter>() as MockRemoveConditionPresenter;

            // null
            removeConditionUseCase.Handle(null, "", 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.ArgumentNull));
            removeConditionUseCase.Handle("", null, 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                removeConditionUseCase.Handle(menuId, "", 0, 0);
                Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.MenuDoesNotExist));
            }

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid branch
            removeConditionUseCase.Handle(menuId, "", 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));

            removeConditionUseCase.Handle(menuId, Menu.RegisteredId, 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));

            removeConditionUseCase.Handle(menuId, Menu.UnregisteredId, 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));

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
            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], -1, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 3, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, -1);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 3);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            // Success
            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(2));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 1);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(3));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, 2);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(2));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));

            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(1));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));

            removeConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(mode0().Branches[1].Conditions.Count, Is.EqualTo(0));
        }
    }
}
