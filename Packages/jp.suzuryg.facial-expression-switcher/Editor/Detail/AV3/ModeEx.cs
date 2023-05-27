using Suzuryg.FacialExpressionSwitcher.Domain;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class ModeEx
    {
        public string PathToMode { get; set; }
        public int DefaultEmoteIndex { get; set; }
        public ModeExInner Mode { get; set; }
    }

    public class ModeExInner : IMode
    {
        private IMode _mode;

        public ModeExInner(IMode mode)
        {
            _mode = mode;
        }

        public bool ChangeDefaultFace => _mode.ChangeDefaultFace;
        public string DisplayName => _mode.DisplayName;
        public IReadOnlyList<IBranch> Branches => _mode.Branches;
        public IMenuItemList Parent => _mode.Parent;
        public IBranch GetGestureCell(HandGesture left, HandGesture right) => _mode.GetGestureCell(left, right);
        public string GetId() => _mode.GetId();

        // Return constants if ChangeDefaultFace is false.
        public bool UseAnimationNameAsDisplayName => _mode.ChangeDefaultFace ? _mode.UseAnimationNameAsDisplayName : false;
        public EyeTrackingControl EyeTrackingControl => _mode.ChangeDefaultFace ? _mode.EyeTrackingControl : EyeTrackingControl.Tracking;
        public MouthTrackingControl MouthTrackingControl => _mode.ChangeDefaultFace ? _mode.MouthTrackingControl : MouthTrackingControl.Tracking;
        public bool BlinkEnabled => _mode.ChangeDefaultFace ? _mode.BlinkEnabled : true;
        public bool MouthMorphCancelerEnabled => _mode.ChangeDefaultFace ? _mode.MouthMorphCancelerEnabled : true;
        public Animation Animation => _mode.ChangeDefaultFace ? _mode.Animation : null;
    }
}
