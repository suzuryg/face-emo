using System;
using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Drawing;
using System.Reflection;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class ExpressionPreviewWindow : SceneView, ISubWindow
    {
        public bool IsInitialized { get; set; } = false;

        public bool IsDocked
        {
            get
            {
#if UNITY_2019
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                MethodInfo method = GetType().GetProperty( "docked", flags ).GetGetMethod( true );
                return (bool)method.Invoke( this, null );
#else
                return docked;
#endif
            }
        }

        private AV3.ExpressionEditor _expressionEditor;
        private Texture2D _renderCache;

        public void Initialize(AV3.ExpressionEditor expressionEditor, SceneView lastActiveSceneView)
        {
            // Dependencies
            _expressionEditor = expressionEditor;

            // Initialization
            if (lastActiveSceneView != null && lastActiveSceneView.cameraSettings != null)
            {
                var copied = new CameraSettings();

                copied.speed = lastActiveSceneView.cameraSettings.speed;
                copied.speedNormalized = lastActiveSceneView.cameraSettings.speedNormalized;
                copied.speedMin = lastActiveSceneView.cameraSettings.speedMin;
                copied.speedMax = lastActiveSceneView.cameraSettings.speedMax;
                copied.easingEnabled = lastActiveSceneView.cameraSettings.easingEnabled;
                copied.easingDuration = lastActiveSceneView.cameraSettings.easingDuration;
                copied.accelerationEnabled = lastActiveSceneView.cameraSettings.accelerationEnabled;
                copied.fieldOfView = lastActiveSceneView.cameraSettings.fieldOfView;
                copied.nearClip = lastActiveSceneView.cameraSettings.nearClip;
                copied.farClip = lastActiveSceneView.cameraSettings.farClip;
                copied.dynamicClip = lastActiveSceneView.cameraSettings.dynamicClip;
                copied.occlusionCulling = lastActiveSceneView.cameraSettings.occlusionCulling;

                cameraSettings = copied;
            }

            drawGizmos = false;
            const float initialZoom = 0.12f;
            LookAt(point: _expressionEditor.GetAvatarViewPosition(),
                direction: Quaternion.Euler(-5, 180, 0), newSize: initialZoom, ortho: false, instant: true);
        }

        public override void OnEnable()
        {
            // (Workaround) To avoid an error when the icon is not found in SceneView.OnEnable, generate the icon to handle the situation.
            const string IconDir = "Assets/Editor Default Resources/Icons";
            AV3Utility.CreateFolderRecursively(IconDir);
            var iconPath = IconDir + $"/{typeof(ExpressionPreviewWindow).FullName}.png";
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath) == null)
            {
                AssetDatabase.CopyAsset($"{DetailConstants.IconDirectory}/sentiment_satisfied_FILL0_wght400_GRAD200_opsz48.png", iconPath);
            }
            minSize = new Vector2(300, 300);
            base.OnEnable();

            // Workaround for the title being changed to "Scene" when restarting Unity.
            titleContent = new GUIContent(DomainConstants.SystemName);
        }

        public void UpdateRenderCache()
        {
            if (camera == null) { return; }

            // When using the camera without copying, the aspect ratio of the SceneView goes wrong
            var cameraRoot = new GameObject();
            try
            {
                var renderCamera = cameraRoot.AddComponent<Camera>();
                renderCamera.CopyFrom(camera);

                var drawScale = position.width / renderCamera.pixelWidth;
                var scaledTextureWidth = (int)Math.Round(position.width * DetailConstants.UiScale, MidpointRounding.AwayFromZero);
                var scaledTextureHeight = (int)Math.Round(renderCamera.pixelHeight * drawScale * DetailConstants.UiScale, MidpointRounding.AwayFromZero);

                _renderCache = DrawingUtility.GetRenderedTexture(scaledTextureWidth, scaledTextureHeight, renderCamera);
            }
            finally
            {
                UnityEngine.GameObject.DestroyImmediate(cameraRoot);
            }
        }

        public void CloseIfNotDocked()
        {
            if (!IsDocked)
            {
                Close();
            }
            else
            {
                // Must be initialized the next time opened from the main window.
                IsInitialized = false;
            }
        }

#if UNITY_2019
        protected override void OnGUI()
#else
        protected override void OnSceneGUI()
#endif
        {
            // When the animation changes are saved with Ctrl-S, the AnimationMode is stopped.
            // Therefore, the following process is performed to resume sampling.
            if (ReferenceEquals(focusedWindow, this) && !AnimationMode.InAnimationMode() && _expressionEditor?.IsDisposed == false)
            {
                _expressionEditor?.StartSampling();
            }

            // If in AnimationMode, draw SceneView.
            if (AnimationMode.InAnimationMode())
            {
#if UNITY_2019
                base.OnGUI();
#else
                base.OnSceneGUI();
#endif
            }
            // If not in AnimationMode, draw the cache.
            else
            {
                if (_renderCache != null)
                {
                    var x = 0;
#if UNITY_2019
                    var y = position.height - _renderCache.height / DetailConstants.UiScale;
#else
                    var y = 0;
#endif
                    var width = _renderCache.width / DetailConstants.UiScale;
                    var height = _renderCache.height / DetailConstants.UiScale;

                    GUI.DrawTexture(new Rect(x, y, width, height), _renderCache, ScaleMode.ScaleToFit, alphaBlend: false);
                }
            }
        }

        private void OnFocus()
        {
            if (_expressionEditor?.IsDisposed == false)
            {
                _expressionEditor?.StartSampling();
            }
        }

#if UNITY_2019
        private void OnLostFocus()
        {
            try
            {
                UpdateRenderCache();
            }
            finally
            {
                _expressionEditor?.StopSampling();
            }
        }
#else
        // In Unity2022, OnLostFocus() is executed when wheel-clicking or right-clicking on SceneView.
        // Therefore, do not override OnLostFocus().
#endif

        public override void OnDisable()
        {
            base.OnDisable();
            _expressionEditor?.StopSampling();
        }
    }
}
