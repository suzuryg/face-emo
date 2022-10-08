namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IMenuRepository
    {
        void Save(string destination, Menu menu);
        Menu Load(string source);
    }
}
