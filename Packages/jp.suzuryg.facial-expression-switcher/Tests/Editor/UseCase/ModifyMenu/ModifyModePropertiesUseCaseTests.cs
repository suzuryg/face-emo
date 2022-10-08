using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public class MockModifyModePropertiesPresenter : IModifyModePropertiesPresenter
    {
        public ModifyModePropertiesResult Result { get; private set; }

        void IModifyModePropertiesPresenter.Complete(ModifyModePropertiesResult modifyModePropertiesResult, in Menu menu, string errorMessage)
        {
            Result = modifyModePropertiesResult;
        }
    }

    public class ModifyModePropertiesUseCaseTests
    {
        [Test]
        public void Test()
        {
            // Setup
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            ModifyModePropertiesUseCase modifyModePropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyModePropertiesUseCase>();
            MockModifyModePropertiesPresenter mockModifyModePropertiesPresenter = new MockModifyModePropertiesPresenter();
            modifyModePropertiesUseCase.SetPresenter(mockModifyModePropertiesPresenter);

            // null
            modifyModePropertiesUseCase.Handle(null);
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.ArgumentNull));

            // Menu is not opened
            modifyModePropertiesUseCase.Handle("");
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.MenuIsNotOpened));

            // Create Menu
            useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>().Handle();
            var menu = menuEditingSession.Menu;
            menuEditingSession.SaveAs("dest");

            // Mode is not created
            modifyModePropertiesUseCase.Handle("");
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.ModeIsNotContained));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            menuEditingSession.Save();
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            var mode0Id = menu.Registered.Order[0];
            var mode0 = menu.Registered.GetMode(mode0Id);
            Assert.That(mode0.DisplayName, Is.EqualTo("NewMode"));
            Assert.That(mode0.UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(mode0.EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(mode0.MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));

            var mode1Id = menu.Registered.Order[1];
            var mode1 = menu.Registered.GetMode(mode1Id);
            Assert.That(mode1.DisplayName, Is.EqualTo("NewMode"));
            Assert.That(mode1.UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(mode1.EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(mode1.MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));

            // Invalid mode
            modifyModePropertiesUseCase.Handle("");
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.ModeIsNotContained));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            // Change mode properties
            modifyModePropertiesUseCase.Handle(
                mode0Id,
                displayName: "Changed",
                useAnimationNameAsDisplayName: false,
                eyeTrackingControl: EyeTrackingControl.Animation,
                mouthTrackingControl: MouthTrackingControl.Animation);
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));

            Assert.That(mode0.DisplayName, Is.EqualTo("Changed"));
            Assert.That(mode0.UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(mode0.EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(mode0.MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));

            Assert.That(mode1.DisplayName, Is.EqualTo("NewMode"));
            Assert.That(mode1.UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(mode1.EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(mode1.MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
        }
    }
}
