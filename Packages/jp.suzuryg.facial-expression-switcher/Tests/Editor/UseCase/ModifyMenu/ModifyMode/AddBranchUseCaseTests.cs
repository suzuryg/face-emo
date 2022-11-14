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

        public event System.Action<AddBranchResult, IMenu, string> OnCompleted;

        void IAddBranchPresenter.Complete(AddBranchResult addBranchResult, in IMenu menu, string errorMessage)
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

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestSetting.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            MockAddBranchPresenter mockAddBranchPresenter = useCaseTestsInstaller.Container.Resolve<IAddBranchPresenter>() as MockAddBranchPresenter;

            // null
            addBranchUseCase.Handle(null, "");
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.ArgumentNull));
            addBranchUseCase.Handle("", null);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.ArgumentNull));

            // Menu is not opened
            addBranchUseCase.Handle(menuId, "");
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.MenuDoesNotExist));

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid destination
            addBranchUseCase.Handle(menuId, "");
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.InvalidDestination));

            addBranchUseCase.Handle(menuId, Menu.RegisteredId);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.InvalidDestination));

            addBranchUseCase.Handle(menuId, Menu.UnregisteredId);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.InvalidDestination));

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);

            // Add branch
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches.Count, Is.EqualTo(0));

            var conditions = new List<Condition>();
            conditions.Add(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals));
            conditions.Add(new Condition(Hand.Both, HandGesture.RockNRoll, ComparisonOperator.NotEqual));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], conditions);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals)));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.RockNRoll, ComparisonOperator.NotEqual)));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches.Count, Is.EqualTo(0));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1]);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals)));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.RockNRoll, ComparisonOperator.NotEqual)));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].Conditions.Count, Is.EqualTo(0));
        }
    }
}
