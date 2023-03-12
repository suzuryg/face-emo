using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
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

        private void Awake()
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
