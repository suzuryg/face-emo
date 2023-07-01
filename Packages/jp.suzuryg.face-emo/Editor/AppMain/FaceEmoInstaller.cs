using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.UseCase.ModifyMenu;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using Suzuryg.FaceEmo.Detail;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Data;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Detail.View;
using Suzuryg.FaceEmo.Detail.View.Element;
using Suzuryg.FaceEmo.Detail.View.ExpressionEditor;
using UnityEngine;
using UnityEditor;
using Zenject;

namespace Suzuryg.FaceEmo.AppMain
{
    public class FaceEmoInstaller
    {
        public DiContainer Container { get; } = new DiContainer();
        public string RootObjectName { get; private set; }

        public FaceEmoInstaller(GameObject launcherObject)
        {
            RootObjectName = launcherObject.name;

            // Bind Monobehaviour instances
            var menuRepositoryComponent = launcherObject.GetComponent<MenuRepositoryComponent>();
            if (menuRepositoryComponent == null) { menuRepositoryComponent = launcherObject.AddComponent<MenuRepositoryComponent>(); }
            menuRepositoryComponent.hideFlags = HideFlags.HideInInspector;
            Container.Bind<MenuRepositoryComponent>().FromInstance(menuRepositoryComponent).AsSingle();

            var launcher = launcherObject.GetComponent<FaceEmoLauncherComponent>();
            if (launcher == null) { launcher = launcherObject.AddComponent<FaceEmoLauncherComponent>(); }
            launcher.hideFlags = HideFlags.None;

            // Bind ScriptableObject instances
            if (launcher.AV3Setting == null) { launcher.AV3Setting = ScriptableObject.CreateInstance<AV3Setting>(); }
            if (launcher.ExpressionEditorSetting == null) { launcher.ExpressionEditorSetting = ScriptableObject.CreateInstance<ExpressionEditorSetting>(); }
            if (launcher.ThumbnailSetting == null) { launcher.ThumbnailSetting = ScriptableObject.CreateInstance<ThumbnailSetting>(); }
            if (launcher.HierarchyViewState == null) { launcher.HierarchyViewState = ScriptableObject.CreateInstance<HierarchyViewState>(); }
            if (launcher.MenuItemListViewState == null) { launcher.MenuItemListViewState = ScriptableObject.CreateInstance<MenuItemListViewState>(); }
            if (launcher.ViewSelection == null) { launcher.ViewSelection = ScriptableObject.CreateInstance<ViewSelection>(); }
            if (launcher.InspectorViewState == null) { launcher.InspectorViewState = ScriptableObject.CreateInstance<InspectorViewState>(); }

            // Avoid binding the same reference when the component is copied.
            var instanceId = launcherObject.GetInstanceID();
            if (instanceId != launcher.InstanceId)
            {
                var AV3Setting = ScriptableObject.CreateInstance<AV3Setting>();
                EditorUtility.CopySerialized(launcher.AV3Setting, AV3Setting);
                launcher.AV3Setting = AV3Setting;

                var ExpressionEditorSetting = ScriptableObject.CreateInstance<ExpressionEditorSetting>();
                EditorUtility.CopySerialized(launcher.ExpressionEditorSetting, ExpressionEditorSetting);
                launcher.ExpressionEditorSetting = ExpressionEditorSetting;

                var ThumbnailSetting = ScriptableObject.CreateInstance<ThumbnailSetting>();
                EditorUtility.CopySerialized(launcher.ThumbnailSetting, ThumbnailSetting);
                launcher.ThumbnailSetting = ThumbnailSetting;

                var HierarchyViewState = ScriptableObject.CreateInstance<HierarchyViewState>();
                EditorUtility.CopySerialized(launcher.HierarchyViewState, HierarchyViewState);
                launcher.HierarchyViewState = HierarchyViewState;

                var MenuItemListViewState = ScriptableObject.CreateInstance<MenuItemListViewState>();
                EditorUtility.CopySerialized(launcher.MenuItemListViewState, MenuItemListViewState);
                launcher.MenuItemListViewState = MenuItemListViewState;

                var ViewSelection = ScriptableObject.CreateInstance<ViewSelection>();
                EditorUtility.CopySerialized(launcher.ViewSelection, ViewSelection);
                launcher.ViewSelection = ViewSelection;

                var InspectorViewState = ScriptableObject.CreateInstance<InspectorViewState>();
                EditorUtility.CopySerialized(launcher.InspectorViewState, InspectorViewState);
                launcher.InspectorViewState = InspectorViewState;

                launcher.InstanceId = instanceId;
            }

            Container.Bind<AV3Setting>().FromInstance(launcher.AV3Setting).AsSingle();
            Container.Bind<ExpressionEditorSetting>().FromInstance(launcher.ExpressionEditorSetting).AsSingle();
            Container.Bind<ThumbnailSetting>().FromInstance(launcher.ThumbnailSetting).AsSingle();
            Container.Bind<HierarchyViewState>().FromInstance(launcher.HierarchyViewState).AsSingle();
            Container.Bind<MenuItemListViewState>().FromInstance(launcher.MenuItemListViewState).AsSingle();
            Container.Bind<ViewSelection>().FromInstance(launcher.ViewSelection).AsSingle();
            Container.Bind<InspectorViewState>().FromInstance(launcher.InspectorViewState).AsSingle();

            // Bind non-serialized classes
            Container.Bind<ICreateMenuUseCase>().To<CreateMenuUseCase>().AsTransient();
            Container.Bind<IModifyMenuPropertiesUseCase>().To<ModifyMenuPropertiesUseCase>().AsTransient();
            Container.Bind<IAddMenuItemUseCase>().To<AddMenuItemUseCase>().AsTransient();
            Container.Bind<ICopyMenuItemUseCase>().To<CopyMenuItemUseCase>().AsTransient();
            Container.Bind<IModifyModePropertiesUseCase>().To<ModifyModePropertiesUseCase>().AsTransient();
            Container.Bind<IModifyGroupPropertiesUseCase>().To<ModifyGroupPropertiesUseCase>().AsTransient();
            Container.Bind<IMoveMenuItemUseCase>().To<MoveMenuItemUseCase>().AsTransient();
            Container.Bind<IRemoveMenuItemUseCase>().To<RemoveMenuItemUseCase>().AsTransient();
            Container.Bind<IGenerateFxUseCase>().To<GenerateFxUseCase>().AsTransient();
            Container.Bind<IAddBranchUseCase>().To<AddBranchUseCase>().AsTransient();
            Container.Bind<IAddMultipleBranchesUseCase>().To<AddMultipleBranchesUseCase>().AsTransient();
            Container.Bind<ICopyBranchUseCase>().To<CopyBranchUseCase>().AsTransient();
            Container.Bind<IModifyBranchPropertiesUseCase>().To<ModifyBranchPropertiesUseCase>().AsTransient();
            Container.Bind<IChangeBranchOrderUseCase>().To<ChangeBranchOrderUseCase>().AsTransient();
            Container.Bind<IRemoveBranchUseCase>().To<RemoveBranchUseCase>().AsTransient();
            Container.Bind<IAddConditionUseCase>().To<AddConditionUseCase>().AsTransient();
            Container.Bind<IChangeConditionOrderUseCase>().To<ChangeConditionOrderUseCase>().AsTransient();
            Container.Bind<IModifyConditionUseCase>().To<ModifyConditionUseCase>().AsTransient();
            Container.Bind<IRemoveConditionUseCase>().To<RemoveConditionUseCase>().AsTransient();
            Container.Bind<ISetExistingAnimationUseCase>().To<SetExistingAnimationUseCase>().AsTransient();

            Container.Bind<ICreateMenuPresenter>().To<CreateMenuPresenter>().AsSingle();
            Container.Bind<IModifyMenuPropertiesPresenter>().To<ModifyMenuPropertiesPresenter>().AsSingle();
            Container.Bind<IAddMenuItemPresenter>().To<AddMenuItemPresenter>().AsSingle();
            Container.Bind<ICopyMenuItemPresenter>().To<CopyMenuItemPresenter>().AsSingle();
            Container.Bind<IModifyModePropertiesPresenter>().To<ModifyModePropertiesPresenter>().AsSingle();
            Container.Bind<IModifyGroupPropertiesPresenter>().To<ModifyGroupPropertiesPresenter>().AsSingle();
            Container.Bind<IMoveMenuItemPresenter>().To<MoveMenuItemPresenter>().AsSingle();
            Container.Bind<IRemoveMenuItemPresenter>().To<RemoveMenuItemPresenter>().AsSingle();
            Container.Bind<IGenerateFxPresenter>().To<GenerateFxPresenter>().AsSingle();
            Container.Bind<IAddBranchPresenter>().To<AddBranchPresenter>().AsSingle();
            Container.Bind<IAddMultipleBranchesPresenter>().To<AddMultipleBranchesPresenter>().AsSingle();
            Container.Bind<ICopyBranchPresenter>().To<CopyBranchPresenter>().AsSingle();
            Container.Bind<IModifyBranchPropertiesPresenter>().To<ModifyBranchPropertiesPresenter>().AsSingle();
            Container.Bind<IChangeBranchOrderPresenter>().To<ChangeBranchOrderPresenter>().AsSingle();
            Container.Bind<IRemoveBranchPresenter>().To<RemoveBranchPresenter>().AsSingle();
            Container.Bind<IAddConditionPresenter>().To<AddConditionPresenter>().AsSingle();
            Container.Bind<IChangeConditionOrderPresenter>().To<ChangeConditionOrderPresenter>().AsSingle();
            Container.Bind<IModifyConditionPresenter>().To<ModifyConditionPresenter>().AsSingle();
            Container.Bind<IRemoveConditionPresenter>().To<RemoveConditionPresenter>().AsSingle();
            Container.Bind<ISetExistingAnimationPresenter>().To<SetExistingAnimationPresenter>().AsSingle();

            Container.BindInterfacesTo<SubWindowManager>().AsSingle();
            Container.BindInterfacesTo<MenuRepository>().AsSingle();
            Container.BindInterfacesTo<LocalizationSetting>().AsSingle();
            Container.Bind<IBackupper>().To<FaceEmoBackupper>().AsSingle();
            Container.Bind<UpdateMenuSubject>().AsSingle();
            Container.Bind<SelectionSynchronizer>().AsSingle();
            Container.Bind<MainThumbnailDrawer>().AsSingle();
            Container.Bind<GestureTableThumbnailDrawer>().AsSingle();
            Container.Bind<ExMenuThumbnailDrawer>().AsSingle();
            Container.Bind<InspectorThumbnailDrawer>().AsSingle();
            Container.Bind<ModeNameProvider>().AsSingle();
            Container.Bind<AnimationElement>().AsSingle();
            Container.Bind<ExpressionEditor>().AsSingle();
            Container.Bind<MainWindowProvider>().AsSingle();

            Container.Bind<BranchListElement>().AsTransient();
            Container.Bind<GestureTableElement>().AsTransient();
            Container.Bind<DefaultsProviderGenerator>().AsTransient();

            Container.Bind<IFxGenerator>().To<FxGenerator>().AsTransient();

            Container.Bind<MainView>().AsTransient();
            Container.Bind<HierarchyView>().AsTransient();
            Container.Bind<MenuItemListView>().AsTransient();
            Container.Bind<BranchListView>().AsTransient();
            Container.Bind<SettingView>().AsTransient();
            Container.Bind<GestureTableView>().AsTransient();
            Container.Bind<InspectorView>().AsTransient();
            Container.Bind<ExpressionEditorView>().AsTransient();
            Container.Bind<UseCaseErrorHandler>().AsTransient();
        }

