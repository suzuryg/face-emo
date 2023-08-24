using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail
{
    public class BlendShapeTests
    {
        [Test]
        public void Test()
        {
            var dic = new Dictionary<BlendShape, int>()
            {
                { new BlendShape(path: "a", name: "d"), 0 },
                { new BlendShape(path: "b", name: "e"), 1 },
                { new BlendShape(path: "c", name: "f"), 2 },
            };
            Assert.That(dic.Keys.Count, Is.EqualTo(3));
            Assert.That(dic.Values.Count, Is.EqualTo(3));
            Assert.That(dic[new BlendShape(path: "a", name: "d")],  Is.EqualTo(0));
            Assert.That(dic[new BlendShape(path: "b", name: "e")],  Is.EqualTo(1));
            Assert.That(dic[new BlendShape(path: "c", name: "f")],  Is.EqualTo(2));

            Assert.That(new BlendShape(path: "a", name: "d").Equals(new BlendShape(path: "a", name: "d")), Is.True);
            Assert.That(new BlendShape(path: "a", name: "d").Equals(new BlendShape(path: "a", name: "e")), Is.False);
            Assert.That(new BlendShape(path: "a", name: "d").Equals(new BlendShape(path: "b", name: "d")), Is.False);

            Assert.That(new BlendShape(path: "a", name: "d") == new BlendShape(path: "a", name: "d"), Is.True);
            Assert.That(new BlendShape(path: "a", name: "d") == new BlendShape(path: "a", name: "e"), Is.False);
            Assert.That(new BlendShape(path: "a", name: "d") == new BlendShape(path: "b", name: "d"), Is.False);

            Assert.That(new BlendShape(path: "a", name: "d") != new BlendShape(path: "a", name: "d"), Is.False);
            Assert.That(new BlendShape(path: "a", name: "d") != new BlendShape(path: "a", name: "e"), Is.True);
            Assert.That(new BlendShape(path: "a", name: "d") != new BlendShape(path: "b", name: "d"), Is.True);

            BlendShape blendShape = null;
            Assert.That(blendShape == null, Is.True);
            Assert.That(blendShape != null, Is.False);
            Assert.That(blendShape == new BlendShape("a", "b"), Is.False);
            Assert.That(blendShape != new BlendShape("a", "b"), Is.True);

            blendShape = new BlendShape("a", "b");
            Assert.That(blendShape == null, Is.False);
            Assert.That(blendShape != null, Is.True);
            Assert.That(blendShape == new BlendShape("a", "b"), Is.True);
            Assert.That(blendShape != new BlendShape("a", "b"), Is.False);

            var list = ScriptableObject.CreateInstance<BlendShapeList>();
            list.BlendShapes = dic.Keys.ToList();
            var serialized = JsonUtility.ToJson(list);
            Debug.Log(serialized);

            var deserialized = ScriptableObject.CreateInstance<BlendShapeList>();
            JsonUtility.FromJsonOverwrite(serialized, deserialized);
            Assert.That(deserialized.BlendShapes[0], Is.EqualTo(new BlendShape(path: "a", name: "d")));
            Assert.That(deserialized.BlendShapes[1], Is.EqualTo(new BlendShape(path: "b", name: "e")));
            Assert.That(deserialized.BlendShapes[2], Is.EqualTo(new BlendShape(path: "c", name: "f")));
        }

        private class BlendShapeList : ScriptableObject
        {
            public List<BlendShape> BlendShapes;
        }
    }
}
