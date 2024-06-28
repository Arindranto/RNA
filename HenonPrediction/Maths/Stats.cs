using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HenonPrediction.Maths
{
    class Stats
    {
        public static double Covariance(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new Exception("Les vecteurs ne sont pas de même longueur");
            }
            double mx = Mean(x);
            double my = Mean(y);
            double ret = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                double d = (x[i] - mx) * (y[i] - my);
                ret += d;
            }
            return ret;
        }
        public static double Variance(double[] vector)
        {
            return Covariance(vector, vector);
        }
        public static double Mean(double[] vector)
        {
            return vector.Sum() / vector.Length;
        }
    }
}
