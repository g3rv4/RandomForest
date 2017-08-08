using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RandomForest
{
    internal class Predicate
    {
        public ComparisonPredicate Comparison { get; set; }
        public double? Value { get; set; }
        public string FieldName { get; set; }

        private Predicate()
        {
            // protobuf
        }

        public Predicate(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "True":
                    Comparison = ComparisonPredicate.AlwaysTrue;
                    Value = null;
                    FieldName = null;
                    break;
                case "SimplePredicate":
                    Comparison = ComparisonPredicateHelper.FromString(element.Attribute("operator").Value);
                    FieldName = element.Attribute("field").Value;
                    if (double.TryParse(element.Attribute("value").Value, out var val))
                    {
                        Value = val;
                    }
                    else
                    {
                        throw new Exception("Invalid value!");
                    }
                    break;
                default:
                    throw new ArgumentException("Could not create a predicate from the XElement");
            }
        }

        public bool Matches(Dictionary<string, double> row)
        {
            if (Comparison == ComparisonPredicate.AlwaysTrue)
            {
                return true;
            }

            double value = 0;
            if (row.TryGetValue(FieldName, out var val))
            {
                value = val;
            }
            if (Comparison == ComparisonPredicate.GreaterThan)
            {
                return value > Value.Value;
            }
            if (Comparison == ComparisonPredicate.LessThanOrEqualTo)
            {
                return value <= Value.Value;
            }

            throw new Exception("Invalid comparison predicate?");
        }
    }
}
