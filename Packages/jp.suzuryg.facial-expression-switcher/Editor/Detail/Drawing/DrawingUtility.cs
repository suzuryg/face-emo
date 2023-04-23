using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Drawing
{
    public class DrawingUtility
    {
        public static Texture2D GetRenderedTexture(int width, int height, Camera camera)
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
