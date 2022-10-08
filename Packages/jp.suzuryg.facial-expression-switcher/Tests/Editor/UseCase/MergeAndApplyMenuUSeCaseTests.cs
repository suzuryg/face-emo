using NUnit.Framework;
using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public class MockMergeExistingMenuPresenter : IMergeExistingMenuPresenter
    {
        public MergeExistingMenuResult Result { get; private set; }

        public MergedMenuItemList Merged;

        public void Complete(MergeExistingMenuResult mergeExistingMenuItemResult, MergedMenuItemList mergedMenuItems, in Menu menu, string errorMessage)
        {
            Result = mergeExistingMenuItemResult;
            Merged = mergedMenuItems;
        }
    }

    public class MockApplyMenuPresenter : IApplyMenuPresenter
    {
        public ApplyMenuResult Result { get; private set; }

        public void Complete(ApplyMenuResult applyMenuResult, in Menu menu, string errorMessage = "")
        {
            Result = applyMenuResult;
        }
    }

    public class MockExistingMenuItem : IExistingMenuItem
    {
        public string DisplayName { get; set; }
        public MockExistingMenuItem(string displayName)
        {
            DisplayName = displayName;
        }
    }

    public class MockMenuApplier : IMenuApplier
    {
        public void Apply(MergedMenuItemList mergedMenuItems, Menu menu)
        {
            // NOP
        }
    }

    public class MergeAndApplyMenuUseCaseTests
    {
        [Test]
        public void Test()
        {
            // Setup
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            MergeExistingMenuUseCase mergeExistingMenuUseCase = useCaseTestsInstaller.Container.Resolve<MergeExistingMenuUseCase>();
            ApplyMenuUseCase applyMenuUseCase = useCaseTestsInstaller.Container.Resolve<ApplyMenuUseCase>();
            var existingMenuItems = new List<IExistingMenuItem>();
            MergedMenuItemList mergedMenuItems;

            MockMergeExistingMenuPresenter mockMergeExistingMenuPresenter = new MockMergeExistingMenuPresenter();
            mergeExistingMenuUseCase.SetPresenter(mockMergeExistingMenuPresenter);
            MockApplyMenuPresenter mockApplyMenuPresenter = new MockApplyMenuPresenter();
            applyMenuUseCase.SetPresenter(mockApplyMenuPresenter);

            // null
            mergeExistingMenuUseCase.Handle(null);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.ArgumentNull));
            applyMenuUseCase.Handle(null);
            Assert.That(mockApplyMenuPresenter.Result, Is.EqualTo(ApplyMenuResult.ArgumentNull));

            // Menu is not opened
            mergeExistingMenuUseCase.Handle(new List<IExistingMenuItem>());
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.MenuIsNotOpened));
            applyMenuUseCase.Handle(new MergedMenuItemList());
            Assert.That(mockApplyMenuPresenter.Result, Is.EqualTo(ApplyMenuResult.MenuIsNotOpened));

            // Create Menu
            useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>().Handle();
            var menu = menuEditingSession.Menu;
            menuEditingSession.SaveAs("dest");

            // Add item
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            ModifyModePropertiesUseCase modifyModePropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyModePropertiesUseCase>();
            ModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyGroupPropertiesUseCase>();
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            var g0 = menu.Registered.Order[0];
            var g1 = menu.Registered.Order[1];
            var m0 = menu.Registered.Order[2];
            var m1 = menu.Registered.Order[3];
            modifyGroupPropertiesUseCase.Handle(g0, displayName: "g0");
            modifyGroupPropertiesUseCase.Handle(g1, displayName: "g1");
            modifyModePropertiesUseCase.Handle(m0, displayName:  "m0");
            modifyModePropertiesUseCase.Handle(m1, displayName:  "m1");

            addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Mode);
            var m2 = menu.Unregistered.Order[0];
            var m3 = menu.Unregistered.Order[1];
            var m4 = menu.Unregistered.Order[2];
            var m5 = menu.Unregistered.Order[3];
            modifyModePropertiesUseCase.Handle(m2, displayName: "m2");
            modifyModePropertiesUseCase.Handle(m3, displayName: "m3");
            modifyModePropertiesUseCase.Handle(m4, displayName: "m4");
            modifyModePropertiesUseCase.Handle(m5, displayName: "m5");

            menuEditingSession.Save();

            // Apply invalid menu item
            mergedMenuItems = new MergedMenuItemList();
            mergedMenuItems.Add(new MockExistingMenuItem("e0"));
            mergedMenuItems.Add(menu.GetMode(m1), m1);
            mergedMenuItems.Add(new MockExistingMenuItem("e1"));
            mergedMenuItems.Add(menu.GetGroup(g1), g1);
            mergedMenuItems.Add(new MockExistingMenuItem("e2"));
            mergedMenuItems.Add(menu.GetGroup(g0), g0);
            mergedMenuItems.Add(new Mode(""), "");
            mergedMenuItems.Add(new MockExistingMenuItem("e3"));
            //applyMenuUseCase.Handle(mergedMenuItems);
            //Assert.That(MockApplyMenuPresenter.Result, Is.EqualTo(ApplyMenuResult.MenuItemsAreNotContained));
            //Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            //menuEditingSession.Save();

            mergedMenuItems = new MergedMenuItemList();
            mergedMenuItems.Add(new MockExistingMenuItem("e0"));
            mergedMenuItems.Add(menu.GetMode(m1), m1);
            mergedMenuItems.Add(new MockExistingMenuItem("e1"));
            mergedMenuItems.Add(new Group(""), "");
            mergedMenuItems.Add(new MockExistingMenuItem("e2"));
            mergedMenuItems.Add(menu.GetGroup(g0), g0);
            mergedMenuItems.Add(menu.GetMode(m0), m0);
            mergedMenuItems.Add(new MockExistingMenuItem("e3"));
            //applyMenuUseCase.Handle(mergedMenuItems);
            //Assert.That(MockApplyMenuPresenter.Result, Is.EqualTo(ApplyMenuResult.MenuItemsAreNotContained));
            //Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            //menuEditingSession.Save();

            // Too many existing menu items
            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem(""));
            existingMenuItems.Add(new MockExistingMenuItem(""));
            existingMenuItems.Add(new MockExistingMenuItem(""));
            existingMenuItems.Add(new MockExistingMenuItem(""));
            existingMenuItems.Add(new MockExistingMenuItem(""));
            existingMenuItems.Add(new MockExistingMenuItem(""));
            existingMenuItems.Add(new MockExistingMenuItem(""));
            existingMenuItems.Add(new MockExistingMenuItem(""));
            existingMenuItems.Add(new MockExistingMenuItem(""));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.InvalidArgument));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            // Too many merged menu items
            mergedMenuItems = new MergedMenuItemList();
            mergedMenuItems.Add(new MockExistingMenuItem(""));
            mergedMenuItems.Add(new MockExistingMenuItem(""));
            mergedMenuItems.Add(new MockExistingMenuItem(""));
            mergedMenuItems.Add(new MockExistingMenuItem(""));
            mergedMenuItems.Add(new MockExistingMenuItem(""));
            mergedMenuItems.Add(new MockExistingMenuItem(""));
            mergedMenuItems.Add(new MockExistingMenuItem(""));
            mergedMenuItems.Add(new MockExistingMenuItem(""));
            mergedMenuItems.Add(new MockExistingMenuItem(""));
            applyMenuUseCase.Handle(mergedMenuItems);
            Assert.That(mockApplyMenuPresenter.Result, Is.EqualTo(ApplyMenuResult.InvalidArgument));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            // Merge before applying
            existingMenuItems.Clear();
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(4));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(1).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(2).DisplayName,  Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(3).DisplayName,  Is.EqualTo("m1"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e2"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            existingMenuItems.Add(new MockExistingMenuItem("e4"));
            existingMenuItems.Add(new MockExistingMenuItem("e5"));
            existingMenuItems.Add(new MockExistingMenuItem("e6"));
            existingMenuItems.Add(new MockExistingMenuItem("e7"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(12));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(1).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(3).DisplayName, Is.EqualTo("e3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e4"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(5).DisplayName, Is.EqualTo("e5"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(6).DisplayName, Is.EqualTo("e6"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(7).DisplayName, Is.EqualTo("e7"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(8).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(9).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(10).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(11).DisplayName, Is.EqualTo("m1"));

            // Apply (re-ordered by user)
            mergedMenuItems = new MergedMenuItemList();
            mergedMenuItems.Add(new MockExistingMenuItem("e0"));
            mergedMenuItems.Add(menu.GetMode(m1), m1);
            mergedMenuItems.Add(new MockExistingMenuItem("e1"));
            mergedMenuItems.Add(menu.GetGroup(g1), g1);
            mergedMenuItems.Add(new MockExistingMenuItem("e2"));
            mergedMenuItems.Add(menu.GetGroup(g0), g0);
            mergedMenuItems.Add(menu.GetMode(m0), m0);
            mergedMenuItems.Add(new MockExistingMenuItem("e3"));
            applyMenuUseCase.Handle(mergedMenuItems);
            Assert.That(mockApplyMenuPresenter.Result, Is.EqualTo(ApplyMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(4));
            Assert.That(menu.Registered.GetModeAt(0).DisplayName, Is.EqualTo("m1"));
            Assert.That(menu.Registered.GetGroupAt(1).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetGroupAt(2).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m0"));

            // Re-merge with same existing menu items
            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e2"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(8));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(3).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(5).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(6).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(7).DisplayName, Is.EqualTo("e3"));

            // Re-merge with more existing menu items
            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e2"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            existingMenuItems.Add(new MockExistingMenuItem("e4"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(9));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(3).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(5).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(6).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(7).DisplayName, Is.EqualTo("e3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(8).DisplayName, Is.EqualTo("e4"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e2"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            existingMenuItems.Add(new MockExistingMenuItem("e4"));
            existingMenuItems.Add(new MockExistingMenuItem("e5"));
            existingMenuItems.Add(new MockExistingMenuItem("e6"));
            existingMenuItems.Add(new MockExistingMenuItem("e7"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(12));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(3).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(5).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(6).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(7).DisplayName, Is.EqualTo("e3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(8).DisplayName, Is.EqualTo("e4"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(9).DisplayName, Is.EqualTo("e5"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(10).DisplayName, Is.EqualTo("e6"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(11).DisplayName, Is.EqualTo("e7"));

            // Re-merge with less existing menu items
            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(7));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(3).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(5).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(6).DisplayName, Is.EqualTo("m0"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(5));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(2).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(3).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(4).DisplayName, Is.EqualTo("m0"));

            // Re-merge with more inner menu items
            MoveMenuItemUseCase moveMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<MoveMenuItemUseCase>();

            moveMenuItemUseCase.Handle(m2, Menu.RegisteredId, 4);
            moveMenuItemUseCase.Handle(m3, Menu.RegisteredId, 5);
            moveMenuItemUseCase.Handle(m4, Menu.RegisteredId, 6);
            moveMenuItemUseCase.Handle(m5, Menu.RegisteredId, 7);
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetModeAt(0).DisplayName, Is.EqualTo("m1"));
            Assert.That(menu.Registered.GetGroupAt(1).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetGroupAt(2).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetModeAt(4).DisplayName, Is.EqualTo("m2"));
            Assert.That(menu.Registered.GetModeAt(5).DisplayName, Is.EqualTo("m3"));
            Assert.That(menu.Registered.GetModeAt(6).DisplayName, Is.EqualTo("m4"));
            Assert.That(menu.Registered.GetModeAt(7).DisplayName, Is.EqualTo("m5"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e2"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(12));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(3).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(5).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(6).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(7).DisplayName, Is.EqualTo("e3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(8).DisplayName, Is.EqualTo("m2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(9).DisplayName, Is.EqualTo("m3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(10).DisplayName, Is.EqualTo("m4"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(11).DisplayName, Is.EqualTo("m5"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e2"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            existingMenuItems.Add(new MockExistingMenuItem("e4"));
            existingMenuItems.Add(new MockExistingMenuItem("e5"));
            existingMenuItems.Add(new MockExistingMenuItem("e6"));
            existingMenuItems.Add(new MockExistingMenuItem("e7"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(16));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(3).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(5).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(6).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(7).DisplayName, Is.EqualTo("e3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(8).DisplayName, Is.EqualTo("e4"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(9).DisplayName, Is.EqualTo("e5"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(10).DisplayName, Is.EqualTo("e6"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(11).DisplayName, Is.EqualTo("e7"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(12).DisplayName, Is.EqualTo("m2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(13).DisplayName, Is.EqualTo("m3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(14).DisplayName, Is.EqualTo("m4"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(15).DisplayName, Is.EqualTo("m5"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(10));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(3).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(4).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(6).DisplayName, Is.EqualTo("m2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(7).DisplayName, Is.EqualTo("m3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(8).DisplayName, Is.EqualTo("m4"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(9).DisplayName, Is.EqualTo("m5"));

            // Re-merge with less inner menu items
            moveMenuItemUseCase.Handle(m2, Menu.UnregisteredId);
            moveMenuItemUseCase.Handle(m3, Menu.UnregisteredId);
            moveMenuItemUseCase.Handle(m4, Menu.UnregisteredId);
            moveMenuItemUseCase.Handle(m5, Menu.UnregisteredId);
            moveMenuItemUseCase.Handle(g1, Menu.UnregisteredId);
            moveMenuItemUseCase.Handle(m1, Menu.UnregisteredId);
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("m0"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e2"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(6));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(1).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(3).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(5).DisplayName, Is.EqualTo("e3"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e2"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            existingMenuItems.Add(new MockExistingMenuItem("e4"));
            existingMenuItems.Add(new MockExistingMenuItem("e5"));
            existingMenuItems.Add(new MockExistingMenuItem("e6"));
            existingMenuItems.Add(new MockExistingMenuItem("e7"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(10));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(1).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(3).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(5).DisplayName, Is.EqualTo("e3"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(6).DisplayName, Is.EqualTo("e4"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(7).DisplayName, Is.EqualTo("e5"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(8).DisplayName, Is.EqualTo("e6"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(9).DisplayName, Is.EqualTo("e7"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(4));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(1).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(3).DisplayName, Is.EqualTo("m0"));

            // Re-order inner menu items and re-merge
            moveMenuItemUseCase.Handle(g0, Menu.RegisteredId, 0);
            moveMenuItemUseCase.Handle(g1, Menu.RegisteredId, 1);
            moveMenuItemUseCase.Handle(m0, Menu.RegisteredId, 2);
            moveMenuItemUseCase.Handle(m1, Menu.RegisteredId, 3);
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(4));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetGroupAt(1).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m1"));

            existingMenuItems.Clear();
            existingMenuItems.Add(new MockExistingMenuItem("e0"));
            existingMenuItems.Add(new MockExistingMenuItem("e1"));
            existingMenuItems.Add(new MockExistingMenuItem("e2"));
            existingMenuItems.Add(new MockExistingMenuItem("e3"));
            mergeExistingMenuUseCase.Handle(existingMenuItems);
            Assert.That(mockMergeExistingMenuPresenter.Result, Is.EqualTo(MergeExistingMenuResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            Assert.That(mockMergeExistingMenuPresenter.Merged.Count, Is.EqualTo(8));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(0).DisplayName, Is.EqualTo("e0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(1).DisplayName, Is.EqualTo("g0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(2).DisplayName, Is.EqualTo("e1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetGroupAt(3).DisplayName, Is.EqualTo("g1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(4).DisplayName, Is.EqualTo("e2"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetModeAt(6).DisplayName, Is.EqualTo("m1"));
            Assert.That(mockMergeExistingMenuPresenter.Merged.GetExistingMenuItemAt(7).DisplayName, Is.EqualTo("e3"));
        }
    }
}
