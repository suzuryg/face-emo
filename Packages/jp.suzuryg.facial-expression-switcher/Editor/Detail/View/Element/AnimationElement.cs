using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View.Element
{
    public class AnimationElement
    {
        // TODO: Specify up-left point, not a rect.
        public static void Draw(Rect rect, Domain.Animation animation, ThumbnailDrawer thumbnailDrawer,
            Action<string> createNewAnimationAction, // The argument is new animation's GUID.
            Action<string> setExistingAnimationAction, // The argument is new animation's GUID.
            Action<string> copyAnimationAction, // The argument is new animation's GUID.
            Action editAnimationAction)
        {
            // Thumbnail
            LoadTexture();
            thumbnailDrawer.Prioritize(animation);

            var thumbnailSize = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailSize) : DetailConstants.MinMainThumbnailSize;
            Rect thumbnailRect = new Rect(rect.x, rect.y, thumbnailSize, thumbnailSize);
            float xCurrent = rect.x;
            float yCurrent = rect.y + thumbnailSize;

            var animationTexture = thumbnailDrawer.GetThumbnail(animation).main;
            if (animationTexture is Texture2D)
            {
                GUI.DrawTexture(thumbnailRect, animationTexture);
            }

            // ObjectField
            var path = AssetDatabase.GUIDToAssetPath(animation?.GUID);

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            var newClip = EditorGUI.ObjectField(new Rect(xCurrent, yCurrent, thumbnailSize, EditorGUIUtility.singleLineHeight), clip, typeof(AnimationClip), false);
            if (!ReferenceEquals(clip, newClip))
            {
                var newPath = AssetDatabase.GetAssetPath(newClip);
                var newGUID = AssetDatabase.AssetPathToGUID(newPath);
                setExistingAnimationAction(newGUID);
            }

            if (thumbnailRect.Contains(Event.current.mousePosition))
            {
                GUI.DrawTexture(thumbnailRect, BlackTranslucent, ScaleMode.StretchToFill, alphaBlend: true);

                const float margin = 5;
                var width = thumbnailRect.width / 2 - margin * 2;
                var height = thumbnailRect.height / 2 - margin * 2;

                var createRect = new Rect(thumbnailRect.x + margin, thumbnailRect.y + margin, width, height);
                var openRect = new Rect(thumbnailRect.x + thumbnailRect.width / 2 + margin, thumbnailRect.y + margin, width, height);
                var copyRect = new Rect(thumbnailRect.x + margin, thumbnailRect.y + thumbnailRect.height / 2 + margin, width, height);
                var editRect = new Rect(thumbnailRect.x + thumbnailRect.width / 2 + margin, thumbnailRect.y + thumbnailRect.height / 2 + margin, width, height);

                // Create
                if (GUI.Button(createRect, CreateIcon))
                {
                    if (GetAnimationGUID() is string guid)
                    {
                        createNewAnimationAction(guid);
                    }
                }

                // Open
                if (GUI.Button(openRect, OpenIcon))
                {
                    if (GetAnimationGUID() is string guid)
                    {
                        setExistingAnimationAction(guid);
                    }
                }

                // Copy
                if (GUI.Button(copyRect, CopyIcon))
                {
                    if (GetAnimationGUID() is string guid)
                    {
                        copyAnimationAction(guid);
                    }
                }

                // Edit
                if (GUI.Button(editRect, EditIcon))
                {
                    editAnimationAction();
                }
            }
        }

        public static float GetWidth()
        {
            var thumbnailSize = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailSize) : DetailConstants.MinMainThumbnailSize;
            return thumbnailSize;
        }

        public static float GetHeight()
        {
            var thumbnailSize = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailSize) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailSize) : DetailConstants.MinMainThumbnailSize;
            return thumbnailSize + EditorGUIUtility.singleLineHeight;
        }

        private static string GetAnimationGUID()
        {
            var selectedPath = EditorUtility.OpenFilePanel(null, null, "anim");
            if (selectedPath is string && selectedPath.Length > 0)
            {
                var unityPath = PathConverter.ToUnityPath(selectedPath);
                var guid = AssetDatabase.AssetPathToGUID(unityPath);
                return guid;
            }
            else
            {
                return null;
            }
        }

        private static Texture2D BlackTranslucent = null;
        private static Texture2D CreateIcon = null;
        private static Texture2D OpenIcon = null;
        private static Texture2D CopyIcon = null;
        private static Texture2D EditIcon = null;

        private static void LoadTexture()
        {
            if (BlackTranslucent == null)
            {
                BlackTranslucent = new Texture2D(1, 1, TextureFormat.RGBA32, true);
                BlackTranslucent.wrapMode = TextureWrapMode.Repeat;
                BlackTranslucent.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
                BlackTranslucent.Apply();
            }

            if (CreateIcon is null)
            {
                CreateIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/note_add_FILL0_wght400_GRAD200_opsz48.png");
            }

            if (OpenIcon is null)
            {
                OpenIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/folder_open_FILL0_wght400_GRAD200_opsz48.png");
            }

            if (CopyIcon is null)
            {
                CopyIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/content_copy_FILL0_wght400_GRAD200_opsz48.png");
            }

            if (EditIcon is null)
            {
                EditIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/edit_FILL0_wght400_GRAD200_opsz48.png");
            }
        }
    }
}
