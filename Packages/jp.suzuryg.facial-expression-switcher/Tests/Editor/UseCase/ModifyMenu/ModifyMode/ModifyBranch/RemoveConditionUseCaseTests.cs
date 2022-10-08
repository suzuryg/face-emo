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

        void IRemoveConditionPresenter.Complete(RemoveConditionResult removeConditionResult, in Menu menu, string errorMessage)
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
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            RemoveConditionUseCase removeConditionUseCase = useCaseTestsInstaller.Container.Resolve<RemoveConditionUseCase>();
            MockRemoveConditionPresenter mockRemoveConditionPresenter = new MockRemoveConditionPresenter();
            removeConditionUseCase.SetPresenter(mockRemoveConditionPresenter);

            // null
            removeConditionUseCase.Handle(null, 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.ArgumentNull));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Menu is not opened
            removeConditionUseCase.Handle("", 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.MenuIsNotOpened));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle();
            menuEditingSession.SaveAs("dest");
            var menu = menuEditingSession.Menu;

            // Invalid branch
            removeConditionUseCase.Handle("", 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(menuEditingSession.IsModified, Is.False);

            removeConditionUseCase.Handle(Menu.RegisteredId, 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(menuEditingSession.IsModified, Is.False);

            removeConditionUseCase.Handle(Menu.UnregisteredId, 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
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

            // Add condition
            AddConditionUseCase addConditionUseCase = useCaseTestsInstaller.Container.Resolve<AddConditionUseCase>();
            addConditionUseCase.Handle(menu.Registered.Order[0], 0, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            addConditionUseCase.Handle(menu.Registered.Order[0], 0, new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals));
            addConditionUseCase.Handle(menu.Registered.Order[0], 0, new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual));
            addConditionUseCase.Handle(menu.Registered.Order[0], 1, new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals));
            addConditionUseCase.Handle(menu.Registered.Order[0], 1, new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals));
            addConditionUseCase.Handle(menu.Registered.Order[0], 1, new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual));
            menuEditingSession.Save();
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            // Modify condition
            // Invalid
            removeConditionUseCase.Handle(menu.Registered.Order[0], -1, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(3));
            Assert.That(b1.Conditions.Count, Is.EqualTo(3));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menu.Registered.Order[0], 3, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(3));
            Assert.That(b1.Conditions.Count, Is.EqualTo(3));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menu.Registered.Order[0], 0, -1);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(3));
            Assert.That(b1.Conditions.Count, Is.EqualTo(3));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menu.Registered.Order[0], 0, 3);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.InvalidCondition));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(3));
            Assert.That(b1.Conditions.Count, Is.EqualTo(3));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            // Success
            removeConditionUseCase.Handle(menu.Registered.Order[0], 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(2));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b0.Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));
            Assert.That(b1.Conditions.Count, Is.EqualTo(3));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menu.Registered.Order[0], 0, 1);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(1));
            Assert.That(b0.Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions.Count, Is.EqualTo(3));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menu.Registered.Order[0], 0, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(3));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[2], Is.EqualTo(new Condition(Hand.Both, HandGesture.Fist, ComparisonOperator.NotEqual)));

            removeConditionUseCase.Handle(menu.Registered.Order[0], 1, 2);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(2));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals)));
            Assert.That(b1.Conditions[1], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));

            removeConditionUseCase.Handle(menu.Registered.Order[0], 1, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(1));
            Assert.That(b1.Conditions[0], Is.EqualTo(new Condition(Hand.Right, HandGesture.RockNRoll, ComparisonOperator.Equals)));

            removeConditionUseCase.Handle(menu.Registered.Order[0], 1, 0);
            Assert.That(mockRemoveConditionPresenter.Result, Is.EqualTo(RemoveConditionResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(b0.Conditions.Count, Is.EqualTo(0));
            Assert.That(b1.Conditions.Count, Is.EqualTo(0));
        }
    }
}
