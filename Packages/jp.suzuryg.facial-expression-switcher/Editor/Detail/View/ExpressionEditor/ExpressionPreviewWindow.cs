using System;
using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View
{
    public class ExpressionPreviewWindow : SceneView
    {
        private AV3.ExpressionEditor _expressionEditor;
        private Texture2D _renderCache;

        public void Initialize(AV3.ExpressionEditor expressionEditor)
        {
            // Dependencies
            _expressionEditor = expressionEditor;

            // Initialization
            drawGizmos = false;
            const float initialZoom = 0.2f;
            LookAt(point: _expressionEditor.GetAvatarViewPosition(),
                direction: Quaternion.Euler(0, 180, 0), newSize: initialZoom, ortho: false, instant: true);
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
        }

        public void UpdateRenderCache()
        {
            // When using the camera without copying, the aspect ratio of the SceneView goes wrong
            var cameraRoot = new GameObject();
            try
            {
                var renderCamera = cameraRoot.AddComponent<Camera>();
                renderCamera.CopyFrom(camera);
                _renderCache = DrawingUtility.GetRenderedTexture(renderCamera.pixelWidth, renderCamera.pixelHeight, renderCamera);
            }
            finally
            {
                UnityEngine.GameObject.DestroyImmediate(cameraRoot);
            }
        }

        protected override void OnGUI()
        {
            // When the animation changes are saved with Ctrl-S, the AnimationMode is stopped.
            // Therefore, the following process is performed to resume sampling.
            if (ReferenceEquals(focusedWindow, this) && !AnimationMode.InAnimationMode())
            {
                _expressionEditor?.StartSampling();
            }

            // If in AnimationMode, draw SceneView.
            if (AnimationMode.InAnimationMode())
            {
                base.OnGUI();
            }
            // If not in AnimationMode, draw the cache.
            else
            {
                if (_renderCache != null)
                {
                    GUI.DrawTexture(new Rect(position.width - _renderCache.width, position.height - _renderCache.height, _renderCache.width, _renderCache.height),
                        _renderCache, ScaleMode.ScaleToFit, alphaBlend: false);
                }
            }
        }

        private void OnFocus()
        {
            _expressionEditor?.StartSampling();
        }

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
    }
}
