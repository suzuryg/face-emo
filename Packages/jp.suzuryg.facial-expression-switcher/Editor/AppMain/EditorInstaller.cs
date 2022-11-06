using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.View;
using Zenject;

namespace Suzuryg.FacialExpressionSwitcher.AppMain
{
    public class EditorInstaller
    {
        public DiContainer Container { get; } = new DiContainer();

        public void Install()
        {
            Container.Bind<HierarchyControl>().AsTransient();
            Container.Bind<MenuItemListControl>().AsTransient();
            Container.Bind<BranchListControl>().AsTransient();
            Container.Bind<MainWindow>().AsTransient();
        }
    }
}
