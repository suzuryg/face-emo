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

        private Texture2D _blackTranslucent;
        private Texture2D _createIcon;
        private Texture2D _openIcon;
        private Texture2D _copyIcon;
        private Texture2D _editIcon;

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

            // Initialization
            SetIcon();
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        private void SetIcon()
        {
            _blackTranslucent = new Texture2D(1, 1, TextureFormat.RGBA32, true);
            _blackTranslucent.wrapMode = TextureWrapMode.Repeat;
            _blackTranslucent.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
            _blackTranslucent.Apply();

            _createIcon = ViewUtility.GetIconTexture("note_add_FILL0_wght400_GRAD200_opsz150.png");
            _openIcon = ViewUtility.GetIconTexture("folder_open_FILL0_wght400_GRAD200_opsz150.png");
            _copyIcon = ViewUtility.GetIconTexture("content_copy_FILL0_wght400_GRAD200_opsz150.png");
            _editIcon = ViewUtility.GetIconTexture("edit_FILL0_wght400_GRAD200_opsz150.png");

            NullChecker.Check(_blackTranslucent, _createIcon, _openIcon, _copyIcon, _editIcon);
        }

        // TODO: Specify up-left point, not a rect.
        public void Draw(Rect rect, Domain.Animation animation, MainThumbnailDrawer thumbnailDrawer,
            Action<string> setAnimationClipAction, // The argument is new animation's GUID.
            string modeDisplayName)
        {
            // Thumbnail
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
                GUI.DrawTexture(thumbnailRect, _blackTranslucent, ScaleMode.StretchToFill, alphaBlend: true);

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
                if (GUI.Button(createRect, new GUIContent(string.Empty, _localizationTable.AnimationElement_Tooltip_Create)))
                {
                    if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_NotPossibleInPlayMode, "OK"); return; }

                    var guid = GetAnimationGuidWithDialog(DialogMode.Create, path, modeDisplayName);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        _expressionEditor.Open(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)));
                        setAnimationClipAction(guid);
                    }
                }
                GUI.DrawTexture(new Rect(createRect.x + iconMargin, createRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), _createIcon, ScaleMode.ScaleToFit, alphaBlend: true);

                // Open
                if (GUI.Button(openRect, new GUIContent(string.Empty, _localizationTable.AnimationElement_Tooltip_Open)))
                {
                    if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_NotPossibleInPlayMode, "OK"); return; }

                    var guid = GetAnimationGuidWithDialog(DialogMode.Open, path, modeDisplayName);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        _expressionEditor.OpenIfOpenedAlready(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)));
                        setAnimationClipAction(guid);
                    }
                }
                GUI.DrawTexture(new Rect(openRect.x + iconMargin, openRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), _openIcon, ScaleMode.ScaleToFit, alphaBlend: true);

                // Copy
                using (new EditorGUI.DisabledScope(!clipExits))
                {
                    if (GUI.Button(copyRect, new GUIContent(string.Empty, _localizationTable.AnimationElement_Tooltip_Copy)))
                    {
                        if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_NotPossibleInPlayMode, "OK"); return; }

                        var guid = GetAnimationGuidWithDialog(DialogMode.Copy, path, modeDisplayName);
                        if (!string.IsNullOrEmpty(guid))
                        {
                            _expressionEditor.Open(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)));
                            setAnimationClipAction(guid);
                        }
                    }
                }
                GUI.DrawTexture(new Rect(copyRect.x + iconMargin, copyRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), _copyIcon, ScaleMode.ScaleToFit, alphaBlend: true);
                if (!clipExits)
                {
                    GUI.DrawTexture(copyRect, _blackTranslucent, ScaleMode.StretchToFill, alphaBlend: true);
                }

                // Edit
                using (new EditorGUI.DisabledScope(!clipExits))
                {
                    if (GUI.Button(editRect, new GUIContent(string.Empty, _localizationTable.AnimationElement_Tooltip_Edit)))
                    {
                        if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_NotPossibleInPlayMode, "OK"); return; }

                        _expressionEditor.Open(clip);
                    }
                }
                GUI.DrawTexture(new Rect(editRect.x + iconMargin, editRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), _editIcon, ScaleMode.ScaleToFit, alphaBlend: true);
                if (!clipExits)
                {
                    GUI.DrawTexture(editRect, _blackTranslucent, ScaleMode.StretchToFill, alphaBlend: true);
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
    }
}
