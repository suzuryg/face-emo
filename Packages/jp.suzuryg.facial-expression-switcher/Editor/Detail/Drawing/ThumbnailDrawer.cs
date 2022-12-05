using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Drawing
{
    public class ThumbnailDrawer
    {
        private static readonly string EmptyClipKey = "EmptyClipKey";

        private Animator _avatarAnimator;
        private Dictionary<string, (Texture2D main, Texture2D gesture)> _cache = new Dictionary<string, (Texture2D main, Texture2D gesture)>();
        private HashSet<string> _prioritized = new HashSet<string>();

        private Texture2D _errorIcon;

        public ThumbnailDrawer()
        {
            _errorIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/error_FILL0_wght400_GRAD200_opsz48.png");
        }

        public void SetAvatar(Animator avatarAnimator)
        {
            _avatarAnimator = avatarAnimator;
        }

        public (Texture2D main, Texture2D gesture) GetThumbnail(Domain.Animation animation)
        {
            var guid = animation?.GUID;
            if (string.IsNullOrEmpty(guid))
            {
                guid = EmptyClipKey;
            }

            if (!_cache.ContainsKey(guid))
            {
                Update(new[] { guid });
            }

            if (_cache.ContainsKey(guid))
            {
                return _cache[guid];
            }
            else
            {
                return (_errorIcon, _errorIcon);
            }
        }

        public void Prioritize(Domain.Animation animation)
        {
            var guid = animation?.GUID;
            if (string.IsNullOrEmpty(guid))
            {
                guid = EmptyClipKey;
            }
            _prioritized.Add(guid);
        }

        public void ResetPriority()
        {
            _prioritized.Clear();
        }
        
        public void RemoveCache(Domain.Animation animation)
        {
            if (animation is Domain.Animation && animation.GUID is string && _cache.ContainsKey(animation.GUID))
            {
                _cache.Remove(animation.GUID);
            }
        }

        public void UpdateAll()
        {
            var latter = _cache.Keys.Where(x => !_prioritized.Contains(x));
            var guids = _prioritized.Concat(latter).ToList();
            Update(guids);
        }

        // TODO: Use IEnumerable
        private void Update(IReadOnlyList<string> guids)
        {
            if (_avatarAnimator is null)
            {
                return;
            }

            var wasActive = _avatarAnimator.gameObject.activeSelf;
            GameObject clonedAvatar = null;
            GameObject cameraRoot = new GameObject();
            try
            {
                clonedAvatar = UnityEngine.Object.Instantiate(_avatarAnimator.gameObject);
                clonedAvatar.SetActive(true);
                _avatarAnimator.gameObject.SetActive(false);

                var camera = GetCamera(cameraRoot);

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

                // TODO: Use foreach
                for (int i = 0; i< guids.Count; i++)
                {
                    var guid = guids[i];

                    var texture = Render(guid, clonedAvatar, camera);
                    _cache[guid] = texture;

                    // Workaround for updating muscles
                    clonedAvatar.SetActive(false);
                    clonedAvatar.SetActive(true);
                }
            }
            finally
            {
                _avatarAnimator.gameObject.SetActive(wasActive);
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

        // From CGE
        private Camera GetCamera(GameObject cameraRoot)
        {
            var camera = cameraRoot.AddComponent<Camera>();

            var sceneCamera = SceneView.lastActiveSceneView.camera;
            camera.transform.position = sceneCamera.transform.position;
            camera.transform.rotation = sceneCamera.transform.rotation;

            var whRatio = (1f * sceneCamera.pixelWidth / sceneCamera.pixelHeight);
            camera.fieldOfView = whRatio < 1 ? sceneCamera.fieldOfView * whRatio : sceneCamera.fieldOfView;
            camera.orthographic = sceneCamera.orthographic;
            camera.nearClipPlane = sceneCamera.nearClipPlane;
            camera.farClipPlane = sceneCamera.farClipPlane;
            camera.orthographicSize = sceneCamera.orthographicSize;

            return camera;
        }

        // From CGE
        private (Texture2D main, Texture2D gesture) Render(string clipGUID, GameObject clonedAvatar, Camera camera)
        {
            AnimationClip clip;
            if (clipGUID == EmptyClipKey)
            {
                clip = new AnimationClip();
            }
            else
            {
                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(clipGUID));
            }

            if (clip is null)
            {
                return (_errorIcon, _errorIcon);
            }

            var initPos = clonedAvatar.transform.position;
            var initRot = clonedAvatar.transform.rotation;

            try
            {
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(clonedAvatar, clip, clip.length);
                AnimationMode.EndSampling();

                // Workaround for moving origin
                clonedAvatar.transform.position = initPos;
                clonedAvatar.transform.rotation = initRot;

                var mainSize = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailSize) : DetailConstants.MinMainThumbnailSize;
                var gestureSize = EditorPrefs.HasKey(DetailConstants.KeyGestureThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyGestureThumbnailSize) : DetailConstants.MinGestureThumbnailSize;

                var mainTexture = GetTexture(mainSize, camera);
                var gestureTexture = GetTexture(gestureSize, camera);

                return (mainTexture, gestureTexture);
            }
            finally
            {
                AnimationMode.StopAnimationMode();
                clonedAvatar.transform.position = initPos;
                clonedAvatar.transform.rotation = initRot;
            }
        }

        private static Texture2D GetTexture(int size, Camera camera)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGB24, true);
            var renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 24);
            renderTexture.wrapMode = TextureWrapMode.Clamp;

            RenderCamera(renderTexture, camera);
            RenderTextureTo(renderTexture, texture);
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture;
        }

        // From CGE
        private static void RenderCamera(RenderTexture renderTexture, Camera camera)
        {
            var originalRenderTexture = camera.targetTexture;
            var originalAspect = camera.aspect;
            try
            {
                camera.targetTexture = renderTexture;
                camera.aspect = (float) renderTexture.width / renderTexture.height;
                camera.Render();
            }
            finally
            {
                camera.targetTexture = originalRenderTexture;
                camera.aspect = originalAspect;
            }
        }

        // From CGE
        private static void RenderTextureTo(RenderTexture renderTexture, Texture2D texture2D)
        {
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
        }
    }
}
