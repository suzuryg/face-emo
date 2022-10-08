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

        void ISetNewAnimationPresenter.Complete(SetNewAnimationResult setNewAnimationResult, in Menu menu, string errorMessage)
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
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            SetNewAnimationUseCase setNewAnimationUseCase = useCaseTestsInstaller.Container.Resolve<SetNewAnimationUseCase>();
            MockSetNewAnimationPresenter mockSetNewAnimationPresenter = new MockSetNewAnimationPresenter();
            setNewAnimationUseCase.SetPresenter(mockSetNewAnimationPresenter);

            // null
            setNewAnimationUseCase.Handle(null, null);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.ArgumentNull));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Menu is not opened
            MockAnimationEditor mockAnimationEditor = new MockAnimationEditor();
            var animation = mockAnimationEditor.Create("");
            setNewAnimationUseCase.Handle("", ""); 
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.MenuIsNotOpened));
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
            setNewAnimationUseCase.Handle("", "");
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setNewAnimationUseCase.Handle("", Menu.RegisteredId);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setNewAnimationUseCase.Handle("", Menu.UnregisteredId);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setNewAnimationUseCase.Handle("", "", 0, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setNewAnimationUseCase.Handle("", Menu.RegisteredId, 0, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setNewAnimationUseCase.Handle("", Menu.UnregisteredId, 0, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setNewAnimationUseCase.Handle("", menu.Registered.Order[0], -1, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setNewAnimationUseCase.Handle("", menu.Registered.Order[0], 1, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            setNewAnimationUseCase.Handle("", menu.Registered.Order[0], 0, null);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Set animation
            Assert.That(menu.Registered.GetModeAt(0).Animation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle("", menu.Registered.Order[0]);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle("", menu.Registered.Order[0], 0, BranchAnimationType.Base);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle("", menu.Registered.Order[0], 0, BranchAnimationType.Left);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle("", menu.Registered.Order[0], 0, BranchAnimationType.Right);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Null);

            setNewAnimationUseCase.Handle("", menu.Registered.Order[0], 0, BranchAnimationType.Both);
            Assert.That(mockSetNewAnimationPresenter.Result, Is.EqualTo(SetNewAnimationResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.True);
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetModeAt(0).Animation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation, Is.Not.Null);
            Assert.That(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation, Is.Not.Null);

            var ids = new List<string>();
            ids.Add(menu.Registered.GetModeAt(0).Animation.GUID);
            ids.Add(menu.Registered.GetModeAt(0).Branches[0].BaseAnimation.GUID);
            ids.Add(menu.Registered.GetModeAt(0).Branches[0].LeftHandAnimation.GUID);
            ids.Add(menu.Registered.GetModeAt(0).Branches[0].RightHandAnimation.GUID);
            ids.Add(menu.Registered.GetModeAt(0).Branches[0].BothHandsAnimation.GUID);
            CollectionAssert.AllItemsAreUnique(ids);
        }
    }
}
