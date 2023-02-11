using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using Suzuryg.FacialExpressionSwitcher.Detail;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using Suzuryg.FacialExpressionSwitcher.Detail.Subject;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using UnityEngine;
using UnityEditor;
using Zenject;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    public class FESInstaller
    {
        public DiContainer Container { get; } = new DiContainer();

        public FESInstaller(GameObject launcherObject)
        {
            // Bind MonoBehaviour classes
            var serializableMenu = launcherObject.GetComponent<SerializableMenu>();
            if (serializableMenu is null)
            {
                serializableMenu = launcherObject.AddComponent<SerializableMenu>();
            }
            Container.Bind<SerializableMenu>().FromInstance(serializableMenu).AsSingle();

            var hierarchyViewState = launcherObject.GetComponent<HierarchyViewState>();
            if (hierarchyViewState is null)
            {
                hierarchyViewState = launcherObject.AddComponent<HierarchyViewState>();
            }
            Container.Bind<HierarchyViewState>().FromInstance(hierarchyViewState).AsSingle();

            var menuItemiListViewState = launcherObject.GetComponent<MenuItemListViewState>();
            if (menuItemiListViewState is null)
            {
                menuItemiListViewState = launcherObject.AddComponent<MenuItemListViewState>();
            }
            Container.Bind<MenuItemListViewState>().FromInstance(menuItemiListViewState).AsSingle();

            // Bind non-MonoBehaviour classes
            Container.Bind<IMenuRepository>().To<MenuRepository>().AsSingle();
            Container.Bind<IFxGenerator>().To<FxGenerator>().AsTransient();

            Container.Bind<UpdateMenuSubject>().AsSingle();
            Container.Bind<ChangeHierarchySelectionSubject>().AsSingle();
            Container.Bind<ChangeMenuItemListRootSubject>().AsSingle();
            Container.Bind<ChangeMenuItemListSelectionSubject>().AsSingle();
            Container.Bind<ChangeBranchSelectionSubject>().AsSingle();

            Container.Bind<ThumbnailDrawer>().AsSingle();

            Container.BindInterfacesTo<LocalizationSetting>().AsSingle();

            Container.Bind<ICreateMenuUseCase>().To<CreateMenuUseCase>().AsTransient();
            Container.Bind<IModifyMenuPropertiesUseCase>().To<ModifyMenuPropertiesUseCase>().AsTransient();
            Container.Bind<IAddMenuItemUseCase>().To<AddMenuItemUseCase>().AsTransient();
            Container.Bind<IModifyModePropertiesUseCase>().To<ModifyModePropertiesUseCase>().AsTransient();
            Container.Bind<IModifyGroupPropertiesUseCase>().To<ModifyGroupPropertiesUseCase>().AsTransient();
            Container.Bind<IMoveMenuItemUseCase>().To<MoveMenuItemUseCase>().AsTransient();
            Container.Bind<IRemoveMenuItemUseCase>().To<RemoveMenuItemUseCase>().AsTransient();
            Container.Bind<IMergeExistingMenuUseCase>().To<MergeExistingMenuUseCase>().AsTransient();
            Container.Bind<IApplyMenuUseCase>().To<ApplyMenuUseCase>().AsTransient();
            Container.Bind<IGenerateFxUseCase>().To<GenerateFxUseCase>().AsTransient();
            Container.Bind<IAddBranchUseCase>().To<AddBranchUseCase>().AsTransient();
            Container.Bind<IModifyBranchPropertiesUseCase>().To<ModifyBranchPropertiesUseCase>().AsTransient();
            Container.Bind<IChangeBranchOrderUseCase>().To<ChangeBranchOrderUseCase>().AsTransient();
            Container.Bind<IRemoveBranchUseCase>().To<RemoveBranchUseCase>().AsTransient();
            Container.Bind<IAddConditionUseCase>().To<AddConditionUseCase>().AsTransient();
            Container.Bind<IChangeConditionOrderUseCase>().To<ChangeConditionOrderUseCase>().AsTransient();
            Container.Bind<IModifyConditionUseCase>().To<ModifyConditionUseCase>().AsTransient();
            Container.Bind<IRemoveConditionUseCase>().To<RemoveConditionUseCase>().AsTransient();
            Container.Bind<ISetNewAnimationUseCase>().To<SetNewAnimationUseCase>().AsTransient();
            Container.Bind<ISetExistingAnimationUseCase>().To<SetExistingAnimationUseCase>().AsTransient();

            Container.Bind<ICreateMenuPresenter>().To<CreateMenuPresenter>().AsSingle();
            Container.Bind<IModifyMenuPropertiesPresenter>().To<ModifyMenuPropertiesPresenter>().AsSingle();
            Container.Bind<IAddMenuItemPresenter>().To<AddMenuItemPresenter>().AsSingle();
            Container.Bind<IModifyModePropertiesPresenter>().To<ModifyModePropertiesPresenter>().AsSingle();
            Container.Bind<IModifyGroupPropertiesPresenter>().To<ModifyGroupPropertiesPresenter>().AsSingle();
            Container.Bind<IMoveMenuItemPresenter>().To<MoveMenuItemPresenter>().AsSingle();
            Container.Bind<IRemoveMenuItemPresenter>().To<RemoveMenuItemPresenter>().AsSingle();
            Container.Bind<IMergeExistingMenuPresenter>().To<MergeExistingMenuPresenter>().AsSingle();
            Container.Bind<IApplyMenuPresenter>().To<ApplyMenuPresenter>().AsSingle();
            Container.Bind<IGenerateFxPresenter>().To<GenerateFxPresenter>().AsSingle();
            Container.Bind<IAddBranchPresenter>().To<AddBranchPresenter>().AsSingle();
            Container.Bind<IModifyBranchPropertiesPresenter>().To<ModifyBranchPropertiesPresenter>().AsSingle();
            Container.Bind<IChangeBranchOrderPresenter>().To<ChangeBranchOrderPresenter>().AsSingle();
            Container.Bind<IRemoveBranchPresenter>().To<RemoveBranchPresenter>().AsSingle();
            Container.Bind<IAddConditionPresenter>().To<AddConditionPresenter>().AsSingle();
            Container.Bind<IChangeConditionOrderPresenter>().To<ChangeConditionOrderPresenter>().AsSingle();
            Container.Bind<IModifyConditionPresenter>().To<ModifyConditionPresenter>().AsSingle();
            Container.Bind<IRemoveConditionPresenter>().To<RemoveConditionPresenter>().AsSingle();
            Container.Bind<ISetNewAnimationPresenter>().To<SetNewAnimationPresenter>().AsSingle();
            Container.Bind<ISetExistingAnimationPresenter>().To<SetExistingAnimationPresenter>().AsSingle();

            Container.Bind<MainView>().AsTransient();
            Container.Bind<HierarchyView>().AsTransient();
            Container.Bind<MenuItemListView>().AsTransient();
            Container.Bind<BranchListView>().AsTransient();
            Container.Bind<SettingView>().AsTransient();
            Container.Bind<GestureTableView>().AsTransient();
            Container.Bind<InspectorView>().AsTransient();
            Container.Bind<UseCaseErrorHandler>().AsTransient();
        }

        public static FESInstaller GetInstaller(string rootObjectPath)
        {
            if (!rootObjectPath.StartsWith("/"))
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"{rootObjectPath} is not a full path.", "OK");
                return null;
            }

            var launcherObject = GameObject.Find(rootObjectPath);
            if (launcherObject is null)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"{rootObjectPath} was not found. Please activate the GameObject.", "OK");
                return null;
            }

            // Unity's bug: If the object is nested more than 1 level, the object is found even if it is deactivated. This code does not deal with the bug.
            launcherObject.SetActive(false);
            var anotherObject = GameObject.Find(rootObjectPath);
            launcherObject.SetActive(true);
            if (anotherObject is GameObject)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"{rootObjectPath} has duplicate path. Please change GameObject's name.", "OK");
                return null;
            }

            return new FESInstaller(launcherObject);
        }
    }
}
