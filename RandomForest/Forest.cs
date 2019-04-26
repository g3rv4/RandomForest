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
        protected enum ForestType
        {
            Unknown,
            Regression,
            Classification
        }

        internal List<DecisionTree<T>> Trees { get; set; }
        protected ForestType Type { get; set; }

        private RandomForest()
        {
            // protobuf
        }

        internal abstract DecisionTree<T> NewDecisionTree(XElement element);
        internal abstract void ValidateRandomForestData();

        public RandomForest(XmlReader reader, string targetDataField = "_target")
        {
            Trees = new List<DecisionTree<T>>();

            ReadDataFields(reader, targetDataField);

            reader.ReadToFollowing("MiningModel");
            if(reader.Name == "MiningModel")
            {
                switch (reader.GetAttribute("functionName"))
                {
                    case "regression":
                        Type = ForestType.Regression;
                        break;
                    case "classification":
                        Type = ForestType.Classification;
                        break;
                    default:
                        Type = ForestType.Unknown;
                        break;
                }
            }

            ValidateRandomForestData();

            reader.ReadToFollowing("Segment");
            while (reader.Name == "Segment")
            {
                reader.ReadToFollowing("Node");
                Trees.Add(NewDecisionTree((XElement)XNode.ReadFrom(reader)));

                reader.ReadToFollowing("Segment");
            }
        }

        protected virtual void ReadDataFields(XmlReader reader, string targetDataField) { }

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
        public RegressionRandomForest(XmlReader reader, string targetDataField = "_target") : base(reader, targetDataField) { }

        internal override void ValidateRandomForestData()
        {
            if (Type != ForestType.Regression)
            {
                throw new Exception("Wrong class... the random forest you're trying to load does not do regression.");
            }
        }

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

        public ClassificationRandomForest(XmlReader reader, string targetDataField = "_target") : base(reader, targetDataField) { }

        internal override void ValidateRandomForestData()
        {
            if (Type != ForestType.Classification)
            {
                throw new Exception("Wrong class... the random forest you're trying to load does not do classification.");
            }
        }

        internal override DecisionTree<string> NewDecisionTree(XElement element)
        {
            return new ClassificationDecisionTree(element);
        }

        protected override void ReadDataFields(XmlReader reader, string targetDataField)
        {
            ValuePositionLookup = new Dictionary<string, int>();
            Values = new List<string>();

            reader.ReadToFollowing("DataField");
            while (reader.Name == "DataField")
            {
                if(reader.GetAttribute("name") == targetDataField)
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

        public Dictionary<string, double> GetProbabilities(Dictionary<string, string> row)
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

            return GetProbabilities(realRow);
        }

        public Dictionary<string, double> GetProbabilities(Dictionary<string, double> row)
        {
            int[] predictions = new int[Values.Count];

            Parallel.ForEach(Trees, (currentTree) =>
            {
                var p = currentTree.Predict(row);
                Interlocked.Increment(ref predictions[ValuePositionLookup[p]]);
            });

            var res = new Dictionary<string, double>();
            for (var i = 0; i < predictions.Length; i++)
            {
                res[Values[i]] = predictions[i] / (double)Trees.Count;
            }

            return res;
        }
    }
}
