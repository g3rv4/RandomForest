using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RandomForest
{
    internal abstract class DecisionTree<T>
    {
        private TreeNode Root { get; set; }

        private DecisionTree()
        {
            // protobuf
        }

        internal DecisionTree(XElement element)
        {
            Root = new TreeNode(element, GetParseFunction());
        }

        protected abstract Func<string, T> GetParseFunction();

        internal T Predict(Dictionary<string, double> row)
        {
            return Root.Predict(row);
        }

        private class TreeNode
        {
            public int Id { get; set; }
            public Predicate Predicate { get; set; }
            public TreeNode Left { get; set; }
            public TreeNode Right { get; set; }

            public bool HasPrediction { get; set; }
            public T Prediction { get; set; }

            private TreeNode()
            {
                // protobuf
            }

            public TreeNode(XElement element, Func<string, T> parseFunction)
            {
                Id = int.Parse(element.Attribute("id").Value);
                if (element.Attribute("score") != null)
                {
                    HasPrediction = true;
                    Prediction = parseFunction(element.Attribute("score").Value);
                }

                var a1 = element.Elements();

                Predicate = new Predicate(a1.First());
                var tempNode = a1.Skip(1).FirstOrDefault();
                if (tempNode != null)
                {
                    Left = new TreeNode(tempNode, parseFunction);
                    tempNode = a1.Skip(2).FirstOrDefault();
                    if (tempNode != null)
                    {
                        Right = new TreeNode(tempNode, parseFunction);
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
    }
    internal class ClassificationDecisionTree : DecisionTree<string>
    {
        internal ClassificationDecisionTree(XElement element) : base(element) { }

        protected override Func<string, string> GetParseFunction()
        {
            return (string s) => s;
        }
    }

    internal class RegressionDecisionTree : DecisionTree<double>
    {
        internal RegressionDecisionTree(XElement element) : base(element) { }

        protected override Func<string, double> GetParseFunction()
        {
            return (string s) => double.Parse(s);
        }
    }
}