        public void SaveUIStates()
        {
            // If the Unity editor exits without saving the scene, the UI state changes are discarded (so that the scene is not saved on its own).
            // If TreeViewState can be changed with SerializedObject, it may not be necessary to set the Dirty flag.
            EditorUtility.SetDirty(Container.Resolve<HierarchyViewState>());
            EditorUtility.SetDirty(Container.Resolve<MenuItemListViewState>());
            EditorUtility.SetDirty(Container.Resolve<ViewSelection>());
            EditorUtility.SetDirty(Container.Resolve<InspectorViewState>());
        }

        public static FaceEmoInstaller GetInstaller(string rootObjectPath)
        {
            if (!rootObjectPath.StartsWith("/"))
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"{rootObjectPath} is not a full path.", "OK");
                return null;
            }

            var launcherObject = GameObject.Find(rootObjectPath);
            if (launcherObject == null)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"{rootObjectPath} was not found. Please activate the GameObject.", "OK");
                return null;
            }

            // Unity's bug: If the object is nested more than 1 level, the object is found even if it is deactivated. This code does not deal with the bug.
            launcherObject.SetActive(false);
            var anotherObject = GameObject.Find(rootObjectPath);
            launcherObject.SetActive(true);
            if (anotherObject != null)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"{rootObjectPath} has duplicate path. Please change GameObject's name.", "OK");
                return null;
            }

            return new FaceEmoInstaller(launcherObject);
        }
    }
}
