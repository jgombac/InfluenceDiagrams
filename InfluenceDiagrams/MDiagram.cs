using InfluenceDiagrams.Nodes;
using InfluenceDiagrams.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using InfluenceDiagrams.Calculations;
using InfluenceDiagrams.PropertyControls;

namespace InfluenceDiagrams
{
    public static class MDiagram 
    {

        static HashSet<Node> nodes = new HashSet<Node>();
        static HashSet<Relation> relations = new HashSet<Relation>();

        private static bool evaluating = false;

        public static void EvaluateDiagram()
        {
            evaluating = true;
            foreach (Node node in nodes)
            {
                
                switch (node.Type)
                {
                    case NodeType.Decision:
                        (node.Control as DecisionNode).UnbundlingEnabled(true);
                        (node.Control as DecisionNode).DeleteEnabled(false);
                        break;
                    case NodeType.Event:
                        (node.Control as EventNode).UnbundlingEnabled(true);
                        (node.Control as EventNode).DeleteEnabled(false);
                        break;
                    case NodeType.Value:
                        (node.Control as ValueNode).DeleteEnabled(false);
                        break;
                }
                foreach(Relation rel in relations)
                {
                    rel.TurningEnabled(true);
                    rel.DeleteEnabled(false);
                }
            }
        }


        public static void UnbundleEvent(Node node)
        {
            EventNode eNode = node.Control as EventNode;
            List<Relation> deleteRels = new List<Relation>();
            List<Node> updateValues = new List<Node>();
            List<KeyValuePair<Node,Anchor>> extendRelations = new List<KeyValuePair<Node,Anchor>>();
            foreach (Relation rel in relations)
            {
                if (rel.Parent == node || rel.Child == node)
                {
                    deleteRels.Add(rel);
                    if (rel.Parent == node && rel.Child.Type == NodeType.Value)
                    {
                        updateValues.Add(rel.Child);
                    }
                    if (rel.Child == node && rel.Parent.Type == NodeType.Event)
                    {
                        extendRelations.Add(new KeyValuePair<Node, Anchor>(rel.Parent, rel.ParentAnchor));
                    }
                }
            }

            if (extendRelations.Count > 0)
            {
                Console.WriteLine("EXTENDED");
                Node[] extendedParents = ExtendedParents(node);
                foreach(Node vNode in updateValues)
                {
                    ValueOutcome[] oldOutcomes = (vNode.Control as ValueNode).Outcomes;
                    ValueOutcome[] newOutcomes = NewOutcomes(vNode, node, extendedParents);
                    foreach(ValueOutcome newOutc in newOutcomes)
                    {
                        List<ValueOutcome> relevantOlds = new List<ValueOutcome>();
                        foreach(ValueOutcome oldOutc in oldOutcomes)
                        {
                            if (IsRelevant(newOutc, oldOutc) && oldOutc.Factors.Intersect(newOutc.Factors).Any())//ne sme met negacije
                            {
                                relevantOlds.Add(oldOutc);
                                Console.WriteLine("RELEVANTOUTCOME " + oldOutc.ToString());
                            }
                        }

                        double newValue = 0;
                        foreach(ValueOutcome relevant in relevantOlds)
                        {
                            foreach(Probability prob in eNode.Probabilities)
                            {
                                if (relevant.Factors.Intersect(prob.Factors).Any() && newOutc.Factors.Intersect(prob.Factors).Any() && IsRelevant(prob, relevant))
                                {
                                    Console.WriteLine("RELEVANTPROB " + prob.ToString());
                                    newValue += relevant.Value * prob.Value;
                                }
                            }
                        }
                        newOutc.Value = newValue;
                    }
                    (vNode.Control as ValueNode).Outcomes = newOutcomes;
                    (vNode.OutcomeControl as PropertyValue).UpdateOutcomes();
                    foreach (KeyValuePair<Node, Anchor> newParent in extendRelations)
                    {
                        new Relation(newParent.Key, newParent.Value, vNode, vNode.TopAnchor, false);
                    }
                }
            }


            foreach (Relation rel in deleteRels)
                RemoveRelation(rel);
            if (extendRelations.Count == 0)
            {
                Console.WriteLine("NORAMAL");
                foreach (Node value in updateValues)
                {
                    ValueNode vNode = value.Control as ValueNode;
                    ValueOutcome[] oldOutc = vNode.Outcomes;
                    vNode.UpdateOutcomes();
                    foreach (ValueOutcome newOutc in vNode.Outcomes)
                    {
                        List<ValueOutcome> relevantOutc = new List<ValueOutcome>();
                        foreach (ValueOutcome oldc in oldOutc)
                        {
                            if (IsRelevant(newOutc, oldc) && newOutc.Factors.Intersect(oldc.Factors).Any())
                            {
                                Console.WriteLine("RELEVANT= " + newOutc.ToString() + " - " + oldc.ToString());
                                relevantOutc.Add(oldc);
                            }
                        }
                        double newValue = 0;
                        foreach (ValueOutcome relevantOut in relevantOutc)
                        {
                            string diff = relevantOut.Factors.Except(newOutc.Factors).ToArray()[0];

                            foreach (Probability prob in eNode.Probabilities)
                            {
                                if (IsRelevant(prob, relevantOut) && prob.Factors.Intersect(relevantOut.Factors).Any())//prob.Probabilities.Contains(diff)
                                {
                                    newValue += (prob.Value * relevantOut.Value);
                                }
                            }
                        }
                        newOutc.Value = newValue;
                    }
                    (value.OutcomeControl as PropertyValue).UpdateOutcomes();
                    foreach (KeyValuePair<Node, Anchor> newParent in extendRelations)
                    {
                        new Relation(newParent.Key, newParent.Value, value, value.TopAnchor, false);
                    }
                }
            }
            RemoveNode(node);
            MainWindow.GetCanvas.Children.Remove(node.Control);
            MainWindow.GetCanvas.Children.Remove(node.DefinitionControl);
            MainWindow.GetCanvas.Children.Remove(node.ProbabilityControl);
            CheckCorrectness();
            RefreshCalculator();
        }

