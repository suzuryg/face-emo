using NUnit.Framework;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode
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

            // Specify order
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches.Count, Is.EqualTo(0));

            var neutral = new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals);
            var fist = new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals);
            var open = new Condition(Hand.Left, HandGesture.HandOpen, ComparisonOperator.Equals);
            var point = new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals);
            var victory = new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.Equals);
            var gun = new Condition(Hand.Left, HandGesture.HandGun, ComparisonOperator.Equals);
            var rock = new Condition(Hand.Left, HandGesture.RockNRoll, ComparisonOperator.Equals);
            var thumbs = new Condition(Hand.Left, HandGesture.ThumbsUp, ComparisonOperator.Equals);

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1], conditions: new[] { neutral }, order: 0);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(neutral));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1], conditions: new[] { fist }, order: 0);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(neutral));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1], conditions: new[] { open }, order: 0);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(neutral));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1], conditions: new[] { point }, order: 1);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[3].Conditions[0], Is.EqualTo(neutral));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1], conditions: new[] { victory }, order: 3);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[3].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[4].Conditions[0], Is.EqualTo(neutral));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1], conditions: new[] { gun }, order: 5);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[3].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[4].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[5].Conditions[0], Is.EqualTo(gun));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1], conditions: new[] { rock }, order: 99);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[3].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[4].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[5].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[6].Conditions[0], Is.EqualTo(rock));

            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[1], conditions: new[] { thumbs }, order: -99);
            Assert.That(mockAddBranchPresenter.Result, Is.EqualTo(AddBranchResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[3].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[4].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[5].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[6].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[7].Conditions[0], Is.EqualTo(rock));
        }
    }
}
