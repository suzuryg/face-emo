namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IMenuRepository
    {
        void Save(string destination, Menu menu);
        bool Exists(string source);
        Menu Load(string source);
    }
}
