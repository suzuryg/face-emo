using System;
using System.Collections.Generic;
using System.Linq;
using Suzuryg.FaceEmo.Detail.Localization;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class TogglesView : IDisposable
    {
        public IObservable<(int key, GameObject target, bool value)> OnAdded => _onAdded.AsObservable();

        private readonly CompositeDisposable _disposable = new();
        private readonly Subject<(int key, GameObject target, bool value)> _onAdded = new();

        private readonly IReadOnlyDictionary<int, (GameObject target, bool value)> _toggles;
        private readonly IReadOnlyDictionary<int, (GameObject target, bool value)> _animatedToggles;
        private readonly ExpressionEditorStyles _styles;

        private LocalizationTable _loc;
        private bool _foldoutState;

        public TogglesView(
            IReadOnlyDictionary<int, (GameObject target, bool value)> toggles,
            IReadOnlyDictionary<int, (GameObject target, bool value)> animatedToggles,
            ExpressionEditorStyles styles,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _toggles = toggles;
            _animatedToggles = animatedToggles;
            _styles = styles;
            _loc = localizationSetting.Table;
            localizationSetting.OnTableChanged.Subscribe(table => _loc = table).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _onAdded.Dispose();
        }

        public void OnGUI()
        {
            if (!_toggles.Any()) return;

            _foldoutState = EditorGUILayout.Foldout(_foldoutState, _loc.ExpressionEditorView_AddtionalToggleObjects);
            if (!_foldoutState) return;

            foreach (var toggle in _toggles)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(ExpressionEditorViewConstants.IndentWidth);

                    if (_animatedToggles.ContainsKey(toggle.Key))
                    {
                        GUILayout.Label(toggle.Value.target?.name);
                    }
                    else
                    {
                        if (GUILayout.Button(toggle.Value.target?.name, _styles.AddPropertyButtonStyle))
                        {
                            _onAdded.OnNext((toggle.Key, toggle.Value.target, toggle.Value.value));
                        }
                    }
                }
            }
        }
    }
}
