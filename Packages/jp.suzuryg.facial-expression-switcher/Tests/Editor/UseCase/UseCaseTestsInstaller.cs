using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyAnimation;
using Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu.ModifyMode.ModifyBranch;
using Zenject;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public class UseCaseTestsInstaller
    {
        public DiContainer Container { get; } = new DiContainer();

        public void Install()
        {
            Container.Bind<IMenuRepository>().To<MockMenuRepository>().AsTransient();
            Container.Bind<IMenuApplier>().To<MockMenuApplier>().AsTransient();
            Container.Bind<IAnimation>().To<MockAnimation>().AsTransient();
            Container.Bind<IAnimationEditor>().To<MockAnimationEditor>().AsTransient();

            Container.Bind<MenuEditingSession>().AsSingle();

            Container.Bind<CreateMenuUseCase>().AsTransient();
            Container.Bind<AddMenuItemUseCase>().AsTransient();
            Container.Bind<ModifyModePropertiesUseCase>().AsTransient();
            Container.Bind<ModifyGroupPropertiesUseCase>().AsTransient();
            Container.Bind<MoveMenuItemUseCase>().AsTransient();
            Container.Bind<RemoveMenuItemUseCase>().AsTransient();
            Container.Bind<MergeExistingMenuUseCase>().AsTransient();
            Container.Bind<ApplyMenuUseCase>().AsTransient();
            Container.Bind<AddBranchUseCase>().AsTransient();
            Container.Bind<ModifyBranchPropertiesUseCase>().AsTransient();
            Container.Bind<ChangeBranchOrderUseCase>().AsTransient();
            Container.Bind<RemoveBranchUseCase>().AsTransient();
            Container.Bind<AddConditionUseCase>().AsTransient();
            Container.Bind<ChangeConditionOrderUseCase>().AsTransient();
            Container.Bind<ModifyConditionUseCase>().AsTransient();
            Container.Bind<RemoveConditionUseCase>().AsTransient();
            Container.Bind<SetNewAnimationUseCase>().AsTransient();
            Container.Bind<SetExistingAnimationUseCase>().AsTransient();
        }
    }
}
