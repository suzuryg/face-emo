using System;
using System.Collections.Generic;
using System.Linq;
using Suzuryg.FaceEmo.Detail.Localization;
using Suzuryg.FaceEmo.Domain;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor.Views
{
    internal sealed class HintView : IDisposable
    {
        private readonly CompositeDisposable _disposable = new();

        private readonly IReadOnlyDictionary<BlendShape, float> _blinkBlendShapes;
        private readonly IReadOnlyDictionary<BlendShape, float> _lipSyncBlendShapes;
        private readonly IReadOnlyDictionary<BlendShape, float> _animatedBlendShapes;

        private LocalizationTable _loc;

        public HintView(IReadOnlyDictionary<BlendShape, float> blinkBlendShapes,
            IReadOnlyDictionary<BlendShape, float> lipSyncBlendShapes,
            IReadOnlyDictionary<BlendShape, float> animatedBlendShapes,
            IReadOnlyLocalizationSetting localizationSetting)
        {
            _blinkBlendShapes = blinkBlendShapes;
            _lipSyncBlendShapes = lipSyncBlendShapes;
            _animatedBlendShapes = animatedBlendShapes;
            _loc = localizationSetting.Table;
            localizationSetting.OnTableChanged.Subscribe(table => _loc = table).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        public void OnGUI()
        {
            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints)
                ? EditorPrefs.GetBool(DetailConstants.KeyShowHints)
                : DetailConstants.DefaultShowHints;
            if (!showHints) { return; }

            GUILayout.FlexibleSpace();

            if (_animatedBlendShapes.Any(blendShape => _blinkBlendShapes.ContainsKey(blendShape.Key)))
            {
                HelpBoxDrawer.WarnLayout(_loc.Hints_BlinkBlendShapeIncluded);
            }
            if (_animatedBlendShapes.Any(blendShape => _lipSyncBlendShapes.ContainsKey(blendShape.Key)))
            {
                HelpBoxDrawer.WarnLayout(_loc.Hints_LipSyncBlendShapeIncluded);
            }

            HelpBoxDrawer.InfoLayout(_loc.Hints_SeparatedFaceMeshes);
            HelpBoxDrawer.InfoLayout(_loc.Hints_ExpressionEditorLayout);
            HelpBoxDrawer.InfoLayout(_loc.Hints_ExpressionPreview);
        }
    }
}
