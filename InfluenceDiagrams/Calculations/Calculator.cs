using InfluenceDiagrams.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluenceDiagrams.Calculations
{
    public static class Calculator
    {
        static Dictionary<string, Probability> probabilities = new Dictionary<string, Probability>();
        static Node[] nodes = new Node[0];

        public static void AddProbability(Probability prob)
        {
            if (!probabilities.ContainsKey(prob.ToString()))
                probabilities.Add(prob.ToString(), prob);
            else
                probabilities[prob.ToString()] = prob;
        }

        public static void Clear()
        {
            probabilities.Clear();
        }

        public static void AddNode(Node node)
        {
            nodes = nodes.Concat(new Node[] { node }).ToArray();
        }


        public static double CalculateProbability(Probability prob)
        {
            if (probabilities.ContainsKey(prob.ToString()))
                return probabilities[prob.ToString()].Value;
            prob = JoinDoubledConditions(prob);
            if (probabilities.ContainsKey(prob.ToString()))
                return probabilities[prob.ToString()].Value;
            //prob = SeparateProbability(prob);
            //if (probabilities.ContainsKey(prob.ToString()))
            //    return probabilities[prob.ToString()].Value;
            else if (IsRule3(prob))
                return UseRule3(prob);
            else if (IsRule1(prob))
                return SaveProbability(prob, 1);
            else if (CheckNegation(prob))
                return SaveProbability(prob, 0);
            else if (IsRule5(prob))
                return UseRule5(prob);
            else if (IsRule6a(prob))
                return UseRule6a(prob);
            else if (IsRule6b(prob))
                return UseRule6b(prob);

            return 999;
        }

        public static Probability JoinDoubledConditions(Probability prob)
        {
            string[] conds = prob.Conditions;
            Node[] condNodes = prob.CondNodes;
            Dictionary<string, Node> nonDuplicate = new Dictionary<string, Node>();
            for (int i = 0; i < conds.Length; i++)
            {
                if (!nonDuplicate.ContainsKey(conds[i]))
                {
                    nonDuplicate.Add(conds[i], condNodes[i]);
                }
            }
            string[] newConds = new string[0];
            Node[] newNodes = new Node[0];
            foreach (KeyValuePair<string, Node> pair in nonDuplicate)
            {
                newConds = newConds.Concat(new string[] { pair.Key }).ToArray();
                newNodes = newNodes.Concat(new Node[] { pair.Value }).ToArray();
            }
            if (conds.SequenceEqual(newConds))
                return prob;
            else
            {
                Probability newProb = new Probability(prob.Probabilities, prob.ProbNodes, newConds, newNodes);
                return newProb;
            }
        }

        public static Probability SeparateProbability(Probability prob)
        {
            string[] conds = prob.Conditions;
            Node[] condNodes = prob.CondNodes;
            string[] newConds = new string[0];
            Node[] newNodes = new Node[0];
            for (int i = 0; i < condNodes.Length; i++)
            {
                if (prob.ProbNodes[0].Parents.Contains(condNodes[i]))
                {
                    newNodes = newNodes.Concat(new Node[] { condNodes[i] }).ToArray();
                    newConds = newConds.Concat(new string[] { conds[i] }).ToArray();
                }
            }
            if (conds.SequenceEqual(newConds))
                return prob;
            else
            {
                Probability newProb = new Probability(prob.Probabilities, prob.ProbNodes, newConds, newNodes);
                return newProb;
            }
        }

        public static bool CheckNegation(Probability prob)
        {
            string probability = prob.Probabilities[0];
            for (int i = 0; i < prob.Conditions.Length; i++)
            {
                Node currentNode = prob.CondNodes[i];
                string currentCond = prob.Conditions[i];
                if (currentNode.Definitions.Contains(probability) && probability != currentCond)
                {
                    Console.WriteLine(prob.ToString() + "  negacija");
                    return true;
                }
            }
            return false;
        }

        public static bool IsRule2(Probability prob)
        {
            return !prob.ProbNodes[0].Definitions.Where(s => s != prob.Probabilities[0]).Except(prob.Conditions).Any();
        }

        public static bool IsRule5(Probability prob)
        {
            Node parent = prob.ProbNodes[0];
            foreach (Node condNode in prob.CondNodes)
                if (parent.Children.Contains(condNode))
                    return true;
            return false;
        }

        public static double UseRule5(Probability prob)
        {
            Console.WriteLine("Use5");
            Node parent = prob.ProbNodes[0];
            string[] children = new string[0];
            Node[] childNodes = new Node[0];
            string[] conds = new string[0];
            Node[] condNodes = new Node[0];

            for (int i = 0; i < prob.Conditions.Length; i++)
            {
                Node condNode = prob.CondNodes[i];
                string cond = prob.Conditions[i];
                if (parent.Children.Contains(condNode))
                {
                    children = children.Concat(new string[] { cond }).ToArray();
                    childNodes = childNodes.Concat(new Node[] { condNode }).ToArray();
                }
                else
                {
                    conds = conds.Concat(new string[] { cond }).ToArray();
                    condNodes = condNodes.Concat(new Node[] { condNode }).ToArray();
                }
            }
            string[] newConds = conds.Except(children).ToArray();
            Node[] newCondNodes = condNodes.Except(childNodes).ToArray();
            Probability a = new Probability(prob.Probabilities, prob.ProbNodes, newConds, newCondNodes);
            double aResult = SaveProbability(a, CalculateProbability(a));
            Probability b = new Probability(children, childNodes, prob.Probabilities.Concat(newConds).ToArray(), prob.ProbNodes.Concat(newCondNodes).ToArray());
            double bResult = SaveProbability(b, CalculateProbability(b));
            Probability c = new Probability(children, childNodes, newConds, newCondNodes);
            double cResult = SaveProbability(c, CalculateProbability(c));

            return SaveProbability(prob, aResult / (bResult / cResult));
        }

        public static bool IsRule6b(Probability prob)
        {
            return prob.ProbNodes[0].Parents.Count > 0;
        }

        public static double UseRule6b(Probability prob)
        {
            Console.WriteLine("Use6b");
            Node[] parents = prob.ProbNodes[0].Parents.ToArray();
            string[][] options = new string[0][];
            foreach (Node parent in parents)
            {
                Array.Resize(ref options, options.Length + 1);
                options[options.Length - 1] = parent.Definitions;
            }
            var combinations = CartesianProduct(options);

            double result = 0;
            foreach (var combination in combinations)
            {
                string[] comb = combination.ToArray();
                Probability a = new Probability(prob.Probabilities, prob.ProbNodes, comb, parents);
                double aResult = SaveProbability(a, CalculateProbability(a));
                Probability b = new Probability(comb, parents, prob.Conditions, prob.CondNodes);
                double bResult = SaveProbability(b, CalculateProbability(b));
                result += (aResult * bResult);
            }
            return SaveProbability(prob, result);
        }

        public static bool IsRule6a(Probability prob)
        {
            return prob.ProbNodes[0].Parents.Count == 0;
        }

        public static double UseRule6a(Probability prob)
        {
            Console.WriteLine("Use6a");
            Probability temp = new Probability(prob.Probabilities, prob.ProbNodes, new string[0], new Node[0]);
            return SaveProbability(prob, CalculateProbability(temp));
        }

        public static bool IsRule1(Probability prob)
        {
            return !prob.Probabilities.Except(prob.Conditions).Any();
        }

        public static bool IsRule3(Probability prob)
        {
            return prob.Probabilities.Length > 1;
        }

        public static double UseRule3(Probability prob)
        {
            Console.WriteLine("Use3");
            string[] probs = prob.Probabilities;
            Node[] probNodes = prob.ProbNodes;
            string[] conds = prob.Conditions;
            Node[] condNodes = prob.CondNodes;

            string[] used = new string[0];
            Node[] usedNodes = new Node[0];

            List<Probability> segments = new List<Probability>();
            for (int i = 0; i < probs.Length; i++)
            {
                Probability segment = new Probability(new string[] { probs[i] }, new Node[] { probNodes[i] }, used.Concat(conds).ToArray(), usedNodes.Concat(condNodes).ToArray());
                segments.Add(segment);
                used = used.Concat(new string[] { probs[i] }).ToArray();
                usedNodes = usedNodes.Concat(new Node[] { probNodes[i] }).ToArray();
            }
            double result = 1.0;
            for (int i = 0; i < segments.Count; i++)
            {
                double segmentResult = CalculateProbability(segments[i]);
                SaveProbability(segments[i], segmentResult);
                result = result * segmentResult;
            }
            return SaveProbability(prob, result);
        }

        public static double SaveProbability(Probability prob, double result)
        {

            prob.Value = result;
            if (!probabilities.ContainsKey(prob.ToString()))
                probabilities.Add(prob.ToString(), prob);
            return result;
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            // base case: 
            IEnumerable<IEnumerable<T>> result = new[] { Enumerable.Empty<T>() };
            foreach (var sequence in sequences)
            {
                var s = sequence; // don’t close over the loop variable 
                                  // recursive case: use SelectMany to build the new product out of the old one 
                result =
                  from seq in result
                  from item in s
                  select seq.Concat(new[] { item });
            }
            return result;
        }
    }
}
