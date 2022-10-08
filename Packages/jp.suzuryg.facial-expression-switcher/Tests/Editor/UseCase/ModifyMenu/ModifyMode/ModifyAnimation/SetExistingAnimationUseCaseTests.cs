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

        void ISetExistingAnimationPresenter.Complete(SetExistingAnimationResult setExistingAnimationResult, in Menu menu, string errorMessage)
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
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            SetExistingAnimationUseCase setExistingAnimationUseCase = useCaseTestsInstaller.Container.Resolve<SetExistingAnimationUseCase>();
            MockSetExistingAnimationPresenter mockSetExistingAnimationPresenter = new MockSetExistingAnimationPresenter();
            setExistingAnimationUseCase.SetPresenter(mockSetExistingAnimationPresenter);

            // null
            setExistingAnimationUseCase.Handle(null, null);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.ArgumentNull));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Menu is not opened
            MockAnimationEditor mockAnimationEditor = new MockAnimationEditor();
            var animation = mockAnimationEditor.Create("");
            setExistingAnimationUseCase.Handle(animation, ""); 
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.MenuIsNotOpened));
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
            menuEditingSession.Save();

            // Invalid destination
            setExistingAnimationUseCase.Handle(animation, "");
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setExistingAnimationUseCase.Handle(animation, Menu.RegisteredId);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setExistingAnimationUseCase.Handle(animation, Menu.UnregisteredId);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setExistingAnimationUseCase.Handle(animation, "", 0, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setExistingAnimationUseCase.Handle(animation, Menu.RegisteredId, 0, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setExistingAnimationUseCase.Handle(animation, Menu.UnregisteredId, 0, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setExistingAnimationUseCase.Handle(animation, menu.Registered.Order[0], -1, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setExistingAnimationUseCase.Handle(animation, menu.Registered.Order[0], 1, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setExistingAnimationUseCase.Handle(animation, menu.Registered.Order[0], 0, null);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Set animation
            Assert.That(menu.Registered.GetModeAt(0).Animation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

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

            setExistingAnimationUseCase.Handle(a0, menu.Registered.Order[0]);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setExistingAnimationUseCase.Handle(a1, menu.Registered.Order[0], 0, BranchAnimationType.Base);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID, Is.EqualTo(a1.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setExistingAnimationUseCase.Handle(a2, menu.Registered.Order[0], 0, BranchAnimationType.Left);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID, Is.EqualTo(a1.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation.GUID, Is.EqualTo(a2.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setExistingAnimationUseCase.Handle(a3, menu.Registered.Order[0], 0, BranchAnimationType.Right);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID, Is.EqualTo(a1.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation.GUID, Is.EqualTo(a2.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation.GUID, Is.EqualTo(a3.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setExistingAnimationUseCase.Handle(a4, menu.Registered.Order[0], 0, BranchAnimationType.Both);
            Assert.That(mockSetExistingAnimationPresenter.Result, Is.EqualTo(SetExistingAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation.GUID, Is.EqualTo(a0.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID, Is.EqualTo(a1.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation.GUID, Is.EqualTo(a2.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation.GUID, Is.EqualTo(a3.GUID));
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation.GUID, Is.EqualTo(a4.GUID));
        }
    }
}
