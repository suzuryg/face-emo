using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.Components;
using Suzuryg.FaceEmo.Components.Data;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Data;
using Suzuryg.FaceEmo.Detail.View;
using UnityEditor;
using UnityEngine;
using Suzuryg.FaceEmo.Domain;
using System;
using System.Linq;
using UniRx;
using VRC.SDK3.Avatars.Components;
using Suzuryg.FaceEmo.Detail.Localization;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;

namespace Suzuryg.FaceEmo.Detail
{
    public class FaceEmoBackupper : IBackupper, IDisposable
    {
        public static readonly string BackupDir = $"Assets/Suzuryg/{DomainConstants.SystemName}/AutoBackup";
        public static readonly int MaxNumOfAutoBackup = 100;

        private string _backupName;
        private IMenuBackupper _menuBackupper;
        private IMenuRepository _menuRepository;
        private SelectionSynchronizer _selectionSynchronizer;
        private AV3Setting _aV3Setting;
        private ExpressionEditorSetting _expressionEditorSetting;
        private ThumbnailSetting _thumbnailSetting;
        private SerializedObject _restorationCheckpoint;
        private LocalizationTable _localizationTable;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public FaceEmoBackupper(
            IMenuBackupper menuBackupper,
            IMenuRepository menuRepository,
            SelectionSynchronizer selectionSynchronizer,
            AV3Setting aV3Setting,
            ExpressionEditorSetting expressionEditorSetting,
            ThumbnailSetting thumbnailSetting,
            RestorationCheckpoint restorationCheckpoint,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _menuBackupper = menuBackupper;
            _menuRepository = menuRepository;
            _selectionSynchronizer = selectionSynchronizer;
            _aV3Setting = aV3Setting;
            _expressionEditorSetting = expressionEditorSetting;
            _thumbnailSetting = thumbnailSetting;
            _restorationCheckpoint = new SerializedObject(restorationCheckpoint);

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
            var guids = AssetDatabase.FindAssets($"t:{nameof(FaceEmoProject)}", new[] { dir });
            if (guids.Count() >= MaxNumOfAutoBackup)
            {
                var toRemove = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).OrderBy(x => x).First();
                AssetDatabase.DeleteAsset(toRemove);
            }

            var path = $"{dir}/{_backupName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.asset";
            var project = SaveProject(path);

            _restorationCheckpoint.Update();
            _restorationCheckpoint.FindProperty(nameof(RestorationCheckpoint.TargetAvatar)).objectReferenceValue = _aV3Setting.TargetAvatar;
            _restorationCheckpoint.FindProperty(nameof(RestorationCheckpoint.LatestBackup)).objectReferenceValue = project;
            _restorationCheckpoint.ApplyModifiedProperties();
        }

        public void Export(string path) => SaveProject(path);

        public FaceEmoProject SaveProject(string path)
        {
            BackupPath();

            var project = ScriptableObject.CreateInstance<FaceEmoProject>();
            AssetDatabase.CreateAsset(project, path);

            project.SerializableMenu = ScriptableObject.CreateInstance<SerializableMenu>();
            AssetDatabase.AddObjectToAsset(project.SerializableMenu, project);
            _menuBackupper.Export(project.SerializableMenu);
            project.SerializableMenu.name = nameof(SerializableMenu);

            project.AV3Setting = ScriptableObject.CreateInstance<AV3Setting>();
            AssetDatabase.AddObjectToAsset(project.AV3Setting, project);
            EditorUtility.CopySerialized(_aV3Setting, project.AV3Setting);
            project.AV3Setting.name = nameof(AV3Setting);
            // Avoid sharing FaceEmoPrefab when restored from backup.
            project.AV3Setting.MARootObjectPrefab = null;

            project.ExpressionEditorSetting = ScriptableObject.CreateInstance<ExpressionEditorSetting>();
            AssetDatabase.AddObjectToAsset(project.ExpressionEditorSetting, project);
            EditorUtility.CopySerialized(_expressionEditorSetting, project.ExpressionEditorSetting);
            project.ExpressionEditorSetting.name = nameof(ExpressionEditorSetting);

            project.ThumbnailSetting = ScriptableObject.CreateInstance<ThumbnailSetting>();
            AssetDatabase.AddObjectToAsset(project.ThumbnailSetting, project);
            EditorUtility.CopySerialized(_thumbnailSetting, project.ThumbnailSetting);
            project.ThumbnailSetting.name = nameof(ThumbnailSetting);

            AssetDatabase.SaveAssets();

            return project;
        }

