using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class Interval
    {
        public int index { get; }
        public float l_boundary { get; }
        public float r_boundary { get; }
        public float value_actual { get; set; }
        public float value_theor { get; set; }

        public Interval(int index, float l_boundary, float r_boundary, float value_actual)
        {
            this.index = index;
            this.l_boundary = l_boundary;
            this.r_boundary = r_boundary;
            this.value_actual = value_actual;
        }
    }
}
