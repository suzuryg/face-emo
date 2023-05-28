namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class DefaultsProvider
    {
        public bool? ChangeDefaultFace { get; set; } = null;
        public bool? UseAnimationNameAsDisplayName { get; set; } = null;
        public EyeTrackingControl? EyeTrackingControl { get; set; } = null;
        public MouthTrackingControl? MouthTrackingControl { get; set; } = null;
        public bool? BlinkEnabled { get; set; } = null;
        public bool? MouthMorphCancelerEnabled { get; set; } = null;
    }
}
