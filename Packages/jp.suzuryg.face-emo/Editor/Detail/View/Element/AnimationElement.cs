﻿using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Suzuryg.FaceEmo.Detail.ExpressionEditor;
using UnityEngine;
using UnityEditor;
using UniRx;

namespace Suzuryg.FaceEmo.Detail.View.Element
{
    public class AnimationElement : IDisposable
    {
        private IExpressionEditor _expressionEditor;
        private AV3Setting _aV3Setting;
        private ThumbnailSetting _thumbnailSetting;
        private SerializedObject _aV3Object;
        private LocalizationTable _localizationTable;

        private Texture2D _blackTranslucent;
        private Texture2D _createIcon;
        private Texture2D _openIcon;
        private Texture2D _combineIcon;
        private Texture2D _editIcon;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public AnimationElement(
            IExpressionEditor expressionEditor,
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
            _combineIcon = ViewUtility.GetIconTexture("cell_merge_FILL0_wght400_GRAD200_opsz150.png");
            _editIcon = ViewUtility.GetIconTexture("edit_FILL0_wght400_GRAD200_opsz150.png");

            NullChecker.Check(_blackTranslucent, _createIcon, _openIcon, _combineIcon, _editIcon);
        }

        // TODO: Specify up-left point, not a rect.
        public void Draw(Rect rect, Domain.Animation animation, MainThumbnailDrawer thumbnailDrawer,
            Action<string> setAnimationClipAction, // The argument is new animation's GUID.
            string defaultClipName = null,
            bool canCombine = true, Domain.Animation leftCombine = null, Domain.Animation rightCombine = null)
        {
            // Thumbnail
            var thumbnailWidth = EditorPrefsStore.MainViewThumbnailWidthInMemory;
            var thumbnailHeight = EditorPrefsStore.MainViewThumbnailHeightInMemory;

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
                if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_NotPossibleInPlayMode, "OK"); return; }

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
                var combineRect = new Rect(thumbnailRect.x + margin, thumbnailRect.y + thumbnailRect.height / 2 + margin, width, height);
                var editRect = new Rect(thumbnailRect.x + thumbnailRect.width / 2 + margin, thumbnailRect.y + thumbnailRect.height / 2 + margin, width, height);

                const float iconMarginRate = 0.1f;
                var iconMargin = Math.Min(width * iconMarginRate, height * iconMarginRate);
                var clipExits = clip != null;

