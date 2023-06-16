using Hai.ComboGesture.Scripts.Editor.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Suzuryg.FaceEmo.External.Hai.ComboGestureIntegrator
{
    public class ComboGestureIntegratorProxy
    {
        public static void DoGenerate(AnimatorController animatorController, RuntimeAnimatorController assetContainer, bool writeDefaults)
        {
            new ComboGestureCompilerInternal(animatorController, assetContainer, writeDefaults).IntegrateWeightCorrection();
        }
    }

    internal class ComboGestureCompilerInternal
    {
        private readonly AnimatorController _animatorController;
        private readonly CgeConflictPrevention _conflictPrevention;
        private readonly CgeAssetContainer _assetContainer;
        private readonly bool _universalAnalogSupport;
        private readonly AvatarMask _nothingMask;

        public ComboGestureCompilerInternal(AnimatorController animatorController, RuntimeAnimatorController assetContainer, bool writeDefaults)
        {
            _animatorController = animatorController;
            _conflictPrevention = CgeConflictPrevention.OfIntegrator(writeDefaults);

            _assetContainer = CgeAssetContainer.FromExisting(assetContainer);
            _universalAnalogSupport = false;
        }

        public void IntegrateWeightCorrection()
        {
            CreateOrReplaceWeightCorrection(
                _nothingMask,
                _assetContainer,
                _animatorController,
                _conflictPrevention,
                _universalAnalogSupport
            );
            CreateOrReplaceSmoothing(_nothingMask, _assetContainer, _animatorController, _conflictPrevention);

            ReapAnimator(_animatorController);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void CreateOrReplaceWeightCorrection(AvatarMask weightCorrectionAvatarMask, CgeAssetContainer assetContainer, AnimatorController animatorController, CgeConflictPrevention conflictPrevention, bool universalAnalogSupport)
        {
            new CgeLayerForWeightCorrection(assetContainer, animatorController, weightCorrectionAvatarMask, conflictPrevention.ShouldWriteDefaults, universalAnalogSupport).Create();
        }

        private static void CreateOrReplaceSmoothing(AvatarMask weightCorrectionAvatarMask, CgeAssetContainer assetContainer, AnimatorController animatorController, CgeConflictPrevention conflictPrevention)
        {
            new CgeLayerForAnalogFistSmoothing(assetContainer, weightCorrectionAvatarMask, conflictPrevention.ShouldWriteDefaults, animatorController).Create();
        }

        private static void ReapAnimator(AnimatorController animatorController)
        {
            if (AssetDatabase.GetAssetPath(animatorController) == "")
            {
                return;
            }

            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(animatorController));

            var reachableMotions = CgeSharedLayerUtils.FindAllReachableClipsAndBlendTrees(animatorController)
                .ToList<Object>();
            Reap(allSubAssets, typeof(BlendTree), reachableMotions, o => o.name.StartsWith("autoBT_"));
        }

        private static void Reap(Object[] allAssets, Type type, List<Object> existingAssets, Predicate<Object> predicate)
        {
            foreach (var o in allAssets)
            {
                if (o != null && (o.GetType() == type || o.GetType().IsSubclassOf(type)) && !existingAssets.Contains(o) && predicate.Invoke(o))
                {
                    AssetDatabase.RemoveObjectFromAsset(o);
                }
            }
        }

        private class CgeConflictPrevention
        {
            public bool ShouldGenerateExhaustiveAnimations { get; }
            public bool ShouldWriteDefaults { get; }

            private CgeConflictPrevention(bool shouldGenerateExhaustiveAnimations, bool shouldWriteDefaults)
            {
                ShouldGenerateExhaustiveAnimations = shouldGenerateExhaustiveAnimations;
                ShouldWriteDefaults = shouldWriteDefaults;
            }

            public static CgeConflictPrevention OfIntegrator(bool writeDefaults)
            {
                return new CgeConflictPrevention(false, writeDefaults);
            }
        }
    }
}
