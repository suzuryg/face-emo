#if USE_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Suzuryg.FaceEmo.Domain;
using VRC.SDK3.Avatars.Components;

namespace Suzuryg.FaceEmo.Detail.AV3
{
    public class EmoteOverrideGenerator
    {
        [MenuItem("Assets/Create/FaceEmo_EmoteOverrideExample")]
        public static void Generate()
        {
#if USE_MODULAR_AVATAR
            string dirPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs.FirstOrDefault());
            if (string.IsNullOrEmpty(dirPath))
            {
                dirPath = "Assets";
            }
            else if (Path.GetExtension(dirPath) != "")
            {
                var fileName = Path.GetFileName(dirPath);
                dirPath = dirPath.Replace(fileName, "");

                if (string.IsNullOrEmpty(dirPath))
                {
                    dirPath = "Assets";
                }
            }

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
                throw new FaceEmoException($"Failed to copy asset to {controllerPath}");
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
                throw new FaceEmoException($"Failed to create asset to {prefabPath}");
            }
#else
            Debug.LogError("Please install Modular Avatar!");
#endif
        }
    }
}
