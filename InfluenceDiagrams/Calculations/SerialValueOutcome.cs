using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluenceDiagrams.Calculations
{
    [Serializable]
    public class SerialValueOutcome
    {
        public string[] Factors { get; set; }
        public string[] FacNodes { get; set; }
        public double Value { get; set; }
    }
}
