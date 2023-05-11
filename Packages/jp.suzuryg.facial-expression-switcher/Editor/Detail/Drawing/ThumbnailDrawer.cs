using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UniRx;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Drawing
{
    public class MainThumbnailDrawer : ThumbnailDrawerBase
    {
        protected override int Width => _thumbnailSetting.Main_Width;
        protected override int Height => _thumbnailSetting.Main_Height;
        protected override float OrthoSize => _thumbnailSetting.Main_OrthoSize;
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
        protected override float OrthoSize => _thumbnailSetting.Main_OrthoSize;
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
        protected override float OrthoSize => _thumbnailSetting.Main_OrthoSize;
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
        protected override float OrthoSize => _thumbnailSetting.Main_OrthoSize;
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
        protected abstract float OrthoSize { get; }
        protected abstract float CameraPosX { get; }
        protected abstract float CameraPosY { get; }
        protected abstract float CameraAngleX { get; }
        protected abstract float CameraAngleY { get; }

        // Dependencies
        private AV3Setting _aV3Setting;
        protected ThumbnailSetting _thumbnailSetting;

        // Other fields
        private HashSet<string> _requests = new HashSet<string>();
        private Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();
        private Texture2D _errorIcon;
        private object _lockRequests = new object();
        private Scene _previewScene;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public ThumbnailDrawerBase(AV3Setting aV3Setting, ThumbnailSetting thumbnailSetting)
        {
            // Dependencies
            _aV3Setting = aV3Setting;
            _thumbnailSetting = thumbnailSetting;

            // Others
            _errorIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/error_FILL0_wght400_GRAD200_opsz300.png");

            // Update thumbnails when animation is updated
            // (Called after updating animation and saving with Ctrl-S)
            AssetUpdateDetector.OnAnimationClipUpdated.Synchronize().ObserveOnMainThread().Subscribe(guids =>
            {
                foreach (var guid in guids)
                {
                    if (_cache.ContainsKey(guid))
                    {
                        lock (_lockRequests)
                        {
                            _requests.Add(guid);
                        }
                    }
                }
            }).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        public Texture2D GetThumbnail(Domain.Animation animation)
        {
            var guid = animation?.GUID;
            if (string.IsNullOrEmpty(guid))
            {
                guid = EmptyClipKey;
            }

            if (_cache.ContainsKey(guid))
            {
                return _cache[guid];
            }
            else
            {
                lock (_lockRequests)
                {
                    _requests.Add(guid);
                }
                return _errorIcon;
            }
        }

        public void RequestUpdate(AnimationClip animationClip)
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animationClip));
            if (!string.IsNullOrEmpty(guid))
            {
                lock (_lockRequests)
                {
                    _requests.Add(guid);
                }
            }
        }

        public void ClearCache()
        {
            _cache.Clear();
        }

        // private static Unity.Profiling.ProfilerMarker _customMarker = new Unity.Profiling.ProfilerMarker($"{nameof(ThumbnailDrawerBase)}.{nameof(Update)}");

        public void Update()
        {
            // using (_customMarker.Auto()){

            // If there is no request for thumbnail generation, it is not executed (does not trigger a GameObject Instantiate).
            lock (_lockRequests)
            {
                if (!_requests.Any())
                {
                    return;
                }
            }

            // Get Animator
            var avatarAnimator = AV3Utility.GetAnimator(_aV3Setting);
            if (avatarAnimator == null)
            {
                return;
            }

            // Prepare objects
            InitializePreviewScene();
            GameObject clonedAvatar = null;
            GameObject cameraRoot = new GameObject();
            try
            {
                // Clone avatar
                clonedAvatar = UnityEngine.Object.Instantiate(avatarAnimator.gameObject);
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

                        var distance = Math.Abs(animator.GetBoneTransform(HumanBodyBones.Neck).position.y - _aV3Setting.TargetAvatar.ViewPosition.y);
                        y = Mathf.Lerp(-distance, distance, CameraPosY);
                    }
                    else
                    {
                        camera.transform.parent = animator.transform;

                        x = Mathf.Lerp(-1, 1, CameraPosX);
                        y = Mathf.Lerp(-1, 1, CameraPosY);
                    }
                }

                camera.orthographic = true;
                camera.orthographicSize = OrthoSize;
                camera.transform.position = new Vector3(x, _aV3Setting.TargetAvatar.ViewPosition.y + y, 1);
                cameraRoot.transform.rotation = Quaternion.Euler(0, 180, 0);
                camera.transform.RotateAround(_aV3Setting.TargetAvatar.ViewPosition, Vector3.left, CameraAngleX);
                camera.transform.RotateAround(clonedAvatar.transform.position, Vector3.down, CameraAngleY);

                // Generate thumbnails
                List<string> requests;
                lock (_lockRequests)
                {
                    requests = new List<string>(_requests);
                    _requests.Clear();
                }
                foreach (var guid in requests)
                {
                    _cache[guid] = RenderAnimatedAvatar(guid, clonedAvatar, camera);

                    // Padding if necessary
                    if (this is ExMenuThumbnailDrawer && _cache[guid] != null)
                    {
                        _cache[guid] = DrawingUtility.PaddingWithTransparentPixels(_cache[guid], ThumbnailSetting.ExMenu_OuterWidth, ThumbnailSetting.ExMenu_OuterHeight);
                    }

                    // Reset animator status
                    clonedAvatar.SetActive(false);
                    clonedAvatar.SetActive(true);
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

            // Synthesize avatar pose
            var synthesized = AV3Utility.SynthesizeAvatarPose(clip);

            // Sample animation clip and render
            var positionCache = animatorRoot.transform.position;
            var rotationCache = animatorRoot.transform.rotation;
            try
            {
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(animatorRoot, synthesized, synthesized.length);
                AnimationMode.EndSampling();

                // When sampling, the object relocates to the origin, so it must be restored to its initial position
                animatorRoot.transform.position = positionCache;
                animatorRoot.transform.rotation = rotationCache;

                var texture = DrawingUtility.GetRenderedTexture(Width, Height, camera);

                return texture;
            }
            finally
            {
                AnimationMode.StopAnimationMode();
                animatorRoot.transform.position = positionCache;
                animatorRoot.transform.rotation = rotationCache;
            }
        }
    }
}
