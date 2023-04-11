using Suzuryg.FacialExpressionSwitcher.Domain;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class ModeEx
    {
        public string PathToMode { get; set; }
        public int DefaultEmoteIndex { get; set; }
        public IMode Mode { get; set; }
    }
}
