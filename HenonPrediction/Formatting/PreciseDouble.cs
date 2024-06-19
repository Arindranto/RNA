using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HenonPrediction.Formatting
{
    class PreciseDouble
    {
        protected int precision = 8;
        protected string precisionFormat;
        public int Precision
        {
            get { return precision; }
            set
            {
                precision = value;
                precisionFormat = string.Concat("0.", new string('0', precision));
                PostModification();
            }
        }
        public double Parse(double d)
        {
            return double.Parse(d.ToString(precisionFormat));
        }
        protected PreciseDouble(int precision)
        {
            this.Precision = precision;
        }
        protected virtual void PostModification()
        {
            return;
        }
    }
}
