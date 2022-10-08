using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public class MockAddBranchPresenter : IAddBranchPresenter
    {
        public AddBranchResult Result { get; private set; }

        void IAddBranchPresenter.Complete(AddBranchResult addBranchResult, in Menu menu, string errorMessage)
        {
            Result = addBranchResult;
        }
    }

    public class AddBranchUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            MockAddBranchPresenter mockAddBranchPresenter = new MockAddBranchPresenter();
            addBranchUseCase.SetPresenter(mockAddBranchPresenter);

            // null
            addBranchUseCase.Handle(null);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.ArgumentNull));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Menu is not opened
            addBranchUseCase.Handle("");
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.MenuIsNotOpened));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle();
            menuEditingSession.SaveAs("dest");
            var menu = menuEditingSession.Menu;

            // Invalid destination
            addBranchUseCase.Handle("");
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            addBranchUseCase.Handle(Menu.RegisteredId);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            addBranchUseCase.Handle(Menu.UnregisteredId);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            menuEditingSession.Save();

            // Add branch
            addBranchUseCase.Handle(menu.Registered.Order[0]);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches.Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(menu.Registered.GetModeAt(1).Branches.Count, Is.EqualTo(0));

            var conditions = new List<Condition>();
            conditions.Add(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals));
            conditions.Add(new Condition(Hand.Both, HandGesture.RockNRoll, ComparisonOperator.NotEqual));

            addBranchUseCase.Handle(menu.Registered.Order[0], conditions);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].Conditions.Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals)));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.RockNRoll, ComparisonOperator.NotEqual)));
            Assert.That(menu.Registered.GetModeAt(1).Branches.Count, Is.EqualTo(0));

            addBranchUseCase.Handle(menu.Registered.Order[1]);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].Conditions.Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals)));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.RockNRoll, ComparisonOperator.NotEqual)));
            Assert.That(menu.Registered.GetModeAt(1).Branches.Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetModeAt(1).Branches[0].Conditions.Count, Is.EqualTo(0));
        }
    }
}
