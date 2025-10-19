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

            // FIXME: Unable to support the case that avatar's body shape balance is tuned by root object's scale.
            // (Is it necessary to assume this case...?)
            _previewAvatar.transform.position = Origin;
            _previewAvatar.transform.rotation = Quaternion.identity;
            _previewAvatar.transform.localScale = Vector3.one;
            _previewAvatar.hideFlags = HideFlags.HideAndDontSave;
            _previewAvatar.SetActive(true);
        }

        public Vector3 GetAvatarViewPosition()
        {
            var avatarDescriptor = _previewAvatar?.GetComponent<VRCAvatarDescriptor>();
            if (avatarDescriptor == null) return Origin;

            // Returns view position if previewable in T-pose.
            if (AV3Utility.GetAvatarPoseClip(avatarDescriptor) != null)
                return avatarDescriptor.GetScaledViewPosition() + Origin;

            // Returns head position if not previewable in T-pose.
            return GetAvatarHeadPosition();
        }

        private Vector3 GetAvatarHeadPosition()
        {
            var animator = _previewAvatar?.GetComponent<Animator>();
            var avatarDescriptor = _previewAvatar?.GetComponent<VRCAvatarDescriptor>();
            if (animator == null || !animator.isHuman || avatarDescriptor == null) return Origin;

            var clip = AV3Utility.GetAvatarPoseClip(_av3Setting?.TargetAvatar as VRCAvatarDescriptor);
            if (clip == null) clip = new AnimationClip();

            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(_previewAvatar, clip, clip.length);
            AnimationMode.EndSampling();

            _previewAvatar.transform.position = Origin;
            _previewAvatar.transform.rotation = Quaternion.identity;
            return animator.GetBoneTransform(HumanBodyBones.Head).position;
        }
    }
}
