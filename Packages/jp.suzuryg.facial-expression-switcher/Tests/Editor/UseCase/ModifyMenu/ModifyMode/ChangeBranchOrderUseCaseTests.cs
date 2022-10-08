using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public class MockChangeBranchOrderPresenter : IChangeBranchOrderPresenter
    {
        public ChangeBranchOrderResult Result { get; private set; }

        void IChangeBranchOrderPresenter.Complete(ChangeBranchOrderResult changeBranchOrderResult, in Menu menu, string errorMessage)
        {
            Result = changeBranchOrderResult;
        }
    }

    public class ChangeBranchOrderUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            ChangeBranchOrderUseCase changeBranchOrderUseCase = useCaseTestsInstaller.Container.Resolve<ChangeBranchOrderUseCase>();
            MockChangeBranchOrderPresenter mockChangeBranchOrderPresenter = new MockChangeBranchOrderPresenter();
            changeBranchOrderUseCase.SetPresenter(mockChangeBranchOrderPresenter);

            // null
            changeBranchOrderUseCase.Handle(null, 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.ArgumentNull));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Menu is not opened
            changeBranchOrderUseCase.Handle("", 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.MenuIsNotOpened));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle();
            menuEditingSession.SaveAs("dest");
            var menu = menuEditingSession.Menu;

            // Invalid branch
            changeBranchOrderUseCase.Handle("", 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            changeBranchOrderUseCase.Handle(Menu.RegisteredId, 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            changeBranchOrderUseCase.Handle(Menu.UnregisteredId, 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));
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

            // Change branch order
            // Invalid
            changeBranchOrderUseCase.Handle(menu.Registered.Order[0], -1, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();

            changeBranchOrderUseCase.Handle(menu.Registered.Order[0], 3, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);
            menuEditingSession.Save();

            // Success
            changeBranchOrderUseCase.Handle(menu.Registered.Order[0], 2, -1);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches[0], Is.SameAs(b2));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1], Is.SameAs(b0));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2], Is.SameAs(b1));

            changeBranchOrderUseCase.Handle(menu.Registered.Order[0], 1, 9999);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches[0], Is.SameAs(b2));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1], Is.SameAs(b1));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2], Is.SameAs(b0));

            changeBranchOrderUseCase.Handle(menu.Registered.Order[0], 1, 1);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches[0], Is.SameAs(b2));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1], Is.SameAs(b1));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2], Is.SameAs(b0));

            changeBranchOrderUseCase.Handle(menu.Registered.Order[0], 0, 1);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Branches[0], Is.SameAs(b1));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1], Is.SameAs(b2));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2], Is.SameAs(b0));
        }
    }
}