        public void Import(string path)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) { throw new FaceEmoException(_localizationTable.Common_Message_GuidWasNotFound); }

            var imported = AssetDatabase.LoadAssetAtPath<FaceEmoProject>(path);
            if (imported == null) { throw new FaceEmoException("Failed to load FaceEmoProject asset."); }

            _menuBackupper.Import(imported.SerializableMenu);
            EditorUtility.CopySerialized(imported.AV3Setting, _aV3Setting);
            EditorUtility.CopySerialized(imported.ExpressionEditorSetting, _expressionEditorSetting);
            EditorUtility.CopySerialized(imported.ThumbnailSetting, _thumbnailSetting);

            RestorePath();

            var menu = _menuRepository.Load(string.Empty);
            if (menu.Registered.Order.Any())
            {
                _selectionSynchronizer.ChangeMenuItemListViewSelection(menu.Registered.Order.First());
            }
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        private void BackupPath()
        {
            // Avatar
            _aV3Setting.TargetAvatarPath = _aV3Setting.TargetAvatar?.gameObject?.GetFullPath();

            // Sub avatars
            _aV3Setting.SubTargetAvatarPaths.Clear();
            foreach (var subAvatar in _aV3Setting.SubTargetAvatars)
            {
                if (subAvatar != null) { _aV3Setting.SubTargetAvatarPaths.Add(subAvatar?.gameObject?.GetFullPath()); }
            }

            // Meshes
            _aV3Setting.AdditionalSkinnedMeshPaths.Clear();
            foreach (var mesh in _aV3Setting.AdditionalSkinnedMeshes)
            {
                if (mesh != null) { _aV3Setting.AdditionalSkinnedMeshPaths.Add(mesh.gameObject.GetFullPath()); }
            }

            // Toggles
            _aV3Setting.AdditionalToggleObjectPaths.Clear();
            foreach (var toggle in _aV3Setting.AdditionalToggleObjects)
            {
                if (toggle != null) { _aV3Setting.AdditionalToggleObjectPaths.Add(toggle.GetFullPath()); }
            }

            // Transforms
            _aV3Setting.AdditionalTransformObjectPaths.Clear();
            foreach (var transform in _aV3Setting.AdditionalTransformObjects)
            {
                if (transform != null) { _aV3Setting.AdditionalTransformObjectPaths.Add(transform.GetFullPath()); }
            }

            // Contact receivers
            _aV3Setting.ContactReceiverPaths.Clear();
            _aV3Setting.ContactReceiverParameterNames.Clear();
            foreach (var item in _aV3Setting.ContactReceivers)
            {
                if (item is ContactReceiver contactReceiver && contactReceiver != null)
                {
                    _aV3Setting.ContactReceiverPaths.Add(contactReceiver.gameObject.GetFullPath());
                    _aV3Setting.ContactReceiverParameterNames.Add(contactReceiver.parameter);
                }
            }
        }

        private void RestorePath()
        {
            // Avatar
            if (_aV3Setting.TargetAvatarPath.StartsWith("/") &&
                GameObject.Find(_aV3Setting.TargetAvatarPath) is GameObject avatarRoot && avatarRoot != null &&
                avatarRoot.GetComponent<VRCAvatarDescriptor>() is VRCAvatarDescriptor avatarDescriptor && avatarDescriptor != null)
            {
                _aV3Setting.TargetAvatar = avatarDescriptor;
            }
            else
            {
                Debug.LogError($"{_localizationTable.Backupper_Message_FailedToFindTargetAvatar}\n{_aV3Setting.TargetAvatarPath}");
            }

            // Sub avatars
            _aV3Setting.SubTargetAvatars.Clear();
            foreach (var path in _aV3Setting.SubTargetAvatarPaths)
            {
                if (path.StartsWith("/") &&
                    GameObject.Find(path) is GameObject gameObject && gameObject != null &&
                    gameObject.GetComponent<VRCAvatarDescriptor>() is VRCAvatarDescriptor subAvatar && subAvatar != null)
                {
                    _aV3Setting.SubTargetAvatars.Add(subAvatar);
                }
                else
                {
                    Debug.LogError($"{_localizationTable.Backupper_Message_FailedToFindSubTargetAvatar}\n{path}");
                }
            }

            // Meshes
            _aV3Setting.AdditionalSkinnedMeshes.Clear();
            foreach (var path in _aV3Setting.AdditionalSkinnedMeshPaths)
            {
                if (path.StartsWith("/") &&
                    GameObject.Find(path) is GameObject gameObject && gameObject != null &&
                    gameObject.GetComponent<SkinnedMeshRenderer>() is SkinnedMeshRenderer skinnedMesh && skinnedMesh != null)
                {
                    _aV3Setting.AdditionalSkinnedMeshes.Add(skinnedMesh);
                }
                else
                {
                    Debug.LogError($"{_localizationTable.Backupper_Message_FailedToFindSkinnedMesh}\n{path}");
                }
            }

            // Toggles
            _aV3Setting.AdditionalToggleObjects.Clear();
            foreach (var path in _aV3Setting.AdditionalToggleObjectPaths)
            {
                if (path.StartsWith("/") &&
                    GameObject.Find(path) is GameObject toggle && toggle != null)
                {
                    _aV3Setting.AdditionalToggleObjects.Add(toggle);
                }
                else
                {
                    Debug.LogError($"{_localizationTable.Backupper_Message_FailedToFindToggleObject}\n{path}");
                }
            }

            // Transforms
            _aV3Setting.AdditionalTransformObjects.Clear();
            foreach (var path in _aV3Setting.AdditionalTransformObjectPaths)
            {
                if (path.StartsWith("/") &&
                    GameObject.Find(path) is GameObject transform && transform != null)
                {
                    _aV3Setting.AdditionalTransformObjects.Add(transform);
                }
                else
                {
                    Debug.LogError($"{_localizationTable.Backupper_Message_FailedToFindTransformObject}\n{path}");
                }
            }

            // Contact receivers
            _aV3Setting.ContactReceivers.Clear();
            for (int i = 0; i < _aV3Setting.ContactReceiverPaths.Count; i++)
            {
                var path = _aV3Setting.ContactReceiverPaths[i];
                string parameterName;
                if (i < _aV3Setting.ContactReceiverParameterNames.Count)
                {
                    parameterName = _aV3Setting.ContactReceiverParameterNames[i];
                }
                else { break; }

                if (path.StartsWith("/") &&
                    GameObject.Find(path) is GameObject gameObject && gameObject != null)
                {
                    var receivers = gameObject.GetComponents<VRCContactReceiver>();
                    var found = false;
                    foreach (var receiver in receivers)
                    {
                        if (receiver != null && receiver.parameter == parameterName)
                        {
                            found = true;
                            _aV3Setting.ContactReceivers.Add(receiver);
                            break;
                        }
                    }

                    if (!found)
                    {
                        Debug.LogError($"{_localizationTable.Backupper_Message_FailedToFindContactReceiver}\npath: {path}, parameter: {parameterName}");
                    }
                }
                else
                {
                    Debug.LogError($"{_localizationTable.Backupper_Message_FailedToFindContactReceiver}\npath: {path}, parameter: {parameterName}");
                }
            }
        }
    }
}
