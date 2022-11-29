using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public class MockAddMenuItemPresenter : IAddMenuItemPresenter
    {
        public AddMenuItemResult Result { get; private set; }

        public System.IObservable<(AddMenuItemResult, IMenu, string)> Observable => throw new System.NotImplementedException();

        public void Complete(AddMenuItemResult addMenuItemResult, in IMenu menu, string errorMessage)
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

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            MockAddMenuItemPresenter mockAddMenuItemPresenter = useCaseTestsInstaller.Container.Resolve<IAddMenuItemPresenter>() as MockAddMenuItemPresenter;

            // null
            addMenuItemUseCase.Handle(null, "", AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.ArgumentNull));
            addMenuItemUseCase.Handle("", null, AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.ArgumentNull));

            // Menu does not exist
            if (!UseCaseTestConstants.UseActualRepository)
            {
                addMenuItemUseCase.Handle(menuId, "", AddMenuItemType.Mode);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.MenuDoesNotExist));
                addMenuItemUseCase.Handle(menuId, "", AddMenuItemType.Group);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.MenuDoesNotExist));
            }

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid destination
            addMenuItemUseCase.Handle(menuId, "", AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.InvalidMenuItemListId));
            addMenuItemUseCase.Handle(menuId, "", AddMenuItemType.Group);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.InvalidMenuItemListId));

            // Add menu item to registered
            for (int i = 0; i < 4; i++)
            {
                addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Group);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            }
            for (int i = 0; i < 4; i++)
            {
                addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            }
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.InvalidMenuItemListId));

            // Add menu item to unregistered
            for (int i = 0; i < 5; i++)
            {
                addMenuItemUseCase.Handle(menuId, Menu.UnregisteredId, AddMenuItemType.Group);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            }
            for (int i = 0; i < 4; i++)
            {
                addMenuItemUseCase.Handle(menuId, Menu.UnregisteredId, AddMenuItemType.Mode);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            }

            // Add menu item to group
            var groupId = loadMenu().Registered.Order[0];
            for (int i = 0; i < 3; i++)
            {
                addMenuItemUseCase.Handle(menuId, groupId, AddMenuItemType.Group);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            }
            for (int i = 0; i < 5; i++)
            {
                addMenuItemUseCase.Handle(menuId, groupId, AddMenuItemType.Mode);
                Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            }
            addMenuItemUseCase.Handle(menuId, groupId, AddMenuItemType.Mode);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.InvalidMenuItemListId));

            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[0]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[1]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[2]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[3]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[4]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[5]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[6]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[7]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Registered.GetGroup(loadMenu().Registered.Order[0]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Registered.GetGroup(loadMenu().Registered.Order[1]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Registered.GetGroup(loadMenu().Registered.Order[2]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Registered.GetGroup(loadMenu().Registered.Order[3]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[4]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[5]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[6]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[7]).DisplayName, Is.EqualTo("NewMode"));

            Assert.That(loadMenu().Unregistered.Order.Count, Is.EqualTo(9));
            Assert.That(loadMenu().Unregistered.GetType(loadMenu().Unregistered.Order[0]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Unregistered.GetType(loadMenu().Unregistered.Order[1]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Unregistered.GetType(loadMenu().Unregistered.Order[2]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Unregistered.GetType(loadMenu().Unregistered.Order[3]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Unregistered.GetType(loadMenu().Unregistered.Order[4]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Unregistered.GetType(loadMenu().Unregistered.Order[5]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Unregistered.GetType(loadMenu().Unregistered.Order[6]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Unregistered.GetType(loadMenu().Unregistered.Order[7]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Unregistered.GetType(loadMenu().Unregistered.Order[8]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Unregistered.GetGroup(loadMenu().Unregistered.Order[0]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Unregistered.GetGroup(loadMenu().Unregistered.Order[1]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Unregistered.GetGroup(loadMenu().Unregistered.Order[2]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Unregistered.GetGroup(loadMenu().Unregistered.Order[3]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Unregistered.GetGroup(loadMenu().Unregistered.Order[4]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Unregistered.GetMode(loadMenu().Unregistered.Order[5]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Unregistered.GetMode(loadMenu().Unregistered.Order[6]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Unregistered.GetMode(loadMenu().Unregistered.Order[7]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Unregistered.GetMode(loadMenu().Unregistered.Order[8]).DisplayName, Is.EqualTo("NewMode"));

            var group = loadMenu().Registered.GetGroup(loadMenu().Registered.Order[0]);
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
