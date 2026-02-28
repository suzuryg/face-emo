from pathlib import Path

src_dir = Path('../combo-gesture-expressions-av3/Assets/Hai/ComboGesture/Scripts/Editor/Internal')
dst_dir = Path('../../Packages/jp.suzuryg.face-emo/Ext/ComboGestureIntegrator/Internal')
ignore_dirs = []
ignore_files = ['CgeAnimationNeutralizer.cs', 'CgeAvatarSnapshot.cs', 'CgeBlendTreeAutoWeightCorrector.cs', 'CgeComboGestureCompilerInternal.cs', 'CgeConflictPrevention.cs', 'CgeCurveKey.cs', 'CgeExpressionCombiner.cs', 'CgeFeatureToggles.cs', 'CgeIntermediateCombinator.cs', 'CgeLayerForBlinkingOverrideView.cs', 'CgeLayerForExpressionsView.cs', 'CgeLayerForVirtualActivity.cs', 'CgeManifestFromActivity.cs', 'CgeManifestFromMassiveBlend.cs', 'CgeManifestFromSingle.cs', 'CgeMaskApplicator.cs', 'CgeMassiveBlendManifest.cs', 'CgePermutationManifest.cs', 'CgeQualifiedAnimation.cs', 'CgeSharedLayerUtils.cs', 'CgeSingleManifest.cs']
target_extensions = ['.cs',]
gitignore_content = '''*.cs
*.meta
!*.asmdef.meta
'''
