using NUnit.Framework;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.UseCase.ModifyMenu;
using System.Collections.Generic;
using System.Linq;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode
{
    public class MockAddMultipleBranchesPresenter : IAddMultipleBranchesPresenter
    {
        public AddMultipleBranchesResult Result { get; private set; }

        public System.IObservable<(AddMultipleBranchesResult, IMenu, string)> Observable => throw new System.NotImplementedException();

        void IAddMultipleBranchesPresenter.Complete(AddMultipleBranchesResult AddMultipleBranchesResult, in IMenu menu, string errorMessage)
        {
            Result = AddMultipleBranchesResult;
        }
    }

    public class AddMultipleBranchesUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            AddMultipleBranchesUseCase AddMultipleBranchesUseCase = useCaseTestsInstaller.Container.Resolve<AddMultipleBranchesUseCase>();
            MockAddMultipleBranchesPresenter mockAddMultipleBranchesPresenter = useCaseTestsInstaller.Container.Resolve<IAddMultipleBranchesPresenter>() as MockAddMultipleBranchesPresenter;

            // null
            AddMultipleBranchesUseCase.Handle("", null, null);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.ArgumentNull));
            AddMultipleBranchesUseCase.Handle(null, "", null);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.ArgumentNull));
            AddMultipleBranchesUseCase.Handle(null, null, new List<List<Condition>>());
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                AddMultipleBranchesUseCase.Handle(menuId, "", new List<List<Condition>>());
                Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.MenuDoesNotExist));
            }

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid destination
            AddMultipleBranchesUseCase.Handle(menuId, "", new List<List<Condition>>());
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.InvalidDestination));

            AddMultipleBranchesUseCase.Handle(menuId, Menu.RegisteredId, new List<List<Condition>>());
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.InvalidDestination));

            AddMultipleBranchesUseCase.Handle(menuId, Menu.UnregisteredId, new List<List<Condition>>());
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.InvalidDestination));

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);

            // Add branches
            var neutral = new Condition(Hand.Left, HandGesture.Neutral, ComparisonOperator.Equals);
            var fist = new Condition(Hand.Left, HandGesture.Fist, ComparisonOperator.Equals);
            var open = new Condition(Hand.Left, HandGesture.HandOpen, ComparisonOperator.Equals);
            var point = new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals);
            var victory = new Condition(Hand.Left, HandGesture.Victory, ComparisonOperator.Equals);
            var gun = new Condition(Hand.Left, HandGesture.HandGun, ComparisonOperator.Equals);
            var rock = new Condition(Hand.Left, HandGesture.RockNRoll, ComparisonOperator.Equals);
            var thumbs = new Condition(Hand.Left, HandGesture.ThumbsUp, ComparisonOperator.Equals);

            var branches = new[]
            {
                new[] { neutral },
                new[] { fist },
                new[] { open },
                new[] { point },
                new[] { victory },
                new[] { gun },
                new[] { rock },
                new[] { thumbs },
            };

            var reverse = new[]
            {
                new[] { thumbs },
                new[] { rock },
                new[] { gun },
                new[] { victory },
                new[] { point },
                new[] { open },
                new[] { fist },
                new[] { neutral },
            };

            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[0], branches);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[6].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[7].Conditions[0], Is.EqualTo(thumbs));

            // Specify order
            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[0], branches, order: 2);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[6].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[7].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[8].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[9].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[10].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[11].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[12].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[13].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[14].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[15].Conditions[0], Is.EqualTo(thumbs));

            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[0], branches, order: -99);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[6].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[7].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[8].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[9].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[10].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[11].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[12].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[13].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[14].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[15].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[16].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[17].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[18].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[19].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[20].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[21].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[22].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[23].Conditions[0], Is.EqualTo(thumbs));

            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[0], branches, order: 99);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[2].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[3].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[4].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[5].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[6].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[7].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[8].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[9].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[10].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[11].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[12].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[13].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[14].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[15].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[16].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[17].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[18].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[19].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[20].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[21].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[22].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[23].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[24].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[25].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[26].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[27].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[28].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[29].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[30].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[0]).Branches[31].Conditions[0], Is.EqualTo(thumbs));

            // Specify order (edge case)
            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[1], branches);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[3].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[4].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[5].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[6].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[7].Conditions[0], Is.EqualTo(thumbs));

            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[1], reverse, order: 7);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[3].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[4].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[5].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[6].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[7].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[8].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[9].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[10].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[11].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[12].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[13].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[14].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[15].Conditions[0], Is.EqualTo(thumbs));

            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[1], branches, order: 14);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[3].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[4].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[5].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[6].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[7].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[8].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[9].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[10].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[11].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[12].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[13].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[14].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[15].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[16].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[17].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[18].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[19].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[20].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[21].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[22].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[23].Conditions[0], Is.EqualTo(thumbs));

            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[1], branches, order: 24);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[0].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[1].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[2].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[3].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[4].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[5].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[6].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[7].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[8].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[9].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[10].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[11].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[12].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[13].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[14].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[15].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[16].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[17].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[18].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[19].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[20].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[21].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[22].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[23].Conditions[0], Is.EqualTo(thumbs));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[24].Conditions[0], Is.EqualTo(neutral));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[25].Conditions[0], Is.EqualTo(fist));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[26].Conditions[0], Is.EqualTo(open));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[27].Conditions[0], Is.EqualTo(point));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[28].Conditions[0], Is.EqualTo(victory));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[29].Conditions[0], Is.EqualTo(gun));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[30].Conditions[0], Is.EqualTo(rock));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[1]).Branches[31].Conditions[0], Is.EqualTo(thumbs));

            // Defaults provider
            branches = new[]
            {
                new[] { neutral },
            };

            // No change
            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[2], branches, defaultsProvider: null);
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[0].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[0].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[0].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[0].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[0].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[0].IsRightTriggerUsed, Is.EqualTo(false));

            // EyeTrackingControl
            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[2], branches, defaultsProvider: new DefaultsProvider()
            {
                EyeTrackingControl = EyeTrackingControl.Animation,
            });
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[1].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[1].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[1].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[1].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[1].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[1].IsRightTriggerUsed, Is.EqualTo(false));

            // MouthTrackingControl
            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[2], branches, defaultsProvider: new DefaultsProvider()
            {
                MouthTrackingControl = MouthTrackingControl.Animation,
            });
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[2].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[2].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[2].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[2].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[2].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[2].IsRightTriggerUsed, Is.EqualTo(false));

            // BlinkEnabled
            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[2], branches, defaultsProvider: new DefaultsProvider()
            {
                BlinkEnabled = false,
            });
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[3].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[3].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[3].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[3].MouthMorphCancelerEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[3].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[3].IsRightTriggerUsed, Is.EqualTo(false));

            // MouthMorphCancelerEnabled
            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[2], branches, defaultsProvider: new DefaultsProvider()
            {
                MouthMorphCancelerEnabled = false,
            });
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[4].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[4].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Tracking));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[4].BlinkEnabled, Is.EqualTo(true));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[4].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[4].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[4].IsRightTriggerUsed, Is.EqualTo(false));

            // All
            AddMultipleBranchesUseCase.Handle(menuId, loadMenu().Registered.Order[2], branches, defaultsProvider: new DefaultsProvider()
            {
                ChangeDefaultFace = true,
                UseAnimationNameAsDisplayName = true,
                EyeTrackingControl = EyeTrackingControl.Animation,
                MouthTrackingControl = MouthTrackingControl.Animation,
                BlinkEnabled = false,
                MouthMorphCancelerEnabled = false,
            });
            Assert.That(mockAddMultipleBranchesPresenter.Result, Is.EqualTo(AddMultipleBranchesResult.Succeeded));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[5].EyeTrackingControl, Is.EqualTo(EyeTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[5].MouthTrackingControl, Is.EqualTo(MouthTrackingControl.Animation));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[5].BlinkEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[5].MouthMorphCancelerEnabled, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[5].IsLeftTriggerUsed, Is.EqualTo(false));
            Assert.That(loadMenu().Registered.GetMode(loadMenu().Registered.Order[2]).Branches[5].IsRightTriggerUsed, Is.EqualTo(false));
        }
    }
}
