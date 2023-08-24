using System;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail
{
    [Serializable]
    public class BlendShape : IEquatable<BlendShape>
    {
        /// <summary>
        /// Transform path from animator
        /// </summary>
        public string Path => _path;

        /// <summary>
        /// Name of blend shape
        /// </summary>
        public string Name => _name;

        [SerializeField] private string _path;
        [SerializeField] private string _name;

        public BlendShape(string path, string name)
        {
            _path = path;
            _name = name;
        }

        public bool Equals(BlendShape other)
        {
            if (other is null)
            {
                return false;
            }
            return _path == other._path && _name == other._name;
        }

        public override bool Equals(object obj) => Equals(obj as BlendShape);

        public override int GetHashCode() => (_path, _name).GetHashCode();

        public static bool operator ==(BlendShape left, BlendShape right)
        {
            if (left is null)
            {
                return right is null;
            }
            else
            {
                return left.Equals(right);
            }
        }

        public static bool operator !=(BlendShape left, BlendShape right)
        {
            return !(left == right);
        }
    }
}