        private static bool IsRelevant(Probability prob, ValueOutcome outc)
        {
            var newFactors = outc.Factors.Zip(outc.FacNodes, (a, b) => new { a, b });

            foreach (var newFactor in newFactors)
            {
                foreach (var probFac in prob.Factors)
                {
                    if (!(new string[] { newFactor.a, probFac }.Distinct().Except(newFactor.b.Definitions).Any()) && newFactor.a != probFac)
                        return false;
                }
            }
            return true;
        }

        public static ValueOutcome[] NewOutcomes(Node vNode, Node eNode, Node[] newParents)
        {
            Node[] parents = vNode.Parents.Except(new Node[] { eNode }).Concat(newParents).Distinct().ToArray();
            string[] parentNames = new string[0];
            string[][] outcomes = new string[0][];

            foreach (Node parent in parents)
            {
                Array.Resize(ref parentNames, parentNames.Length + 1);
                parentNames[parentNames.Length - 1] = parent.Name;
                Array.Resize(ref outcomes, outcomes.Length + 1);
                outcomes[outcomes.Length - 1] = parent.Definitions;
            }

            var conditionCombinations = CartesianCompute.CartesianProduct(outcomes);
            HashSet<ValueOutcome> values = new HashSet<ValueOutcome>();
            if (conditionCombinations != null && conditionCombinations.Count() > 0)
            {
                foreach (var comb in conditionCombinations)
                {
                    string[] combArr = comb.ToArray();
                    ValueOutcome value = new ValueOutcome(combArr.ToArray(), parents, 0);

                    Console.WriteLine("ValueOutcome: " + value.ToString());
                    values.Add(value);
                }
            }

            return values.ToArray();

        }

        private static bool IsRelevant(ValueOutcome newOut, ValueOutcome oldOut)
        {
            var newFactors = newOut.Factors.Zip(newOut.FacNodes, (a, b) => new { a, b });

            foreach(var newFactor in newFactors)
            {
                foreach(var oldFactor in oldOut.Factors)
                {
                    if (!(new string[] { newFactor.a, oldFactor }.Distinct().Except(newFactor.b.Definitions).Any()) && newFactor.a != oldFactor)
                        return false;
                }
            }
            return true;
        }

