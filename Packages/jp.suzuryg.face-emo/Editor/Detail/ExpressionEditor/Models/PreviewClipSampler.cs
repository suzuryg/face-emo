using System;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Models
{
    internal sealed class PreviewClipSampler : IDisposable
    {
        private static readonly Vector3 Origin = new(100, 100, 100);

        public Vector3 AvatarViewPosition { get; private set; } = Origin;

        private readonly AV3Setting _av3Setting;

        [CanBeNull] private GameObject _previewAvatar;
        [CanBeNull] private AnimationClip _previewClip;
        private bool _isDisposed;

        public PreviewClipSampler(AV3Setting av3Setting)
        {
            _av3Setting = av3Setting;
        }

        public void Dispose()
        {
            _isDisposed = true;
            StopSampling();
            if (_previewAvatar != null) Object.DestroyImmediate(_previewAvatar);
        }

        public void SetPreviewClip(AnimationClip clip)
        {
            _previewClip = clip;
            StartSampling();
        }

        public void StartSampling()
        {
            if (_isDisposed || _previewAvatar == null || _previewClip == null) return;

            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(_previewAvatar, _previewClip, _previewClip.length);
            AnimationMode.EndSampling();

            _previewAvatar.transform.position = Origin;
            _previewAvatar.transform.rotation = Quaternion.identity;
        }

        public static void StopSampling() => AnimationMode.StopAnimationMode();

        public void FetchPreviewAvatar()
        {
            var avatarRoot = _av3Setting?.TargetAvatar?.gameObject;
            if (avatarRoot == null) return;

            if (_previewAvatar != null) Object.DestroyImmediate(_previewAvatar);
            _previewAvatar = Object.Instantiate(avatarRoot);
            if (_previewAvatar == null) return;

            var animator = _previewAvatar.GetComponent<Animator>();
            var desc = _previewAvatar.GetComponent<VRCAvatarDescriptor>();
            AvatarViewPosition = GetAvatarViewPosition(_previewAvatar, animator, desc);
            if (desc != null) Object.DestroyImmediate(desc);

            // FIXME: Unable to support the case that avatar's body shape balance is tuned by root object's scale.
            // (Is it necessary to assume this case...?)
            _previewAvatar.transform.position = Origin;
            _previewAvatar.transform.rotation = Quaternion.identity;
            _previewAvatar.transform.localScale = Vector3.one;
            _previewAvatar.hideFlags = HideFlags.HideAndDontSave;
            _previewAvatar.SetActive(true);
            return;

            static Vector3 GetAvatarViewPosition(GameObject clonedAvatar, Animator animator, VRCAvatarDescriptor desc)
            {
                if (desc == null) return Origin;

                // Returns view position if previewable in T-pose.
                if (AV3Utility.GetAvatarPoseClip(desc) != null)
                    return desc.GetScaledViewPosition() + Origin;

                // Returns head position if not previewable in T-pose.
                return GetAvatarHeadPosition(clonedAvatar, animator, desc);
            }

            static Vector3 GetAvatarHeadPosition(GameObject clonedAvatar, Animator animator, VRCAvatarDescriptor desc)
            {
                if (animator == null || !animator.isHuman || desc == null) return Origin;

                var clip = AV3Utility.GetAvatarPoseClip(desc);
                if (clip == null) clip = new AnimationClip();

                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(clonedAvatar, clip, clip.length);
                AnimationMode.EndSampling();

                clonedAvatar.transform.position = Origin;
                clonedAvatar.transform.rotation = Quaternion.identity;
                return animator.GetBoneTransform(HumanBodyBones.Head).position;
            }
        }
    }
}
