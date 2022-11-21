using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
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

            var viewState = launcherObject.GetComponent<HierarchyViewState>();
            if (viewState is null)
            {
                viewState = launcherObject.AddComponent<HierarchyViewState>();
            }
            Container.Bind<HierarchyViewState>().FromInstance(viewState).AsSingle();

            // Bind non-MonoBehaviour classes
            Container.Bind<IMenuRepository>().To<MenuRepository>().AsSingle();

            Container.Bind<UpdateMenuSubject>().AsSingle();

            // TODO: Should it be transient?
            Container.Bind<SettingRepository>().AsTransient();

            Container.Bind<ICreateMenuUseCase>().To<CreateMenuUseCase>().AsTransient();

            Container.Bind<ICreateMenuPresenter>().To<CreateMenuPresenter>().AsSingle();

            Container.Bind<HierarchyView>().AsTransient();
            Container.Bind<MenuItemListView>().AsTransient();
            Container.Bind<BranchListView>().AsTransient();
            Container.Bind<MainView>().AsTransient();
        }
    }
}
