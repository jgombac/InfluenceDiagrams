using InfluenceDiagrams.Calculations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InfluenceDiagrams.Nodes
{
    [Serializable]
    public class SerialNode
    {
        public Point Position { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string[] Definitions { get; set; }
        public SerialProbability[] Probabilities { get; set; }
        public string Better { get; set; }
        public SerialValueOutcome[] Outcomes { get; set; }
    }
}
