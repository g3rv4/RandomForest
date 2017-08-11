using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RandomForest
{
    internal abstract class DecisionTree<T>
    {
        public int Id { get; set; }
        public Predicate Predicate { get; set; }
        public DecisionTree<T> Left { get; set; }
        public DecisionTree<T> Right { get; set; }

        public bool HasPrediction { get; set; }
        public T Prediction { get; set; }

        private DecisionTree()
        {
            // protobuf
        }

        protected abstract T ParsePrediction(string value);
        protected abstract DecisionTree<T> BuildTreeOfSameType(XElement element);

        public DecisionTree(XElement element)
        {
            Id = int.Parse(element.Attribute("id").Value);
            if (element.Attribute("score") != null)
            {
                HasPrediction = true;
                Prediction = ParsePrediction(element.Attribute("score").Value);
            }

            var a1 = element.Elements();

            Predicate = new Predicate(a1.First());
            var tempNode = a1.Skip(1).FirstOrDefault();
            if (tempNode != null)
            {
                Left = BuildTreeOfSameType(tempNode);
                tempNode = a1.Skip(2).FirstOrDefault();
                if (tempNode != null)
                {
                    Right = BuildTreeOfSameType(tempNode);
                }
            }
        }

        public T Predict(Dictionary<string, double> row)
        {
            if (HasPrediction)
            {
                return Prediction;
            }
            if (Left != null && Left.Predicate.Matches(row))
            {
                return Left.Predict(row);
            }
            if (Right != null && Right.Predicate.Matches(row))
            {
                return Right.Predict(row);
            }
            throw new Exception("Invalid decision tree... leaf doesn't have a prediction");
        }
    }
    internal class ClassificationDecisionTree : DecisionTree<string>
    {
        internal ClassificationDecisionTree(XElement element) : base(element) { }

        protected override string ParsePrediction(string value)
        {
            return value;
        }

        protected override DecisionTree<string> BuildTreeOfSameType(XElement element)
        {
            return new ClassificationDecisionTree(element);
        }
    }

    internal class RegressionDecisionTree : DecisionTree<double>
    {
        internal RegressionDecisionTree(XElement element) : base(element) { }

        protected override double ParsePrediction(string value)
        {
            return double.Parse(value);
        }

        protected override DecisionTree<double> BuildTreeOfSameType(XElement element)
        {
            return new RegressionDecisionTree(element);
        }
    }
}
