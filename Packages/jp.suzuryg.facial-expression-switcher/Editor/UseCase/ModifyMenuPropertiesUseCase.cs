using Suzuryg.FacialExpressionSwitcher.Domain;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public interface IModifyMenuPropertiesUseCase
    {
        void SetPresenter(IModifyMenuPropertiesPresenter modifyMenuPropertiesPresenter);
        void Handle(bool? writeDefaults = null, double? transitionDurationSeconds = null);
    }

    public interface IModifyMenuPropertiesPresenter
    {
    }

    public class ModifyMenuPropertiesUseCase : IModifyMenuPropertiesUseCase
    {
        IModifyMenuPropertiesPresenter _modifyMenuPropertiesPresenter;
        MenuEditingSession _menuEditingSession;

        public ModifyMenuPropertiesUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IModifyMenuPropertiesPresenter modifyMenuPropertiesPresenter)
        {
            _modifyMenuPropertiesPresenter = modifyMenuPropertiesPresenter;
        }

        public void Handle(bool? writeDefaults = null, double? transitionDurationSeconds = null)
        {
        }
    }
}
