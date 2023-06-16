using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.Drawing
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

        public static Texture2D PaddingWithTransparentPixels(Texture2D original, int outerWidth, int outerHeight)
        {
            // Create a new transparent texture
            Texture2D result = new Texture2D(outerWidth, outerHeight, TextureFormat.ARGB32, false);
            Color[] clearColorArray = new Color[outerWidth * outerHeight];
            for (int i = 0; i < clearColorArray.Length; i++) { clearColorArray[i] = new Color(0, 0, 0, 0); }
            result.SetPixels(clearColorArray);

            // Copy the original image into its top-center
            int offsetX = (outerWidth - original.width) / 2;
            int offsetY = outerHeight - original.height;
            for (int y = 0; y < original.height; y++)
            {
                for (int x = 0; x < original.width; x++)
                {
                    result.SetPixel(x + offsetX, y + offsetY, original.GetPixel(x, y));
                }
            }

            result.Apply();
            return result;
        }

        /// <summary>
        /// Gamma correction
        /// </summary>
        public static void ApplyGammaCorrectionCPU(Texture2D texture, float gamma)
        {
            // Loop through each pixel.
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    // Get the color of the pixel.
                    Color color = texture.GetPixel(x, y);

                    // Apply gamma correction.
                    color.r = Mathf.Pow(color.r, gamma);
                    color.g = Mathf.Pow(color.g, gamma);
                    color.b = Mathf.Pow(color.b, gamma);

                    // Set the new color.
                    texture.SetPixel(x, y, color);
                }
            }

            // Apply the changes to the texture.
            texture.Apply();
        }

        private static Material GammaCorrectionMaterial;

        /// <summary>
        /// Gamma correction
        /// Execution of this method may stall the drawing thread
        /// </summary>
        public static void ApplyGammaCorrectionGPU(Texture2D texture, float gamma)
        {
            if (GammaCorrectionMaterial == null)
            {
                GammaCorrectionMaterial = new Material(Shader.Find("Suzuryg/GammaCorrection"));
            }
            GammaCorrectionMaterial.SetFloat("_Gamma", gamma);

            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height);
            Graphics.Blit(texture, renderTexture, GammaCorrectionMaterial);

            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
        }
    }
}
