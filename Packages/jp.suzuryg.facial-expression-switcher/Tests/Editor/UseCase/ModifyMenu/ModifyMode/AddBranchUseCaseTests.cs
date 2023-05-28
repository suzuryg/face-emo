using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode
{
    public class MockAddBranchPresenter : IAddBranchPresenter
    {
        public AddBranchResult Result { get; private set; }

        public System.IObservable<(AddBranchResult, IMenu, string)> Observable => throw new System.NotImplementedException();

        void IAddBranchPresenter.Complete(AddBranchResult addBranchResult, in IMenu menu, string errorMessage)
        {
            Result = addBranchResult;
        }
    }

    public class AddBranchUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            MockAddBranchPresenter mockAddBranchPresenter = useCaseTestsInstaller.Container.Resolve<IAddBranchPresenter>() as MockAddBranchPresenter;

            // null
            addBranchUseCase.Handle(null, "");
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.ArgumentNull));
            addBranchUseCase.Handle("", null);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                addBranchUseCase.Handle(menuId, "");
                Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.MenuDoesNotExist));
            }

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid destination
            addBranchUseCase.Handle(menuId, "");
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.InvalidDestination));

            addBranchUseCase.Handle(menuId, Menu.RegisteredId);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.InvalidDestination));

            addBranchUseCase.Handle(menuId, Menu.UnregisteredId);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.InvalidDestination));

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);

            // Add branch
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0]);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches.Count, Is.EqualTo(0));

            var conditions = new List<Condition>();
            conditions.Add(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals));
            conditions.Add(new Condition(Hand.Both, HandGesture.RockNRoll, ComparisonOperator.NotEqual));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], conditions);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals)));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.RockNRoll, ComparisonOperator.NotEqual)));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches.Count, Is.EqualTo(0));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1]);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions.Count, Is.EqualTo(0));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[0], Is.EqualTo(new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals)));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions[1], Is.EqualTo(new Condition(Hand.Both, HandGesture.RockNRoll, ComparisonOperator.NotEqual)));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetModeAt(1).Branches[0].Conditions.Count, Is.EqualTo(0));

            // Defaults provider
            createMenuUseCase.Handle(menuId);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode, displayName: null, defaultsProvider: null);
            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches.Count, Is.EqualTo(0));

            // No change
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], conditions: null, defaultsProvider: null);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));

            // EyeTrackingControl
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], conditions: null, defaultsProvider: new DefaultsProvider()
            {
                EyeTrackingControl = EyeTrackingControl.Animation,
            });
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));

            // MouthTrackingControl
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], conditions: null, defaultsProvider: new DefaultsProvider()
            {
                MouthTrackingControl = MouthTrackingControl.Animation,
            });
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].IsRightTriggerUsed, Is.EqualTo(false));

            // BlinkEnabled
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], conditions: null, defaultsProvider: new DefaultsProvider()
            {
                BlinkEnabled = false,
            });
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].IsRightTriggerUsed, Is.EqualTo(false));

            // MouthMorphCancelerEnabled
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], conditions: null, defaultsProvider: new DefaultsProvider()
            {
                MouthMorphCancelerEnabled = false,
            });
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].IsRightTriggerUsed, Is.EqualTo(false));

            // All
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], conditions: null, defaultsProvider: new DefaultsProvider()
            {
                ChangeDefaultFace = true,
                UseAnimationNameAsDisplayName = true,
                EyeTrackingControl = EyeTrackingControl.Animation,
                MouthTrackingControl = MouthTrackingControl.Animation,
                BlinkEnabled = false,
                MouthMorphCancelerEnabled = false,
            });
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].IsRightTriggerUsed, Is.EqualTo(false));
        }
    }
}
