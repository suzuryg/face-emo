using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public class MockRemoveMenuItemPresenter : IRemoveMenuItemPresenter
    {
        public RemoveMenuItemResult Result { get; private set; }

        public event System.Action<RemoveMenuItemResult, IMenu, string> OnCompleted;

        void IRemoveMenuItemPresenter.Complete(RemoveMenuItemResult removeMenuItemResult, in IMenu menu, string errorMessage)
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

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            RemoveMenuItemUseCase removeMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<RemoveMenuItemUseCase>();
            MockRemoveMenuItemPresenter mockRemoveMenuItemPresenter = useCaseTestsInstaller.Container.Resolve<IRemoveMenuItemPresenter>() as MockRemoveMenuItemPresenter;

            // null
            removeMenuItemUseCase.Handle("", null);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.ArgumentNull));
            removeMenuItemUseCase.Handle(null, "");
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                removeMenuItemUseCase.Handle(menuId, "");
                Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.MenuDoesNotExist));
            }

            // Create Menu
            useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>().Handle(menuId);

            // Item is not created
            removeMenuItemUseCase.Handle(menuId, "");
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.InvalidId));

            // Add item
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            ModifyModePropertiesUseCase modifyModePropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyModePropertiesUseCase>();
            ModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyGroupPropertiesUseCase>();

            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Group);
            var g0 = loadMenu().Registered.Order[0];
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Group);
            var g1 = loadMenu().GetGroup(g0).Order[0];
            addMenuItemUseCase.Handle(menuId, g1, AddMenuItemType.Mode);
            var m0 = loadMenu().GetGroup(g1).Order[0];
            modifyGroupPropertiesUseCase.Handle(menuId, g0, displayName: "g0");
            modifyGroupPropertiesUseCase.Handle(menuId, g1, displayName: "g1");
            modifyModePropertiesUseCase.Handle(menuId, m0, displayName: "m0");

            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            var m1 = loadMenu().GetGroup(g0).Order[1];
            var m2 = loadMenu().GetGroup(g0).Order[2];
            modifyModePropertiesUseCase.Handle(menuId, m1, displayName: "m1");
            modifyModePropertiesUseCase.Handle(menuId, m2, displayName: "m2");

            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            var r1 = loadMenu().Registered.Order[1];
            var r2 = loadMenu().Registered.Order[2];
            modifyModePropertiesUseCase.Handle(menuId, r1, displayName: "r1");
            modifyModePropertiesUseCase.Handle(menuId, r2, displayName: "r2");

            addMenuItemUseCase.Handle(menuId, Menu.UnregisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.UnregisteredId, AddMenuItemType.Mode);
            var u0 = loadMenu().Unregistered.Order[0];
            var u1 = loadMenu().Unregistered.Order[1];
            modifyModePropertiesUseCase.Handle(menuId, u0, displayName: "u0");
            modifyModePropertiesUseCase.Handle(menuId, u1, displayName: "u1");

            Assert.That(loadMenu().Registered.Count, Is.EqualTo(3));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("r1"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("r2"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).Count, Is.EqualTo(3));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m2"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m0"));

            Assert.That(loadMenu().Unregistered.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("u0"));
            Assert.That(loadMenu().Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("u1"));

            // Invalid item
            removeMenuItemUseCase.Handle(menuId, "");
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.InvalidId));

            // Remove mode
            removeMenuItemUseCase.Handle(menuId, m1);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(!loadMenu().ContainsMode(m1));
            Assert.That(!loadMenu().ContainsGroup(m1));
            Assert.That(loadMenu().Registered.GetGroupAt(0).Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m2"));

            removeMenuItemUseCase.Handle(menuId, m1);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.InvalidId));

            removeMenuItemUseCase.Handle(menuId, m2);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(!loadMenu().ContainsMode(m2));
            Assert.That(!loadMenu().ContainsGroup(m2));
            Assert.That(loadMenu().Registered.GetGroupAt(0).Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));

            removeMenuItemUseCase.Handle(menuId, r1);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(!loadMenu().ContainsMode(r1));
            Assert.That(!loadMenu().ContainsGroup(r1));
            Assert.That(loadMenu().Registered.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("r2"));

            removeMenuItemUseCase.Handle(menuId, r2);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(!loadMenu().ContainsMode(r2));
            Assert.That(!loadMenu().ContainsGroup(r2));
            Assert.That(loadMenu().Registered.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));

            removeMenuItemUseCase.Handle(menuId, u0);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(!loadMenu().ContainsMode(u0));
            Assert.That(!loadMenu().ContainsGroup(u0));
            Assert.That(loadMenu().Unregistered.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("u1"));

            removeMenuItemUseCase.Handle(menuId, u1);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(!loadMenu().ContainsMode(u1));
            Assert.That(!loadMenu().ContainsGroup(u1));
            Assert.That(loadMenu().Unregistered.Count, Is.EqualTo(0));

            // Remove group recursively
            removeMenuItemUseCase.Handle(menuId, g0);
            Assert.That(mockRemoveMenuItemPresenter.Result, Is.EqualTo(RemoveMenuItemResult.Succeeded));
            Assert.That(!loadMenu().ContainsMode(g0));
            Assert.That(!loadMenu().ContainsMode(g1));
            Assert.That(!loadMenu().ContainsMode(m0));
            Assert.That(!loadMenu().ContainsGroup(g0));
            Assert.That(!loadMenu().ContainsGroup(g1));
            Assert.That(!loadMenu().ContainsGroup(m0));
            Assert.That(loadMenu().Registered.Count, Is.EqualTo(0));
        }
    }
}