        public static Node GetNode(string id)
        {
            foreach (Node node in nodes)
                return (node.ID == id) ? node : null;
            return null;
        }

        public static bool WillExtend(Node node)
        {
            foreach(Node parent in node.Parents)
            {
                if (parent.Type == NodeType.Event)
                    return true;
            }
            return false;
        }

        public static Node[] ExtendedParents(Node node)
        {
            List<Node> parents = new List<Node>();
            foreach(Node parent in node.Parents)
            {
                if(parent.Type == NodeType.Event)
                {
                    parents.Add(parent);
                }
            }
            return parents.ToArray();
        }

        public static void UnbundleDecision(Node node)
        {
            List<Relation> deleteRels = new List<Relation>();
            List<Node> updateValues = new List<Node>();
            List<Node> extendRelations = new List<Node>();
            foreach(Relation rel in relations)
            {
                if (rel.Parent == node || rel.Child == node)
                {
                    deleteRels.Add(rel);
                    if(rel.Parent == node && rel.Child.Type == NodeType.Value)
                    {
                        updateValues.Add(rel.Child);
                    }
                    if(rel.Child == node)
                    {
                        extendRelations.Add(rel.Parent);
                    }
                }
            }
            foreach (Relation rel in deleteRels)
                RemoveRelation(rel);
            foreach(Node value in updateValues)
            {
                ValueNode vNode = value.Control as ValueNode;
                ValueOutcome[] oldOutcomes = vNode.Outcomes;
                vNode.UpdateOutcomes();
                string bestDecision = "";
                foreach (ValueOutcome outc in vNode.Outcomes)
                {
                    List<ValueOutcome> relevant = new List<ValueOutcome>();
                    foreach(ValueOutcome olds in oldOutcomes)
                    {
                        if (!outc.Factors.Except(olds.Factors).Any())
                        {
                            relevant.Add(olds);
                        }
                    }
                    double bestValue = Double.NaN;
                    
                    foreach (ValueOutcome olds in relevant)
                    {
                        if(vNode.Better == "max")
                        {
                            if(Double.IsNaN(bestValue) || olds.Value > bestValue)
                            {
                                bestValue = olds.Value;
                                bestDecision = (olds.Factors.Intersect(node.Definitions).ToArray().First() as string) + " - " + node.Name;
                            }
                        }
                        else if(vNode.Better == "min")
                        {
                            if (Double.IsNaN(bestValue) || olds.Value < bestValue)
                            {
                                bestValue = olds.Value;
                                bestDecision = (olds.Factors.Intersect(node.Definitions).ToArray().First() as string) + " - " + node.Name;
                            }
                        }
                    }
                    outc.Value = bestValue;
                    
                }
                (value.Control as ValueNode).BestDecisions = (value.Control as ValueNode).BestDecisions.Concat(new string[] { bestDecision }).ToArray();
                (value.OutcomeControl as PropertyValue).UpdateOutcomes();
                
            }
            RemoveNode(node);
            MainWindow.GetCanvas.Children.Remove(node.Control);
            MainWindow.GetCanvas.Children.Remove(node.DefinitionControl);
        }

        public static void TurnRelation(Relation rel)
        {

            Probability[] newChildProbs = ChildProbabilities(rel.Parent, rel.Child);
            Probability[] newParentProbs = ParentProbabilities(rel.Parent, rel.Child);
            if(WillHaveChildren(rel.Parent, rel))
            {
                Console.WriteLine("WILL HAVE CHILDERN");
                (rel.Parent.Control as EventNode).Probabilities = CalculateNewProbabilities(rel.Parent, newParentProbs, true);
                (rel.Parent.ProbabilityControl as PropertyEvent).UpdateProbabilities();
            }
            if (WillHaveChildren(rel.Child, rel))
            {
                (rel.Child.Control as EventNode).Probabilities = CalculateNewProbabilities(rel.Child, newChildProbs, false);
                (rel.Child.ProbabilityControl as PropertyEvent).UpdateProbabilities();
            }
            RemoveRelation(rel);
            Relation newRel = new Relation(rel.Child, rel.ChildAnchor, rel.Parent, rel.ParentAnchor, false);
            
            CheckCorrectness();
            RefreshCalculator();
        }

