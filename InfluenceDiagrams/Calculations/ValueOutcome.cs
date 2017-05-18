using InfluenceDiagrams.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluenceDiagrams.Calculations
{
    [Serializable]
    public class ValueOutcome
    {
        
        private string[] factors;
        private Node[] facNodes;

        private double mValue;

        public ValueOutcome(SerialValueOutcome serial)
        {
            factors = serial.Factors;
            mValue = serial.Value;
            List<Node> facNodes = new List<Node>();
            foreach(string facNode in serial.FacNodes)
            {
                foreach(Node node in MDiagram.Nodes)
                {
                    if (node.Name == facNode)
                        facNodes.Add(node);
                }
            }
            this.facNodes = facNodes.ToArray();
        }

        public ValueOutcome(string[] factors, Node[] facNodes, double value)
        {
            var joined = factors.Zip(facNodes, (a, b) => new { a, b }).OrderBy(x => x.a);
            this.factors = joined.Select(x => x.a).ToArray();
            this.facNodes = joined.Select(x => x.b).ToArray();
            this.mValue = value;
        }

        public SerialValueOutcome Serialize()
        {
            SerialValueOutcome serial = new SerialValueOutcome();
            serial.Factors = factors;
            serial.Value = mValue;
            List<string> facNodes = new List<string>();
            foreach (Node node in FacNodes)
                facNodes.Add(node.Name);
            serial.FacNodes = facNodes.ToArray();
            return serial;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("V(");
            for (int i = 0; i < factors.Length; i++)
            {
                sb.Append(factors[i]);
            }
            sb.Append(")");

            return sb.ToString();
        }

        public Node[] FacNodes
        {
            get { return facNodes; }
        }

        public double Value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        public string[] Factors
        {
            get { return factors; }
            set { factors = value; }
        }

    }
}
