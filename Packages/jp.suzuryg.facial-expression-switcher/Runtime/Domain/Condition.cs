using System;

namespace Suzuryg.FacialExpressionSwitcher.Domain
{
    public class Condition : IEquatable<Condition>
    {
        public Hand Hand { get; }
        public HandGesture HandGesture { get; }
        public ComparisonOperator ComparisonOperator { get; }

        public Condition(Hand hand, HandGesture handGesture, ComparisonOperator comparisonOperator)
        {
            Hand = hand;
            HandGesture = handGesture;
            ComparisonOperator = comparisonOperator;
        }

        bool IEquatable<Condition>.Equals(Condition other)
        {
            if (other is null)
            {
                return false;
            }

            return Hand == other.Hand && HandGesture == other.HandGesture && ComparisonOperator == other.ComparisonOperator;
        }

        public override bool Equals(Object obj)
        {
            if (obj is Condition other)
            {
                return Hand == other.Hand && HandGesture == other.HandGesture && ComparisonOperator == other.ComparisonOperator;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return new { Hand, HandGesture, ComparisonOperator }.GetHashCode();
        }

        public static bool operator == (Condition condition1, Condition condition2)
        {
            if (condition1 is null || condition2 is null)
            {
                return ReferenceEquals(condition1, condition2);
            }

            return condition1.Equals(condition2);
        }

        public static bool operator != (Condition condition1, Condition condition2)
        {
            if (condition1 is null || condition2 is null)
            {
                return !ReferenceEquals(condition1, condition2);
            }

            return !condition1.Equals(condition2);
        }
    }
}
