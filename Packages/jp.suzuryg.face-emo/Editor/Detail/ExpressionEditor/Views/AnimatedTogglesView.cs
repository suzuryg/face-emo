using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class AnimatedTogglesView : IDisposable
    {
        public IObservable<(int id, GameObject target, bool value)> OnValueChanged => _onValueChanged.AsObservable();
        public IObservable<(int id, GameObject target)> OnRemoved => _onRemoved.AsObservable();

        private readonly Subject<(int id, GameObject target, bool value)> _onValueChanged = new();
        private readonly Subject<(int id, GameObject target)> _onRemoved = new();

        private readonly IReadOnlyDictionary<int, (GameObject target, bool value)> _animatedToggles;
        private readonly ExpressionEditorStyles _styles;

        private float? _labelWidth;

        public AnimatedTogglesView(IReadOnlyDictionary<int, (GameObject target, bool value)> animatedToggles,
            ExpressionEditorStyles styles)
        {
            _animatedToggles = animatedToggles;
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
            if (!_animatedToggles.Any()) return;

            _labelWidth ??= _animatedToggles
                .Select(toggle => GUI.skin.label.CalcSize(new GUIContent(toggle.Value.target?.name)).x)
                .DefaultIfEmpty()
                .Max() + 10;

            EditorGUILayout.Space();

            // Draw controls
            foreach (var toggle in _animatedToggles)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Label
                    var labelContent = new GUIContent(toggle.Value.target?.name);
                    var labelRect =
                        GUILayoutUtility.GetRect(labelContent, GUI.skin.label, GUILayout.Width(_labelWidth.Value));
                    GUI.Label(labelRect, labelContent);

                    // Toggle
                    var isActive = GUILayout.Toggle(toggle.Value.value, string.Empty);
                    if (isActive != toggle.Value.value)
                    {
                        _onValueChanged.OnNext((toggle.Key, toggle.Value.target, isActive));
                        break;
                    }

                    GUILayout.FlexibleSpace();

                    // Remove button
                    if (GUILayout.Button("x", _styles.RemoveButtonStyle, GUILayout.Width(20)))
                    {
                        _onRemoved.OnNext((toggle.Key, toggle.Value.target));
                        break;
                    }
                }
            }
        }
    }
}
