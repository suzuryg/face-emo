using Suzuryg.FacialExpressionSwitcher.Domain;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public class SerializableCondition : ScriptableObject
    {
        public Hand Hand;
        public HandGesture HandGesture;
        public ComparisonOperator ComparisonOperator;

        public void Save(Condition condition)
        {
            Hand = condition.Hand;
            HandGesture = condition.HandGesture;
            ComparisonOperator = condition.ComparisonOperator;
        }

        public Condition Load()
        {
            return new Condition(Hand, HandGesture, ComparisonOperator);
        }
    }
}
