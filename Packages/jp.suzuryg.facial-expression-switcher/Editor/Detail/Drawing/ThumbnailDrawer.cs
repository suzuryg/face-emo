using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Drawing
{
    public class MainThumbnailDrawer : ThumbnailDrawerBase
    {
        public override string WidthKey => DetailConstants.KeyMainThumbnailWidth;
        public override string HeightKey => DetailConstants.KeyMainThumbnailHeight;
        public override int DefaultWidth => DetailConstants.DefaultMainThumbnailWidth;
        public override int DefaultHeight => DetailConstants.DefaultMainThumbnailHeight;
        public MainThumbnailDrawer(AV3Setting aV3Setting) : base(aV3Setting) { }
    }

    public class GestureTableThumbnailDrawer : ThumbnailDrawerBase
    {
        public override string WidthKey => DetailConstants.KeyGestureThumbnailWidth;
        public override string HeightKey => DetailConstants.KeyGestureThumbnailHeight;
        public override int DefaultWidth => DetailConstants.DefaultGestureThumbnailWidth;
        public override int DefaultHeight => DetailConstants.DefaultGestureThumbnailHeight;
        public GestureTableThumbnailDrawer(AV3Setting aV3Setting) : base(aV3Setting) { }
    }

    public abstract class ThumbnailDrawerBase
    {
        // Constants
        private static readonly string EmptyClipKey = "EmptyClipKey";

        // Properties
        public abstract string WidthKey { get; }
        public abstract string HeightKey { get; }
        public abstract int DefaultWidth { get; }
        public abstract int DefaultHeight { get; }

        // Dependencies
        private AV3Setting _aV3Setting;

        // Other fields
        private HashSet<string> _requests = new HashSet<string>();
        private Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();
        private Texture2D _errorIcon;
        private object _lockRequests = new object();

        public ThumbnailDrawerBase(AV3Setting aV3Setting)
        {
            // Dependencies
            _aV3Setting = aV3Setting;

            // Others
            _errorIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/error_FILL0_wght400_GRAD200_opsz300.png");
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

        public void ClearCache()
        {
            _cache.Clear();
        }

        public void Update()
        {
            // When updating thumbnails in Play mode, the following error occurs in VRC.Dynamics.PhysBoneManager.
            // "Buffer already contains chain of id:XXXX"
            if (EditorApplication.isPlaying)
            {
                return;
            }

            // Get Animator
            // Error occurs when returning from Play mode to Edit mode if Null conditional operator is used to determine Null
            if (_aV3Setting == null || _aV3Setting.TargetAvatar == null)
            {
                return;
            }
            var avatarAnimator = _aV3Setting.TargetAvatar.GetComponent<Animator>();
            if (avatarAnimator == null)
            {
                return;
            }

            // Prepare objects
            var wasActive = avatarAnimator.gameObject.activeSelf;
            GameObject clonedAvatar = null;
            GameObject cameraRoot = new GameObject();
            try
            {
                // Clone avatar
                clonedAvatar = UnityEngine.Object.Instantiate(avatarAnimator.gameObject);
                clonedAvatar.SetActive(true);
                avatarAnimator.gameObject.SetActive(false);

                // Clone camera
                var camera = cameraRoot.AddComponent<Camera>();
                camera.CopyFrom(SceneView.lastActiveSceneView.camera);

                // Adjust the camera position to the avatar's head
                var animator = clonedAvatar.GetComponent<Animator>();
                if (animator is Animator)
                {
                    if (animator.isHuman)
                    {
                        camera.transform.parent = animator.GetBoneTransform(HumanBodyBones.Head);
                    }
                    else
                    {
                        camera.transform.parent = animator.transform;
                    }
                }

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

                    // Reset animator status
                    clonedAvatar.SetActive(false);
                    clonedAvatar.SetActive(true);
                }
            }
            finally
            {
                avatarAnimator.gameObject.SetActive(wasActive);
                if (cameraRoot is GameObject)
                {
                    UnityEngine.Object.DestroyImmediate(cameraRoot);
                }
                if (clonedAvatar is GameObject)
                {
                    UnityEngine.Object.DestroyImmediate(clonedAvatar);
                }
            }
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
            if (clip is null)
            {
                return _errorIcon;
            }

            // Sample animation clip and render
            var positionCache = animatorRoot.transform.position;
            var rotationCache = animatorRoot.transform.rotation;
            try
            {
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(animatorRoot, clip, clip.length);
                AnimationMode.EndSampling();

                // When sampling, the object relocates to the origin, so it must be restored to its initial position
                animatorRoot.transform.position = positionCache;
                animatorRoot.transform.rotation = rotationCache;

                var width = EditorPrefs.HasKey(WidthKey) ? EditorPrefs.GetInt(WidthKey) : DefaultWidth;
                var height = EditorPrefs.HasKey(HeightKey) ? EditorPrefs.GetInt(HeightKey) : DefaultHeight;
                var texture = GetRenderedTexture(width, height, camera);

                return texture;
            }
            finally
            {
                AnimationMode.StopAnimationMode();
                animatorRoot.transform.position = positionCache;
                animatorRoot.transform.rotation = rotationCache;
            }
        }

        private static Texture2D GetRenderedTexture(int width, int height, Camera camera)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);

            var renderTexture = RenderTexture.GetTemporary(texture.width, texture.height,
                format: RenderTextureFormat.ARGB32, readWrite: RenderTextureReadWrite.sRGB, depthBuffer: 24, antiAliasing: 8);
            try
            {
                renderTexture.wrapMode = TextureWrapMode.Clamp;
                renderTexture.filterMode = FilterMode.Bilinear;

                RenderCamera(renderTexture, camera);
                CopyRenderTexture(renderTexture, texture);
            }
            finally
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            return texture;
        }

        private static void RenderCamera(RenderTexture renderTexture, Camera camera)
        {
            var targetTextureCache = camera.targetTexture;
            var aspectCache = camera.aspect;
            try
            {
                camera.targetTexture = renderTexture;
                camera.aspect = (float) renderTexture.width / renderTexture.height;
                camera.Render();
            }
            finally
            {
                camera.targetTexture = targetTextureCache;
                camera.aspect = aspectCache;
            }
        }

        private static void CopyRenderTexture(RenderTexture source, Texture2D destination)
        {
            var activeRenderTextureCache = RenderTexture.active;
            try
            {
                RenderTexture.active = source;
                destination.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, recalculateMipMaps: false);
                destination.Apply();
            }
            finally
            {
                RenderTexture.active = activeRenderTextureCache;
            }
        }
    }
}
