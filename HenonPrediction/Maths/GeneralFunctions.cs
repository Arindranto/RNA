using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HenonPrediction.Maths
{
    class GeneralFunctions
    {
        public static double Identity(double input)
        {
            return input;
        }
        public static double IdentityPrime(double input)
        {
            return 1.0;
        }
        public static double Sigmoid(double input)
        {
            return 1 / (1 + Math.Exp(-input));
        }
        public static double SigmoidPrime(double input)
        {
            return Math.Exp(-input)/Math.Pow((1 + Math.Exp(-input)), 2);
        }
    }
}
