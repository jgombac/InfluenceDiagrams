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
            //enable unbundling and disable deletion
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
            List<Relation> deleteRels = new List<Relation>(); //relations to delete
            List<Node> updateValues = new List<Node>(); //value nodes to update
            List<KeyValuePair<Node,Anchor>> extendRelations = new List<KeyValuePair<Node,Anchor>>(); //relations to extend
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

            if (extendRelations.Count > 0) //if any relations has to be extended
            {
                Node[] extendedParents = ExtendedParents(node);
                foreach(Node vNode in updateValues)
                {
                    ValueOutcome[] oldOutcomes = (vNode.Control as ValueNode).Outcomes; //values before unbundling
                    ValueOutcome[] newOutcomes = NewOutcomes(vNode, node, extendedParents); //new values after unbundling
                    foreach(ValueOutcome newOutc in newOutcomes)
                    {
                        List<ValueOutcome> relevantOlds = new List<ValueOutcome>();
                        foreach(ValueOutcome oldOutc in oldOutcomes) // find relevant outcomes out of old ones
                        {
                            if (IsRelevant(newOutc, oldOutc) && oldOutc.Factors.Intersect(newOutc.Factors).Any()) //check if value doesnt negate itself and if factors align
                            {
                                relevantOlds.Add(oldOutc);
                            }
                        }

                        double newValue = 0;
                        foreach(ValueOutcome relevant in relevantOlds)
                        {
                            foreach(Probability prob in eNode.Probabilities)
                            {
                                //probability has any common factors with old relevant outcome and new outcome. It must not negate any of it
                                if (relevant.Factors.Intersect(prob.Factors).Any() && newOutc.Factors.Intersect(prob.Factors).Any() && IsRelevant(prob, relevant))
                                {
                                    newValue += relevant.Value * prob.Value;
                                }
                            }
                        }
                        newOutc.Value = newValue;
                    }
                    (vNode.Control as ValueNode).Outcomes = newOutcomes; //assing new values to tha respective value node
                    (vNode.OutcomeControl as PropertyValue).UpdateOutcomes(); //update UI
                    foreach (KeyValuePair<Node, Anchor> newParent in extendRelations)
                    {
                        new Relation(newParent.Key, newParent.Value, vNode, vNode.TopAnchor, false); //create new extended relation
                    }
                }
            }

            foreach (Relation rel in deleteRels) //delete all previous relations
                RemoveRelation(rel);

            if (extendRelations.Count == 0) //if no realtions have to be extended
            {
                foreach (Node value in updateValues)
                {
                    ValueNode vNode = value.Control as ValueNode;
                    ValueOutcome[] oldOutc = vNode.Outcomes;
                    vNode.UpdateOutcomes(); // have value node produce new outcomes based on parents
                    foreach (ValueOutcome newOutc in vNode.Outcomes) //go trough every new outcome end find relevant outcomes amongst old ones
                    {
                        List<ValueOutcome> relevantOutc = new List<ValueOutcome>();
                        foreach (ValueOutcome oldc in oldOutc)
                        {
                            if (IsRelevant(newOutc, oldc) && newOutc.Factors.Intersect(oldc.Factors).Any())
                            {
                                relevantOutc.Add(oldc);
                            }
                        }
                        double newValue = 0;
                        foreach (ValueOutcome relevantOut in relevantOutc)
                        {
                            foreach (Probability prob in eNode.Probabilities)
                            {
                                //probability must not negate old relevant outcomes and needs to have common factors
                                if (IsRelevant(prob, relevantOut) && prob.Factors.Intersect(relevantOut.Factors).Any())
                                {
                                    newValue += (prob.Value * relevantOut.Value);
                                }
                            }
                        }
                        newOutc.Value = newValue;
                    }
                    (value.OutcomeControl as PropertyValue).UpdateOutcomes(); //update UI
                    foreach (KeyValuePair<Node, Anchor> newParent in extendRelations) //quite unnecessary
                    {
                        new Relation(newParent.Key, newParent.Value, value, value.TopAnchor, false);
                    }
                }
            }
            RemoveNode(node);
            MainWindow.GetCanvas.Children.Remove(node.Control); //remove nodes
            MainWindow.GetCanvas.Children.Remove(node.DefinitionControl);
            MainWindow.GetCanvas.Children.Remove(node.ProbabilityControl);
            CheckCorrectness(); //check if any nodes doesnt have outgoing relations and delete it
            RefreshCalculator(); //refresh probability set
        }

        private static bool IsRelevant(Probability prob, ValueOutcome outc)
        {
            //basically checks if probability and value outcome dont negate eachother
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

        public static ValueOutcome[] NewOutcomes(Node vNode, Node eNode, Node[] newParents) //generate new outcomes wih extended parents
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
                    values.Add(value);
                }
            }
            return values.ToArray();
        }

        private static bool IsRelevant(ValueOutcome newOut, ValueOutcome oldOut)
        {
            //check if outcomes dont negate eachother
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

        public static bool WillExtend(Node node) // returns true if by unbundling event node, we have to extend parent relation
        {
            foreach(Node parent in node.Parents)
            {
                if (parent.Type == NodeType.Event)
                    return true;
            }
            return false;
        }

        public static Node[] ExtendedParents(Node node) //find all nodes that will extend after unbundling event node
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
            List<Relation> deleteRels = new List<Relation>(); //relations to delete
            List<Node> updateValues = new List<Node>(); //value nodes to update
            List<Node> extendRelations = new List<Node>(); //unnecessary, since no relation will extend
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
            foreach (Relation rel in deleteRels) //remove relations
                RemoveRelation(rel);
            foreach(Node value in updateValues)
            {
                ValueNode vNode = value.Control as ValueNode;
                ValueOutcome[] oldOutcomes = vNode.Outcomes; //save old outcomes
                vNode.UpdateOutcomes(); //have value node update its outcomes
                string bestDecision = ""; //keep track of nodes best decision
                foreach (ValueOutcome outc in vNode.Outcomes) //find relevant old outcomes
                {
                    List<ValueOutcome> relevant = new List<ValueOutcome>();
                    foreach(ValueOutcome olds in oldOutcomes)
                    {
                        //when unbundling decision, old outcome has to be a superset to new outcome to be relevant
                        if (!outc.Factors.Except(olds.Factors).Any())
                        {
                            relevant.Add(olds);
                        }
                    }
                    double bestValue = Double.NaN;                   
                    foreach (ValueOutcome olds in relevant)
                    {
                        //find new best value and decision based on min/max rule
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
                (value.Control as ValueNode).BestDecisions = (value.Control as ValueNode).BestDecisions.Concat(new string[] { bestDecision }).ToArray(); //save best decision
                (value.OutcomeControl as PropertyValue).UpdateOutcomes(); // update UI              
            }
            RemoveNode(node); //remove node
            MainWindow.GetCanvas.Children.Remove(node.Control);
            MainWindow.GetCanvas.Children.Remove(node.DefinitionControl);
        }

        public static void TurnRelation(Relation rel)
        {
            Probability[] newChildProbs = ChildProbabilities(rel.Parent, rel.Child); //find new probabilities after relation will turn
            Probability[] newParentProbs = ParentProbabilities(rel.Parent, rel.Child);
            if(WillHaveChildren(rel.Parent, rel)) //check if relation will have outputs, so we dont calculate anything unnecessarely
            {
                (rel.Parent.Control as EventNode).Probabilities = CalculateNewProbabilities(rel.Parent, newParentProbs, true);
                (rel.Parent.ProbabilityControl as PropertyEvent).UpdateProbabilities(); //update UI
            }
            if (WillHaveChildren(rel.Child, rel))
            {
                (rel.Child.Control as EventNode).Probabilities = CalculateNewProbabilities(rel.Child, newChildProbs, false);
                (rel.Child.ProbabilityControl as PropertyEvent).UpdateProbabilities();
            }
            RemoveRelation(rel); //remove old and create new relation
            Relation newRel = new Relation(rel.Child, rel.ChildAnchor, rel.Parent, rel.ParentAnchor, false);           
            CheckCorrectness(); //remove any node without outgoing relations
            RefreshCalculator(); //refresh probability set
        }

        private static bool WillHaveChildren(Node node, Relation rel)
        {
            //checks whether a node will have any outgoing relations after turning current one
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
                foreach(Probability prob in eNode.Probabilities) // find old relevant probabilities
                {
                    // if child => old probability factors have to be a superset to new one
                    // if parent => old and new probability factors need to have something in common
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
                    for(int i = 0; i < relProb.Conditions.Length; i++) //build a new probability based on node parents
                    {
                        string cond = relProb.Conditions[i];
                        Node n = relProb.CondNodes[i];
                        if(n.Type != NodeType.Decision) //if parent is event node
                        {
                            Node decParent = HasDecisionParent(n); //returns a decision node if event has it as a parent *could potentially have more
                            if(decParent != null) //if theres no decision parents, we only save the events, otherwise decisions too
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
                    // P(a) = p(a|b) + p(b) if b is parent to a
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



        public static Probability[] ParentProbabilities(Node parent, Node child) //find parent probabilities after relation turn
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

        public static Probability[] ChildProbabilities(Node parent, Node child) // find child probabilities after relation turn
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
            foreach(Node node in nodes) //check if all value nodes have values as children
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
            for(int i = 0; i < nodes.Count; i++) //remove any node other than value without outgoing relations
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


        public static void ShowAllAnchors() //show relations when dragging a new one
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
