using NUnit.Framework;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode
{
    public class MockChangeBranchOrderPresenter : IChangeBranchOrderPresenter
    {
        public ChangeBranchOrderResult Result { get; private set; }

        public System.IObservable<(ChangeBranchOrderResult, IMenu, string)> Observable => throw new System.NotImplementedException();

        void IChangeBranchOrderPresenter.Complete(ChangeBranchOrderResult changeBranchOrderResult, in IMenu menu, string errorMessage)
        {
            Result = changeBranchOrderResult;
        }
    }

    public class ChangeBranchOrderUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            ChangeBranchOrderUseCase changeBranchOrderUseCase = useCaseTestsInstaller.Container.Resolve<ChangeBranchOrderUseCase>();
            MockChangeBranchOrderPresenter mockChangeBranchOrderPresenter = useCaseTestsInstaller.Container.Resolve<IChangeBranchOrderPresenter>() as MockChangeBranchOrderPresenter;

            // null
            changeBranchOrderUseCase.Handle(null, "", 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.ArgumentNull));
            changeBranchOrderUseCase.Handle("", null, 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                changeBranchOrderUseCase.Handle(menuId, "", 0, 0);
                Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.MenuDoesNotExist));
            }

            // Create menu
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            createMenuUseCase.Handle(menuId);

            // Invalid branch
            changeBranchOrderUseCase.Handle(menuId, "", 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));

            changeBranchOrderUseCase.Handle(menuId, Menu.RegisteredId, 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));

            changeBranchOrderUseCase.Handle(menuId, Menu.UnregisteredId, 0, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));

            // Add mode
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);

            // Add branch
            AddBranchUseCase addBranchUseCase = useCaseTestsInstaller.Container.Resolve<AddBranchUseCase>();
            var c0 = new List<Condition>() { new Condition(Hand.Left, HandGesture.Fingerpoint, ComparisonOperator.Equals) };
            var c1 = new List<Condition>() { new Condition(Hand.Right, HandGesture.Fist, ComparisonOperator.NotEqual) };
            var c2 = new List<Condition>() { new Condition(Hand.Both, HandGesture.HandGun, ComparisonOperator.Equals) };
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], c0);
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], c1);
            addBranchUseCase.Handle(menuId, loadMenu().Registered.Order[0], c2);

            // Change branch order
            // Invalid
            changeBranchOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], -1, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));

            changeBranchOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 3, 0);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.InvalidBranch));

            // Success
            changeBranchOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 2, -1);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.Succeeded));
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions, c2);
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions, c0);
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[2].Conditions, c1);

            changeBranchOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, 9999);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.Succeeded));
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions, c2);
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions, c1);
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[2].Conditions, c0);

            changeBranchOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 1, 1);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.Succeeded));
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions, c2);
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions, c1);
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[2].Conditions, c0);

            changeBranchOrderUseCase.Handle(menuId, loadMenu().Registered.Order[0], 0, 1);
            Assert.That(mockChangeBranchOrderPresenter.Result, Is.EqualTo(ChangeBranchOrderResult.Succeeded));
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[0].Conditions, c1);
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[1].Conditions, c2);
            CollectionAssert.AreEqual(loadMenu().Registered.GetModeAt(0).Branches[2].Conditions, c0);
        }
    }
}
