using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class ViewUtility
    {
        private static readonly Color SelectedColorDark = new Color(0.15f, 0.35f, 0.55f, 1f);
        private static readonly Color SelectedColorLight = new Color(0.47f, 0.67f, 0.87f, 1f);

        public static Color GetSelectedColor()
        {
            if (EditorGUIUtility.isProSkin) { return SelectedColorDark; }
            else { return SelectedColorLight; }
        }

        public static void LayoutDummyToggle(string label)
        {
            using (new EditorGUI.DisabledScope(true))
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Toggle(string.Empty, false, GUILayout.Width(15));
                GUILayout.Label(label);
            }
        }

        public static void RectDummyToggle(Rect rect, float toggleWidth, string label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                var useRightTrigger = GUI.Toggle(new Rect(rect.x, rect.y, toggleWidth, rect.height), false, string.Empty);
                GUI.Label(new Rect(rect.x + toggleWidth, rect.y, rect.width - toggleWidth, rect.height), label);
            }
        }

        public static Texture2D GetIconTexture(string fileName)
        {
            var path = DetailConstants.IconDirectory + "/" + fileName;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) { Debug.LogError($"Icon image not found: {path}"); }

            if (EditorGUIUtility.isProSkin)
            {
                return texture;
            }
            else
            {
                try
                {
                    return InvertTextureColors(MakeTextureReadable(texture));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to generate icon for light theme.\n{ex.ToString()}");
                    return texture;
                }
            }
        }

        public static Image GetIconElement(string fileName)
        {
            var texture = GetIconTexture(fileName);
            var icon = new Image();
            icon.image = texture;
            icon.style.width = 24;
            icon.style.height = 24;
            return icon;
        }

        private static Texture2D InvertTextureColors(Texture2D original)
        {
            // Create a copy of the original texture
            Texture2D inverted = new Texture2D(original.width, original.height);

            // Get all the colors in the original texture
            Color[] originalColors = original.GetPixels();

            // Prepare an array for the inverted colors
            Color[] invertedColors = new Color[originalColors.Length];

            // Iterate over the colors
            for (int i = 0; i < originalColors.Length; i++)
            {
                // Get the original color
                Color originalColor = originalColors[i];

                // Invert the color
                Color invertedColor = new Color(1 - originalColor.r, 1 - originalColor.g, 1 - originalColor.b, originalColor.a);

                // Set the inverted color
                invertedColors[i] = invertedColor;
            }

            // Set the inverted colors on the new texture
            inverted.SetPixels(invertedColors);

            // Apply the changes to the texture
            inverted.Apply();

            return inverted;
        }

        private static Texture2D MakeTextureReadable(Texture2D original)
        {
            // Create a temporary RenderTexture
            RenderTexture tmp = RenderTexture.GetTemporary(
                original.width,
                original.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(original, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D readableText = new Texture2D(original.width, original.height);
            readableText.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readableText.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            return readableText;
        }

        /// <summary>
        /// Helper function to create a solid color texture
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static Texture2D MakeTexture(Color col)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, col);
            texture.Apply();
            return texture;
        }
    }
}
