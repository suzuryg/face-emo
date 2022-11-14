using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public class MockRemoveBranchPresenter : IRemoveBranchPresenter
    {
        public RemoveBranchResult Result { get; private set; }

        public event System.Action<RemoveBranchResult, IMenu, string> OnCompleted;

        void IRemoveBranchPresenter.Complete(RemoveBranchResult removeBranchResult, in IMenu menu, string errorMessage)
        {
            Result = removeBranchResult;
        }
    }

    public class RemoveBranchUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestSetting.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            RemoveBranchUseCase removeBranchUseCase = useCaseTestsInstaller.Container.Resolve<RemoveBranchUseCase>();
            MockRemoveBranchPresenter mockRemoveBranchPresenter = useCaseTestsInstaller.Container.Resolve<IRemoveBranchPresenter>() as MockRemoveBranchPresenter;

            // null
            removeBranchUseCase.Handle(null, "", 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.ArgumentNull));
            removeBranchUseCase.Handle("", null, 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.ArgumentNull));

            // Menu is not opened
            removeBranchUseCase.Handle(menuId, "", 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.MenuDoesNotExist));

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid branch
            removeBranchUseCase.Handle(menuId, "", 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));

            removeBranchUseCase.Handle(menuId, Menu.RegisteredId, 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));

            removeBranchUseCase.Handle(menuId, Menu.UnregisteredId, 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));

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

            // Remove branch
            removeBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], -1);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));

            removeBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], 3);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));

            removeBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0], Is.SameAs(b0));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1], Is.SameAs(b2));

            removeBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0], Is.SameAs(b2));

            removeBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(0));
        }
    }
}
