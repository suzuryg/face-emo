using System;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.ExpressionEditor
{
    public interface IExpressionEditor : IDisposable
    {
        void Open(AnimationClip clip);
        void OpenIfOpenedAlready(AnimationClip clip);
    }
}
