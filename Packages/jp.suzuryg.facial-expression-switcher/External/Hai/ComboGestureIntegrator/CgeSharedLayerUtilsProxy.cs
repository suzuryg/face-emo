using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    class CgeSharedLayerUtils
    {
        internal const string HaiGestureComboLeftWeightProxy = "_Hai_GestureLWProxy";
        internal const string HaiGestureComboRightWeightProxy = "_Hai_GestureRWProxy";
        internal const string HaiVirtualActivity = "_Hai_GestureVirtualActivity";
        internal const string HaiGestureComboLeftWeightSmoothing = "_Hai_GestureLWSmoothing";
        internal const string HaiGestureComboRightWeightSmoothing = "_Hai_GestureRWSmoothing";
        internal const string HaiGestureComboSmoothingFactor = "_Hai_GestureSmoothingFactor";

        public static IEnumerable<Motion> FindAllReachableClipsAndBlendTrees(AnimatorController animatorController)
        {
            return ConcatStateMachines(animatorController)
                .SelectMany(machine => machine.states)
                .Select(state => state.state.motion)
                .Where(motion => motion != null)
                .SelectMany(Unwrap)
                .Distinct();
        }

        private static IEnumerable<AnimatorStateMachine> ConcatStateMachines(AnimatorController animatorController)
        {
            return animatorController.layers.Select(layer => layer.stateMachine)
                .Concat(animatorController.layers.SelectMany(layer => layer.stateMachine.stateMachines).Select(machine => machine.stateMachine));
        }

        private static IEnumerable<Motion> Unwrap(Motion motion)
        {
            var itself = new[] {motion};
            return motion is BlendTree bt ? itself.Concat(AllChildrenOf(bt)) : itself;
        }

        private static IEnumerable<Motion> AllChildrenOf(BlendTree blendTree)
        {
            return blendTree.children
                .Select(motion => motion.motion)
                .Where(motion => motion != null)
                .SelectMany(Unwrap)
                .ToList();
        }
    }
}
