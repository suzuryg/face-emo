using Suzuryg.FaceEmo.Components.MA;
using System;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public class AvatarProcessor : IVRCSDKPreprocessAvatarCallback
    {
        // Place after EditorOnly processing (which runs at -1024) and before modular avatar processing (which runs at -25). 
        public int callbackOrder => -50;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            try
            {
                ProcessAvatar(avatarGameObject);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        public static void ProcessAvatar(GameObject avatarGameObject)
        {
            foreach (var component in avatarGameObject.GetComponentsInChildren<RunBeforeModularAvatar>(includeInactive: true))
            {
                if (!component.enabled) { continue; }
                component.OnPreProcessAvatar();
            }
        }
    }
}
