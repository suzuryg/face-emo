using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public class MockModifyGroupPropertiesPresenter : IModifyGroupPropertiesPresenter
    {
        public ModifyGroupPropertiesResult Result { get; private set; }

        void IModifyGroupPropertiesPresenter.Complete(ModifyGroupPropertiesResult modifyGroupPropertiesResult, in Menu menu, string errorMessage)
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
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            ModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyGroupPropertiesUseCase>();
            MockModifyGroupPropertiesPresenter mockModifyGroupPropertiesPresenter = new MockModifyGroupPropertiesPresenter();
            modifyGroupPropertiesUseCase.SetPresenter(mockModifyGroupPropertiesPresenter);

            // null
            modifyGroupPropertiesUseCase.Handle(null);
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.ArgumentNull));

            // Menu is not opened
            modifyGroupPropertiesUseCase.Handle("");
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.MenuIsNotOpened));

            // Create Menu
            useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>().Handle();
            var menu = menuEditingSession.Menu;
            menuEditingSession.SaveAs("dest");

            // Group is not created
            modifyGroupPropertiesUseCase.Handle("");
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.GroupIsNotContained));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            // Add group
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Group);
            menuEditingSession.Save();
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            var group0Id = menu.Registered.Order[0];
            var group0 = menu.Registered.GetGroup(group0Id);
            Assert.That(group0.DisplayName, Is.EqualTo("NewGroup"));

            var group1Id = menu.Registered.Order[1];
            var group1 = menu.Registered.GetGroup(group1Id);
            Assert.That(group1.DisplayName, Is.EqualTo("NewGroup"));

            // Invalid group
            modifyGroupPropertiesUseCase.Handle("");
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.GroupIsNotContained));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            // Change gruop properties
            modifyGroupPropertiesUseCase.Handle(
                group0Id,
                displayName: "Changed");
            Assert.That(mockModifyGroupPropertiesPresenter.Result, Is.EqualTo(ModifyGroupPropertiesResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));

            Assert.That(group0.DisplayName, Is.EqualTo("Changed"));

            Assert.That(group1.DisplayName, Is.EqualTo("NewGroup"));
        }
    }
}
