using System;
using System.Collections.Generic;
using System.Linq;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Localization;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class TransformsView : IDisposable
    {
        public IObservable<(int key, TransformProxy value)> OnAdded => _onAdded.AsObservable();

        private readonly CompositeDisposable _disposable = new();
        private readonly Subject<(int key, TransformProxy value)> _onAdded = new();

        private readonly IReadOnlyDictionary<int, TransformProxy> _transforms;
        private readonly IReadOnlyDictionary<int, TransformProxy> _animatedTransforms;
        private readonly ExpressionEditorStyles _styles;

        private LocalizationTable _loc;
        private bool _foldoutState;

        public TransformsView(
            IReadOnlyDictionary<int, TransformProxy> transforms,
            IReadOnlyDictionary<int, TransformProxy> animatedTransforms,
            ExpressionEditorStyles styles,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _transforms = transforms;
            _animatedTransforms = animatedTransforms;
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
            if (!_transforms.Any()) return;

            _foldoutState = EditorGUILayout.Foldout(_foldoutState, _loc.ExpressionEditorView_AddtionalTransformObjects);
            if (!_foldoutState) return;

            foreach (var transform in _transforms)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(ExpressionEditorViewConstants.IndentWidth);

                    if (_animatedTransforms.ContainsKey(transform.Key))
                    {
                        GUILayout.Label(transform.Value.GameObject?.name);
                    }
                    else
                    {
                        if (!GUILayout.Button(transform.Value.GameObject?.name, _styles.AddPropertyButtonStyle))
                            continue;
                        _onAdded.OnNext((transform.Key, transform.Value));
                    }
                }
            }
        }
    }
}
