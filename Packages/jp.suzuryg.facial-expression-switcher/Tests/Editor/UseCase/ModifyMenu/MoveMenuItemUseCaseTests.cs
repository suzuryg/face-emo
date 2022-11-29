using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public class MockMoveMenuItemPresenter : IMoveMenuItemPresenter
    {
        public MoveMenuItemResult Result { get; private set; }

        public event System.Action<MoveMenuItemResult, IMenu, string> OnCompleted;

        void IMoveMenuItemPresenter.Complete(MoveMenuItemResult moveMenuItemResult, in IMenu menu, string errorMessage)
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

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestConstants.MenuId;
            System.Func<Menu> loadMenu = () => menuRepository.Load(menuId);

            MoveMenuItemUseCase moveMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<MoveMenuItemUseCase>();
            MockMoveMenuItemPresenter mockMoveMenuItemPresenter = useCaseTestsInstaller.Container.Resolve<IMoveMenuItemPresenter>() as MockMoveMenuItemPresenter;

            // null
            moveMenuItemUseCase.Handle(null, "", "");
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.ArgumentNull));
            moveMenuItemUseCase.Handle("", null, "");
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.ArgumentNull));
            moveMenuItemUseCase.Handle("", "", null);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.ArgumentNull));

            // Menu is not opened
            if (!UseCaseTestConstants.UseActualRepository)
            {
                moveMenuItemUseCase.Handle(menuId, "", "");
                Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.MenuDoesNotExist));
            }

            // Create Menu
            useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>().Handle(menuId);

            // Add item
            AddMenuItemUseCase addMenuItemUseCase = useCaseTestsInstaller.Container.Resolve<AddMenuItemUseCase>();
            ModifyModePropertiesUseCase modifyModePropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyModePropertiesUseCase>();
            ModifyGroupPropertiesUseCase modifyGroupPropertiesUseCase = useCaseTestsInstaller.Container.Resolve<ModifyGroupPropertiesUseCase>();

            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.RegisteredId, AddMenuItemType.Mode);
            var g0 = loadMenu().Registered.Order[0];
            var m0 = loadMenu().Registered.Order[1];
            var m1 = loadMenu().Registered.Order[2];
            var m2 = loadMenu().Registered.Order[3];
            var m3 = loadMenu().Registered.Order[4];
            var m4 = loadMenu().Registered.Order[5];
            var m5 = loadMenu().Registered.Order[6];
            var m6 = loadMenu().Registered.Order[7];

            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Group);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, g0, AddMenuItemType.Mode);
            var g1  = loadMenu().GetGroup(g0).Order[0];
            var m7  = loadMenu().GetGroup(g0).Order[1];
            var m8  = loadMenu().GetGroup(g0).Order[2];
            var m9  = loadMenu().GetGroup(g0).Order[3];
            var m10 = loadMenu().GetGroup(g0).Order[4];
            var m11 = loadMenu().GetGroup(g0).Order[5];
            var m12 = loadMenu().GetGroup(g0).Order[6];
            var m13 = loadMenu().GetGroup(g0).Order[7];

            addMenuItemUseCase.Handle(menuId, g1, AddMenuItemType.Mode);
            var m14 = loadMenu().GetGroup(g1).Order[0];

            addMenuItemUseCase.Handle(menuId, Menu.UnregisteredId, AddMenuItemType.Mode);
            addMenuItemUseCase.Handle(menuId, Menu.UnregisteredId, AddMenuItemType.Mode);
            var m15 = loadMenu().Unregistered.Order[0];
            var m16 = loadMenu().Unregistered.Order[1];

            modifyGroupPropertiesUseCase.Handle(menuId, g0, displayName: "g0");
            modifyGroupPropertiesUseCase.Handle(menuId, g1, displayName: "g1");
            modifyModePropertiesUseCase.Handle(menuId, m0, displayName:  "m0");
            modifyModePropertiesUseCase.Handle(menuId, m1, displayName:  "m1");
            modifyModePropertiesUseCase.Handle(menuId, m2, displayName:  "m2");
            modifyModePropertiesUseCase.Handle(menuId, m3, displayName:  "m3");
            modifyModePropertiesUseCase.Handle(menuId, m4, displayName:  "m4");
            modifyModePropertiesUseCase.Handle(menuId, m5, displayName:  "m5");
            modifyModePropertiesUseCase.Handle(menuId, m6, displayName:  "m6");
            modifyModePropertiesUseCase.Handle(menuId, m7, displayName:  "m7");
            modifyModePropertiesUseCase.Handle(menuId, m8, displayName:  "m8");
            modifyModePropertiesUseCase.Handle(menuId, m9, displayName:  "m9");
            modifyModePropertiesUseCase.Handle(menuId, m10, displayName: "m10");
            modifyModePropertiesUseCase.Handle(menuId, m11, displayName: "m11");
            modifyModePropertiesUseCase.Handle(menuId, m12, displayName: "m12");
            modifyModePropertiesUseCase.Handle(menuId, m13, displayName: "m13");
            modifyModePropertiesUseCase.Handle(menuId, m14, displayName: "m14");
            modifyModePropertiesUseCase.Handle(menuId, m15, displayName: "m15");
            modifyModePropertiesUseCase.Handle(menuId, m16, displayName: "m16");

            Assert.That(loadMenu().Registered.Order.Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m2"));
            Assert.That(loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m3"));
            Assert.That(loadMenu().Registered.GetModeAt(5).DisplayName, Is.EqualTo("m4"));
            Assert.That(loadMenu().Registered.GetModeAt(6).DisplayName, Is.EqualTo("m5"));
            Assert.That(loadMenu().Registered.GetModeAt(7).DisplayName, Is.EqualTo("m6"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).Order.Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m7"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m8"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m9"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m10"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m11"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m12"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(7).DisplayName, Is.EqualTo("m13"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).Order.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m14"));

            Assert.That(loadMenu().Unregistered.Order.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));
            Assert.That(loadMenu().Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m16"));

            // Invalid source
            moveMenuItemUseCase.Handle(menuId, "", g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidSource));

            // Invalid destination
            moveMenuItemUseCase.Handle(menuId, g0, "");
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));

            moveMenuItemUseCase.Handle(menuId, g0, g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));

            // exceeds limit
            moveMenuItemUseCase.Handle(menuId, g1, Menu.RegisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));

            moveMenuItemUseCase.Handle(menuId, m14, Menu.RegisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));

            moveMenuItemUseCase.Handle(menuId, m15, Menu.RegisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));

            moveMenuItemUseCase.Handle(menuId, m0, g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));

            moveMenuItemUseCase.Handle(menuId, m14, g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));

            moveMenuItemUseCase.Handle(menuId, m15, g0);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.InvalidDestination));

            // swap
            moveMenuItemUseCase.Handle(menuId, m0, Menu.RegisteredId, 6);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m4"));
            Assert.That(loadMenu().Registered.GetModeAt(5).DisplayName, Is.EqualTo("m5"));
            Assert.That(loadMenu().Registered.GetModeAt(6).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetModeAt(7).DisplayName, Is.EqualTo("m6"));

            moveMenuItemUseCase.Handle(menuId, m8, g0, 5);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(loadMenu().Registered.GetGroupAt(0).Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m7"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m9"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m10"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m11"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m8"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m12"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(7).DisplayName, Is.EqualTo("m13"));

            moveMenuItemUseCase.Handle(menuId, m16, Menu.UnregisteredId, -1);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));
            Assert.That(loadMenu().Unregistered.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(loadMenu().Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m15"));

            // register -> group
            moveMenuItemUseCase.Handle(menuId, m4, g1, 100);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));

            Assert.That(loadMenu().Registered.Count, Is.EqualTo(7));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m5"));
            Assert.That(loadMenu().Registered.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetModeAt(6).DisplayName, Is.EqualTo("m6"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m7"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m9"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m10"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m11"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m8"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m12"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(7).DisplayName, Is.EqualTo("m13"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).Order.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m14"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m4"));

            Assert.That(loadMenu().Unregistered.Order.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(loadMenu().Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m15"));

            // unregister -> group
            moveMenuItemUseCase.Handle(menuId, m16, g1, -100);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));

            Assert.That(loadMenu().Registered.Count, Is.EqualTo(7));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m5"));
            Assert.That(loadMenu().Registered.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetModeAt(6).DisplayName, Is.EqualTo("m6"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).DisplayName, Is.EqualTo("g1"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m7"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m9"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m10"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m11"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m8"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m12"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(7).DisplayName, Is.EqualTo("m13"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).Order.Count, Is.EqualTo(3));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(loadMenu().Unregistered.Order.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));

            // group -> register
            moveMenuItemUseCase.Handle(menuId, g1, Menu.RegisteredId, 6);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));

            Assert.That(loadMenu().Registered.Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m5"));
            Assert.That(loadMenu().Registered.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetGroupAt(6).DisplayName, Is.EqualTo("g1"));
            Assert.That(loadMenu().Registered.GetModeAt(7).DisplayName, Is.EqualTo("m6"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).Count, Is.EqualTo(7));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m7"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m9"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m10"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m11"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m8"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m12"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(6).DisplayName, Is.EqualTo("m13"));

            Assert.That(loadMenu().Registered.GetGroupAt(6).Count, Is.EqualTo(3));
            Assert.That(loadMenu().Registered.GetGroupAt(6).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(loadMenu().Registered.GetGroupAt(6).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(loadMenu().Registered.GetGroupAt(6).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(loadMenu().Unregistered.Count, Is.EqualTo(1));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));

            // group -> unregister
            moveMenuItemUseCase.Handle(menuId, m10, Menu.UnregisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));

            Assert.That(loadMenu().Registered.Count, Is.EqualTo(8));
            Assert.That(loadMenu().Registered.GetGroupAt(0).DisplayName, Is.EqualTo("g0"));
            Assert.That(loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m1"));
            Assert.That(loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m2"));
            Assert.That(loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m3"));
            Assert.That(loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m5"));
            Assert.That(loadMenu().Registered.GetModeAt(5).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetGroupAt(6).DisplayName, Is.EqualTo("g1"));
            Assert.That(loadMenu().Registered.GetModeAt(7).DisplayName, Is.EqualTo("m6"));

            Assert.That(loadMenu().Registered.GetGroupAt(0).Count, Is.EqualTo(6));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(0).DisplayName, Is.EqualTo("m7"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(1).DisplayName, Is.EqualTo("m9"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(2).DisplayName, Is.EqualTo("m11"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(3).DisplayName, Is.EqualTo("m8"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(4).DisplayName, Is.EqualTo("m12"));
            Assert.That(loadMenu().Registered.GetGroupAt(0).GetModeAt(5).DisplayName, Is.EqualTo("m13"));

            Assert.That(loadMenu().Registered.GetGroupAt(6).Count, Is.EqualTo(3));
            Assert.That(loadMenu().Registered.GetGroupAt(6).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(loadMenu().Registered.GetGroupAt(6).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(loadMenu().Registered.GetGroupAt(6).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(loadMenu().Unregistered.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));
            Assert.That(loadMenu().Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m10"));

            // register -> unregister
            moveMenuItemUseCase.Handle(menuId, g0, Menu.UnregisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));

            Assert.That(loadMenu().Registered.Count, Is.EqualTo(7));
            Assert.That( loadMenu().Registered.GetModeAt(0).DisplayName, Is.EqualTo("m1"));
            Assert.That( loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m2"));
            Assert.That( loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m3"));
            Assert.That( loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m5"));
            Assert.That( loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetGroupAt(5).DisplayName, Is.EqualTo("g1"));
            Assert.That( loadMenu().Registered.GetModeAt(6).DisplayName, Is.EqualTo("m6"));

            Assert.That(loadMenu().Unregistered.GetGroupAt(2).Count, Is.EqualTo(6));
            Assert.That(loadMenu().Unregistered.GetGroupAt(2).GetModeAt(0).DisplayName, Is.EqualTo("m7"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(2).GetModeAt(1).DisplayName, Is.EqualTo("m9"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(2).GetModeAt(2).DisplayName, Is.EqualTo("m11"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(2).GetModeAt(3).DisplayName, Is.EqualTo("m8"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(2).GetModeAt(4).DisplayName, Is.EqualTo("m12"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(2).GetModeAt(5).DisplayName, Is.EqualTo("m13"));

            Assert.That(loadMenu().Registered.GetGroupAt(5).Count, Is.EqualTo(3));
            Assert.That(loadMenu().Registered.GetGroupAt(5).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(loadMenu().Registered.GetGroupAt(5).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(loadMenu().Registered.GetGroupAt(5).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(loadMenu().Unregistered.Count, Is.EqualTo(3));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m15"));
            Assert.That(loadMenu().Unregistered.GetModeAt(1).DisplayName, Is.EqualTo("m10"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(2).DisplayName, Is.EqualTo("g0"));

            // unregister -> register
            moveMenuItemUseCase.Handle(menuId, m15, Menu.RegisteredId);
            Assert.That(mockMoveMenuItemPresenter.Result, Is.EqualTo(MoveMenuItemResult.Succeeded));

            Assert.That(loadMenu().Registered.Count, Is.EqualTo(8));
            Assert.That( loadMenu().Registered.GetModeAt(0).DisplayName, Is.EqualTo("m1"));
            Assert.That( loadMenu().Registered.GetModeAt(1).DisplayName, Is.EqualTo("m2"));
            Assert.That( loadMenu().Registered.GetModeAt(2).DisplayName, Is.EqualTo("m3"));
            Assert.That( loadMenu().Registered.GetModeAt(3).DisplayName, Is.EqualTo("m5"));
            Assert.That( loadMenu().Registered.GetModeAt(4).DisplayName, Is.EqualTo("m0"));
            Assert.That(loadMenu().Registered.GetGroupAt(5).DisplayName, Is.EqualTo("g1"));
            Assert.That( loadMenu().Registered.GetModeAt(6).DisplayName, Is.EqualTo("m6"));
            Assert.That( loadMenu().Registered.GetModeAt(7).DisplayName, Is.EqualTo("m15"));

            Assert.That(loadMenu().Unregistered.GetGroupAt(1).Count, Is.EqualTo(6));
            Assert.That(loadMenu().Unregistered.GetGroupAt(1).GetModeAt(0).DisplayName, Is.EqualTo("m7"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(1).GetModeAt(1).DisplayName, Is.EqualTo("m9"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(1).GetModeAt(2).DisplayName, Is.EqualTo("m11"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(1).GetModeAt(3).DisplayName, Is.EqualTo("m8"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(1).GetModeAt(4).DisplayName, Is.EqualTo("m12"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(1).GetModeAt(5).DisplayName, Is.EqualTo("m13"));

            Assert.That(loadMenu().Registered.GetGroupAt(5).Count, Is.EqualTo(3));
            Assert.That(loadMenu().Registered.GetGroupAt(5).GetModeAt(0).DisplayName, Is.EqualTo("m16"));
            Assert.That(loadMenu().Registered.GetGroupAt(5).GetModeAt(1).DisplayName, Is.EqualTo("m14"));
            Assert.That(loadMenu().Registered.GetGroupAt(5).GetModeAt(2).DisplayName, Is.EqualTo("m4"));

            Assert.That(loadMenu().Unregistered.Count, Is.EqualTo(2));
            Assert.That(loadMenu().Unregistered.GetModeAt(0).DisplayName, Is.EqualTo("m10"));
            Assert.That(loadMenu().Unregistered.GetGroupAt(1).DisplayName, Is.EqualTo("g0"));
        }
    }
}
