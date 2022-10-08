using Suzuryg.FacialExpressionSwitcher.Domain;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public class MockMenuRepository : IMenuRepository
    {
        Menu IMenuRepository.Load(string source)
        {
            return new Menu();
        }

        void IMenuRepository.Save(string destination, Menu menu)
        {
            // NOP
        }
    }
}
