using InfluenceDiagrams.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluenceDiagrams.Calculations
{
    [Serializable]
    public class SerialProbability
    {
        public string[] Probabilities { get; set; }
        public string[] ProbNodes { get; set; }
        public string[] Conditions { get; set; }
        public string[] CondNodes { get; set; }
        public double Value { get; set; }
    }
}
