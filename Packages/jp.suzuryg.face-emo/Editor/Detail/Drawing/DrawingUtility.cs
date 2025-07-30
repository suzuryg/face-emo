using Suzuryg.FaceEmo.Domain;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.Drawing
{
    public static class DrawingUtility
    {
        private const string UseTransparentThumbnailsMenuPath = "FaceEmo/Use Transparent Thumbnails";
        private static readonly string UseTransparentThumbnailsEditorPrefsKey = $"{DomainConstants.SystemName}_UseTransparentThumbnails";
        private const int Order = 101;

        private static bool UseTransparentThumbnails
        {
            get
            {
                if (_useTransparentThumbnails == null)
                {
                    _useTransparentThumbnails = EditorPrefs.GetBool(UseTransparentThumbnailsEditorPrefsKey, false);
                }
                return _useTransparentThumbnails.Value;
            }
            set
            {
                _useTransparentThumbnails = value;
                EditorPrefs.SetBool(UseTransparentThumbnailsEditorPrefsKey, value);
            }
        }

        private static bool? _useTransparentThumbnails;

        [MenuItem(UseTransparentThumbnailsMenuPath, false, Order)]
        private static void ToggleUseTransparentThumbnails()
        {
            UseTransparentThumbnails = !UseTransparentThumbnails;
        }

        [MenuItem(UseTransparentThumbnailsMenuPath, true, Order)]
        private static bool ValidateToggleUseTransparentThumbnails()
        {
            UnityEditor.Menu.SetChecked(UseTransparentThumbnailsMenuPath, UseTransparentThumbnails);
            return true;
        }

        public static Texture2D GetRenderedTexture(int width, int height, Camera camera)
        {
            return UseTransparentThumbnails ?
                DrawingUtility_Transparent.GetRenderedTexture(width, height, camera) :
                DrawingUtility_Opaque.GetRenderedTexture(width, height, camera);
        }

        public static Texture2D PaddingWithTransparentPixels(Texture2D original, int outerWidth, int outerHeight)
        {
            return UseTransparentThumbnails ?
                DrawingUtility_Transparent.PaddingWithTransparentPixels(original, outerWidth, outerHeight) :
                DrawingUtility_Opaque.PaddingWithTransparentPixels(original, outerWidth, outerHeight);
        }

        public static void ApplyGammaCorrectionGPU(Texture2D texture, float gamma)
        {
            if (UseTransparentThumbnails)
                DrawingUtility_Transparent.ApplyGammaCorrectionGPU(texture, gamma);
            else
                DrawingUtility_Opaque.ApplyGammaCorrectionGPU(texture, gamma);
        }
    }
}
