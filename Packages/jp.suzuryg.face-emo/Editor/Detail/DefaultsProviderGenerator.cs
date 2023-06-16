using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Detail.AV3;

namespace Suzuryg.FaceEmo.Detail
{
    public class DefaultsProviderGenerator
    {
        private AV3Setting _aV3Setting;

        public DefaultsProviderGenerator(AV3Setting aV3Setting)
        {
            _aV3Setting = aV3Setting;
        }

        public DefaultsProvider Generate()
        {
            return new DefaultsProvider()
            {
                ChangeDefaultFace = _aV3Setting.ExpressionDefaults_ChangeDefaultFace,
                UseAnimationNameAsDisplayName = _aV3Setting.ExpressionDefaults_UseAnimationNameAsDisplayName,
                EyeTrackingControl = _aV3Setting.ExpressionDefaults_EyeTrackingEnabled ? EyeTrackingControl.Tracking : EyeTrackingControl.Animation,
                MouthTrackingControl = _aV3Setting.ExpressionDefaults_MouthTrackingEnabled ? MouthTrackingControl.Tracking : MouthTrackingControl.Animation,
                BlinkEnabled = _aV3Setting.ExpressionDefaults_BlinkEnabled,
                MouthMorphCancelerEnabled = _aV3Setting.ExpressionDefaults_MouthMorphCancelerEnabled,
            };
        }
    }
}
