using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public class MockAddMenuItemPresenter : IAddMenuItemPresenter
    {
        public AddMenuItemResult Result { get; private set; }

        public void Complete(AddMenuItemResult addMenuItemResult, in Menu menu, string errorMessage)
        {
            Result = addMenuItemResult;
        }
    }

    public class AddMenuItemUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            MockAddMenuItemPresenter mockAddMenuItemPresenter = new MockAddMenuItemPresenter();
            addMenuItemUseCase.SetPresenter(mockAddMenuItemPresenter);

            // null
            addMenuItemUseCase.Handle(null, AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.ArgumentNull));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Menu is not opened
            addMenuItemUseCase.Handle("", AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.MenuIsNotOpened));
            Assert.That(menuEditingSession.IsModified, Is.False);
            addMenuItemUseCase.Handle("", AddMenuItemType.Group);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.MenuIsNotOpened));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle();
            menuEditingSession.SaveAs("dest");
            var menu = menuEditingSession.Menu;

            // Invalid destination
            addMenuItemUseCase.Handle("", AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);
            addMenuItemUseCase.Handle("", AddMenuItemType.Group);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Add menu item to registered
            for (int i = 0; i < 4; i++)
            {
                addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Group);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
                Assert.That(menuEditingSession.IsModified, Is.True);
                menuEditingSession.Save();
            }
            for (int i = 0; i < 4; i++)
            {
                addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
                Assert.That(menuEditingSession.IsModified, Is.True);
                menuEditingSession.Save();
            }
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Add menu item to unregistered
            for (int i = 0; i < 5; i++)
            {
                addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Group);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
                Assert.That(menuEditingSession.IsModified, Is.True);
                menuEditingSession.Save();
            }
            for (int i = 0; i < 4; i++)
            {
                addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Mode);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
                Assert.That(menuEditingSession.IsModified, Is.True);
                menuEditingSession.Save();
            }

            // Add menu item to group
            var groupId = menu.Registered.Order[0];
            for (int i = 0; i < 3; i++)
            {
                addMenuItemUseCase.Handle(groupId, AddMenuItemType.Group);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
                Assert.That(menuEditingSession.IsModified, Is.True);
                menuEditingSession.Save();
            }
            for (int i = 0; i < 5; i++)
            {
                addMenuItemUseCase.Handle(groupId, AddMenuItemType.Mode);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
                Assert.That(menuEditingSession.IsModified, Is.True);
                menuEditingSession.Save();
            }
            addMenuItemUseCase.Handle(groupId, AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            Assert.That(menu.Registered.Order.Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetType(menu.Registered.Order[0]), Is.EqualTo(MenuItemType.Group));
            Assert.That(menu.Registered.GetType(menu.Registered.Order[1]), Is.EqualTo(MenuItemType.Group));
            Assert.That(menu.Registered.GetType(menu.Registered.Order[2]), Is.EqualTo(MenuItemType.Group));
            Assert.That(menu.Registered.GetType(menu.Registered.Order[3]), Is.EqualTo(MenuItemType.Group));
            Assert.That(menu.Registered.GetType(menu.Registered.Order[4]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(menu.Registered.GetType(menu.Registered.Order[5]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(menu.Registered.GetType(menu.Registered.Order[6]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(menu.Registered.GetType(menu.Registered.Order[7]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(menu.Registered.GetGroup(menu.Registered.Order[0]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(menu.Registered.GetGroup(menu.Registered.Order[1]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(menu.Registered.GetGroup(menu.Registered.Order[2]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(menu.Registered.GetGroup(menu.Registered.Order[3]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(menu.Registered.GetMode(menu.Registered.Order[4]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(menu.Registered.GetMode(menu.Registered.Order[5]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(menu.Registered.GetMode(menu.Registered.Order[6]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(menu.Registered.GetMode(menu.Registered.Order[7]).DisplayName, Is.EqualTo("NewMode"));

            Assert.That(menu.Unregistered.Order.Count, Is.EqualTo(9));
            Assert.That(menu.Unregistered.GetType(menu.Unregistered.Order[0]), Is.EqualTo(MenuItemType.Group));
            Assert.That(menu.Unregistered.GetType(menu.Unregistered.Order[1]), Is.EqualTo(MenuItemType.Group));
            Assert.That(menu.Unregistered.GetType(menu.Unregistered.Order[2]), Is.EqualTo(MenuItemType.Group));
            Assert.That(menu.Unregistered.GetType(menu.Unregistered.Order[3]), Is.EqualTo(MenuItemType.Group));
            Assert.That(menu.Unregistered.GetType(menu.Unregistered.Order[4]), Is.EqualTo(MenuItemType.Group));
            Assert.That(menu.Unregistered.GetType(menu.Unregistered.Order[5]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(menu.Unregistered.GetType(menu.Unregistered.Order[6]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(menu.Unregistered.GetType(menu.Unregistered.Order[7]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(menu.Unregistered.GetType(menu.Unregistered.Order[8]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(menu.Unregistered.GetGroup(menu.Unregistered.Order[0]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(menu.Unregistered.GetGroup(menu.Unregistered.Order[1]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(menu.Unregistered.GetGroup(menu.Unregistered.Order[2]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(menu.Unregistered.GetGroup(menu.Unregistered.Order[3]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(menu.Unregistered.GetGroup(menu.Unregistered.Order[4]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(menu.Unregistered.GetMode(menu.Unregistered.Order[5]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(menu.Unregistered.GetMode(menu.Unregistered.Order[6]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(menu.Unregistered.GetMode(menu.Unregistered.Order[7]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(menu.Unregistered.GetMode(menu.Unregistered.Order[8]).DisplayName, Is.EqualTo("NewMode"));

            var group = menu.Registered.GetGroup(menu.Registered.Order[0]);
            Assert.That(group.Order.Count, Is.EqualTo(8));
            Assert.That(group.GetType(group.Order[0]), Is.EqualTo(MenuItemType.Group));
            Assert.That(group.GetType(group.Order[1]), Is.EqualTo(MenuItemType.Group));
            Assert.That(group.GetType(group.Order[2]), Is.EqualTo(MenuItemType.Group));
            Assert.That(group.GetType(group.Order[3]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(group.GetType(group.Order[4]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(group.GetType(group.Order[5]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(group.GetType(group.Order[6]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(group.GetType(group.Order[7]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(group.GetGroup(group.Order[0]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(group.GetGroup(group.Order[1]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(group.GetGroup(group.Order[2]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(group.GetMode(group.Order[3]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(group.GetMode(group.Order[4]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(group.GetMode(group.Order[5]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(group.GetMode(group.Order[6]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(group.GetMode(group.Order[7]).DisplayName, Is.EqualTo("NewMode"));
        }
    }
}
