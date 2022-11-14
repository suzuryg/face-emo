using Suzuryg.FacialExpressionSwitcher.Domain;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public class MockMenuRepository : IMenuRepository
    {
        void IMenuRepository.Save(string destination, Menu menu)
        {
            // NOP
        }

        bool IMenuRepository.Exists(string source) => true;

        Menu IMenuRepository.Load(string source)
        {
            return new Menu();
        }
    }
}