                // Create
                if (GUI.Button(createRect, new GUIContent(string.Empty, _localizationTable.AnimationElement_Tooltip_Create)))
                {
                    if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_NotPossibleInPlayMode, "OK"); return; }

                    var guid = GetAnimationGuidWithDialog(DialogMode.Create, path, defaultClipName);
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

                    var guid = GetAnimationGuidWithDialog(DialogMode.Open, path, defaultClipName);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        _expressionEditor.OpenIfOpenedAlready(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)));
                        setAnimationClipAction(guid);
                    }
                }
                GUI.DrawTexture(new Rect(openRect.x + iconMargin, openRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), _openIcon, ScaleMode.ScaleToFit, alphaBlend: true);

                // Combine
                using (new EditorGUI.DisabledScope(!canCombine))
                {
                    if (GUI.Button(combineRect, new GUIContent(string.Empty, _localizationTable.AnimationElement_Tooltip_Combine)))
                    {
                        if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_NotPossibleInPlayMode, "OK"); return; }

                        if (leftCombine is null) { leftCombine = animation; }

                        CombineClipsDialog.Show(this, thumbnailDrawer,
                            thumbnailWidth, thumbnailHeight,
                            _expressionEditor, setAnimationClipAction,
                            leftAnimation: leftCombine, rightAnimation: rightCombine);
                    }
                }
                GUI.DrawTexture(new Rect(combineRect.x + iconMargin, combineRect.y + iconMargin, width - iconMargin * 2, height - iconMargin * 2), _combineIcon, ScaleMode.ScaleToFit, alphaBlend: true);
                if (!canCombine)
                {
                    GUI.DrawTexture(combineRect, _blackTranslucent, ScaleMode.StretchToFill, alphaBlend: true);
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
            var thumbnailWidth = EditorPrefsStore.MainViewThumbnailWidthInMemory;
            return thumbnailWidth;
        }

        public float GetHeight()
        {
            var thumbnailHeight = EditorPrefsStore.MainViewThumbnailHeightInMemory;
            return thumbnailHeight + EditorGUIUtility.singleLineHeight;
        }

        public string GetAnimationGuidWithDialog(DialogMode dialogMode, string existingAnimationPath, string defaultClipName)
        {
            // Open dialog and get the path of the AnimationClip
            var defaultDir = GetDefaultDir(existingAnimationPath, dialogMode);
            var selectedPath = string.Empty;
            if (dialogMode == DialogMode.Open || dialogMode == DialogMode.Copy)
            {
                var title = dialogMode == DialogMode.Open ? _localizationTable.AnimationElement_Dialog_Open : _localizationTable.AnimationElement_Dialog_Copy;
                selectedPath = EditorUtility.OpenFilePanelWithFilters(title: title, directory: defaultDir, filters: new[] { "AnimationClip" , "anim" });
            }
            else if (dialogMode == DialogMode.Create)
            {
                var baseName = !string.IsNullOrEmpty(defaultClipName) ? defaultClipName : _localizationTable.AnimationElement_NewClipName;
                var defaultName = baseName + DateTime.Now.ToString("_yyyyMMdd_HHmmss");
                selectedPath = EditorUtility.SaveFilePanelInProject(title: null, defaultName: defaultName, extension: "anim", message: null, path: defaultDir);
            }

            // Retrieve and return the GUID of the AnimationClip from the path
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // OpenFilePanel path is in OS format, so convert it to Unity format
                var unityPath = selectedPath;
                if (dialogMode == DialogMode.Open || dialogMode == DialogMode.Copy)
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
                    var src = unityPath;
                    var dirName = System.IO.Path.GetDirectoryName(src).Replace(System.IO.Path.DirectorySeparatorChar, '/');
                    var baseName = System.IO.Path.GetFileNameWithoutExtension(src);
                    var newName = GetNewAnimationName(dirName, baseName);
                    var dst = dirName + "/" + newName;
                    AssetDatabase.CopyAsset(src, dst);
                    unityPath = dst;
                }

                // Retrieve GUID
                var guid = AssetDatabase.AssetPathToGUID(unityPath);
                if (string.IsNullOrEmpty(guid))
                {
                    // Refresh and retry
                    AssetDatabase.Refresh();
                    guid = AssetDatabase.AssetPathToGUID(unityPath);
                }

                if (!string.IsNullOrEmpty(guid))
                {
                    _aV3Object.Update();
                    if (dialogMode == DialogMode.Open)
                    {
                        _aV3Object.FindProperty(nameof(AV3Setting.LastOpenedAnimationPath)).stringValue = unityPath;
                    }
                    else
                    {
                        _aV3Object.FindProperty(nameof(AV3Setting.LastSavedAnimationPath)).stringValue = unityPath;
                    }
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

        public enum DialogMode
        {
            Open,
            Create,
            Copy,
        }

        private string GetDefaultDir(string existingAnimationPath, DialogMode dialogMode)
        {
            string path;

            // Use existing animation path (open mode only)
            if (dialogMode == DialogMode.Open)
            {
                path = existingAnimationPath;
                while (!string.IsNullOrEmpty(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        return path;
                    }
                }
            }

            // Use last opened or saved animation path
            _aV3Object.Update();
            if (dialogMode == DialogMode.Open)
            {
                path = _aV3Object.FindProperty(nameof(AV3Setting.LastOpenedAnimationPath)).stringValue;
            }
            else
            {
                path = _aV3Object.FindProperty(nameof(AV3Setting.LastSavedAnimationPath)).stringValue;
            }
            while (!string.IsNullOrEmpty(path))
            {
                path = System.IO.Path.GetDirectoryName(path);
                if (AssetDatabase.IsValidFolder(path))
                {
                    return path;
                }
            }

            // Use default path
            return "Assets";
        }

        public static string GetNewAnimationName(string defaultDir, string baseAnimationName)
        {
            // Define a regex pattern to match the ending _(number)
            string pattern = @"_\(\d+\)$";
            Regex regex = new Regex(pattern);

            // Replace the matched pattern with an empty string
            baseAnimationName = regex.Replace(baseAnimationName, "");

            // Decide animation name
            var animationName = $"{baseAnimationName}.anim";
            for (int i = 2; i < int.MaxValue; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<AnimationClip>($"{defaultDir}/{animationName}") != null)
                {
                    animationName = $"{baseAnimationName}_({i}).anim";
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
