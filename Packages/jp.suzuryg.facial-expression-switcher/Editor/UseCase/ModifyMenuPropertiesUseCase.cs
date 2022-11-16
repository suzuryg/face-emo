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

        public ModifyMenuPropertiesUseCase()
        {
        }

        public void Handle(bool? writeDefaults = null, double? transitionDurationSeconds = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
