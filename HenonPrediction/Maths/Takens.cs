using HenonPrediction.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HenonPrediction.Maths
{
    class Takens: PreciseDouble
    {
        public Takens(int precision): base(precision)
        {
        }

        private double mean(double[] vector)
        {
            return Parse(vector.Sum() / vector.Length);
        }
        private double covariance(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new Exception("Les vecteurs ne sont pas de même longueur");
            }
            double mx = mean(x);
            double my = mean(y);
            double ret = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                double d = (x[i] - mx) * (y[i]  - my);
                ret += d;
            }
            return ret;
        }
        private double variance(double[] vector)
        {
            return covariance(vector, vector);
        }
        public double[] ApproximationError(double[] series)
        {
            return new double[1];
        }
    }
}
