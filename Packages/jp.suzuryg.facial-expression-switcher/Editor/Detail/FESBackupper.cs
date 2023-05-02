using Suzuryg.FacialExpressionSwitcher.UseCase;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using UnityEditor;
using UnityEngine;
using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Linq;
using UniRx;
using VRC.SDK3.Avatars.Components;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public class FESBackupper : IBackupper, IDisposable
    {
        public static readonly string BackupDir = $"Assets/Suzuryg/{DomainConstants.SystemName}/AutoBackup";
        public static readonly int MaxNumOfAutoBackup = 100;

        private string _backupName;
        private IMenuBackupper _menuBackupper;
        private AV3Setting _aV3Setting;
        private ExpressionEditorSetting _expressionEditorSetting;
        private ThumbnailSetting _thumbnailSetting;
        private LocalizationTable _localizationTable;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public FESBackupper(
            IMenuBackupper menuBackupper,
            AV3Setting aV3Setting,
            ExpressionEditorSetting expressionEditorSetting,
            ThumbnailSetting thumbnailSetting,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _menuBackupper = menuBackupper;
            _aV3Setting = aV3Setting;
            _expressionEditorSetting = expressionEditorSetting;
            _thumbnailSetting = thumbnailSetting;

            // Localization table changed event handler
            localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);
            SetText(localizationSetting.Table);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void SetName(string name)
        {
            _backupName = AV3Utility.ConvertNameToSafePath(name);
        }

        public void AutoBackup()
        {
            if (string.IsNullOrEmpty(_backupName)) { Debug.LogError("Please set backup name."); return; }

            var dir = $"{BackupDir}/{_backupName}";
            AV3Utility.CreateFolderRecursively(dir);
            var guids = AssetDatabase.FindAssets($"t:{nameof(FESProject)}", new[] { dir });
            if (guids.Count() >= MaxNumOfAutoBackup)
            {
                var toRemove = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).OrderBy(x => x).First();
                AssetDatabase.DeleteAsset(toRemove);
            }

            var path = $"{dir}/{_backupName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.asset";
            Export(path);
        }
        
        public void Export(string path)
        {
            BackupPath();

            var project = ScriptableObject.CreateInstance<FESProject>();
            AssetDatabase.CreateAsset(project, path);

            project.SerializableMenu = ScriptableObject.CreateInstance<SerializableMenu>();
            AssetDatabase.AddObjectToAsset(project.SerializableMenu, project);
            _menuBackupper.Export(project.SerializableMenu);
            project.SerializableMenu.name = nameof(SerializableMenu);

            project.AV3Setting = ScriptableObject.CreateInstance<AV3Setting>();
            AssetDatabase.AddObjectToAsset(project.AV3Setting, project);
            EditorUtility.CopySerialized(_aV3Setting, project.AV3Setting);
            project.AV3Setting.name = nameof(AV3Setting);

            project.ExpressionEditorSetting = ScriptableObject.CreateInstance<ExpressionEditorSetting>();
            AssetDatabase.AddObjectToAsset(project.ExpressionEditorSetting, project);
            EditorUtility.CopySerialized(_expressionEditorSetting, project.ExpressionEditorSetting);
            project.ExpressionEditorSetting.name = nameof(ExpressionEditorSetting);

            project.ThumbnailSetting = ScriptableObject.CreateInstance<ThumbnailSetting>();
            AssetDatabase.AddObjectToAsset(project.ThumbnailSetting, project);
            EditorUtility.CopySerialized(_thumbnailSetting, project.ThumbnailSetting);
            project.ThumbnailSetting.name = nameof(ThumbnailSetting);

            AssetDatabase.SaveAssets();
        }

        public void Import(string path)
        {
            var imported = AssetDatabase.LoadAssetAtPath<FESProject>(path);
            if (imported is null) { throw new FacialExpressionSwitcherException("Failed to load FESProject asset."); }

            _menuBackupper.Import(imported.SerializableMenu);
            EditorUtility.CopySerialized(imported.AV3Setting, _aV3Setting);
            EditorUtility.CopySerialized(imported.ExpressionEditorSetting, _expressionEditorSetting);
            EditorUtility.CopySerialized(imported.ThumbnailSetting, _thumbnailSetting);

            RestorePath();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        private void BackupPath()
        {
            // Avatar
            _aV3Setting.TargetAvatarPath = _aV3Setting.TargetAvatar?.gameObject?.GetFullPath();

            // Toggles
            _aV3Setting.AdditionalToggleObjectPaths.Clear();
            foreach (var toggle in _aV3Setting.AdditionalToggleObjects)
            {
                _aV3Setting.AdditionalToggleObjectPaths.Add(toggle.GetFullPath());
            }

            // Transforms
            _aV3Setting.AdditionalTransformObjectPaths.Clear();
            foreach (var transform in _aV3Setting.AdditionalTransformObjects)
            {
                _aV3Setting.AdditionalTransformObjectPaths.Add(transform.GetFullPath());
            }
        }

        private void RestorePath()
        {
            // Avatar
            if (_aV3Setting.TargetAvatarPath.StartsWith("/") &&
                GameObject.Find(_aV3Setting.TargetAvatarPath) is GameObject avatarRoot &&
                avatarRoot.GetComponent<VRCAvatarDescriptor>() is VRCAvatarDescriptor avatarDescriptor)
            {
                _aV3Setting.TargetAvatar = avatarDescriptor;
            }
            else
            {
                Debug.LogError($"{_localizationTable.FESBackupper_Message_FailedToFindTargetAvatar}\n{_aV3Setting.TargetAvatarPath}");
            }

            // Toggles
            _aV3Setting.AdditionalToggleObjects.Clear();
            foreach (var path in _aV3Setting.AdditionalToggleObjectPaths)
            {
                if (path.StartsWith("/") &&
                    GameObject.Find(path) is GameObject toggle)
                {
                    _aV3Setting.AdditionalToggleObjects.Add(toggle);
                }
                else
                {
                    Debug.LogError($"{_localizationTable.FESBackupper_Message_FailedToFindToggleObject}\n{path}");
                }
            }

            // Transforms
            _aV3Setting.AdditionalTransformObjects.Clear();
            foreach (var path in _aV3Setting.AdditionalTransformObjectPaths)
            {
                if (path.StartsWith("/") &&
                    GameObject.Find(path) is GameObject transform)
                {
                    _aV3Setting.AdditionalTransformObjects.Add(transform);
                }
                else
                {
                    Debug.LogError($"{_localizationTable.FESBackupper_Message_FailedToFindTransformObject}\n{path}");
                }
            }
        }
    }
}
