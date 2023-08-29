﻿using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Components.States;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Detail.View.Element;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UniRx;
using VRC.SDK3.Avatars.Components;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class InspectorView : IDisposable
    {
        private static readonly float ToggleWidth = 15;

        public IObservable<Unit> OnLaunchButtonClicked => _onLaunchButtonClicked.AsObservable();
        public IObservable<Locale> OnLocaleChanged => _onLocaleChanged.AsObservable();

        private Subject<Unit> _onLaunchButtonClicked = new Subject<Unit>();
        private Subject<Locale> _onLocaleChanged = new Subject<Locale>();

        private ILocalizationSetting _localizationSetting;
        private InspectorThumbnailDrawer _thumbnailDrawer;
        private SerializedObject _inspectorViewState;
        private SerializedObject _av3Setting;
        private SerializedObject _thumbnailSetting;

        private ReorderableList _subTargetAvatars;
        private ReorderableList _mouthMorphBlendShapes;
        private ReorderableList _additionalSkinnedMeshes;
        private ReorderableList _additionalToggleObjects;
        private ReorderableList _additionalTransformObjects;

        private LocalizationTable _localizationTable;

        private GUIStyle _centerStyle;
        private GUIStyle _boldStyle;
        private GUIStyle _versionLabelStyle = new GUIStyle();
        private GUIStyle _launchButtonStyle = new GUIStyle();
        private GUIStyle _warningLabelStyle = new GUIStyle();

        private CompositeDisposable _disposables = new CompositeDisposable();

        public InspectorView(
            ILocalizationSetting localizationSetting,
            InspectorThumbnailDrawer inspectorThumbnailDrawer,
            InspectorViewState inspectorViewState,
            AV3Setting av3Setting,
            ThumbnailSetting thumbnailSetting)
        {
            // Dependencies
            _localizationSetting = localizationSetting;
            _thumbnailDrawer = inspectorThumbnailDrawer;
            _inspectorViewState = new SerializedObject(inspectorViewState);
            _av3Setting = new SerializedObject(av3Setting);
            _thumbnailSetting = new SerializedObject(thumbnailSetting);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Styles
            try
            {
                _centerStyle = new GUIStyle(EditorStyles.label);
                _boldStyle = new GUIStyle(EditorStyles.label);
                _versionLabelStyle = new GUIStyle(EditorStyles.label);
                _launchButtonStyle = new GUIStyle(EditorStyles.miniButton);
                _warningLabelStyle = new GUIStyle(EditorStyles.label);
            }
            catch (NullReferenceException)
            {
                // Workaround for play mode
                _centerStyle = new GUIStyle();
                _boldStyle = new GUIStyle();
                _versionLabelStyle = new GUIStyle();
                _launchButtonStyle = new GUIStyle();
                _warningLabelStyle = new GUIStyle();
            }
            _centerStyle.alignment = TextAnchor.MiddleCenter;
            _boldStyle.fontStyle = FontStyle.Bold;
            _versionLabelStyle.fontSize = 15;
            _versionLabelStyle.fontStyle = FontStyle.Bold;
            _versionLabelStyle.alignment = TextAnchor.UpperCenter;
            _versionLabelStyle.padding = new RectOffset(10, 10, 10, 10);
            _launchButtonStyle.fontSize = 15;
            _launchButtonStyle.fixedHeight = EditorGUIUtility.singleLineHeight * 3;
            _warningLabelStyle.normal.textColor = Color.red;
            
            // Sub avatars
            _subTargetAvatars = new ReorderableList(_av3Setting, _av3Setting.FindProperty(nameof(AV3Setting.SubTargetAvatars)));
            _subTargetAvatars.headerHeight = 0;
            _subTargetAvatars.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _subTargetAvatars.serializedProperty.GetArrayElementAtIndex(index);
                var avatar = EditorGUI.ObjectField(rect, GUIContent.none, element.objectReferenceValue, typeof(VRCAvatarDescriptor), allowSceneObjects: true) as VRCAvatarDescriptor;
                if (!ReferenceEquals(avatar, element.objectReferenceValue))
                {
                    element.objectReferenceValue = avatar;
                }
            };
            _subTargetAvatars.drawNoneElementCallback = (Rect rect) =>
            {
                GUI.Label(rect, _localizationTable.InspectorView_EmptyAvatars, _centerStyle);
            };

            // Mouth morph blendshapes
            _mouthMorphBlendShapes = new ReorderableList(null, typeof(BlendShape));
            _mouthMorphBlendShapes.onAddCallback = AddMouthMorphBlendShape;
            _mouthMorphBlendShapes.onRemoveCallback = RemoveMouthMorphBlendShape;
            _mouthMorphBlendShapes.draggable = false;
            _mouthMorphBlendShapes.headerHeight = 0;
            _mouthMorphBlendShapes.drawNoneElementCallback = (Rect rect) =>
            {
                GUI.Label(rect, _localizationTable.InspectorView_EmptyBlendShapes, _centerStyle);
            };

            // Additional skinned meshes
            _additionalSkinnedMeshes = new ReorderableList(_av3Setting, _av3Setting.FindProperty(nameof(AV3Setting.AdditionalSkinnedMeshes)));
            _additionalSkinnedMeshes.headerHeight = 0;
            _additionalSkinnedMeshes.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _additionalSkinnedMeshes.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
            _additionalSkinnedMeshes.drawNoneElementCallback = (Rect rect) =>
            {
                GUI.Label(rect, _localizationTable.InspectorView_EmptyObjects, _centerStyle);
            };

            // Additional expression objects
            _additionalToggleObjects = new ReorderableList(_av3Setting, _av3Setting.FindProperty(nameof(AV3Setting.AdditionalToggleObjects)));
            _additionalToggleObjects.headerHeight = 0;
            _additionalToggleObjects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _additionalToggleObjects.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
            _additionalToggleObjects.drawNoneElementCallback = (Rect rect) =>
            {
                GUI.Label(rect, _localizationTable.InspectorView_EmptyObjects, _centerStyle);
            };

            _additionalTransformObjects = new ReorderableList(_av3Setting, _av3Setting.FindProperty(nameof(AV3Setting.AdditionalTransformObjects)));
            _additionalTransformObjects.headerHeight = 0;
            _additionalTransformObjects.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _additionalTransformObjects.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
            _additionalTransformObjects.drawNoneElementCallback = (Rect rect) =>
            {
                GUI.Label(rect, _localizationTable.InspectorView_EmptyObjects, _centerStyle);
            };

            // Set text
            SetText(_localizationSetting.Table);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void OnGUI()
        {
            _inspectorViewState.Update();
            _av3Setting.Update();
            _thumbnailSetting.Update();

            Field_CheckVersion();

            // Launch button
            var avatarDescriptor = _av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar)).objectReferenceValue as VRCAvatarDescriptor;
            var canLaunch = avatarDescriptor != null;
            using (new EditorGUI.DisabledScope(!canLaunch))
            {
                var buttonText = canLaunch ? _localizationTable.InspectorView_Launch : _localizationTable.InspectorView_TargetAvatarIsNotSpecified;
                if (GUILayout.Button(buttonText, _launchButtonStyle))
                {
                    _onLaunchButtonClicked.OnNext(Unit.Default);
                }
            }
            EditorGUILayout.Space(10);

            // Target avatar
            Field_TargetAvatar();

            EditorGUILayout.Space(10);

            // Locale
            Field_Locale();

            EditorGUILayout.Space(10);

            // Applying to multiple avatars
            var isApplyingToMultipleAvatarsOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsApplyingToMultipleAvatarsOpened));
            isApplyingToMultipleAvatarsOpened.boolValue = EditorGUILayout.Foldout(isApplyingToMultipleAvatarsOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_ApplyingToMultipleAvatars));
            if (isApplyingToMultipleAvatarsOpened.boolValue)
            {
                Field_ApplyingToMultipleAvatars();
            }

            EditorGUILayout.Space(10);

            // Mouth Morph Blend Shapes
            var isMouthMorphBlendShapesOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsMouthMorphBlendShapesOpened));
            isMouthMorphBlendShapesOpened.boolValue = EditorGUILayout.Foldout(isMouthMorphBlendShapesOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_MouthMorphBlendShapes));
            if (isMouthMorphBlendShapesOpened.boolValue)
            {
                Field_MouthMorphBlendShape();
            }

            EditorGUILayout.Space(10);

            // Additional Skinned Meshes
            var isAddtionalSkinnedMeshesOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsAddtionalSkinnedMeshesOpened));
            isAddtionalSkinnedMeshesOpened.boolValue = EditorGUILayout.Foldout(isAddtionalSkinnedMeshesOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_AddtionalSkinnedMeshes));
            if (isAddtionalSkinnedMeshesOpened.boolValue)
            {
                Field_AdditionalSkinnedMeshes();
            }

            EditorGUILayout.Space(10);

            // Additional Toggle Objects
            var isAddtionalToggleOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsAddtionalToggleOpened));
            isAddtionalToggleOpened.boolValue = EditorGUILayout.Foldout(isAddtionalToggleOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_AddtionalToggleObjects));
            if (isAddtionalToggleOpened.boolValue)
            {
                Field_AdditionalToggleObjects();
            }

            EditorGUILayout.Space(10);

            // Additional Transform Objects
            var isAddtionalTransformOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsAddtionalTransformOpened));
            isAddtionalTransformOpened.boolValue = EditorGUILayout.Foldout(isAddtionalTransformOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_AddtionalTransformObjects));
            if (isAddtionalTransformOpened.boolValue)
            {
                Field_AdditionalTransformObjects();
            }

            EditorGUILayout.Space(10);

            // AFK face setting
            var isAFKOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsAFKOpened));
            isAFKOpened.boolValue = EditorGUILayout.Foldout(isAFKOpened.boolValue, 
                new GUIContent(_localizationTable.InspectorView_AFK));
            if (isAFKOpened.boolValue)
            {
                Field_AFKFace();
            }

            EditorGUILayout.Space(10);

            // Thumbnail Setting
            var isThumbnailOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsThumbnailOpened));

            isThumbnailOpened.boolValue = EditorGUILayout.Foldout(isThumbnailOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_Thumbnail));
            if (isThumbnailOpened.boolValue)
            {
                Field_ThumbnailSetting();
            }

            EditorGUILayout.Space(10);

            // Expressions Menu Setting Items
            var isExpressionsMenuItemsOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsExpressionsMenuItemsOpened));
            isExpressionsMenuItemsOpened.boolValue = EditorGUILayout.Foldout(isExpressionsMenuItemsOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_ExpressionsMenuSettingItems));
            if (isExpressionsMenuItemsOpened.boolValue)
            {
                Field_ExpressionsMenuSettingItems();
            }

            EditorGUILayout.Space(10);

            // Avatar application setting
            var isAvatarApplicationOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsAvatarApplicationOpened));
            isAvatarApplicationOpened.boolValue = EditorGUILayout.Foldout(isAvatarApplicationOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_AvatarApplicationSetting));
            if (isAvatarApplicationOpened.boolValue)
            {
                Field_AvatarApplicationSetting();
            }

            EditorGUILayout.Space(10);

            // Defaults
            var isDefaultsOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsDefaultsOpened));
            isDefaultsOpened.boolValue = EditorGUILayout.Foldout(isDefaultsOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_Defaults));
            if (isDefaultsOpened.boolValue)
            {
                Field_Defaults();
            }

            EditorGUILayout.Space(10);

            // Editor setting
            var isEditorSettingOpened = _inspectorViewState.FindProperty(nameof(InspectorViewState.IsEditorSettingOpened));
            isEditorSettingOpened.boolValue = EditorGUILayout.Foldout(isEditorSettingOpened.boolValue,
                new GUIContent(_localizationTable.InspectorView_EditorSetting));
            if (isEditorSettingOpened.boolValue)
            {
                Field_EditorSetting();
            }

            EditorGUILayout.Space(10);

            _inspectorViewState.ApplyModifiedProperties();
            _av3Setting.ApplyModifiedProperties();
            _thumbnailSetting.ApplyModifiedProperties();
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        private void Field_CheckVersion()
        {
            if (!PackageVersionChecker.IsCompleted) { return; }

            GUILayout.Label($"{DomainConstants.SystemName} {PackageVersionChecker.FaceEmo}", _versionLabelStyle);

            if (string.IsNullOrEmpty(PackageVersionChecker.ModularAvatar))
            {
                HelpBoxDrawer.ErrorLayout(_localizationTable.InspectorView_Message_MAVersionError_NotFound);
            }
            else if (PackageVersionChecker.ModularAvatar == "1.5.0-beta-4" || PackageVersionChecker.ModularAvatar == "1.5.0")
            {
                HelpBoxDrawer.ErrorLayout(_localizationTable.InspectorView_Message_MAVersionError_1_5_0);
            }
        }

        private void Field_TargetAvatar()
        {
            var property = _av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar));
            var avatar = EditorGUILayout.ObjectField(new GUIContent(_localizationTable.InspectorView_TargetAvatar), property.objectReferenceValue, typeof(VRCAvatarDescriptor), allowSceneObjects: true) as VRCAvatarDescriptor;
            if (!ReferenceEquals(avatar, property.objectReferenceValue))
            {
                property.objectReferenceValue = avatar;
            }

            if (avatar != null && avatar.gameObject?.scene.name == null)
            {
                EditorGUILayout.LabelField(_localizationTable.InspectorView_Message_TargetAvatarNotInScene, _warningLabelStyle);
            }
        }

        private void Field_Locale()
        {
            using (new EditorGUILayout.HorizontalScope())
            { 
                var oldLocale = _localizationSetting.Locale;
                var newLocale = (Locale)EditorGUILayout.EnumPopup(new GUIContent("言語設定 (Language Setting)"), oldLocale);
                if (newLocale != oldLocale)
                {
                    _localizationSetting.SetLocale(newLocale);
                    _onLocaleChanged.OnNext(newLocale);
                    SetText(_localizationSetting.Table);
                }
            }
        }

        private void Field_ApplyingToMultipleAvatars()
        {
            // Sub avatars
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Help_ApplyingToMultipleAvatars);
            }

            _subTargetAvatars.DoLayoutList();

            var mainAvatar = _av3Setting?.FindProperty(nameof(AV3Setting.TargetAvatar)).objectReferenceValue as VRCAvatarDescriptor;
            var subAvatars = _av3Setting?.FindProperty(nameof(AV3Setting.SubTargetAvatars));
            for (int i = 0; i < subAvatars?.arraySize; i++)
            {
                var avatar = subAvatars?.GetArrayElementAtIndex(i)?.objectReferenceValue as VRCAvatarDescriptor;
                if (avatar == null) { continue; }
                if (mainAvatar != null && ReferenceEquals(mainAvatar, avatar))
                {
                    EditorGUILayout.LabelField($"{_localizationTable.InspectorView_Message_TargetAvatarIsInSubAvatars} ({avatar.gameObject.name})", _warningLabelStyle);
                }
                if (avatar.gameObject?.scene.name == null)
                {
                    EditorGUILayout.LabelField($"{_localizationTable.InspectorView_Message_SubAvatarNotInScene} ({avatar.gameObject.name})", _warningLabelStyle);
                }
            }

            EditorGUILayout.Space(10);

            // Menu prefab
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Help_MenuPrefab);
            }

            var prefab = _av3Setting.FindProperty(nameof(AV3Setting.MARootObjectPrefab))?.objectReferenceValue;
            using (new EditorGUI.DisabledScope(prefab == null))
            {
                EditorGUILayout.ObjectField(new GUIContent(_localizationTable.InspectorView_MenuPrefab), prefab, typeof(VRCAvatarDescriptor), allowSceneObjects: false);
            }
        }

        private void AddMouthMorphBlendShape(ReorderableList reorderableList)
        {
            var faceBlendShapes = new List<BlendShape>();
            _av3Setting.Update();
            var avatarDescriptor = _av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar)).objectReferenceValue as VRCAvatarDescriptor;
            if (avatarDescriptor != null)
            {
                var replaceBlink = _av3Setting.FindProperty(nameof(AV3Setting.ReplaceBlink)).boolValue;
                var excludeBlink = false;
                var excludeLipSync = true;
                faceBlendShapes = AV3Utility.GetFaceMeshBlendShapeValues(avatarDescriptor, excludeBlink, excludeLipSync).Select(x => x.Key).ToList();

                foreach (var mesh in GetValue<List<SkinnedMeshRenderer>>(_av3Setting.FindProperty(nameof(AV3Setting.AdditionalSkinnedMeshes))))
                {
                    var blendShapes = AV3Utility.GetBlendShapeValues(mesh, avatarDescriptor, excludeBlink, excludeLipSync);
                    foreach (var item in blendShapes) { faceBlendShapes.Add(item.Key); }
                }
            }

            UnityEditor.PopupWindow.Show(
                new Rect(Event.current.mousePosition, Vector2.one),
                new ListSelectPopupContent<BlendShape>(faceBlendShapes, _localizationTable.Common_Add, _localizationTable.Common_Cancel,
                new Action<IReadOnlyList<BlendShape>>(added =>
                {
                    _av3Setting.Update();

                    var property = _av3Setting.FindProperty(nameof(AV3Setting.MouthMorphs));
                    var list = GetValue<List<BlendShape>>(property);
                    foreach (var item in added)
                    {
                        if (!list.Contains(item)) { list.Add(item); }
                    }
                    SetValue(property, list);

                    _av3Setting.ApplyModifiedProperties();
                })));
        }

        private void RemoveMouthMorphBlendShape(ReorderableList reorderableList)
        {
            // Check for abnormal selections.
            if (_mouthMorphBlendShapes.index >= _mouthMorphBlendShapes.list.Count)
            {
                return;
            }

            var selected = _mouthMorphBlendShapes.list[_mouthMorphBlendShapes.index] as BlendShape;

            _av3Setting.Update();

            var property = _av3Setting.FindProperty(nameof(AV3Setting.MouthMorphs));
            var list = GetValue<List<BlendShape>>(property);
            while (list.Contains(selected))
            {
                list.Remove(selected);
            }
            SetValue(property, list);

            _av3Setting.ApplyModifiedProperties();
        }

        private void Field_MouthMorphBlendShape()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_MouthMorphCanceler);
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_AddMouthMorphBlendShape);
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_ConfirmMouthMorphBlendShape);
            }

            _av3Setting.Update();
            var mouthMorphsProperty = _av3Setting.FindProperty(nameof(AV3Setting.MouthMorphs));
            var mouthMorphs = GetValue<List<BlendShape>>(mouthMorphsProperty);

            #pragma warning disable CS0612
            var obsoleteProperty = _av3Setting.FindProperty(nameof(AV3Setting.MouthMorphBlendShapes));
            if (obsoleteProperty.arraySize > 0)
            {
                var obsolete = GetValue<List<string>>(obsoleteProperty);
                var avatarDescriptor = _av3Setting.FindProperty(nameof(AV3Setting.TargetAvatar)).objectReferenceValue as VRCAvatarDescriptor;
                var faceMesh = AV3Utility.GetFaceMesh(avatarDescriptor);
                if (faceMesh != null)
                {
                    var faceMeshPath = AV3Utility.GetPathFromAvatarRoot(faceMesh.transform, avatarDescriptor);
                    foreach (var name in obsolete)
                    {
                        var blendShape = new BlendShape(path: faceMeshPath, name: name);
                        if (!mouthMorphs.Contains(blendShape)) { mouthMorphs.Add(blendShape); }
                    }
                    obsolete.Clear();
                    SetValue(obsoleteProperty, obsolete);
                    SetValue(mouthMorphsProperty, mouthMorphs);
                }
            }
            #pragma warning restore CS0612

            _mouthMorphBlendShapes.list = mouthMorphs; // Is it necessary to get every frame?
            _mouthMorphBlendShapes.DoLayoutList();

            EditorGUILayout.Space(10);

            if (GUILayout.Button(_localizationTable.Common_DeleteAll) &&
                OptoutableDialog.Show(DomainConstants.SystemName,
                    _localizationTable.InspectorView_Message_ClearMouthMorphBlendShapes,
                    _localizationTable.Common_Delete, _localizationTable.Common_Cancel, isRiskyAction: true))
            {
                _av3Setting.FindProperty(nameof(AV3Setting.MouthMorphs)).ClearArray();
                _av3Setting.ApplyModifiedProperties();
            }
        }

        private void Field_AdditionalSkinnedMeshes()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_AdditionalSkinnedMeshes);
            }

            var avatarTransform = (_av3Setting?.FindProperty(nameof(AV3Setting.TargetAvatar))?.objectReferenceValue as VRCAvatarDescriptor)?.gameObject?.transform;

            _additionalSkinnedMeshes.DoLayoutList();

            var meshProperty = _av3Setting?.FindProperty(nameof(AV3Setting.AdditionalSkinnedMeshes));
            for (int i = 0; i < meshProperty?.arraySize; i++)
            {
                var skinnedMesh = meshProperty?.GetArrayElementAtIndex(i)?.objectReferenceValue as SkinnedMeshRenderer;
                if (skinnedMesh == null) { continue; }
                if (avatarTransform == null || !IsDescendantOf(skinnedMesh.transform, avatarTransform))
                {
                    EditorGUILayout.LabelField($"{skinnedMesh.name}{_localizationTable.InspectorView_Message_NotInAvatar}", _warningLabelStyle);
                }
            }
        }

        private void Field_AdditionalToggleObjects()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_AdditionalToggle);
            }

            var avatarTransform = (_av3Setting?.FindProperty(nameof(AV3Setting.TargetAvatar))?.objectReferenceValue as VRCAvatarDescriptor)?.gameObject?.transform;

            _additionalToggleObjects.DoLayoutList();

            var toggleProperty = _av3Setting?.FindProperty(nameof(AV3Setting.AdditionalToggleObjects));
            for (int i = 0; i < toggleProperty?.arraySize; i++)
            {
                var gameObject = toggleProperty?.GetArrayElementAtIndex(i)?.objectReferenceValue as GameObject;
                if (gameObject == null) { continue; }
                if (avatarTransform == null || !IsDescendantOf(gameObject.transform, avatarTransform))
                {
                    EditorGUILayout.LabelField($"{gameObject.name}{_localizationTable.InspectorView_Message_NotInAvatar}", _warningLabelStyle);
                }
            }
        }

        private void Field_AdditionalTransformObjects()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_AdditionalTransform);
            }

            var avatarTransform = (_av3Setting?.FindProperty(nameof(AV3Setting.TargetAvatar))?.objectReferenceValue as VRCAvatarDescriptor)?.gameObject?.transform;

            _additionalTransformObjects.DoLayoutList();

            var transformProperty = _av3Setting?.FindProperty(nameof(AV3Setting.AdditionalTransformObjects));
            for (int i = 0; i < transformProperty?.arraySize; i++)
            {
                var gameObject = transformProperty?.GetArrayElementAtIndex(i)?.objectReferenceValue as GameObject;
                if (gameObject == null) { continue; }
                if (avatarTransform == null || !IsDescendantOf(gameObject.transform, avatarTransform))
                {
                    EditorGUILayout.LabelField($"{gameObject.name}{_localizationTable.InspectorView_Message_NotInAvatar}", _warningLabelStyle);
                }
            }
        }

        private void Field_AFKFace()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_AFK);
            }

            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AfkEnterFace)), new GUIContent(_localizationTable.InspectorView_AFK_EnterFace));
            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AfkFace)), new GUIContent(_localizationTable.InspectorView_AFK_Face));
            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AfkExitFace)), new GUIContent(_localizationTable.InspectorView_AFK_ExitFace));
        }

        private void Field_ThumbnailSetting()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_Thumbnail);
            }

            // Draw thumbnail
            float aspectRatio = 1;
            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            float newHeight = inspectorWidth * aspectRatio;
            var textureWidth = (int)inspectorWidth;
            var textureHeight = (int)newHeight;
            Rect textureRect = GUILayoutUtility.GetRect(textureWidth, textureHeight, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            var width = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Inspector_Width));
            var height = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Inspector_Height));
            if (width.intValue != textureWidth ||
                height.intValue != textureHeight)
            {
                width.intValue = textureWidth;
                height.intValue = textureHeight;
                _thumbnailDrawer.RequestUpdateAll();
            }
            _thumbnailDrawer.Update();
            var texture = _thumbnailDrawer.GetThumbnail(new Domain.Animation(string.Empty));
            GUI.DrawTexture(textureRect, texture);

            EditorGUILayout.Space(10);

            // Draw sliders
            var labelTexts = new []
            {
                _localizationTable.InspectorView_Thumbnail_FOV,
                _localizationTable.InspectorView_Thumbnail_Distance,
                _localizationTable.InspectorView_Thumbnail_HorizontalPosition,
                _localizationTable.InspectorView_Thumbnail_VerticalPosition,
                _localizationTable.InspectorView_Thumbnail_HorizontalAngle,
                _localizationTable.InspectorView_Thumbnail_VerticalAngle,
            };
            var labelWidth = labelTexts
                .Select(text => GUI.skin.label.CalcSize(new GUIContent(text)).x)
                .DefaultIfEmpty()
                .Max();
            labelWidth += 10;

            var fov = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_FOV));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_FOV, fov.floatValue, ThumbnailSetting.MinFOV, ThumbnailSetting.MaxFOV,
                value => { fov.floatValue = value; _thumbnailDrawer.RequestUpdateAll(); }, labelWidth);

            var distance = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_Distance));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_Distance, distance.floatValue, ThumbnailSetting.MinDistance, ThumbnailSetting.MaxDistance,
                value => { distance.floatValue = value; _thumbnailDrawer.RequestUpdateAll(); }, labelWidth);

            var hPosition = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_CameraPosX));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_HorizontalPosition, hPosition.floatValue, 0, 1,
                value => { hPosition.floatValue = value; _thumbnailDrawer.RequestUpdateAll(); }, labelWidth);

            var vPosition = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_CameraPosY));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_VerticalPosition, vPosition.floatValue, 0, 1,
                value => { vPosition.floatValue = value; _thumbnailDrawer.RequestUpdateAll(); }, labelWidth);

            var hAngle = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_CameraAngleH));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_HorizontalAngle, hAngle.floatValue, ThumbnailSetting.MaxCameraAngleH * -1, ThumbnailSetting.MaxCameraAngleH,
                value => { hAngle.floatValue = value; _thumbnailDrawer.RequestUpdateAll(); }, labelWidth);

            var vAngle = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_CameraAngleV));
            Field_Slider(_localizationTable.InspectorView_Thumbnail_VerticalAngle, vAngle.floatValue, ThumbnailSetting.MaxCameraAngleV * -1, ThumbnailSetting.MaxCameraAngleV,
                value => { vAngle.floatValue = value; _thumbnailDrawer.RequestUpdateAll(); }, labelWidth);

            EditorGUILayout.Space(10);

            // Draw reset button
            if (GUILayout.Button(_localizationTable.InspectorView_Thumbnail_Reset))
            {
                fov.floatValue = ThumbnailSetting.DefaultFOV;
                distance.floatValue = ThumbnailSetting.DefaultDistance;
                hPosition.floatValue = ThumbnailSetting.DefaultCameraPosX;
                vPosition.floatValue = ThumbnailSetting.DefaultCameraPosY;
                hAngle.floatValue = ThumbnailSetting.DefaultCameraAngleH;
                vAngle.floatValue = ThumbnailSetting.DefaultCameraAngleV;
                _thumbnailDrawer.RequestUpdateAll();
            }
        }

        private void Field_Slider(string labelText, float value, float minValue, float maxValue, Action<float> onValueChanged, float labelWidth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Label
                GUIContent labelContent = new GUIContent(labelText);
                Rect labelRect = GUILayoutUtility.GetRect(labelContent, GUI.skin.label, GUILayout.Width(labelWidth));
                GUI.Label(labelRect, labelContent);

                // Slider
                Rect sliderRect = GUILayoutUtility.GetRect(labelRect.x, labelRect.y, GUILayout.ExpandWidth(true));
                var sliderValue = GUI.HorizontalSlider(sliderRect, value, minValue, maxValue);
                if (!Mathf.Approximately(sliderValue, value))
                {
                    onValueChanged(sliderValue);
                }

                // FloatField
                var fieldValue = EditorGUILayout.FloatField(value, GUILayout.Width(80));
                if (!Mathf.Approximately(fieldValue, value))
                {
                    onValueChanged(fieldValue);
                }
            }
        }

        private void Field_ExpressionsMenuSettingItems()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_ExMenu);
            }

            var labelTexts = new []
            {
                _localizationTable.InspectorView_AddConfig_BlinkOff,
                _localizationTable.InspectorView_AddConfig_DanceGimmick,
                _localizationTable.InspectorView_AddConfig_ContactLock,
                _localizationTable.InspectorView_AddConfig_Override,
                _localizationTable.InspectorView_AddConfig_Voice,
                _localizationTable.InspectorView_AddConfig_HandPattern,
                _localizationTable.InspectorView_AddConfig_Controller,
                _localizationTable.ExMenu_HandPattern_SwapLR,
                _localizationTable.ExMenu_HandPattern_DisableLeft,
                _localizationTable.ExMenu_HandPattern_DisableRight,
                _localizationTable.ExMenu_Controller_Quest,
                _localizationTable.ExMenu_Controller_Index,
            };
            var labelWidth = labelTexts
                .Select(text => GUI.skin.label.CalcSize(new GUIContent(text)).x)
                .DefaultIfEmpty()
                .Max();
            labelWidth += 10;

            Field_ExpressionsMenuItem(_localizationTable.InspectorView_AddConfig_BlinkOff,      _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_BlinkOff)),        null,                                                                   _localizationTable.InspectorView_Tooltip_ExMenu_BlinkOff,       labelWidth);
            Field_ExpressionsMenuItem(_localizationTable.InspectorView_AddConfig_DanceGimmick,  _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_DanceGimmick)),    null,                                                                   _localizationTable.InspectorView_Tooltip_ExMenu_Dance,          labelWidth);
            Field_ExpressionsMenuItem(_localizationTable.InspectorView_AddConfig_ContactLock,   _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_ContactLock)),     _av3Setting.FindProperty(nameof(AV3Setting.DefaultValue_ContactLock)),  _localizationTable.InspectorView_Tooltip_ExMenu_ContactLock,    labelWidth);
            Field_ExpressionsMenuItem(_localizationTable.InspectorView_AddConfig_Override,      _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_Override)),        _av3Setting.FindProperty(nameof(AV3Setting.DefaultValue_Override)),     _localizationTable.InspectorView_Tooltip_ExMenu_Override,       labelWidth);
            Field_ExpressionsMenuItem(_localizationTable.InspectorView_AddConfig_Voice,         _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_Voice)),           _av3Setting.FindProperty(nameof(AV3Setting.DefaultValue_Voice)),        _localizationTable.InspectorView_Tooltip_ExMenu_Voice,          labelWidth);

            GUILayout.Label(new GUIContent(_localizationTable.InspectorView_AddConfig_HandPattern, _localizationTable.InspectorView_Tooltip_ExMenu_Gesture), _boldStyle);
            Field_ExpressionsMenuItem(_localizationTable.ExMenu_HandPattern_SwapLR,         _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_HandPattern_Swap)),            _av3Setting.FindProperty(nameof(AV3Setting.DefaultValue_HandPattern_Swap)),         _localizationTable.InspectorView_Tooltip_ExMenu_Gesture_Swap,           labelWidth);
            Field_ExpressionsMenuItem(_localizationTable.ExMenu_HandPattern_DisableLeft,    _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_HandPattern_DisableLeft)),     _av3Setting.FindProperty(nameof(AV3Setting.DefaultValue_HandPattern_DisableLeft)),  _localizationTable.InspectorView_Tooltip_ExMenu_Gesture_DisableLeft,    labelWidth);
            Field_ExpressionsMenuItem(_localizationTable.ExMenu_HandPattern_DisableRight,   _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_HandPattern_DisableRight)),    _av3Setting.FindProperty(nameof(AV3Setting.DefaultValue_HandPattern_DisableRight)), _localizationTable.InspectorView_Tooltip_ExMenu_Gesture_DisableRight,   labelWidth);

            GUILayout.Label(new GUIContent(_localizationTable.InspectorView_AddConfig_Controller, _localizationTable.InspectorView_Tooltip_ExMenu_Controller), _boldStyle);
            Field_ExpressionsMenuItem(_localizationTable.ExMenu_Controller_Quest,         _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_Controller_Quest)),            _av3Setting.FindProperty(nameof(AV3Setting.DefaultValue_Controller_Quest)),         _localizationTable.InspectorView_Tooltip_ExMenu_Controller_Quest,   labelWidth);
            Field_ExpressionsMenuItem(_localizationTable.ExMenu_Controller_Index,         _av3Setting.FindProperty(nameof(AV3Setting.AddConfig_Controller_Index)),            _av3Setting.FindProperty(nameof(AV3Setting.DefaultValue_Controller_Index)),         _localizationTable.InspectorView_Tooltip_ExMenu_Controller_Index,   labelWidth);
        }

        private void Field_ExpressionsMenuItem(string labelText, SerializedProperty enableProperty, SerializedProperty defaultValueProperty, string toolTip, float labelWidth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Label
                GUIContent labelContent = new GUIContent(labelText, toolTip);
                Rect labelRect = GUILayoutUtility.GetRect(labelContent, GUI.skin.label, GUILayout.Width(labelWidth));
                GUI.Label(labelRect, labelContent);

                // Enable toggle
                const float margin = 10;
                var value = EditorGUILayout.Toggle(string.Empty, enableProperty.boolValue, GUILayout.Width(ToggleWidth));
                if (value != enableProperty.boolValue)
                {
                    enableProperty.boolValue = value;
                }
                var enableLabelContent = new GUIContent(_localizationTable.InspectorView_AddConfig_Add);
                GUILayout.Label(enableLabelContent, GUILayout.Width(GUI.skin.label.CalcSize(enableLabelContent).x + margin));

                // Default value toggle
                if (defaultValueProperty != null)
                {
                    TogglePropertyField(defaultValueProperty, _localizationTable.InspectorView_AddConfig_Default);
                }
            }
        }

        private void Field_AvatarApplicationSetting()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_Application);
            }


            var label = new GUIContent(_localizationTable.InspectorView_TransitionDuration);
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Math.Max(GUI.skin.label.CalcSize(label).x, EditorGUIUtility.labelWidth);
            EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.TransitionDurationSeconds)), label);
            EditorGUIUtility.labelWidth = oldLabelWidth;

            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddConfig_EmoteSelect)), _localizationTable.InspectorView_EmoteSelect, tooltip: _localizationTable.InspectorView_Tooltip_Application_EmoteSelect);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.GenerateExMenuThumbnails)), _localizationTable.InspectorView_GenerateModeThumbnails);
            using (new EditorGUI.DisabledScope(!_av3Setting.FindProperty(nameof(AV3Setting.GenerateExMenuThumbnails)).boolValue))
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(string.Empty, GUILayout.Width(ToggleWidth));

                label = new GUIContent(_localizationTable.InspectorView_GammaCorrectionValue);
                oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = Math.Max(GUI.skin.label.CalcSize(label).x, EditorGUIUtility.labelWidth);
                EditorGUILayout.PropertyField(_av3Setting.FindProperty(nameof(AV3Setting.GammaCorrectionValueForExMenuThumbnails)), label);
                EditorGUIUtility.labelWidth = oldLabelWidth;
            }
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.SmoothAnalogFist)), _localizationTable.InspectorView_SmoothAnalogFist);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.AddParameterPrefix)), _localizationTable.InspectorView_AddExpressionParameterPrefix);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ReplaceBlink)), _localizationTable.InspectorView_ReplaceBlink);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.DisableTrackingControls)), _localizationTable.InspectorView_DisableTrackingControls);
        }

        private void Field_Defaults()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_Defaults);
            }

            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ExpressionDefaults_ChangeDefaultFace)), _localizationTable.MenuItemListView_ChangeDefaultFace, _localizationTable.MenuItemListView_Tooltip_ChangeDefaultFace);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ExpressionDefaults_UseAnimationNameAsDisplayName)), _localizationTable.MenuItemListView_UseAnimationNameAsDisplayName, _localizationTable.MenuItemListView_Tooltip_UseAnimationNameAsDisplayName);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ExpressionDefaults_EyeTrackingEnabled)), _localizationTable.MenuItemListView_EyeTracking, _localizationTable.Common_Tooltip_EyeTracking);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ExpressionDefaults_BlinkEnabled)), _localizationTable.MenuItemListView_Blink, _localizationTable.Common_Tooltip_Blink);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ExpressionDefaults_MouthTrackingEnabled)), _localizationTable.MenuItemListView_MouthTracking, _localizationTable.Common_Tooltip_LipSync);
            TogglePropertyField(_av3Setting.FindProperty(nameof(AV3Setting.ExpressionDefaults_MouthMorphCancelerEnabled)), _localizationTable.MenuItemListView_MouthMorphCanceler, _localizationTable.Common_Tooltip_MouthMorphCanceler);
        }

        private void Field_EditorSetting()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            if (showHints)
            {
                HelpBoxDrawer.InfoLayout(_localizationTable.InspectorView_Tooltip_Editor);
            }

            // Need to resolve some issues
            // ToggleEditorPrefsField(DetailConstants.KeyAutoSave, DetailConstants.DefaultAutoSave, _localizationTable.InspectorView_AutoSave);
            ToggleEditorPrefsField(DetailConstants.KeyGroupDeleteConfirmation, DetailConstants.DefaultGroupDeleteConfirmation, _localizationTable.InspectorView_GroupDeleteConfirmation);
            ToggleEditorPrefsField(DetailConstants.KeyModeDeleteConfirmation, DetailConstants.DefaultModeDeleteConfirmation, _localizationTable.InspectorView_ModeDeleteConfirmation);
            ToggleEditorPrefsField(DetailConstants.KeyBranchDeleteConfirmation, DetailConstants.DefaultBranchDeleteConfirmation, _localizationTable.InspectorView_BranchDeleteConfirmation);
            ToggleEditorPrefsField(DetailConstants.Key_ExpressionEditor_ShowBlinkBlendShapes, DetailConstants.Default_ExpressionEditor_ShowBlinkBlendShapes, _localizationTable.InspectorView_ShowBlinkBlendShapes);
            ToggleEditorPrefsField(DetailConstants.Key_ExpressionEditor_ShowLipSyncBlendShapes, DetailConstants.Default_ExpressionEditor_ShowLipSyncBlendShapes, _localizationTable.InspectorView_ShowLipSyncBlendShapes);
        }

        private static void TogglePropertyField(SerializedProperty serializedProperty, string label, string tooltip = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var value = EditorGUILayout.Toggle(string.Empty, serializedProperty.boolValue, GUILayout.Width(ToggleWidth));
                if (value != serializedProperty.boolValue)
                {
                    serializedProperty.boolValue = value;
                }
                GUILayout.Label(new GUIContent(label, tooltip));
            }
        }

        private static void ToggleEditorPrefsField(string key, bool defaultValue, string label)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var oldValue = EditorPrefs.HasKey(key) ? EditorPrefs.GetBool(key) : defaultValue;
                var newValue = EditorGUILayout.Toggle(string.Empty, oldValue, GUILayout.Width(ToggleWidth));
                if (newValue != oldValue)
                {
                    EditorPrefs.SetBool(key, newValue);
                }
                GUILayout.Label(label);
            }
        }

        bool IsDescendantOf(Transform child, Transform potentialAncestor)
        {
            while (child != null)
            {
                if (child == potentialAncestor)
                    return true;

                child = child.parent;
            }
            return false;
        }

        private static T GetValue<T>(SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            System.Reflection.FieldInfo fieldInfo = obj.GetType().GetField(property.propertyPath);
            if (fieldInfo != null)
            {
                return (T)fieldInfo.GetValue(obj);
            }
            return default(T);
        }

        private static void SetValue<T>(SerializedProperty property, T newValue)
        {
            object obj = property.serializedObject.targetObject;
            System.Reflection.FieldInfo fieldInfo = obj.GetType().GetField(property.propertyPath);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, newValue);
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
