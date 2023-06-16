using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FaceEmo.Detail
{
    public class PathConverterTests
    {
        private readonly string TestDir = "Assets/PathConverterTests";

        [Test]
        public void Test()
        {
            var unityPath = $"{TestDir}/testAsset.asset";
            AssetDatabaseUtility.CreateFolderRecursively(AssetDatabaseUtility.GetDirectoryName(unityPath));
            var asset = ScriptableObject.CreateInstance<TestAsset>();
            AssetDatabase.CreateAsset(asset, unityPath);

            asset = AssetDatabase.LoadAssetAtPath<TestAsset>(unityPath);
            Assert.That(asset, Is.Not.Null);

            var systemPath = PathConverter.ToSystemPath(unityPath);
            Assert.That(File.Exists(systemPath), Is.True);

            unityPath = PathConverter.ToUnityPath(systemPath);
            asset = AssetDatabase.LoadAssetAtPath<TestAsset>(unityPath);
            Assert.That(asset, Is.Not.Null);
        }
    }
}
