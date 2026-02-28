using System;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using Debug = UnityEngine.Debug;

namespace Suzuryg.FaceEmo.Detail.AV3.Importers
{
    internal static class FxDisableSwitchDetector
    {
        public static bool Detect(VRCAvatarDescriptor avatarDescriptor)
        {
            try
            {
                var fx = ImportUtility.GetFxLayer(avatarDescriptor);
                if (fx == null) return false;

                foreach (var layer in fx.layers)
                {
                    foreach (var state in layer.stateMachine.states)
                    {
                        foreach (var behaviour in state.state.behaviours)
                        {
                            if (behaviour is VRCPlayableLayerControl control &&
                                control.layer == VRC_PlayableLayerControl.BlendableLayer.FX &&
                                control.goalWeight == 0)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError("[FaceEmo] Failed to detect FxDisableSwitch. " + ex);
                return false;
            }
        }
    }
}
