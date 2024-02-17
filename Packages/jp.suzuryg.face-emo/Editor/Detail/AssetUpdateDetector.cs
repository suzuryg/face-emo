using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;

public class AssetUpdateDetector : AssetPostprocessor
{
    public static IObservable<IEnumerable<string>> OnAnimationClipUpdated => _onAnimationClipUpdated.AsObservable();

    public static Subject<IEnumerable<string>> _onAnimationClipUpdated = new Subject<IEnumerable<string>>();

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (importedAssets is null)
        {
            return;
        }

        var clips = importedAssets
            .Where(x => x?.ToLower()?.EndsWith(".anim") == true)
            .Select(x => AssetDatabase.AssetPathToGUID(x))
            .Where(x => !string.IsNullOrEmpty(x));

        if (clips.Any())
        {
            _onAnimationClipUpdated.OnNext(clips);
        }
    }
}
