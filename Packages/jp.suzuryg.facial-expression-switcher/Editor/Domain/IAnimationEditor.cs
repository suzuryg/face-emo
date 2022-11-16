namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public interface IAnimationEditor
    {
        Animation Create(string path);
        void Open(Animation animation);
    }
}
