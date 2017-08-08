using System;

namespace RandomForest
{
    internal enum ComparisonPredicate
    {
        AlwaysTrue,
        LessThanOrEqualTo,
        GreaterThan
    }

    internal static class ComparisonPredicateHelper
    {
        public static ComparisonPredicate FromString(string str)
        {
            switch (str)
            {
                case "lessOrEqual":
                    return ComparisonPredicate.LessThanOrEqualTo;
                case "greaterThan":
                    return ComparisonPredicate.GreaterThan;
                default:
                    throw new ArgumentException("Invalid comparison predicate");
            }
        }
    }
}
