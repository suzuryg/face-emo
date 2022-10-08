namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class NullChecker
    {
        public static void Check(params object[] objects)
        {
            foreach (var obj in objects)
            {
                if (obj is null)
                {
                    throw new FacialExpressionSwitcherException($"{nameof(obj)} can't be null.");
                }
            }
        }
    }
}
