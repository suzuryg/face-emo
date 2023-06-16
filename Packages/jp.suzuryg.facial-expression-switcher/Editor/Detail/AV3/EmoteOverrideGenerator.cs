using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Suzuryg.FacialExpressionSwitcher.Domain;
using VRC.SDK3.Avatars.Components;

#if USE_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class EmoteOverrideGenerator
    {
        [MenuItem("Assets/Create/FES_EmoteOverrideExample")]
        public static void Generate()
        {
#if USE_MODULAR_AVATAR
            var dirPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var prefabPath = dirPath + "/" + Path.GetFileName(AV3Constants.Path_EmoteOverridePrefab);
            var controllerPath = dirPath + "/" + Path.GetFileName(AV3Constants.Path_EmoteOverrideController);

            // Check existence
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"\"{prefabPath}\" already exists!", "OK");
                return;
            }
            else if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                EditorUtility.DisplayDialog(DomainConstants.SystemName, $"\"{controllerPath}\" already exists!", "OK");
                return;
            }

            // Copy controller
            if (!AssetDatabase.CopyAsset(AV3Constants.Path_EmoteOverrideController, controllerPath))
            {
                throw new FacialExpressionSwitcherException($"Failed to copy asset to {controllerPath}");
            }

            // Create prefab
            var prefab = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AV3Constants.Path_EmoteOverridePrefab));
            var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            var modularAvatarMergeAnimator = prefab.AddComponent<ModularAvatarMergeAnimator>();

            modularAvatarMergeAnimator.animator = animatorController;
            modularAvatarMergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            modularAvatarMergeAnimator.deleteAttachedAnimator = true;
            modularAvatarMergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            modularAvatarMergeAnimator.matchAvatarWriteDefaults = false;

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath, out var isSucceeded);
            GameObject.DestroyImmediate(prefab);
            if (!isSucceeded)
            {
                throw new FacialExpressionSwitcherException($"Failed to create asset to {prefabPath}");
            }
#else
            Debug.LogError("Please install Modular Avatar!");
#endif
        }
    }
}
