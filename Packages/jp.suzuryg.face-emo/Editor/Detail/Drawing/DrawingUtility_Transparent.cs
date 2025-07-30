using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.Drawing
{
    public class DrawingUtility_Transparent
    {
        public static Texture2D GetRenderedTexture(int width, int height, Camera camera)
        {
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);

            var renderTexture = RenderTexture.GetTemporary(texture.width, texture.height,
                format: RenderTextureFormat.ARGB32, readWrite: RenderTextureReadWrite.sRGB, depthBuffer: 24, antiAliasing: 8);
            try
            {
                renderTexture.wrapMode = TextureWrapMode.Clamp;
                renderTexture.filterMode = FilterMode.Bilinear;

                // Clear render target to transparent
                var prevActive = RenderTexture.active;
                RenderTexture.active = renderTexture;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = prevActive;

                // Force camera to clear with transparent background
                var oldRt    = camera.targetTexture;
                var oldFlags = camera.clearFlags;
                var oldBg    = camera.backgroundColor;
                var oldAsp   = camera.aspect;

                camera.targetTexture   = renderTexture;
                camera.clearFlags      = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.aspect          = (float)width / height;

                // Render
                camera.Render();

                // Restore camera settings
                camera.targetTexture   = oldRt;
                camera.clearFlags      = oldFlags;
                camera.backgroundColor = oldBg;
                camera.aspect          = oldAsp;

                // Copy into Texture2D including alpha
                var activeBefore = RenderTexture.active;
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.alphaIsTransparency = true;
                texture.Apply();
                RenderTexture.active = activeBefore;
            }
            finally
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            return texture;
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
            result.alphaIsTransparency = true;
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
            texture.alphaIsTransparency = true;
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

            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            var prevActive = RenderTexture.active;
            Graphics.Blit(texture, renderTexture, GammaCorrectionMaterial);

            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.alphaIsTransparency = true;
            texture.Apply();
            RenderTexture.active = prevActive;

            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }
}
