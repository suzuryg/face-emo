using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public class MockRemoveMenuItemPresenter : IRemoveMenuItemPresenter
    {
        public RemoveMenuItemResult Result { get; private set; }

        void IRemoveMenuItemPresenter.Complete(RemoveMenuItemResult removeMenuItemResult, in Menu menu, string errorMessage)
        {
            Result = removeMenuItemResult;
        }
    }

    public class RemoveMenuItemUseCaseTests
    {
        [Test]
        public void Test()
        {
            // Setup
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            RemoveMenuItemUseCase removeMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<RemoveMenuItemUseCase>();

            MockRemoveMenuItemPresenter mockRemoveMenuItemPresenter = new MockRemoveMenuItemPresenter();
            removeMenuItemUseCase.SetPresenter(mockRemoveMenuItemPresenter);

            // null
            removeMenuItemUseCase.Handle(null);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.ArgumentNull));

            // Menu is not opened
            removeMenuItemUseCase.Handle("");
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.MenuIsNotOpened));

            // Create Menu
            useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>().Handle();
            var menu = menuEditingSession.Menu;
            menuEditingSession.SaveAs("dest");

            // Item is not created
            removeMenuItemUseCase.Handle("");
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.InvalidId));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            // Add item
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            ModifyModePropertiesUseCase modifyModePropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyModePropertiesUseCase>();
            ModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyGroupPropertiesUseCase>();

            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Group);
            var g0 = menu.Registered.Order[0];
            addMenuItemUseCase.Handle(g0, AddMenuItemType.Group);
            var g1 = menu.GetGroup(g0).Order[0];
            addMenuItemUseCase.Handle(g1, AddMenuItemType.Mode);
            var m0 = menu.GetGroup(g1).Order[0];
            modifyGroupPropertiesUseCase.Handle(g0, displayName: "g0");
            modifyGroupPropertiesUseCase.Handle(g1, displayName: "g1");
            modifyModePropertiesUseCase.Handle(m0, displayName: "m0");

            addMenuItemUseCase.Handle(g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(g0, AddMenuItemType.Mode);
            var m1 = menu.GetGroup(g0).Order[1];
            var m2 = menu.GetGroup(g0).Order[2];
            modifyModePropertiesUseCase.Handle(m1, displayName: "m1");
            modifyModePropertiesUseCase.Handle(m2, displayName: "m2");

            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            var r1 = menu.Registered.Order[1];
            var r2 = menu.Registered.Order[2];
            modifyModePropertiesUseCase.Handle(r1, displayName: "r1");
            modifyModePropertiesUseCase.Handle(r2, displayName: "r2");

            addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Mode);
            var u0 = menu.Unregistered.Order[0];
            var u1 = menu.Unregistered.Order[1];
            modifyModePropertiesUseCase.Handle(u0, displayName: "u0");
            modifyModePropertiesUseCase.Handle(u1, displayName: "u1");

            menuEditingSession.Save();
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            Assert.That(menu.Registered.Count, Is.EqualTo(3));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("r1"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("r2"));

            Assert.That(menu.Registered.GetGroupAt(0).Count, Is.EqualTo(3));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m2"));

            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m0"));

            Assert.That(menu.Unregistered.Count, Is.EqualTo(2));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("u0"));
            Assert.That(menu.Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("u1"));

            // Invalid item
            removeMenuItemUseCase.Handle("");
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.InvalidId));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            // Remove mode
            removeMenuItemUseCase.Handle(m1);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            Assert.That(!menu.ContainsMode(m1));
            Assert.That(!menu.ContainsGroup(m1));
            Assert.That(menu.Registered.GetGroupAt(0).Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m2"));
            menuEditingSession.Save();

            removeMenuItemUseCase.Handle(m1);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.InvalidId));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            removeMenuItemUseCase.Handle(m2);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            Assert.That(!menu.ContainsMode(m2));
            Assert.That(!menu.ContainsGroup(m2));
            Assert.That(menu.Registered.GetGroupAt(0).Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            menuEditingSession.Save();

            removeMenuItemUseCase.Handle(r1);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            Assert.That(!menu.ContainsMode(r1));
            Assert.That(!menu.ContainsGroup(r1));
            Assert.That(menu.Registered.Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("r2"));
            menuEditingSession.Save();

            removeMenuItemUseCase.Handle(r2);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            Assert.That(!menu.ContainsMode(r2));
            Assert.That(!menu.ContainsGroup(r2));
            Assert.That(menu.Registered.Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            menuEditingSession.Save();

            removeMenuItemUseCase.Handle(u0);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            Assert.That(!menu.ContainsMode(u0));
            Assert.That(!menu.ContainsGroup(u0));
            Assert.That(menu.Unregistered.Count, Is.EqualTo(1));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("u1"));
            menuEditingSession.Save();

            removeMenuItemUseCase.Handle(u1);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            Assert.That(!menu.ContainsMode(u1));
            Assert.That(!menu.ContainsGroup(u1));
            Assert.That(menu.Unregistered.Count, Is.EqualTo(0));
            menuEditingSession.Save();

            // Remove group recursively
            removeMenuItemUseCase.Handle(g0);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            Assert.That(!menu.ContainsMode(g0));
            Assert.That(!menu.ContainsMode(g1));
            Assert.That(!menu.ContainsMode(m0));
            Assert.That(!menu.ContainsGroup(g0));
            Assert.That(!menu.ContainsGroup(g1));
            Assert.That(!menu.ContainsGroup(m0));
            Assert.That(menu.Registered.Count, Is.EqualTo(0));
            menuEditingSession.Save();
        }
    }
}
