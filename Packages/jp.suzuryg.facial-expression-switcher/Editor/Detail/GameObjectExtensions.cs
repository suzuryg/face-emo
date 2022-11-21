using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail
{
    public static class GameObjectExtensions
    {
        public static string GetFullPath(this GameObject gameObject) => GetFullPath(gameObject.transform);

        public static string GetFullPath(this Transform transform)
        {
            List<string> path = new List<string>();
            path.Add(transform.name);
            var parent = transform.parent;
            while (parent is Transform)
            {
                path.Add(parent.name);
                parent = parent.parent;
            }
            path.Reverse();
            return "/" + string.Join("/", path);
        }
    }
}
