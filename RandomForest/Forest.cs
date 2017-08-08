using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RandomForest
{
    public class Forest
    {
        private List<DecisionTree> Trees { get; set; }

        private Forest()
        {
            // protobuf
        }

        public Forest(XmlReader reader)
        {
            Trees = new List<DecisionTree>();

            reader.ReadToFollowing("Segment");

            while (reader.Name == "Segment")
            {
                reader.ReadToFollowing("Node");
                Trees.Add(new DecisionTree((XElement)XNode.ReadFrom(reader)));

                reader.ReadToFollowing("Segment");
            }
        }

        public double Predict(Dictionary<string, double> row)
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
}
