using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch
{
    public class MockAddConditionPresenter : IAddConditionPresenter
    {
        public AddConditionResult Result { get; private set; }

        public event System.Action<AddConditionResult, IMenu, string> OnCompleted;

        void IAddConditionPresenter.Complete(AddConditionResult addConditionResult, in IMenu menu, string errorMessage)
        {
            Result = addConditionResult;
        }
    }

    public class AddConditionUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestSetting.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            AddConditionUseCase addConditionUseCase = useCaseTestsInstaller.Container.Resolve<AddConditionUseCase>();
            MockAddConditionPresenter mockAddConditionPresenter = useCaseTestsInstaller.Container.Resolve<IAddConditionPresenter>() as MockAddConditionPresenter;

            // null
            addConditionUseCase.Handle(null, "", 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.ArgumentNull));
            addConditionUseCase.Handle("", null, 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.ArgumentNull));

            // Menu is not opened
            addConditionUseCase.Handle(menuId, "", 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.MenuDoesNotExist));

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid branch
            addConditionUseCase.Handle(menuId, "", 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));

            addConditionUseCase.Handle(menuId, Menu.RegisteredId, 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));

            addConditionUseCase.Handle(menuId, Menu.UnregisteredId, 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);

            // Add branch
            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            var b0 = loadMenu().Registered.GetModeAt(0).Branches[0];
            var b1 = loadMenu().Registered.GetModeAt(0).Branches[1];
            var b2 = loadMenu().Registered.GetModeAt(0).Branches[2];
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            // Add Condition
            // Invalid
            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], -1, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 3, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            // Success
            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.Succeeded));
            Assert.That(b0.Conditions.Count, Is.EqualTo(1));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.Succeeded));
            Assert.That(b0.Conditions.Count, Is.EqualTo(2));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.Succeeded));
            Assert.That(b0.Conditions.Count, Is.EqualTo(3));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            addConditionUseCase.Handle(menuId, loadMenu().Registered.Order[0], 2, new Condition(Hand.Either, HandGesture.HandGun, ComparisonOperator.NotEqual));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.Succeeded));
            Assert.That(b0.Conditions.Count, Is.EqualTo(3));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(1));
            Assert.That(b2.Conditions[0], Is.EqualTo(new Condition(Hand.Either, HandGesture.HandGun, ComparisonOperator.NotEqual)));
        }
    }
}