        private static bool WillHaveChildren(Node node, Relation rel)
        {
            foreach(Relation allRel in relations)
            {
                if (allRel.Parent == node && allRel != rel)
                    return true;
            }
            return false;
        }

        private static Probability[] CalculateNewProbabilities(Node node, Probability[] newProbabilities, bool isParent)
        {
            EventNode eNode = node.Control as EventNode;
            foreach (Probability newProb in newProbabilities) {
                List<Probability> relevant = new List<Probability>();
                foreach(Probability prob in eNode.Probabilities)
                {
                    if ((!newProb.Factors.Except(prob.Factors).Any() && !isParent) || (newProb.Factors.Intersect(prob.Factors).Any() && isParent))
                    {
                        relevant.Add(prob);
                    }
                }
                double newValue = 0;
                foreach(Probability relProb in relevant)
                {

                    List<Node> newNodes = new List<Node>();
                    List<string> newConds = new List<string>();
                    for(int i = 0; i < relProb.Conditions.Length; i++)
                    {
                        string cond = relProb.Conditions[i];
                        Node n = relProb.CondNodes[i];
                        if(n.Type != NodeType.Decision)
                        {
                            Node decParent = HasDecisionParent(n);
                            if(decParent != null)
                            {
                                newNodes.Add(decParent);
                                foreach(string def in decParent.Definitions)
                                {
                                    if (relProb.Factors.Contains(def))
                                        newConds.Add(def);
                                }
                            }
                            newNodes.Add(n);
                            newConds.Add(cond);
                        }
                    }
                    newValue += (relProb.Value * Calculator.CalculateProbability(new Probability(newConds.ToArray(), newNodes.ToArray(), newProb.Conditions, newProb.CondNodes)));
                }
                newProb.Value = newValue;
            }
            return newProbabilities;
        }

        private static Node HasDecisionParent(Node node)
        {
            foreach(Node parent in node.Parents)
            {
                if(parent.Type == NodeType.Decision)
                {
                    return parent;
                }
            }
            return null;
        }


        public static void RefreshCalculator()
        {
            Calculator.Clear();
            foreach(Node node in nodes)
            {
                if(node.Type == NodeType.Event)
                {
                    foreach (Probability prob in (node.Control as EventNode).Probabilities)
                        Calculator.AddProbability(prob);
                }
            }
        }



        public static Probability[] ParentProbabilities(Node parent, Node child)
        {
            Node[] parents = parent.Parents.Concat(new Node[] { child }).ToArray(); ;
            string[] parentNames = new string[0];
            string[][] outcomes = new string[][] { parent.Definitions };

            bool hasEventParent = false;
            foreach (Node par in parents)
            {
                if (parent.Type == NodeType.Event)
                    hasEventParent = true;
                Array.Resize(ref parentNames, parentNames.Length + 1);
                parentNames[parentNames.Length - 1] = par.Name;
                Array.Resize(ref outcomes, outcomes.Length + 1);
                outcomes[outcomes.Length - 1] = par.Definitions;
            }

            var conditionCombinations = CartesianCompute.CartesianProduct(outcomes);
            HashSet<Probability> probs = new HashSet<Probability>();
            foreach (var comb in conditionCombinations)
            {
                string[] combArr = comb.ToArray();
                Probability prob = null;
                if (hasEventParent)
                    prob = new Probability(new string[] { combArr[0] }, new Node[] { parent }, combArr.Skip(1).ToArray(), parents);
                else
                    prob = new Probability(combArr, new Node[] { parent }.Concat(parents).ToArray(), new string[0], new Node[0]);
                Console.WriteLine("Probability: " + prob.ToString());
                probs.Add(prob);
            }
            return probs.ToArray();
        }

