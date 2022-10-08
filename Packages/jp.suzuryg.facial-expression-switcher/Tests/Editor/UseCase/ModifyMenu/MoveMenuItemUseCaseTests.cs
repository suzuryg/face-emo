using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public class MockMoveMenuItemPresenter : IMoveMenuItemPresenter
    {
        public MoveMenuItemResult Result { get; private set; }

        void IMoveMenuItemPresenter.Complete(MoveMenuItemResult moveMenuItemResult, in Menu menu, string errorMessage)
        {
            Result = moveMenuItemResult;
        }
    }

    public class MoveMenuItemUseCaseTests
    {
        [Test]
        public void Test()
        {
            // Setup
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            MoveMenuItemUseCase moveMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<MoveMenuItemUseCase>();
            MockMoveMenuItemPresenter mockMoveMenuItemPresenter = new MockMoveMenuItemPresenter();
            moveMenuItemUseCase.SetPresenter(mockMoveMenuItemPresenter);

            // null
            moveMenuItemUseCase.Handle(null, null);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.ArgumentNull));

            // Menu is not opened
            moveMenuItemUseCase.Handle("", "");
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.MenuIsNotOpened));

            // Create Menu
            useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>().Handle();
            var menu = menuEditingSession.Menu;
            menuEditingSession.SaveAs("dest");

            // Add item
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            ModifyModePropertiesUseCase modifyModePropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyModePropertiesUseCase>();
            ModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyGroupPropertiesUseCase>();

            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.RegisteredId, AddMenuItemType.Mode);
            var g0 = menu.Registered.Order[0];
            var m0 = menu.Registered.Order[1];
            var m1 = menu.Registered.Order[2];
            var m2 = menu.Registered.Order[3];
            var m3 = menu.Registered.Order[4];
            var m4 = menu.Registered.Order[5];
            var m5 = menu.Registered.Order[6];
            var m6 = menu.Registered.Order[7];

            addMenuItemUseCase.Handle(g0, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(g0, AddMenuItemType.Mode);
            var g1  = menu.GetGroup(g0).Order[0];
            var m7  = menu.GetGroup(g0).Order[1];
            var m8  = menu.GetGroup(g0).Order[2];
            var m9  = menu.GetGroup(g0).Order[3];
            var m10 = menu.GetGroup(g0).Order[4];
            var m11 = menu.GetGroup(g0).Order[5];
            var m12 = menu.GetGroup(g0).Order[6];
            var m13 = menu.GetGroup(g0).Order[7];

            addMenuItemUseCase.Handle(g1, AddMenuItemType.Mode);
            var m14 = menu.GetGroup(g1).Order[0];

            addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(Menu.UnregisteredId, AddMenuItemType.Mode);
            var m15 = menu.Unregistered.Order[0];
            var m16 = menu.Unregistered.Order[1];

            modifyGroupPropertiesUseCase.Handle(g0, displayName: "g0");
            modifyGroupPropertiesUseCase.Handle(g1, displayName: "g1");
            modifyModePropertiesUseCase.Handle(m0, displayName:  "m0");
            modifyModePropertiesUseCase.Handle(m1, displayName:  "m1");
            modifyModePropertiesUseCase.Handle(m2, displayName:  "m2");
            modifyModePropertiesUseCase.Handle(m3, displayName:  "m3");
            modifyModePropertiesUseCase.Handle(m4, displayName:  "m4");
            modifyModePropertiesUseCase.Handle(m5, displayName:  "m5");
            modifyModePropertiesUseCase.Handle(m6, displayName:  "m6");
            modifyModePropertiesUseCase.Handle(m7, displayName:  "m7");
            modifyModePropertiesUseCase.Handle(m8, displayName:  "m8");
            modifyModePropertiesUseCase.Handle(m9, displayName:  "m9");
            modifyModePropertiesUseCase.Handle(m10, displayName: "m10");
            modifyModePropertiesUseCase.Handle(m11, displayName: "m11");
            modifyModePropertiesUseCase.Handle(m12, displayName: "m12");
            modifyModePropertiesUseCase.Handle(m13, displayName: "m13");
            modifyModePropertiesUseCase.Handle(m14, displayName: "m14");
            modifyModePropertiesUseCase.Handle(m15, displayName: "m15");
            modifyModePropertiesUseCase.Handle(m16, displayName: "m16");

            menuEditingSession.Save();
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));

            Assert.That(menu.Registered.Order.Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("m1"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m2"));
            Assert.That(menu.Registered.GetModeAt(4).DisplayName, Is.EqualTo("m3"));
            Assert.That(menu.Registered.GetModeAt(5).DisplayName, Is.EqualTo("m4"));
            Assert.That(menu.Registered.GetModeAt(6).DisplayName, Is.EqualTo("m5"));
            Assert.That(menu.Registered.GetModeAt(7).DisplayName, Is.EqualTo("m6"));

            Assert.That(menu.Registered.GetGroupAt(0).Order.Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m7"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m8"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m9"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m10"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m11"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m12"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(7).DisplayName, Is.EqualTo("m13"));

            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).Order.Count, Is.EqualTo(1));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m14"));

            Assert.That(menu.Unregistered.Order.Count, Is.EqualTo(2));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));
            Assert.That(menu.Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m16"));

            // Invalid source
            moveMenuItemUseCase.Handle("", g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidSource));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            // Invalid destination
            moveMenuItemUseCase.Handle(g0, "");
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            moveMenuItemUseCase.Handle(g0, g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            // exceeds limit
            moveMenuItemUseCase.Handle(g1, Menu.RegisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            moveMenuItemUseCase.Handle(m14, Menu.RegisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            moveMenuItemUseCase.Handle(m15, Menu.RegisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            moveMenuItemUseCase.Handle(m0, g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            moveMenuItemUseCase.Handle(m14, g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            moveMenuItemUseCase.Handle(m15, g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(false));
            menuEditingSession.Save();

            // swap
            moveMenuItemUseCase.Handle(m0, Menu.RegisteredId, 6);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();
            Assert.That(menu.Registered.Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(menu.Registered.GetModeAt(4).DisplayName, Is.EqualTo("m4"));
            Assert.That(menu.Registered.GetModeAt(5).DisplayName, Is.EqualTo("m5"));
            Assert.That(menu.Registered.GetModeAt(6).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetModeAt(7).DisplayName, Is.EqualTo("m6"));

            moveMenuItemUseCase.Handle(m8, g0, 5);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();
            Assert.That(menu.Registered.GetGroupAt(0).Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m7"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m9"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m10"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m11"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m8"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m12"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(7).DisplayName, Is.EqualTo("m13"));

            moveMenuItemUseCase.Handle(m16, Menu.UnregisteredId, -1);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();
            Assert.That(menu.Unregistered.Count, Is.EqualTo(2));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(menu.Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m15"));

            // register -> group
            moveMenuItemUseCase.Handle(m4, g1, 100);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(7));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(menu.Registered.GetModeAt(4).DisplayName, Is.EqualTo("m5"));
            Assert.That(menu.Registered.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetModeAt(6).DisplayName, Is.EqualTo("m6"));

            Assert.That(menu.Registered.GetGroupAt(0).Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m7"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m9"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m10"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m11"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m8"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m12"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(7).DisplayName, Is.EqualTo("m13"));

            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).Order.Count, Is.EqualTo(2));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m14"));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m4"));

            Assert.That(menu.Unregistered.Order.Count, Is.EqualTo(2));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(menu.Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m15"));

            // unregister -> group
            moveMenuItemUseCase.Handle(m16, g1, -100);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(7));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(menu.Registered.GetModeAt(4).DisplayName, Is.EqualTo("m5"));
            Assert.That(menu.Registered.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetModeAt(6).DisplayName, Is.EqualTo("m6"));

            Assert.That(menu.Registered.GetGroupAt(0).Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m7"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m9"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m10"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m11"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m8"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m12"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(7).DisplayName, Is.EqualTo("m13"));

            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).Order.Count, Is.EqualTo(3));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(menu.Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(menu.Unregistered.Order.Count, Is.EqualTo(1));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));

            // group -> register
            moveMenuItemUseCase.Handle(g1, Menu.RegisteredId, 6);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(menu.Registered.GetModeAt(4).DisplayName, Is.EqualTo("m5"));
            Assert.That(menu.Registered.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetGroupAt(6).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetModeAt(7).DisplayName, Is.EqualTo("m6"));

            Assert.That(menu.Registered.GetGroupAt(0).Count, Is.EqualTo(7));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m7"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m9"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m10"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m11"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m8"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m12"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m13"));

            Assert.That(menu.Registered.GetGroupAt(6).Count, Is.EqualTo(3));
            Assert.That(menu.Registered.GetGroupAt(6).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(menu.Registered.GetGroupAt(6).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(menu.Registered.GetGroupAt(6).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(menu.Unregistered.Count, Is.EqualTo(1));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));

            // group -> unregister
            moveMenuItemUseCase.Handle(m10, Menu.UnregisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(8));
            Assert.That(menu.Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(menu.Registered.GetModeAt(4).DisplayName, Is.EqualTo("m5"));
            Assert.That(menu.Registered.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetGroupAt(6).DisplayName, Is.EqualTo("g1"));
            Assert.That(menu.Registered.GetModeAt(7).DisplayName, Is.EqualTo("m6"));

            Assert.That(menu.Registered.GetGroupAt(0).Count, Is.EqualTo(6));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m7"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m9"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m11"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m8"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m12"));
            Assert.That(menu.Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m13"));

            Assert.That(menu.Registered.GetGroupAt(6).Count, Is.EqualTo(3));
            Assert.That(menu.Registered.GetGroupAt(6).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(menu.Registered.GetGroupAt(6).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(menu.Registered.GetGroupAt(6).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(menu.Unregistered.Count, Is.EqualTo(2));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));
            Assert.That(menu.Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m10"));

            // register -> unregister
            moveMenuItemUseCase.Handle(g0, Menu.UnregisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(7));
            Assert.That( menu.Registered.GetModeAt(0).DisplayName, Is.EqualTo("m1"));
            Assert.That( menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("m2"));
            Assert.That( menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("m3"));
            Assert.That( menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m5"));
            Assert.That( menu.Registered.GetModeAt(4).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetGroupAt(5).DisplayName, Is.EqualTo("g1"));
            Assert.That( menu.Registered.GetModeAt(6).DisplayName, Is.EqualTo("m6"));

            Assert.That(menu.Unregistered.GetGroupAt(2).Count, Is.EqualTo(6));
            Assert.That(menu.Unregistered.GetGroupAt(2).GetModeAt(0).DisplayName, Is.EqualTo("m7"));
            Assert.That(menu.Unregistered.GetGroupAt(2).GetModeAt(1).DisplayName, Is.EqualTo("m9"));
            Assert.That(menu.Unregistered.GetGroupAt(2).GetModeAt(2).DisplayName, Is.EqualTo("m11"));
            Assert.That(menu.Unregistered.GetGroupAt(2).GetModeAt(3).DisplayName, Is.EqualTo("m8"));
            Assert.That(menu.Unregistered.GetGroupAt(2).GetModeAt(4).DisplayName, Is.EqualTo("m12"));
            Assert.That(menu.Unregistered.GetGroupAt(2).GetModeAt(5).DisplayName, Is.EqualTo("m13"));

            Assert.That(menu.Registered.GetGroupAt(5).Count, Is.EqualTo(3));
            Assert.That(menu.Registered.GetGroupAt(5).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(menu.Registered.GetGroupAt(5).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(menu.Registered.GetGroupAt(5).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(menu.Unregistered.Count, Is.EqualTo(3));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));
            Assert.That(menu.Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m10"));
            Assert.That(menu.Unregistered.GetGroupAt(2).DisplayName, Is.EqualTo("g0"));

            // unregister -> register
            moveMenuItemUseCase.Handle(m15, Menu.RegisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(menuEditingSession.IsModified, Is.EqualTo(true));
            menuEditingSession.Save();

            Assert.That(menu.Registered.Count, Is.EqualTo(8));
            Assert.That( menu.Registered.GetModeAt(0).DisplayName, Is.EqualTo("m1"));
            Assert.That( menu.Registered.GetModeAt(1).DisplayName, Is.EqualTo("m2"));
            Assert.That( menu.Registered.GetModeAt(2).DisplayName, Is.EqualTo("m3"));
            Assert.That( menu.Registered.GetModeAt(3).DisplayName, Is.EqualTo("m5"));
            Assert.That( menu.Registered.GetModeAt(4).DisplayName, Is.EqualTo("m0"));
            Assert.That(menu.Registered.GetGroupAt(5).DisplayName, Is.EqualTo("g1"));
            Assert.That( menu.Registered.GetModeAt(6).DisplayName, Is.EqualTo("m6"));
            Assert.That( menu.Registered.GetModeAt(7).DisplayName, Is.EqualTo("m15"));

            Assert.That(menu.Unregistered.GetGroupAt(1).Count, Is.EqualTo(6));
            Assert.That(menu.Unregistered.GetGroupAt(1).GetModeAt(0).DisplayName, Is.EqualTo("m7"));
            Assert.That(menu.Unregistered.GetGroupAt(1).GetModeAt(1).DisplayName, Is.EqualTo("m9"));
            Assert.That(menu.Unregistered.GetGroupAt(1).GetModeAt(2).DisplayName, Is.EqualTo("m11"));
            Assert.That(menu.Unregistered.GetGroupAt(1).GetModeAt(3).DisplayName, Is.EqualTo("m8"));
            Assert.That(menu.Unregistered.GetGroupAt(1).GetModeAt(4).DisplayName, Is.EqualTo("m12"));
            Assert.That(menu.Unregistered.GetGroupAt(1).GetModeAt(5).DisplayName, Is.EqualTo("m13"));

            Assert.That(menu.Registered.GetGroupAt(5).Count, Is.EqualTo(3));
            Assert.That(menu.Registered.GetGroupAt(5).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(menu.Registered.GetGroupAt(5).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(menu.Registered.GetGroupAt(5).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(menu.Unregistered.Count, Is.EqualTo(2));
            Assert.That(menu.Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m10"));
            Assert.That(menu.Unregistered.GetGroupAt(1).DisplayName, Is.EqualTo("g0"));
        }
    }
}
