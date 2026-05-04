using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class AnimatedParametersView : IDisposable
    {
        public IObservable<(int id, (string name, float value) value)> OnValueChanged => _onValueChanged.AsObservable();
        public IObservable<(int id, string name)> OnRemoved => _onRemoved.AsObservable();

        private readonly Subject<(int id, (string name, float value) value)> _onValueChanged = new();
        private readonly Subject<(int id, string name)> _onRemoved = new();

        private readonly IReadOnlyDictionary<int, (string name, float value)> _animatedParameters;
        private readonly ExpressionEditorStyles _styles;

        private float? _labelWidth;

        public AnimatedParametersView(IReadOnlyDictionary<int, (string name, float value)> animatedParameters,
            ExpressionEditorStyles styles)
        {
            _animatedParameters = animatedParameters;
            _styles = styles;
        }

        public void Dispose()
        {
            _onValueChanged.Dispose();
            _onRemoved.Dispose();
        }

        public void RebuildGUI()
        {
            _labelWidth = null;
        }

        public void OnGUI()
        {
            if (!_animatedParameters.Any()) return;

            _labelWidth ??= _animatedParameters
                .Select(parameter => GUI.skin.label.CalcSize(new GUIContent(parameter.Value.name)).x)
                .DefaultIfEmpty()
                .Max() + 10;

            EditorGUILayout.Space();

            foreach (var parameter in _animatedParameters)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var labelContent = new GUIContent(parameter.Value.name);
                    var value = EditorGUILayout.FloatField(labelContent, parameter.Value.value);
                    if (!Mathf.Approximately(value, parameter.Value.value))
                    {
                        _onValueChanged.OnNext((parameter.Key, (parameter.Value.name, value)));
                        break;
                    }


                    if (GUILayout.Button("x", _styles.RemoveButtonStyle, GUILayout.Width(20)))
                    {
                        _onRemoved.OnNext((parameter.Key, parameter.Value.name));
                        break;
                    }
                }
            }
        }
    }
}
