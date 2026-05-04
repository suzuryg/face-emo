using System;
using System.Collections.Generic;
using System.Linq;
using Suzuryg.FaceEmo.Detail.Localization;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class ParametersView : IDisposable
    {
        public IObservable<(int id, (string name, float value) value)> OnAdded => _onAdded.AsObservable();

        private readonly CompositeDisposable _disposable = new();
        private readonly Subject<(int id, (string name, float value) value)> _onAdded = new();

        private readonly IReadOnlyDictionary<int, string> _parameters;
        private readonly IReadOnlyDictionary<int, (string name, float value)> _animatedParameters;
        private readonly ExpressionEditorStyles _styles;

        private LocalizationTable _loc;
        private bool _foldoutState;

        public ParametersView(
            IReadOnlyDictionary<int, string> parameters,
            IReadOnlyDictionary<int, (string name, float value)> animatedParameters,
            ExpressionEditorStyles styles,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _parameters = parameters;
            _animatedParameters = animatedParameters;
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
            if (!_parameters.Any()) return;

            _foldoutState = EditorGUILayout.Foldout(_foldoutState, _loc.ExpressionEditorView_AnimationParameters);
            if (!_foldoutState) return;

            foreach (var parameter in _parameters)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(ExpressionEditorViewConstants.IndentWidth);

                    if (_animatedParameters.ContainsKey(parameter.Key))
                    {
                        GUILayout.Label(parameter.Value);
                    }
                    else if (GUILayout.Button(parameter.Value, _styles.AddPropertyButtonStyle))
                    {
                        _onAdded.OnNext((parameter.Key, (parameter.Value, 0f)));
                    }
                }
            }
        }
    }
}
