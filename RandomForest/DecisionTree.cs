using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RandomForest
{
    internal class DecisionTree
    {
        private TreeNode Root { get; set; }

        private DecisionTree()
        {
            // protobuf
        }

        internal DecisionTree(XElement element)
        {
            Root = new TreeNode(element);
        }

        internal double Predict(Dictionary<string, double> row)
        {
            return Root.Predict(row);
        }

        private class TreeNode
        {
            public int Id { get; set; }
            public Predicate Predicate { get; set; }
            public TreeNode Left { get; set; }
            public TreeNode Right { get; set; }

            public double? Prediction { get; set; }

            private TreeNode()
            {
                // protobuf
            }

            public TreeNode(XElement element)
            {
                Id = int.Parse(element.Attribute("id").Value);
                if (element.Attribute("score") != null)
                {
                    Prediction = double.Parse(element.Attribute("score").Value);
                }

                var a1 = element.Elements();

                Predicate = new Predicate(a1.First());
                var tempNode = a1.Skip(1).FirstOrDefault();
                if (tempNode != null)
                {
                    Left = new TreeNode(tempNode);
                    tempNode = a1.Skip(2).FirstOrDefault();
                    if (tempNode != null)
                    {
                        Right = new TreeNode(tempNode);
                    }
                }
            }

            public double Predict(Dictionary<string, double> row)
            {
                if (Prediction.HasValue)
                {
                    return Prediction.Value;
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
}
