﻿using Suzuryg.FaceEmo.Domain;
using NUnit.Framework;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu
{
    public class MockModifyGroupPropertiesPresenter : IModifyGroupPropertiesPresenter
    {
        public ModifyGroupPropertiesResult Result { get; private set; }

        public System.IObservable<(ModifyGroupPropertiesResult modifyGroupPropertiesResult, string modifiedGroupId, IMenu menu, string errorMessage)> Observable => throw new System.NotImplementedException();

        public void Complete(ModifyGroupPropertiesResult modifyGroupPropertiesResult, string modifiedGroupId, in IMenu menu, string errorMessage = "")
        {
            Result = modifyGroupPropertiesResult;
        }
    }

    public class ModifyGroupPropertiesUseCaseTests
    {
        [Test]
        public void Test()
        {
            // Setup
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            ModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyGroupPropertiesUseCase>();
            MockModifyGroupPropertiesPresenter mockModifyGroupPropertiesPresenter = useCaseTestsInstaller.Container.Resolve<IModifyGroupPropertiesPresenter>() as MockModifyGroupPropertiesPresenter;

            // null
            modifyGroupPropertiesUseCase.Handle(null, "");
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.ArgumentNull));
            modifyGroupPropertiesUseCase.Handle("", null);
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                modifyGroupPropertiesUseCase.Handle(menuId, "");
                Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.MenuDoesNotExist));
            }

            // Create Menu
            useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>().Handle(menuId);

            // Group is not created
            modifyGroupPropertiesUseCase.Handle(menuId, "");
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.GroupIsNotContained));

            // Add group
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Group);

            var group0Id = loadMenu().Registered.Order[0];
            Assert.That(loadMenu().GetGroup(group0Id).DisplayName, Is.EqualTo("NewGroup"));

            var group1Id = loadMenu().Registered.Order[1];
            Assert.That(loadMenu().GetGroup(group1Id).DisplayName, Is.EqualTo("NewGroup"));

            // Invalid group
            modifyGroupPropertiesUseCase.Handle(menuId, "");
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.GroupIsNotContained));

            // Change gruop properties
            modifyGroupPropertiesUseCase.Handle(menuId, 
                group0Id,
                displayName: "Changed");
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.Succeeded));

            Assert.That(loadMenu().GetGroup(group0Id).DisplayName, Is.EqualTo("Changed"));

            Assert.That(loadMenu().GetGroup(group1Id).DisplayName, Is.EqualTo("NewGroup"));
        }
    }
}
