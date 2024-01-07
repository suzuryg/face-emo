using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Suzuryg.FaceEmo.Domain
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

        private static readonly Regex _tokenizer = new Regex(@"(?<=[a-z])(?=[A-Z])|[_\-;:,./\\]|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled);
        private List<string> _cachedTokens;
        private string _lastSearch;
        private double _lastScore;

        public double MatchName(string otherName)
        {
            if (ReferenceEquals(otherName, _lastSearch)) return _lastScore;
            _lastSearch = otherName;
            if (_name.Equals(otherName)) return _lastScore = 100;
            if (_name.Equals(otherName, StringComparison.InvariantCultureIgnoreCase)) return _lastScore = 99.9;
            if (_name.StartsWith(otherName, StringComparison.InvariantCultureIgnoreCase)) return _lastScore = 95;
            otherName = otherName.ToLowerInvariant();
            if (_name.ToLowerInvariant().Contains(otherName)) return _lastScore = otherName.Length * 8;

            if (_cachedTokens == null || _cachedTokens.Count == 0)
            {
                _cachedTokens = new List<string>();
                foreach (var s in _tokenizer.Split(_name))
                {
                    string clean = s.Trim().ToLowerInvariant();
                    if (clean.Length == 0) continue;
                    _cachedTokens.Add(clean);
                }
            }


            int nameIndex = 0;
            double score = 0;
            for (var tokenIndex = 0; tokenIndex < _cachedTokens.Count; tokenIndex++)
            {                
                if (" _-,.;:/\\".IndexOf(otherName[nameIndex]) >= 0)
                {
                    nameIndex += 1;
                    if (nameIndex >= otherName.Length) return _lastScore = score;
                }
                var cachedToken = _cachedTokens[tokenIndex];
                int tokenCharIndex = 0;
                while (cachedToken[tokenCharIndex] == otherName[nameIndex])
                {
                    score += 100.0 / (3 * (nameIndex + 1) + 2 * (tokenIndex + 1) + (tokenCharIndex + 1));
                    tokenCharIndex += 1;
                    nameIndex += 1;
                    if (nameIndex >= otherName.Length) return _lastScore = score;
                    if (tokenCharIndex >= cachedToken.Length) break;
                }
            }

            return _lastScore = score;
        }

        public override string ToString()
        {
            return _path + "." + _name;
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