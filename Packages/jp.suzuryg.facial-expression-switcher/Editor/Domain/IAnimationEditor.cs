namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IAnimationEditor
    {
        IAnimation Create(string path);
        void Open(IAnimation animation);
    }
}
