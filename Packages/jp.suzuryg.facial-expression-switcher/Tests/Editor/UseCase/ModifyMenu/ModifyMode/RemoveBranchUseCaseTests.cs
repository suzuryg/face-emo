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

        void IRemoveBranchPresenter.Complete(RemoveBranchResult removeBranchResult, in Menu menu, string errorMessage)
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
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            RemoveBranchUseCase removeBranchUseCase = useCaseTestsInstaller.Container.Resolve<RemoveBranchUseCase>();
            MockRemoveBranchPresenter mockRemoveBranchPresenter = new MockRemoveBranchPresenter();
            removeBranchUseCase.SetPresenter(mockRemoveBranchPresenter);

            // null
            removeBranchUseCase.Handle(null, 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.ArgumentNull));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Menu is not opened
            removeBranchUseCase.Handle("", 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.MenuIsNotOpened));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle();
            menuEditingSession.SaveAs("dest");
            var menu = menuEditingSession.Menu;

            // Invalid branch
            removeBranchUseCase.Handle("", 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            removeBranchUseCase.Handle(Menu.RegisteredId, 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            removeBranchUseCase.Handle(Menu.UnregisteredId, 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));
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

            // Remove branch
            removeBranchUseCase.Handle(menu.Registered.Order[0], -1);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();

            removeBranchUseCase.Handle(menu.Registered.Order[0], 3);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();

            removeBranchUseCase.Handle(menu.Registered.Order[0], 1);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0], Is.SameAs(b0));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1], Is.SameAs(b2));

            removeBranchUseCase.Handle(menu.Registered.Order[0], 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches.Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0], Is.SameAs(b2));

            removeBranchUseCase.Handle(menu.Registered.Order[0], 0);
            Assert.That(mockRemoveBranchPresenter.Result, Is.EqualTo(RemoveBranchResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches.Count, Is.EqualTo(0));
        }
    }
}