        public static Probability[] ChildProbabilities(Node parent, Node child)
        {
            Node[] parents = child.Parents.Where(x => x != parent).ToArray();
            string[] parentNames = new string[0];
            string[][] outcomes = new string[][] { child.Definitions };

            bool hasEventParent = false;
            foreach (Node par in parents)
            {
                if (child.Type == NodeType.Event)
                    hasEventParent = true;
                Array.Resize(ref parentNames, parentNames.Length + 1);
                parentNames[parentNames.Length - 1] = par.Name;
                Array.Resize(ref outcomes, outcomes.Length + 1);
                outcomes[outcomes.Length - 1] = par.Definitions;
            }

            var conditionCombinations = CartesianCompute.CartesianProduct(outcomes);
            HashSet<Probability> probs = new HashSet<Probability>();
            foreach (var comb in conditionCombinations)
            {
                string[] combArr = comb.ToArray();
                Probability prob = null;
                if (hasEventParent)
                    prob = new Probability(new string[] { combArr[0] }, new Node[] { child }, combArr.Skip(1).ToArray(), parents);
                else
                    prob = new Probability(combArr, new Node[] { child }.Concat(parents).ToArray(), new string[0], new Node[0]);
                Console.WriteLine("Probability: " + prob.ToString());
                probs.Add(prob);
            }
            return probs.ToArray();
        }





        public static bool InArray(this string str, string[] values)
        {
            if (Array.IndexOf(values, str) > -1)
                return true;

            return false;
        }

        private static bool CheckCorrectness()
        {
            bool valueChildren = true;
            foreach(Node node in nodes)
            {
                if (node.Type != NodeType.Value)
                    break;
                foreach(Node child in node.Children)
                {
                    if (child.Type != NodeType.Value)
                        valueChildren = false;
                }
            }
            HashSet<Node> removeNodes = new HashSet<Node>();
            for(int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes.ElementAt(i);
                if(node.Type != NodeType.Value)
                {
                    if(node.Children.Count == 0)
                    {
                        removeNodes.Add(node);
                    }
                }
            }
            HashSet<Relation> removeRelations = new HashSet<Relation>();
            foreach(Node node in removeNodes)
            {
                foreach(Relation rel in relations)
                {
                    if(rel.Child == node)
                    {
                        removeRelations.Add(rel);
                    }
                }
                MainWindow.GetCanvas.Children.Remove(node.Control);
                RemoveNode(node);
                MainWindow.GetCanvas.Children.Remove(node.DefinitionControl);
                if (node.Type == NodeType.Event)
                    MainWindow.GetCanvas.Children.Remove(node.ProbabilityControl);
            }
            foreach(Relation rel in removeRelations)
            {
                RemoveRelation(rel);
            }
                
            return valueChildren;
        }


        public static void ShowAllAnchors()
        {
            foreach(Node node in nodes)
            {
                node.TopAnchor.Show();
                node.BottomAnchor.Show();
                node.LeftAnchor.Show();
                node.RightAnchor.Show();
            }
        }

        public static void HideAllAnchors()
        {
            foreach (Node node in nodes)
            {
                node.TopAnchor.Hide();
                node.BottomAnchor.Hide();
                node.LeftAnchor.Hide();
                node.RightAnchor.Hide();
            }
        }



        public static void AddNode(Node node)
        {
            nodes.Add(node);
        }

        public static void RemoveNode(Node node)
        {
            nodes.RemoveWhere(s => s == node);
        }


        public static void AddRelation(Relation relation)
        {
            relations.Add(relation);
        }

        public static void RemoveRelation(Relation relation)
        {
            
            relation.Child.RemoveParent(relation.Parent);
            relation.Parent.RemoveChild(relation.Child);
            relations.RemoveWhere(s => s == relation);
            MainWindow.GetCanvas.Children.Remove(relation);
        }

        public static bool Evaluating
        {
            get { return evaluating; }
        }

        public static Node[] Nodes
        {
            get { return nodes.ToArray(); }
            
        }

        public static Relation[] Relations
        {
            get { return relations.ToArray(); }
        }

    }
}
