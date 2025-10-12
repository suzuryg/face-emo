using System;
using System.IO;
using JetBrains.Annotations;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class LeftHeaderView : IDisposable
    {
        private static readonly string ClipNameFieldName = "ClipNameField";

        public IObservable<AnimationClip> OnOpenClipRequested => _onOpenClipRequested.AsObservable();

        private readonly Subject<AnimationClip> _onOpenClipRequested = new();
        private readonly CompositeDisposable _disposable = new();

        [CanBeNull] private AnimationClip _targetClip;
        [CanBeNull] private string _newClipName;
        private LocalizationTable _loc;
        private bool _isRenamingClip;

        public LeftHeaderView(IReadOnlyLocalizationSetting localizationSetting)
        {
            _loc = localizationSetting.Table;

            localizationSetting.OnTableChanged.Subscribe(table => _loc = table).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _onOpenClipRequested.Dispose();
        }

        public void OpenTargetClip(AnimationClip clip)
        {
            _targetClip = clip;
        }

        public void OnGUI()
        {
            Field_AnimationClip();
            Field_UseMouseWheel();
            Field_ShowOnlyDifferFromDefaultValue();
        }

        private void Field_AnimationClip()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                const float buttonWidth = 60;

                var clipExists = _targetClip != null;

                if (_isRenamingClip && clipExists)
                {
                    // Enter key event
                    var e = Event.current;
                    if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
                    {
                        RenameClip();
                    }

                    // Text field
                    GUI.SetNextControlName(ClipNameFieldName);
                    _newClipName = EditorGUILayout.TextField(_newClipName);

                    // Confirm button
                    if (GUILayout.Button(_loc.ExpressionEditorView_Confirm, GUILayout.Width(buttonWidth)))
                    {
                        RenameClip();
                    }
                }
                else
                {
                    // Update clip name
                    if (clipExists)
                    {
                        var path = AssetDatabase.GetAssetPath(_targetClip);
                        _newClipName = Path.GetFileName(path).Replace(".anim", "");
                    }

                    // Object field
                    var ret = EditorGUILayout.ObjectField(_targetClip, typeof(AnimationClip), allowSceneObjects: false);
                    if (ret is AnimationClip animationClip && animationClip != null && !ReferenceEquals(animationClip, _targetClip))
                    {
                        _onOpenClipRequested.OnNext(animationClip);
                    }

                    // Rename button
                    using (new EditorGUI.DisabledScope(!clipExists))
                    {
                        if (GUILayout.Button(_loc.ExpressionEditorView_Rename, GUILayout.Width(buttonWidth)))
                        {
                            _isRenamingClip = true;
                            EditorGUI.FocusTextInControl(ClipNameFieldName);
                        }
                    }
                }

                // Place a dummy because the input value of TextField will be entered into other fields when the rename is completed.
                _ = EditorGUILayout.TextField(string.Empty, GUILayout.Width(0));
            }
        }

        private void RenameClip()
        {
            var path = AssetDatabase.GetAssetPath(_targetClip);
            var oldName = Path.GetFileName(path).Replace(".anim", "");
            if (_newClipName != oldName)
            {
                var ret = AssetDatabase.RenameAsset(path, _newClipName);
                if (string.IsNullOrEmpty(ret))
                {
                    _isRenamingClip = false;
                }
                else
                {
                    EditorUtility.DisplayDialog(DomainConstants.SystemName,
                        _loc.ExpressionEditorView_Message_FailedToRename, "OK");
                    Debug.LogError(_loc.ExpressionEditorView_Message_FailedToRename + "\n" + ret);
                }
            }
            else
            {
                _isRenamingClip = false;
            }
        }

        private void Field_UseMouseWheel()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var current = EditorPrefsStore.ExpressionEditorSettings.UseMouseWheel;
                if (EditorGUILayout.Toggle(current, GUILayout.Width(ExpressionEditorViewConstants.ToggleWidth)) != current)
                {
                    EditorPrefsStore.ExpressionEditorSettings.UseMouseWheel = !current;
                }
                GUILayout.Label(_loc.ExpressionEditorView_UseMouseWheel);
            }
        }

        private void Field_ShowOnlyDifferFromDefaultValue()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var current = EditorPrefsStore.ExpressionEditorSettings.ShowOnlyDifferFromDefaultValue;
                if (EditorGUILayout.Toggle(current, GUILayout.Width(ExpressionEditorViewConstants.ToggleWidth)) != current)
                {
                    EditorPrefsStore.ExpressionEditorSettings.ShowOnlyDifferFromDefaultValue = !current;
                    _onOpenClipRequested.OnNext(_targetClip);
                }
                GUILayout.Label(_loc.ExpressionEditorView_ShowOnlyDifferFromDefaultValue);
            }
        }
    }
}
