using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation
{
    public class MockSetExistingAnimationPresenter : ISetExistingAnimationPresenter
    {
        public SetExistingAnimationResult Result { get; private set; }

        public event System.Action<SetExistingAnimationResult, IMenu, string> OnCompleted;

        void ISetExistingAnimationPresenter.Complete(SetExistingAnimationResult setExistingAnimationResult, in IMenu menu, string errorMessage)
        {
            Result = setExistingAnimationResult;
        }
    }

    public class SetExistingAnimationUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestSetting.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            SetExistingAnimationUseCase setExistingAnimationUseCase = useCaseTestsInstaller.Container.Resolve<SetExistingAnimationUseCase>();
            MockSetExistingAnimationPresenter mockSetExistingAnimationPresenter = useCaseTestsInstaller.Container.Resolve<ISetExistingAnimationPresenter>() as MockSetExistingAnimationPresenter;

            // null
            setExistingAnimationUseCase.Handle(null, new MockAnimation(), "");
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.ArgumentNull));
            setExistingAnimationUseCase.Handle("", null, "");
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.ArgumentNull));
            setExistingAnimationUseCase.Handle("", new MockAnimation(), null);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.ArgumentNull));

            // Menu is not opened
            MockAnimationEditor mockAnimationEditor = new MockAnimationEditor();
            var animation = mockAnimationEditor.Create("");
            setExistingAnimationUseCase.Handle(menuId, animation, ""); 
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.MenuDoesNotExist));

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
            setExistingAnimationUseCase.Handle(menuId, animation, "");
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));

            setExistingAnimationUseCase.Handle(menuId, animation, Menu.RegisteredId);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));

            setExistingAnimationUseCase.Handle(menuId, animation, Menu.UnregisteredId);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));

            setExistingAnimationUseCase.Handle(menuId, animation, "", 0, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));

            setExistingAnimationUseCase.Handle(menuId, animation, Menu.RegisteredId, 0, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));

            setExistingAnimationUseCase.Handle(menuId, animation, Menu.UnregisteredId, 0, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));

            setExistingAnimationUseCase.Handle(menuId, animation, loadMenu().Registered.Order[0], -1, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));

            setExistingAnimationUseCase.Handle(menuId, animation, loadMenu().Registered.Order[0], 1, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));

            setExistingAnimationUseCase.Handle(menuId, animation, loadMenu().Registered.Order[0], 0, null);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));

            // Set animation
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            var a0 = mockAnimationEditor.Create("");
            var a1 = mockAnimationEditor.Create("");
            var a2 = mockAnimationEditor.Create("");
            var a3 = mockAnimationEditor.Create("");
            var a4 = mockAnimationEditor.Create("");
            var ids = new List<string>();
            ids.Add(a0.GUID);
            ids.Add(a1.GUID);
            ids.Add(a2.GUID);
            ids.Add(a3.GUID);
            ids.Add(a4.GUID);
            CollectionAssert.AllItemsAreUnique(ids);

            setExistingAnimationUseCase.Handle(menuId, a0, loadMenu().Registered.Order[0]);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setExistingAnimationUseCase.Handle(menuId, a1, loadMenu().Registered.Order[0], 0, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID, Is.EqualTo(a1.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setExistingAnimationUseCase.Handle(menuId, a2, loadMenu().Registered.Order[0], 0, BranchAnimationType.Left);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID, Is.EqualTo(a1.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation.GUID, Is.EqualTo(a2.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setExistingAnimationUseCase.Handle(menuId, a3, loadMenu().Registered.Order[0], 0, BranchAnimationType.Right);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID, Is.EqualTo(a1.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation.GUID, Is.EqualTo(a2.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation.GUID, Is.EqualTo(a3.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setExistingAnimationUseCase.Handle(menuId, a4, loadMenu().Registered.Order[0], 0, BranchAnimationType.Both);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(loadMenu().Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID, Is.EqualTo(a1.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].LeftHandAnimation.GUID, Is.EqualTo(a2.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].RightHandAnimation.GUID, Is.EqualTo(a3.GUID));
            Assert.That(loadMenu().Registered.GetModeAt(0).Branches[0].BothHandsAnimation.GUID, Is.EqualTo(a4.GUID));
        }
    }
}
