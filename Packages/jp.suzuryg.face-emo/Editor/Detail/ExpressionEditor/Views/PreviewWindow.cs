using System;
using System.Reflection;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class PreviewWindow : SceneView
    {
        public bool InUse => ReferenceEquals(focusedWindow, this);

        [CanBeNull] private LocalizationTable _loc;
        [CanBeNull] private Camera _renderCamera;
        [CanBeNull] private Texture2D _renderCache;
        [CanBeNull] private SceneView _lastActiveSceneViewCache;
        private bool _isInitialized;

        public void Initialize(Vector3 target)
        {
            if (_isInitialized) return;

            const float initialZoom = 0.12f;
            LookAt(point: target, direction: Quaternion.Euler(-5, 180, 0), newSize: initialZoom, ortho: false,
                instant: true);

            _isInitialized = true;
        }

        public void CloseIfNotDocked()
        {
            _isInitialized = false;
            if (!docked) Close();
        }

        public void UpdateRenderCache()
        {
            if (camera == null || _renderCamera == null) return;
            try
            {
                _renderCamera.CopyFrom(camera);
                _renderCamera.enabled = true;
                var drawScale = position.width / _renderCamera.pixelWidth;
                var scaledTextureWidth = (int)Math.Round(position.width * DetailConstants.UiScale,
                    MidpointRounding.AwayFromZero);
                var scaledTextureHeight =
                    (int)Math.Round(_renderCamera.pixelHeight * drawScale * DetailConstants.UiScale,
                        MidpointRounding.AwayFromZero);
                _renderCache =
                    DrawingUtility.GetRenderedTexture(scaledTextureWidth, scaledTextureHeight, _renderCamera);
            }
            finally
            {
                _renderCamera.targetTexture = null;
                _renderCamera.enabled = false;
            }
        }

        public override void OnEnable()
        {
            // (Workaround) To avoid an error when the icon is not found in SceneView.OnEnable, generate the icon to handle the situation.
            const string iconDir = "Assets/Editor Default Resources/Icons";
            AV3Utility.CreateFolderRecursively(iconDir);
            var iconPath = iconDir + $"/{typeof(PreviewWindow).FullName}.png";
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath) == null)
            {
                AssetDatabase.CopyAsset($"{DetailConstants.IconDirectory}/sentiment_satisfied_FILL0_wght400_GRAD200_opsz48.png", iconPath);
            }
            minSize = new Vector2(300, 300);
            base.OnEnable();

            titleContent = new GUIContent($"{DomainConstants.SystemName} Preview");

            // Copy the camera settings of the last active SceneView.
            if (_lastActiveSceneViewCache != null && _lastActiveSceneViewCache.cameraSettings != null)
            {
                cameraSettings = new CameraSettings
                {
                    speed = _lastActiveSceneViewCache.cameraSettings.speed,
                    speedNormalized = _lastActiveSceneViewCache.cameraSettings.speedNormalized,
                    speedMin = _lastActiveSceneViewCache.cameraSettings.speedMin,
                    speedMax = _lastActiveSceneViewCache.cameraSettings.speedMax,
                    easingEnabled = _lastActiveSceneViewCache.cameraSettings.easingEnabled,
                    easingDuration = _lastActiveSceneViewCache.cameraSettings.easingDuration,
                    accelerationEnabled = _lastActiveSceneViewCache.cameraSettings.accelerationEnabled,
                    fieldOfView = _lastActiveSceneViewCache.cameraSettings.fieldOfView,
                    nearClip = _lastActiveSceneViewCache.cameraSettings.nearClip,
                    farClip = _lastActiveSceneViewCache.cameraSettings.farClip,
                    dynamicClip = _lastActiveSceneViewCache.cameraSettings.dynamicClip,
                    occlusionCulling = _lastActiveSceneViewCache.cameraSettings.occlusionCulling
                };
            }

            // Disable gizmo drawing
            drawGizmos = false;

            // Create a camera for rendering
            var cameraRoot = new GameObject
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _renderCamera = cameraRoot.AddComponent<Camera>();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (_lastActiveSceneViewCache != null)
            {
                SetLastActiveSceneView(_lastActiveSceneViewCache);
            }

            if (_renderCamera != null) DestroyImmediate(_renderCamera.gameObject);
        }

        protected override void OnSceneGUI()
        {
            if (!_isInitialized)
            {
                if (_loc == null) _loc = LocalizationSetting.GetTable(LocalizationSetting.GetLocale());
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_loc?.ExpressionEditorView_Message_NotInitialized);
                    GUILayout.FlexibleSpace();
                }
                return;
            }

            base.OnSceneGUI();
            if (AnimationMode.InAnimationMode() || _renderCache == null) return;

            var width = _renderCache.width / DetailConstants.UiScale;
            var height = _renderCache.height / DetailConstants.UiScale;
            GUI.DrawTexture(new Rect(0, 0, width, height), _renderCache, ScaleMode.ScaleToFit, alphaBlend: false);
        }

        private void OnFocus()
        {
            // When base.OnSceneGUI() is called, lastActiveSceneView becomes active, causing problems with viewpoint manipulation.
            // To avoid this problem, change lastActiveSceneView.
            // Even if this operation is not performed, basic expression editing can be performed with only some problems with zooming by dragging, etc.
            if (lastActiveSceneView != null && !ReferenceEquals(lastActiveSceneView, this))
            {
                _lastActiveSceneViewCache = lastActiveSceneView;
            }
            SetLastActiveSceneView(this);
        }

        private void OnLostFocus()
        {
            if (_lastActiveSceneViewCache != null)
            {
                SetLastActiveSceneView(_lastActiveSceneViewCache);
            }
        }

        private static void SetLastActiveSceneView(SceneView sceneView)
        {
            if (ReferenceEquals(sceneView, lastActiveSceneView)) return;
            var sceneViewType = typeof(SceneView);
            var lastActiveSceneViewInfo =
                sceneViewType.GetProperty("lastActiveSceneView", BindingFlags.Public | BindingFlags.Static);
            if (lastActiveSceneViewInfo != null)
            {
                lastActiveSceneViewInfo.SetValue(null, sceneView, null);
            }
            else
            {
                Debug.LogError("lastActiveSceneView property not found");
            }
        }
    }
}
