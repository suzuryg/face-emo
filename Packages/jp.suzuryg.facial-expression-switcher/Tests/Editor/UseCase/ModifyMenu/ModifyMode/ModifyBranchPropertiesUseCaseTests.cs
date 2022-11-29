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

        public event System.Action<ModifyBranchPropertiesResult, IMenu, string> OnCompleted;

        void IModifyBranchPropertiesPresenter.Complete(ModifyBranchPropertiesResult modifyBranchPropertiesResult, in IMenu menu, string errorMessage)
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

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            ModifyBranchPropertiesUseCase modifyBranchPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyBranchPropertiesUseCase>();
            MockModifyBranchPropertiesPresenter mockModifyBranchPropertiesPresenter = useCaseTestsInstaller.Container.Resolve<IModifyBranchPropertiesPresenter>() as MockModifyBranchPropertiesPresenter;

            // null
            modifyBranchPropertiesUseCase.Handle(null, "", 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.ArgumentNull));
            modifyBranchPropertiesUseCase.Handle("", null, 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                modifyBranchPropertiesUseCase.Handle(menuId, "", 0);
                Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.MenuDoesNotExist));
            }

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);

            // Add branch
            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsRightTriggerUsed, Is.EqualTo(false));

            // Invalid branch
            modifyBranchPropertiesUseCase.Handle(menuId, "", 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));

            modifyBranchPropertiesUseCase.Handle(menuId, Menu.RegisteredId, 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));

            modifyBranchPropertiesUseCase.Handle(menuId, Menu.UnregisteredId, 0);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));

            modifyBranchPropertiesUseCase.Handle(menuId, loadMenu().Registered.Order[0], -1);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));

            modifyBranchPropertiesUseCase.Handle(menuId, loadMenu().Registered.Order[0], 3);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.InvalidBranch));

            // Modify branch properties
            modifyBranchPropertiesUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, eyeTrackingControl: EyeTrackingControl.Animation, isLeftTriggerUsed: true);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.Succeeded));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsRightTriggerUsed, Is.EqualTo(false));

            modifyBranchPropertiesUseCase.Handle(menuId, loadMenu().Registered.Order[0], 2, mouthTrackingControl: MouthTrackingControl.Animation, isRightTriggerUsed: true);
            Assert.That(mockModifyBranchPropertiesPresenter.Result, Is.EqualTo(ModifyBranchPropertiesResult.Succeeded));

            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsLeftTriggerUsed, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[2].IsRightTriggerUsed, Is.EqualTo(true));
        }
    }
}
