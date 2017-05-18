using InfluenceDiagrams.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluenceDiagrams.Calculations
{
    [Serializable]
    public class Probability
    {
        private string[] probability;
        private Node[] probNodes;
        private string[] condition;
        private Node[] condNodes;

        private double mValue;

        public Probability(SerialProbability serial)
        {
            probability = serial.Probabilities;
            condition = serial.Conditions;
            List<Node> pNodes = new List<Node>();
            foreach (string serialNode in serial.ProbNodes)
            {
                foreach (Node node in MDiagram.Nodes)
                {
                    if (node.ID == serialNode)
                        pNodes.Add(node);
                }
            }
            List<Node> cNodes = new List<Node>();
            foreach (string serialNode in serial.CondNodes)
            {
                foreach (Node node in MDiagram.Nodes)
                {
                    if (node.ID == serialNode)
                        cNodes.Add(node);
                }
            }
            probNodes = pNodes.ToArray();
            condNodes = cNodes.ToArray();
            mValue = serial.Value;
            Calculator.AddProbability(this);
        }

        public Probability(string[] probability, Node[] probNodes, string[] condition, Node[] condNodes, double value)
        {
            if (!ConditionNeeded(condNodes))
            {
                probability = probability.Concat(condition).ToArray();
                probNodes = probNodes.Concat(condNodes).ToArray();
                condition = new string[0];
                condNodes = new Node[0];
            }
            var joinedLeft = probability.Zip(probNodes, (a, b) => new { a, b }).OrderBy(x => x.a);
            var joinedRight = condition.Zip(condNodes, (a, b) => new { a, b }).OrderBy(x => x.a);

            this.probability = joinedLeft.Select(x => x.a).ToArray();
            this.condition = joinedRight.Select(x => x.a).ToArray(); ;
            this.probNodes = joinedLeft.Select(x => x.b).ToArray();
            this.condNodes = joinedRight.Select(x => x.b).ToArray(); ;
            this.mValue = value;
            this.mValue = value;
            Calculator.AddProbability(this);
        }

        public Probability(string[] probability, Node[] probNodes, string[] condition, Node[] condNodes)
        {
            if (!ConditionNeeded(condNodes))
            {
                probability = probability.Concat(condition).ToArray();
                probNodes = probNodes.Concat(condNodes).ToArray();
                condition = new string[0];
                condNodes = new Node[0];
            }
            var joinedLeft = probability.Zip(probNodes, (a, b) => new { a, b }).OrderBy(x => x.a);
            var joinedRight = condition.Zip(condNodes, (a, b) => new { a, b }).OrderBy(x => x.a);

            this.probability = joinedLeft.Select(x => x.a).ToArray();
            this.condition = joinedRight.Select(x => x.a).ToArray(); ;
            this.probNodes = joinedLeft.Select(x => x.b).ToArray();
            this.condNodes = joinedRight.Select(x => x.b).ToArray(); 
            this.mValue = 0;
        }

        

        public SerialProbability Serialize()
        {
            SerialProbability prob = new SerialProbability();
            prob.Probabilities = Probabilities;
            prob.Conditions = Conditions;
            List<string> probNodes = new List<string>();
            foreach (Node node in ProbNodes)
                probNodes.Add(node.ID);
            List<string> condNodes = new List<string>();
            foreach (Node node in CondNodes)
                condNodes.Add(node.ID);
            prob.ProbNodes = probNodes.ToArray();
            prob.CondNodes = condNodes.ToArray();
            prob.Value = Value;
            return prob;
        }

        private bool ConditionNeeded(Node[] condNodes)
        {
            foreach(Node cond in condNodes)
            {
                if(cond.Type == NodeType.Event)
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("P(");
            for (int i = 0; i < probability.Length; i++)
            {
                    sb.Append(probability[i]);
            }

            if (condition.Length > 0)
                sb.Append("|");

            for (int i = 0; i < condition.Length; i++)
            {
                sb.Append(condition[i]);
            }

            sb.Append(")");

            return sb.ToString();
        }

        public double Value
        {
            get { return mValue; }
            set { mValue = value;
            }
        }

        public string[] Factors
        {
            get { return probability.Concat(condition).ToArray(); }
        }

        public Node[] ProbNodes
        {
            get { return probNodes; }
        }

        public Node[] CondNodes
        {
            get { return condNodes; }
        }

        public string[] Probabilities
        {
            get { return probability; }
        }

        public string[] Conditions
        {
            get { return condition; }
        }
    }
}
