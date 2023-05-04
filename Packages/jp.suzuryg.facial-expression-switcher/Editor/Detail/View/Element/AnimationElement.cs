using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.View.Element
{
    public class AnimationElement : IDisposable
    {
        private AV3.ExpressionEditor _expressionEditor;
        private AV3Setting _aV3Setting;
        private ThumbnailSetting _thumbnailSetting;
        private SerializedObject _aV3Object;
        private LocalizationTable _localizationTable;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public AnimationElement(
            AV3.ExpressionEditor expressionEditor,
            IReadOnlyLocalizationSetting localizationSetting,
            AV3Setting aV3Setting,
            ThumbnailSetting thumbnailSetting)
        {
            _expressionEditor = expressionEditor;
            _aV3Setting = aV3Setting;
            _thumbnailSetting = thumbnailSetting;
            _localizationTable = localizationSetting.Table;

            _aV3Object = new SerializedObject(_aV3Setting);

            localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        // TODO: Specify up-left point, not a rect.
        public void Draw(Rect rect, Domain.Animation animation, MainThumbnailDrawer thumbnailDrawer,
            Action<string> setAnimationClipAction, // The argument is new animation's GUID.
            string modeDisplayName)
        {
            // Thumbnail
            LoadTexture();

            var thumbnailWidth = _thumbnailSetting.Main_Width;
            var thumbnailHeight = _thumbnailSetting.Main_Height;

            Rect thumbnailRect = new Rect(rect.x, rect.y, thumbnailWidth, thumbnailHeight);
            float xCurrent = rect.x;
            float yCurrent = rect.y + thumbnailHeight;

            var animationTexture = thumbnailDrawer.GetThumbnail(animation);
            if (animationTexture != null)
            {
                GUI.DrawTexture(thumbnailRect, animationTexture);
            }

            // ObjectField
            var path = AssetDatabase.GUIDToAssetPath(animation?.GUID);

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            var newClip = EditorGUI.ObjectField(new Rect(xCurrent, yCurrent, thumbnailWidth, EditorGUIUtility.singleLineHeight), clip, typeof(AnimationClip), false);
            if (!ReferenceEquals(clip, newClip))
            {
                var newPath = AssetDatabase.GetAssetPath(newClip);
                var newGUID = AssetDatabase.AssetPathToGUID(newPath);
                setAnimationClipAction(newGUID);
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

                const float iconMarginRate = 0.1f;
                var iconMargin = Math.Min(width * iconMarginRate, height * iconMarginRate);
                var clipExits = clip != null;

                // Create
                if (GUI.Button(createRect, string.Empty))
                {
                    var guid = GetAnimationGuidWithDialog(DialogMode.Create, path, modeDisplayName);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        _expressionEditor.OpenIfOpenedAlready(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)));
                        setAnimationClipAction(guid);
                    }
                }
                GUI.DrawTexture(new Rect(createRect.x + iconMargin, createRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), CreateIcon, ScaleMode.ScaleToFit, alphaBlend: true);

                // Open
                if (GUI.Button(openRect, string.Empty))
                {
                    var guid = GetAnimationGuidWithDialog(DialogMode.Open, path, modeDisplayName);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        _expressionEditor.OpenIfOpenedAlready(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)));
                        setAnimationClipAction(guid);
                    }
                }
                GUI.DrawTexture(new Rect(openRect.x + iconMargin, openRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), OpenIcon, ScaleMode.ScaleToFit, alphaBlend: true);

                // Copy
                using (new EditorGUI.DisabledScope(!clipExits))
                {
                    if (GUI.Button(copyRect, string.Empty))
                    {
                        var guid = GetAnimationGuidWithDialog(DialogMode.Copy, path, modeDisplayName);
                        if (!string.IsNullOrEmpty(guid))
                        {
                            _expressionEditor.OpenIfOpenedAlready(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)));
                            setAnimationClipAction(guid);
                        }
                    }
                }
                GUI.DrawTexture(new Rect(copyRect.x + iconMargin, copyRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), CopyIcon, ScaleMode.ScaleToFit, alphaBlend: true);
                if (!clipExits)
                {
                    GUI.DrawTexture(copyRect, BlackTranslucent, ScaleMode.StretchToFill, alphaBlend: true);
                }

                // Edit
                using (new EditorGUI.DisabledScope(!clipExits))
                {
                    if (GUI.Button(editRect, string.Empty))
                    {
                        _expressionEditor.Open(clip);
                    }
                }
                GUI.DrawTexture(new Rect(editRect.x + iconMargin, editRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), EditIcon, ScaleMode.ScaleToFit, alphaBlend: true);
                if (!clipExits)
                {
                    GUI.DrawTexture(editRect, BlackTranslucent, ScaleMode.StretchToFill, alphaBlend: true);
                }
            }
        }

        public float GetWidth()
        {
            var thumbnailWidth = _thumbnailSetting.Main_Width;
            return thumbnailWidth;
        }

        public float GetHeight()
        {
            var thumbnailHeight = _thumbnailSetting.Main_Height;
            return thumbnailHeight + EditorGUIUtility.singleLineHeight;
        }

        private string GetAnimationGuidWithDialog(DialogMode dialogMode, string existingAnimationPath, string modeDisplayName)
        {
            // Open dialog and get the path of the AnimationClip
            var defaultDir = GetDefaultDir(existingAnimationPath);
            var selectedPath = string.Empty;
            if (dialogMode == DialogMode.Open)
            {
                selectedPath = EditorUtility.OpenFilePanelWithFilters(title: null, directory: defaultDir, filters: new[] { "AnimationClip" , "anim" });
            }
            else if (dialogMode == DialogMode.Create)
            {
                var baseAnimationName = modeDisplayName.Replace(" ", "_");
                selectedPath = EditorUtility.SaveFilePanelInProject(title: null, defaultName: GetNewAnimationName(defaultDir, baseAnimationName), extension: "anim", message: null, path: defaultDir);
            }
            else if (dialogMode == DialogMode.Copy)
            {
                var baseAnimationName = System.IO.Path.GetFileName(existingAnimationPath).Replace(".anim", string.Empty);
                selectedPath = EditorUtility.SaveFilePanelInProject(title: null, defaultName: GetNewAnimationName(defaultDir, baseAnimationName), extension: "anim", message: null, path: defaultDir);
            }

            // Retrieve and return the GUID of the AnimationClip from the path
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // OpenFilePanel path is in OS format, so convert it to Unity format
                var unityPath = selectedPath;
                if (dialogMode == DialogMode.Open)
                {
                    unityPath = PathConverter.ToUnityPath(selectedPath);
                }

                // Create AnimationClip
                if (dialogMode == DialogMode.Create)
                {
                    var clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, unityPath);
                }
                else if (dialogMode == DialogMode.Copy)
                {
                    AssetDatabase.CopyAsset(existingAnimationPath, unityPath);
                }

                // Retrieve GUID
                var guid = AssetDatabase.AssetPathToGUID(unityPath);
                if (!string.IsNullOrEmpty(guid))
                {
                    _aV3Object.Update();
                    _aV3Object.FindProperty(nameof(AV3Setting.LastOpendOrSavedAnimationPath)).stringValue = unityPath;
                    _aV3Object.ApplyModifiedProperties();

                    return guid;
                }
                else
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.AnimationElement_Message_GuidWasNotFound, "OK");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private enum DialogMode
        {
            Open,
            Create,
            Copy,
        }

        private string GetDefaultDir(string existingAnimationPath)
        {
            // Use existing animation path
            var path = existingAnimationPath;
            while (!string.IsNullOrEmpty(path))
            {
                path = System.IO.Path.GetDirectoryName(path);
                if (AssetDatabase.IsValidFolder(path))
                {
                    return path;
                }
            }

            // Use last opened or saved animation path
            _aV3Object.Update();
            path = _aV3Object.FindProperty(nameof(AV3Setting.LastOpendOrSavedAnimationPath)).stringValue;
            while (!string.IsNullOrEmpty(path))
            {
                path = System.IO.Path.GetDirectoryName(path);
                if (AssetDatabase.IsValidFolder(path))
                {
                    return path;
                }
            }

            return "Assets";
        }

        private string GetNewAnimationName(string defaultDir, string baseAnimationName)
        {
            // Define a regex pattern to match the ending _(number)
            string pattern = @"_\d+$";
            Regex regex = new Regex(pattern);

            // Replace the matched pattern with an empty string
            baseAnimationName = regex.Replace(baseAnimationName, "");

            // Decide animation name
            var animationName = $"{baseAnimationName}.anim";
            for (int i = 0; i < int.MaxValue; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<AnimationClip>($"{defaultDir}/{animationName}") != null)
                {
                    animationName = $"{baseAnimationName}_{i}.anim";
                }
                else
                {
                    return animationName;
                }
            }

            return $"{baseAnimationName}.anim";
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

            if (CreateIcon == null)
            {
                CreateIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/note_add_FILL0_wght400_GRAD200_opsz150.png");
            }

            if (OpenIcon == null)
            {
                OpenIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/folder_open_FILL0_wght400_GRAD200_opsz150.png");
            }

            if (CopyIcon == null)
            {
                CopyIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/content_copy_FILL0_wght400_GRAD200_opsz150.png");
            }

            if (EditIcon == null)
            {
                EditIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/edit_FILL0_wght400_GRAD200_opsz150.png");
            }
        }
    }
}
