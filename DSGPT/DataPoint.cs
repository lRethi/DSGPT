using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSGPT
{
    public class DataPoint
    {
        public double[] Features { get; set; }
        public string Label { get; set; }

        public DataPoint(double[] features, string label)
        {
            Features = features;
            Label = label;
        }
    }
}
