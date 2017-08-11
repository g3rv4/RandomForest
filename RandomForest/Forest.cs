using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace RandomForest
{
    public abstract class RandomForest<T>
    {
        internal List<DecisionTree<T>> Trees { get; set; }

        private RandomForest()
        {
            // protobuf
        }

        internal abstract DecisionTree<T> NewDecisionTree(XElement element);

        public RandomForest(XmlReader reader)
        {
            Trees = new List<DecisionTree<T>>();

            ReadDataFields(reader);

            reader.ReadToFollowing("Segment");

            while (reader.Name == "Segment")
            {
                reader.ReadToFollowing("Node");
                Trees.Add(NewDecisionTree((XElement)XNode.ReadFrom(reader)));

                reader.ReadToFollowing("Segment");
            }
        }

        protected virtual void ReadDataFields(XmlReader reader) { }

        public abstract T Predict(Dictionary<string, double> row);

        public T Predict(Dictionary<string, string> row)
        {
            var realRow = new Dictionary<string, double>();

            foreach (var item in row)
            {
                if (double.TryParse(item.Value, out var dbl))
                {
                    realRow[item.Key] = dbl;
                }
                else
                {
                    realRow[item.Key + item.Value] = 1;
                }
            }

            return Predict(realRow);
        }
    }

    public class RegressionRandomForest : RandomForest<double>
    {
        public RegressionRandomForest(XmlReader reader) : base(reader) { }

        internal override DecisionTree<double> NewDecisionTree(XElement element)
        {
            return new RegressionDecisionTree(element);
        }

        public override double Predict(Dictionary<string, double> row)
        {
            double prediction = 0;
            var predictionLock = new object();

            Parallel.ForEach(Trees, (currentTree) =>
            {
                var p = currentTree.Predict(row);
                lock (predictionLock)
                {
                    prediction += p;
                }
            });

            return prediction / Trees.Count;
        }
    }

    public class ClassificationRandomForest : RandomForest<string>
    {
        private Dictionary<string, int> ValuePositionLookup { get; set; }
        private List<string> Values { get; set; }

        public ClassificationRandomForest(XmlReader reader) : base(reader) { }

        internal override DecisionTree<string> NewDecisionTree(XElement element)
        {
            return new ClassificationDecisionTree(element);
        }

        protected override void ReadDataFields(XmlReader reader)
        {
            ValuePositionLookup = new Dictionary<string, int>();
            Values = new List<string>();

            reader.ReadToFollowing("DataField");
            while (reader.Name == "DataField")
            {
                if(reader.GetAttribute("name") == "_target")
                {
                    reader.ReadToDescendant("Value");
                    while (reader.Name == "Value")
                    {
                        var value = reader.GetAttribute("value");
                        ValuePositionLookup[value] = Values.Count;
                        Values.Add(value);

                        reader.ReadToNextSibling("Value");
                    }
                    break;
                }
                reader.ReadToFollowing("DataField");
            }
        }

        public override string Predict(Dictionary<string, double> row)
        {
            int[] predictions = new int[Values.Count];

            Parallel.ForEach(Trees, (currentTree) =>
            {
                var p = currentTree.Predict(row);
                Interlocked.Increment(ref predictions[ValuePositionLookup[p]]);
            });

            var maxPosition = Array.IndexOf(predictions, predictions.Max());
            return Values[maxPosition];
        }
    }
}
