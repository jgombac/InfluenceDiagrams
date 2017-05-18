using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluenceDiagrams.Relations
{
    [Serializable]
    public class SerialRelation
    {
        public string Parent { get; set; }
        public string ParentAnchor { get; set; }
        public string Child { get; set; }
        public string ChildAnchor { get; set; }
    }
}
