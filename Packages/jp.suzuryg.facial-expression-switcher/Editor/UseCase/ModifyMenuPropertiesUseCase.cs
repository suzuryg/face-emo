using Suzuryg.FacialExpressionSwitcher.Domain;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public interface IModifyMenuPropertiesUseCase
    {
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

        public void Handle(bool? writeDefaults = null, double? transitionDurationSeconds = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
