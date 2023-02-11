using AnimatorAsCode.V0;
using UnityEngine;
using UnityEditor.Animations;

namespace Suzuryg.FacialExpressionSwitcher.Detail.AV3
{
    public class FesAacDefaultsProvider : IAacDefaultsProvider
    {
        private readonly bool _writeDefaults;

        public FesAacDefaultsProvider(bool writeDefaults)
        {
            _writeDefaults = writeDefaults;
        }

        public virtual void ConfigureState(AnimatorState state, AnimationClip emptyClip)
        {
            state.motion = emptyClip;
            state.writeDefaultValues = false;
        }

        public virtual void ConfigureTransition(AnimatorStateTransition transition)
        {
            transition.duration = 0;
            transition.hasExitTime = false;
            transition.exitTime = 0;
            transition.hasFixedDuration = true;
            transition.offset = 0;
            transition.interruptionSource = TransitionInterruptionSource.None;
            transition.orderedInterruption = true;
            transition.canTransitionToSelf = false;
        }

        public virtual string ConvertLayerName(string systemName)
        {
            return systemName;
        }

        public virtual string ConvertLayerNameWithSuffix(string systemName, string suffix)
        {
            return suffix;
        }

        public Vector2 Grid()
        {
            return new Vector2(250, 70);
        }

        public void ConfigureStateMachine(AnimatorStateMachine stateMachine)
        {
            var grid = Grid();
            stateMachine.anyStatePosition = grid * new Vector2(0, 7);
            stateMachine.entryPosition = grid * new Vector2(0, -1);
            stateMachine.exitPosition = grid * new Vector2(7, -1);
            stateMachine.parentStateMachinePosition = grid * new Vector2(3, -1);
        }
    }
}
