using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluenceDiagrams.Nodes
{
    [Serializable]
    public enum NodeType
    {
        Decision = 0,
        Event = 1,
        Value = 2
    }
}
