using Suzuryg.FaceEmo.Domain;
using NUnit.Framework;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu
{
    public class MockAddMenuItemPresenter : IAddMenuItemPresenter
    {
        public AddMenuItemResult Result { get; private set; }

        System.IObservable<(AddMenuItemResult addMenuItemResult, string addedItemId, IMenu menu, string errorMessage)> IAddMenuItemPresenter.Observable => throw new System.NotImplementedException();

        public void Complete(AddMenuItemResult addMenuItemResult, string addedItemId, in IMenu menu, string errorMessage = "")
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
            for (int i = 0; i < 3; i++)
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

            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(7));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[0]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[1]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[2]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[3]), Is.EqualTo(MenuItemType.Group));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[4]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[5]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Registered.GetType(loadMenu().Registered.Order[6]), Is.EqualTo(MenuItemType.Mode));
            Assert.That(loadMenu().Registered.GetGroup(loadMenu().Registered.Order[0]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Registered.GetGroup(loadMenu().Registered.Order[1]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Registered.GetGroup(loadMenu().Registered.Order[2]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Registered.GetGroup(loadMenu().Registered.Order[3]).DisplayName, Is.EqualTo("NewGroup"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[4]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[5]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[6]).DisplayName, Is.EqualTo("NewMode"));

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

            // Defaults provider
            createMenuUseCase.Handle(menuId);
            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(0));

            // No change
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode, displayName: null, defaultsProvider: null);
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).MouthMorphCancelerEnabled, Is.EqualTo(true));

            // ChangeDefaultFace
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode, displayName: null, defaultsProvider: new DefaultsProvider()
            {
                ChangeDefaultFace = true,
            });
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).MouthMorphCancelerEnabled, Is.EqualTo(true));

            // UseAnimationNameAsDisplayName
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode, displayName: null, defaultsProvider: new DefaultsProvider()
            {
                UseAnimationNameAsDisplayName = true,
            });
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).MouthMorphCancelerEnabled, Is.EqualTo(true));

            // EyeTrackingControl
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode, displayName: null, defaultsProvider: new DefaultsProvider()
            {
                EyeTrackingControl = EyeTrackingControl.Animation,
            });
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[3]).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[3]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[3]).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[3]).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[3]).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[3]).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[3]).MouthMorphCancelerEnabled, Is.EqualTo(true));

            // MouthTrackingControl
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode, displayName: null, defaultsProvider: new DefaultsProvider()
            {
                MouthTrackingControl = MouthTrackingControl.Animation,
            });
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[4]).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[4]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[4]).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[4]).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[4]).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[4]).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[4]).MouthMorphCancelerEnabled, Is.EqualTo(true));

            // BlinkEnabled
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode, displayName: null, defaultsProvider: new DefaultsProvider()
            {
                BlinkEnabled = false,
            });
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[5]).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[5]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[5]).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[5]).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[5]).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[5]).BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[5]).MouthMorphCancelerEnabled, Is.EqualTo(true));

            // MouthMorphCancelerEnabled
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode, displayName: null, defaultsProvider: new DefaultsProvider()
            {
                MouthMorphCancelerEnabled = false,
            });
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[6]).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[6]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[6]).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[6]).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[6]).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[6]).BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[6]).MouthMorphCancelerEnabled, Is.EqualTo(false));

            // All
            createMenuUseCase.Handle(menuId);
            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(0));

            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode, displayName: null, defaultsProvider: new DefaultsProvider()
            {
                ChangeDefaultFace = true,
                UseAnimationNameAsDisplayName = true,
                EyeTrackingControl = EyeTrackingControl.Animation,
                MouthTrackingControl = MouthTrackingControl.Animation,
                BlinkEnabled = false,
                MouthMorphCancelerEnabled = false,
            });
            Assert.That(mockAddMenuItemPresenter.Result, Is.EqualTo(AddMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).MouthMorphCancelerEnabled, Is.EqualTo(false));
        }
    }
}
