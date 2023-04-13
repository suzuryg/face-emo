using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UniRx;
using System.Linq;

public class AssetUpdateDetector : AssetPostprocessor
{
    public static IObservable<List<string>> OnAnimationClipUpdated => _onAnimationClipUpdated.AsObservable();

    public static Subject<List<string>> _onAnimationClipUpdated = new Subject<List<string>>();

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            var list = new List<string>();
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath) is AnimationClip)
            {
                list.Add(AssetDatabase.AssetPathToGUID(assetPath));
            }

            if (list.Any())
            {
                _onAnimationClipUpdated.OnNext(list);
            }
        }
    }
}
