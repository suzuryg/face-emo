using System;
using System.Collections.Generic;
using System.Linq;
using Suzuryg.FaceEmo.Detail.AV3;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class AnimatedTransformsView : IDisposable
    {
        public IObservable<(int id, TransformProxy value)> OnValueChanged => _onValueChanged.AsObservable();
        public IObservable<(int id, TransformProxy value)> OnRemoved => _onRemoved.AsObservable();

        private readonly Subject<(int id, TransformProxy value)> _onValueChanged = new();
        private readonly Subject<(int id, TransformProxy value)> _onRemoved = new();

        private readonly IReadOnlyDictionary<int, TransformProxy> _animatedTransforms;
        private readonly ExpressionEditorStyles _styles;

        public AnimatedTransformsView(IReadOnlyDictionary<int, TransformProxy> animatedTransforms,
            ExpressionEditorStyles styles)
        {
            _animatedTransforms = animatedTransforms;
            _styles = styles;
        }

        public void Dispose()
        {
            _onValueChanged.Dispose();
            _onRemoved.Dispose();
        }

        public void OnGUI()
        {
            if (!_animatedTransforms.Any()) return;

            // If no space is inserted here, the horizontal line of the topmost Slider will disappear.
            EditorGUILayout.Space();

            // Draw controls
            foreach (var transform in _animatedTransforms)
            {
                if (transform.Value == null) continue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(transform.Value?.GameObject?.name);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("x", _styles.RemoveButtonStyle, GUILayout.Width(20)))
                    {
                        _onRemoved.OnNext((transform.Key, transform.Value));
                        break;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var pos = EditorGUILayout.Vector3Field("Position", transform.Value.Position);
                    var rot = EditorGUILayout.Vector3Field("Rotation", transform.Value.Rotation);
                    var scale = EditorGUILayout.Vector3Field("Scale", transform.Value.Scale);
                    if (check.changed)
                    {
                        _onValueChanged.OnNext((transform.Key,
                            new TransformProxy(transform.Value.GameObject, pos, rot, scale)));
                        break;
                    }
                }

                EditorGUILayout.Space();
            }
        }
    }
}
