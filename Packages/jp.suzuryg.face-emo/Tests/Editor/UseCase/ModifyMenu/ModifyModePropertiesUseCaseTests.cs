﻿using Suzuryg.FaceEmo.Domain;
using NUnit.Framework;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu
{
    public class MockModifyModePropertiesPresenter : IModifyModePropertiesPresenter
    {
        public ModifyModePropertiesResult Result { get; private set; }

        System.IObservable<(ModifyModePropertiesResult modifyModePropertiesResult, string modifiedModeId, IMenu menu, string errorMessage)> IModifyModePropertiesPresenter.Observable => throw new System.NotImplementedException();

        public void Complete(ModifyModePropertiesResult modifyModePropertiesResult, string modifiedModeId, in IMenu menu, string errorMessage = "")
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

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            ModifyModePropertiesUseCase modifyModePropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyModePropertiesUseCase>();
            MockModifyModePropertiesPresenter mockModifyModePropertiesPresenter = useCaseTestsInstaller.Container.Resolve<IModifyModePropertiesPresenter>() as MockModifyModePropertiesPresenter;

            // null
            modifyModePropertiesUseCase.Handle(null, "");
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.ArgumentNull));
            modifyModePropertiesUseCase.Handle("", null);
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                modifyModePropertiesUseCase.Handle(menuId, "");
                Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.MenuDoesNotExist));
            }

            // Create Menu
            useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>().Handle(menuId);

            // Mode is not created
            modifyModePropertiesUseCase.Handle(menuId, "");
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.ModeIsNotContained));

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);

            var mode0Id = loadMenu().Registered.Order[0];
            Assert.That(loadMenu().GetMode(mode0Id).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().GetMode(mode0Id).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().GetMode(mode0Id).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().GetMode(mode0Id).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().GetMode(mode0Id).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));

            var mode1Id = loadMenu().Registered.Order[1];
            Assert.That(loadMenu().GetMode(mode1Id).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().GetMode(mode1Id).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().GetMode(mode1Id).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().GetMode(mode1Id).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().GetMode(mode1Id).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));

            // Invalid mode
            modifyModePropertiesUseCase.Handle(menuId, "");
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.ModeIsNotContained));

            // Change mode properties
            modifyModePropertiesUseCase.Handle(menuId, 
                mode0Id,
                changeDefaultFace: true,
                displayName: "Changed",
                useAnimationNameAsDisplayName: true,
                eyeTrackingControl: EyeTrackingControl.Animation,
                mouthTrackingControl: MouthTrackingControl.Animation);
            Assert.That(mockModifyModePropertiesPresenter.Result, Is.EqualTo(ModifyModePropertiesResult.Succeeded));

            Assert.That(loadMenu().GetMode(mode0Id).ChangeDefaultFace, Is.EqualTo(true));
            Assert.That(loadMenu().GetMode(mode0Id).DisplayName, Is.EqualTo("Changed"));
            Assert.That(loadMenu().GetMode(mode0Id).UseAnimationNameAsDisplayName, Is.EqualTo(true));
            Assert.That(loadMenu().GetMode(mode0Id).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().GetMode(mode0Id).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));

            Assert.That(loadMenu().GetMode(mode1Id).ChangeDefaultFace, Is.EqualTo(false));
            Assert.That(loadMenu().GetMode(mode1Id).DisplayName, Is.EqualTo("NewMode"));
            Assert.That(loadMenu().GetMode(mode1Id).UseAnimationNameAsDisplayName, Is.EqualTo(false));
            Assert.That(loadMenu().GetMode(mode1Id).EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().GetMode(mode1Id).MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
        }
    }
}
