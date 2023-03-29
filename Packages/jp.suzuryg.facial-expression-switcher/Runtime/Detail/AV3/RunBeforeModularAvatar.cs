using UnityEngine;
using VRC.SDK3.Avatars.Components;
using nadena.dev.modular_avatar.core;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    [DefaultExecutionOrder(-10000)] // run before modular avatar
    public abstract class RunBeforeModularAvatar : AvatarTagComponent // Inherit AvatarTagComponent to register in VRChat whitelist
    {
#if UNITY_EDITOR
        private static bool isPlaying => UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
        private static bool isPlaying => true;
#endif
        /**
        * For the moment, no support for AV3Emu.
        * When using GestureManager, it appears that the changes in the current version of ModularAvatar are not reflected.
        * If GestureManager is started by following the procedure below, the changes will be reflected, so let's use that as a work-around (without using the "Enter PlayMode" button).
        * (1) Press the Play button with no GestureManager object selected.
        * (2) Switch to Scene view.
        * (3) Select the GesureManager object.
        */
        private void Start()
        {
            if (!isPlaying || this == null) return;
            try
            {
                OnPreProcessAvatar();
            }
            finally
            {
                DestroyImmediate(this);
            }
        }

        protected abstract void OnPreProcessAvatar();

        protected VRCAvatarDescriptor GetAvatarDescriptor()
        {
            var target = gameObject.transform;

            while (target != null)
            {
                var av = target.GetComponent<VRCAvatarDescriptor>();
                if (av != null) return av;
                target = target.parent;
            }

            return null;
        }
    }
}
