using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation
{
    public class MockSetNewAnimationPresenter : ISetNewAnimationPresenter
    {
        public SetNewAnimationResult Result { get; private set; }

        public event System.Action<SetNewAnimationResult, IMenu, string> OnCompleted;

        void ISetNewAnimationPresenter.Complete(SetNewAnimationResult setNewAnimationResult, in IMenu menu, string errorMessage)
        {
            Result = setNewAnimationResult;
        }
    }

    public class SetNewAnimationUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            SetNewAnimationUseCase setNewAnimationUseCase = useCaseTestsInstaller.Container.Resolve<SetNewAnimationUseCase>();
            MockSetNewAnimationPresenter mockSetNewAnimationPresenter = useCaseTestsInstaller.Container.Resolve<ISetNewAnimationPresenter>() as MockSetNewAnimationPresenter;

            // null
            setNewAnimationUseCase.Handle(null, "", "");
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.ArgumentNull));
            setNewAnimationUseCase.Handle("", null, "");
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.ArgumentNull));
            setNewAnimationUseCase.Handle("", "", null);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.ArgumentNull));

            // Menu is not opened
            MockAnimationEditor mockAnimationEditor = new MockAnimationEditor();
            var animation = mockAnimationEditor.Create("");
            if (!UseCaseTestConstants.UseActualRepository)
            {
                setNewAnimationUseCase.Handle(menuId, "", "");
                Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.MenuDoesNotExist));
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

            // Invalid destination
            setNewAnimationUseCase.Handle(menuId, "", "");
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));

            setNewAnimationUseCase.Handle(menuId, "", Menu.RegisteredId);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));

            setNewAnimationUseCase.Handle(menuId, "", Menu.UnregisteredId);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));

            setNewAnimationUseCase.Handle(menuId, "", "", 0, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));

            setNewAnimationUseCase.Handle(menuId, "", Menu.RegisteredId, 0, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));

            setNewAnimationUseCase.Handle(menuId, "", Menu.UnregisteredId, 0, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));

            setNewAnimationUseCase.Handle(menuId, "", loadMenu().Registered.Order[0], -1, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));

            setNewAnimationUseCase.Handle(menuId, "", loadMenu().Registered.Order[0], 1, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));

            setNewAnimationUseCase.Handle(menuId, "", loadMenu().Registered.Order[0], 0, null);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));

            // Set animation
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle(menuId, "", loadMenu().Registered.Order[0]);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle(menuId, "", loadMenu().Registered.Order[0], 0, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle(menuId, "", loadMenu().Registered.Order[0], 0, BranchAnimationType.Left);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle(menuId, "", loadMenu().Registered.Order[0], 0, BranchAnimationType.Right);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle(menuId, "", loadMenu().Registered.Order[0], 0, BranchAnimationType.Both);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Not.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Not.Null);

            var ids = new List<string>();
            ids.Add(loadMenu().Registered.GetModeAt(0).Animation.GUID);
            ids.Add(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID);
            ids.Add(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation.GUID);
            ids.Add(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation.GUID);
            ids.Add(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation.GUID);
            CollectionAssert.AllItemsAreUnique(ids);
        }
    }
}
