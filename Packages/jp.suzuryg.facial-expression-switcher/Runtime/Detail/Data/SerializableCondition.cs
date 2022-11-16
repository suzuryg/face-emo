using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
