using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public class MockChangeConditionOrderPresenter : IChangeConditionOrderPresenter
    {
        public ChangeConditionOrderResult Result { get; private set; }

        public event System.Action<ChangeConditionOrderResult, IMenu, string> OnCompleted;

        void IChangeConditionOrderPresenter.Complete(ChangeConditionOrderResult changeConditionOrderResult, in IMenu menu, string errorMessage)
        {
            Result = changeConditionOrderResult;
        }
    }

    public class ChangeConditionOrderUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            ChangeConditionOrderUseCase changeConditionOrderUseCase = useCaseTestsInstaller.Container.Resolve<ChangeConditionOrderUseCase>();
            MockChangeConditionOrderPresenter mockChangeConditionOrderPresenter = useCaseTestsInstaller.Container.Resolve<IChangeConditionOrderPresenter>() as MockChangeConditionOrderPresenter;

            // null
            changeConditionOrderUseCase.Handle("", null, 0, 0, 0);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.ArgumentNull));
            changeConditionOrderUseCase.Handle(null, "", 0, 0, 0);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                changeConditionOrderUseCase.Handle(menuId, "", 0, 0, 0);
                Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.MenuDoesNotExist));
            }

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid branch
            changeConditionOrderUseCase.Handle(menuId, "", 0, 0, 0);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.InvalidCondition));

            changeConditionOrderUseCase.Handle(menuId, Menu.RegisteredId, 0, 0, 0);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.InvalidCondition));

            changeConditionOrderUseCase.Handle(menuId, Menu.UnregisteredId, 0, 0, 0);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.InvalidCondition));

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

            // Change condition order
            changeConditionOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], -1, 0, 0);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            changeConditionOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 3, 0, 0);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            changeConditionOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, -1, 0);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            changeConditionOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 3, 0);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.InvalidCondition));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            changeConditionOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 2, -1);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            changeConditionOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 1, 9999);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            changeConditionOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 1, 1);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            changeConditionOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 0, 1);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            changeConditionOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, 0, 1);
            Assert.That(mockChangeConditionOrderPresenter.Result, Is.EqualTo(ChangeConditionOrderResult.Succeeded));
            Assert.That(mode0().Branches[0].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[0].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(mode0().Branches[0].Conditions[2], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(mode0().Branches[1].Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
        }
    }
}
