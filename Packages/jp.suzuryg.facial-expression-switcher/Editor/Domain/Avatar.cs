namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class Avatar
    {
        public string Path { get; }

        public Avatar(string path)
        {
            if (path.StartsWith("/"))
            {
                Path = path;
            }
            else
            {
                throw new FacialExpressionSwitcherException($"{path} is not a full path.");
            }
        }
    }
}
