using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public class MockModifyBranchPropertiesPresenter : IModifyBranchPropertiesPresenter
    {
        public ModifyBranchPropertiesResult Result { get; private set; }

        void IModifyBranchPropertiesPresenter.Complete(ModifyBranchPropertiesResult modifyBranchPropertiesResult, in Menu menu, string errorMessage)
        {
            Result = modifyBranchPropertiesResult;
        }
    }

    public class ModifyBranchPropertiesUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            ModifyBranchPropertiesUseCase modifyBranchPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyBranchPropertiesUseCase>();
            MockModifyBranchPropertiesPresenter mockModifyBranchPropertiesPresenter = new MockModifyBranchPropertiesPresenter();
            modifyBranchPropertiesUseCase.SetPresenter(mockModifyBranchPropertiesPresenter);

            // null
            modifyBranchPropertiesUseCase.Handle(null, 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.ArgumentNull));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Menu is not opened
            modifyBranchPropertiesUseCase.Handle("", 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.MenuIsNotOpened));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle();
            menuEditingSession.SaveAs("dest");
            var menu = menuEditingSession.Menu;

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            menuEditingSession.Save();

            // Add branch
            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            addBranchUseCase.Handle(menu.Registered.Order[0]);
            addBranchUseCase.Handle(menu.Registered.Order[0]);
            addBranchUseCase.Handle(menu.Registered.Order[0]);
            menuEditingSession.Save();

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].IsRightTriggerUsed, Is.EqualTo(false));

            // Invalid branch
            modifyBranchPropertiesUseCase.Handle("", 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            modifyBranchPropertiesUseCase.Handle(Menu.RegisteredId, 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            modifyBranchPropertiesUseCase.Handle(Menu.UnregisteredId, 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            modifyBranchPropertiesUseCase.Handle(menu.Registered.Order[0], -1);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            modifyBranchPropertiesUseCase.Handle(menu.Registered.Order[0], 3);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Modify branch properties
            modifyBranchPropertiesUseCase.Handle(menu.Registered.Order[0], 1, eyeTrackingControl: EyeTrackingControl.Animation, isLeftTriggerUsed: true);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].IsRightTriggerUsed, Is.EqualTo(false));

            modifyBranchPropertiesUseCase.Handle(menu.Registered.Order[0], 2, mouthTrackingControl: MouthTrackingControl.Animation, isRightTriggerUsed: true);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();

            Assert.That(menu.Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(menu.Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(menu.Registered.GetModeAt(0).Branches[2].IsRightTriggerUsed, Is.EqualTo(true));
        }
    }
}
