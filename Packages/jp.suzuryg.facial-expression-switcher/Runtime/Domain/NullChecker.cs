namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class NullChecker
    {
        public static void Check(params object[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] is null)
                {
                    throw new FacialExpressionSwitcherException($"Arguments can't be null. Index: {i}");
                }
            }
        }
    }
}
