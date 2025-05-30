﻿using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UniRx;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.Drawing
{
    public class MainThumbnailDrawer : ThumbnailDrawerBase
    {
        protected override int Width => _thumbnailSetting.Main_Width;
        protected override int Height => _thumbnailSetting.Main_Height;
        protected override float FOV => _thumbnailSetting.Main_FOV;
        protected override float Distance => _thumbnailSetting.Main_Distance;
        protected override float CameraPosX => _thumbnailSetting.Main_CameraPosX;
        protected override float CameraPosY => _thumbnailSetting.Main_CameraPosY;
        protected override float CameraAngleX => _thumbnailSetting.Main_CameraAngleV;
        protected override float CameraAngleY => _thumbnailSetting.Main_CameraAngleH;
        public MainThumbnailDrawer(AV3Setting aV3Setting, ThumbnailSetting thumbnailSetting) : base(aV3Setting, thumbnailSetting) { }
    }

    public class GestureTableThumbnailDrawer : ThumbnailDrawerBase
    {
        protected override int Width => _thumbnailSetting.GestureTable_Width;
        protected override int Height => _thumbnailSetting.GestureTable_Height;
        protected override float FOV => _thumbnailSetting.Main_FOV;
        protected override float Distance => _thumbnailSetting.Main_Distance;
        protected override float CameraPosX => _thumbnailSetting.Main_CameraPosX;
        protected override float CameraPosY => _thumbnailSetting.Main_CameraPosY;
        protected override float CameraAngleX => _thumbnailSetting.Main_CameraAngleV;
        protected override float CameraAngleY => _thumbnailSetting.Main_CameraAngleH;
        public GestureTableThumbnailDrawer(AV3Setting aV3Setting, ThumbnailSetting thumbnailSetting) : base(aV3Setting, thumbnailSetting) { }
    }

    public class ExMenuThumbnailDrawer : ThumbnailDrawerBase
    {
        protected override int Width => ThumbnailSetting.ExMenu_InnerWidth;
        protected override int Height => ThumbnailSetting.ExMenu_InnerHeight;
        protected override float FOV => _thumbnailSetting.Main_FOV;
        protected override float Distance => _thumbnailSetting.Main_Distance;
        protected override float CameraPosX => _thumbnailSetting.Main_CameraPosX;
        protected override float CameraPosY => _thumbnailSetting.Main_CameraPosY;
        protected override float CameraAngleX => _thumbnailSetting.Main_CameraAngleV;
        protected override float CameraAngleY => _thumbnailSetting.Main_CameraAngleH;
        public ExMenuThumbnailDrawer(AV3Setting aV3Setting, ThumbnailSetting thumbnailSetting) : base(aV3Setting, thumbnailSetting) { }
    }

    public class InspectorThumbnailDrawer : ThumbnailDrawerBase
    {
        protected override int Width => _thumbnailSetting.Inspector_Width;
        protected override int Height => _thumbnailSetting.Inspector_Height;
        protected override float FOV => _thumbnailSetting.Main_FOV;
        protected override float Distance => _thumbnailSetting.Main_Distance;
        protected override float CameraPosX => _thumbnailSetting.Main_CameraPosX;
        protected override float CameraPosY => _thumbnailSetting.Main_CameraPosY;
        protected override float CameraAngleX => _thumbnailSetting.Main_CameraAngleV;
        protected override float CameraAngleY => _thumbnailSetting.Main_CameraAngleH;
        public InspectorThumbnailDrawer(AV3Setting aV3Setting, ThumbnailSetting thumbnailSetting) : base(aV3Setting, thumbnailSetting) { }
    }

    public abstract class ThumbnailDrawerBase : IDisposable
    {
        // Constants
        private static readonly string EmptyClipKey = "EmptyClipKey";

        // Properties
        protected abstract int Width { get; }
        protected abstract int Height { get; }
        protected abstract float FOV { get; }
        protected abstract float Distance { get; }
        protected abstract float CameraPosX { get; }
        protected abstract float CameraPosY { get; }
        protected abstract float CameraAngleX { get; }
        protected abstract float CameraAngleY { get; }

        // Observables
        public IObservable<Unit> OnThumbnailUpdated => _onThumbnailUpdated.AsObservable();
        private Subject<Unit> _onThumbnailUpdated = new Subject<Unit>();

        // Dependencies
        private AV3Setting _aV3Setting;
        protected ThumbnailSetting _thumbnailSetting;

        // Other fields
        private HashSet<string> _requests = new HashSet<string>();
        private Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();
        private Texture2D _hourglassIcon;
        private Texture2D _errorIcon;
        private Scene _previewScene;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public ThumbnailDrawerBase(AV3Setting aV3Setting, ThumbnailSetting thumbnailSetting)
        {
            // Dependencies
            _aV3Setting = aV3Setting;
            _thumbnailSetting = thumbnailSetting;

            // Others
            _hourglassIcon = ViewUtility.GetIconTexture("hourglass_empty_FILL0_wght400_GRAD200_opsz300.png");
            _errorIcon = ViewUtility.GetIconTexture("error_FILL0_wght400_GRAD200_opsz300.png");
            NullChecker.Check(_hourglassIcon, _errorIcon);

            // Update thumbnails when animation is updated
            // (Called after updating animation and saving with Ctrl-S)
            AssetUpdateDetector.OnAnimationClipUpdated.Synchronize().ObserveOnMainThread().Subscribe(guids =>
            {
                foreach (var guid in guids)
                {
                    if (_cache.ContainsKey(guid))
                    {
                        _requests.Add(guid);
                    }
                }
            }).AddTo(_disposables);

            // Periodic update
            if (this is MainThumbnailDrawer || this is GestureTableThumbnailDrawer)
            {
                Observable.FromCoroutine(PeriodicUpdate).Subscribe(_ => _onThumbnailUpdated.OnNext(Unit.Default)).AddTo(_disposables);
            }
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        public Texture2D GetThumbnail(Domain.Animation animation)
        {
            var guid = GetGUID(animation);

            if (_cache.ContainsKey(guid))
            {
                return _cache[guid];
            }
            else
            {
                _requests.Add(guid);
                return _hourglassIcon;
            }
        }

        private string GetGUID(Domain.Animation animation)
        {
            var guid = animation?.GUID;
            if (string.IsNullOrEmpty(guid))
            {
                guid = EmptyClipKey;
            }
            return guid;
        }

        public void RequestUpdate(AnimationClip animationClip)
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animationClip));
            if (!string.IsNullOrEmpty(guid))
            {
                _requests.Add(guid);
            }
        }

        public void RequestUpdateAll()
        {
            var guids = new List<string>(_cache.Keys);
            foreach (var guid in guids)
            {
                _requests.Add(guid);
            }
        }

        public void Update()
        {
            var enumerator = UpdateCoroutine();
            while (enumerator.MoveNext());
        }

        private IEnumerator PeriodicUpdate()
        {
            while (true)
            {
                yield return UpdateCoroutine();
            }
        }

        // private static Unity.Profiling.ProfilerMarker _customMarker = new Unity.Profiling.ProfilerMarker($"{nameof(ThumbnailDrawerBase)}.{nameof(UpdateCoroutine)}");

        private IEnumerator UpdateCoroutine()
        {
            // using (_customMarker.Auto()){

            // If there is no request for thumbnail generation, it is not executed (does not trigger a GameObject Instantiate).
            if (!_requests.Any())
            {
                yield break;
            }

            // Get Animator
            var targetAvatar = _aV3Setting.TargetAvatar as VRCAvatarDescriptor;
            var avatarAnimator = AV3Utility.GetAnimator(_aV3Setting);
            if (targetAvatar == null || avatarAnimator == null)
            {
                yield break;
            }

            // Prepare objects
            InitializePreviewScene();
            GameObject clonedAvatar = null;
            GameObject cameraRoot = new GameObject();
            try
            {
                // Clone avatar
                clonedAvatar = UnityEngine.Object.Instantiate(avatarAnimator.gameObject);
                // FIXME: Unable to support the case that avatar's body shape balance is tuned by root object's scale. (Is it necessary to assume this case...?)
                clonedAvatar.transform.localScale = Vector3.one;
                SceneManager.MoveGameObjectToScene(clonedAvatar, _previewScene);
                clonedAvatar.transform.position = Vector3.zero;
                clonedAvatar.transform.rotation = Quaternion.identity;
                clonedAvatar.SetActive(true);

                // Get camera
                var camera = cameraRoot.AddComponent<Camera>();
                camera.scene = _previewScene;
                SceneManager.MoveGameObjectToScene(cameraRoot, _previewScene);

                // Adjust the camera position to the avatar's head
                var animator = clonedAvatar.GetComponent<Animator>();
                float x = 0;
                float y = 0;
                if (animator != null)
                {
                    if (animator.isHuman)
                    {
                        camera.transform.parent = animator.GetBoneTransform(HumanBodyBones.Head);

                        var leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                        x = Mathf.Lerp(
                            -ThumbnailSetting.CameraPosXCoef * leftShoulder.position.x,
                            ThumbnailSetting.CameraPosXCoef * leftShoulder.position.x,
                            CameraPosX);

                        var distance = Math.Abs(animator.GetBoneTransform(HumanBodyBones.Neck).position.y - targetAvatar.GetScaledViewPosition().y);
                        y = Mathf.Lerp(-distance, distance, CameraPosY);
                    }
                    else
                    {
                        camera.transform.parent = animator.transform;

                        x = Mathf.Lerp(-1, 1, CameraPosX);
                        y = Mathf.Lerp(-1, 1, CameraPosY);
                    }
                }

                camera.orthographic = false;
                camera.nearClipPlane = 0.01f;
                camera.fieldOfView = FOV;
                camera.transform.position = new Vector3(x, targetAvatar.GetScaledViewPosition().y + y, Distance);
                cameraRoot.transform.rotation = Quaternion.Euler(0, 180, 0);
                camera.transform.RotateAround(targetAvatar.GetScaledViewPosition(), Vector3.left, CameraAngleX);
                camera.transform.RotateAround(clonedAvatar.transform.position, Vector3.down, CameraAngleY);

                // Generate thumbnails
                var requests = new List<string>(_requests);
                foreach (var guid in requests)
                {
                    _cache[guid] = RenderAnimatedAvatar(guid, clonedAvatar, camera);
                    _requests.Remove(guid);
                    yield return null;
                }
            }
            finally
            {
                if (cameraRoot != null)
                {
                    UnityEngine.Object.DestroyImmediate(cameraRoot);
                }
                if (clonedAvatar != null)
                {
                    UnityEngine.Object.DestroyImmediate(clonedAvatar);
                }
                EditorSceneManager.ClosePreviewScene(_previewScene);
            }

            // } end of using (_customMarker.Auto())
        }

        private void InitializePreviewScene()
        {
            // Open scene
            _previewScene = EditorSceneManager.NewPreviewScene();
            // Remove skybox for transparency
            RenderSettings.skybox = null;

            // Add light
            var light = new GameObject();
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            light.AddComponent<Light>().type = LightType.Directional;
            SceneManager.MoveGameObjectToScene(light, _previewScene);
        }

        private Texture2D RenderAnimatedAvatar(string clipGUID, GameObject animatorRoot, Camera camera)
        {
            // Get animation clip
            AnimationClip clip;
            if (clipGUID == EmptyClipKey)
            {
                clip = new AnimationClip();
            }
            else
            {
                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(clipGUID));
            }

            // If animation clip is not found, return error icon
            if (clip == null)
            {
                return _errorIcon;
            }

            // Synthesize avatar pose and sample animation
            var synthesized = AV3Utility.SynthesizeAvatarPose(clip, _aV3Setting?.TargetAvatar as VRCAvatarDescriptor);
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(animatorRoot, synthesized, synthesized.length);
            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();

            // Compute render size
            int w = Mathf.RoundToInt(Width * DetailConstants.UiScale);
            int h = Mathf.RoundToInt(Height * DetailConstants.UiScale);

            // Grab a temporary ARGB32 RenderTexture and clear it
            var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, antiAliasing: 1);
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = prevActive;

            // Cache camera state and enforce transparent clear
            var oldRt     = camera.targetTexture;
            var oldAsp    = camera.aspect;
            var oldFlags  = camera.clearFlags;
            var oldBg     = camera.backgroundColor;
            camera.targetTexture   = rt;
            camera.aspect          = (float)w / h;
            camera.clearFlags      = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;

            // Render avatar
            camera.Render();

            // Restore camera state
            camera.targetTexture   = oldRt;
            camera.aspect          = oldAsp;
            camera.clearFlags      = oldFlags;
            camera.backgroundColor = oldBg;

            // Read back into Texture2D
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = null;

            // Un-premultiply alpha
            var pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float a = pixels[i].a;
                if (a > 0f)
                {
                    pixels[i].r /= a;
                    pixels[i].g /= a;
                    pixels[i].b /= a;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            // Mark texture as transparency
            tex.alphaIsTransparency = true;
            return tex;
        }
    }
}
