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

        void IAddConditionPresenter.Complete(AddConditionResult addConditionResult, in Menu menu, string errorMessage)
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
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            AddConditionUseCase addConditionUseCase = useCaseTestsInstaller.Container.Resolve<AddConditionUseCase>();
            MockAddConditionPresenter mockAddConditionPresenter = new MockAddConditionPresenter();
            addConditionUseCase.SetPresenter(mockAddConditionPresenter);

            // null
            addConditionUseCase.Handle(null, 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.ArgumentNull));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Menu is not opened
            addConditionUseCase.Handle("", 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.MenuIsNotOpened));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle();
            menuEditingSession.SaveAs("dest");
            var menu = menuEditingSession.Menu;

            // Invalid branch
            addConditionUseCase.Handle("", 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            addConditionUseCase.Handle(Menu.RegisteredId, 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            addConditionUseCase.Handle(Menu.UnregisteredId, 0, null);
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            menuEditingSession.Save();

            // Add branch
            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            addBranchUseCase.Handle(menu.Registered.Order[0]);
            addBranchUseCase.Handle(menu.Registered.Order[0]);
            addBranchUseCase.Handle(menu.Registered.Order[0]);
            menuEditingSession.Save();
            var b0 = menu.Registered.GetModeAt(0).Branches[0];
            var b1 = menu.Registered.GetModeAt(0).Branches[1];
            var b2 = menu.Registered.GetModeAt(0).Branches[2];
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            // Add Condition
            // Invalid
            addConditionUseCase.Handle(menu.Registered.Order[0], -1, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            addConditionUseCase.Handle(menu.Registered.Order[0], 3, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            // Success
            addConditionUseCase.Handle(menu.Registered.Order[0], 0, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(1));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            addConditionUseCase.Handle(menu.Registered.Order[0], 0, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(2));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            addConditionUseCase.Handle(menu.Registered.Order[0], 0, new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(3));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
            Assert.That(b2.Conditions.Count, Is.EqualTo(0));

            addConditionUseCase.Handle(menu.Registered.Order[0], 2, new Condition(Hand.Either, HandGesture.HandGun, ComparisonOperator.NotEqual));
            Assert.That(mockAddConditionPresenter.Result, Is.EqualTo(AddConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
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
