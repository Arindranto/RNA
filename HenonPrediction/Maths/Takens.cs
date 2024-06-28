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
        public Takens(): this(8) { }

        public double[] ApproximationError(double[] series, int n, int delay)
        {
            // Generate the series considering n and delay
            List<double[]> delaySeries = new List<double[]>();
            Dictionary<int, int> repetition = new Dictionary<int, int>();
            int idx = 0;
            bool exhausted = false;
            double[] delayVector;
            int seriesIndex;
            while (!exhausted)
            {
                bool include = true;
                delayVector = new double[n];
                for (int i = 0; i < n; i++)
                {
                    seriesIndex = idx + i * delay;
                    int j;
                    if (repetition.TryGetValue(seriesIndex, out j))
                    {
                        repetition[seriesIndex] = j + 1;
                    }
                    else
                    {
                        repetition[seriesIndex] = 1;
                    }

                    if (seriesIndex < series.Length)
                    {
                        delayVector[i] = series[seriesIndex];
                    }
                    else
                    {
                        delayVector[i] = 0.0;
                        include = false;    // Don't include uncompleted vectors
                        exhausted = true;   // Series exhausted
                    }
                }
                if (include)
                {
                    delaySeries.Add(delayVector);
                }
                idx++;
            }
            idx = 0;
            foreach (KeyValuePair<int, int> kv in repetition)
            {
                if (kv.Value > 1)
                {
                    Console.WriteLine($"{kv.Key}: {kv.Value}x");
                }
            }
            // Affichage des xbar
            /*foreach (double[] term in delaySeries)
            {
                string str = "[";
                foreach (double d in term)
                {
                    str += $"{Parse(d)}\t";
                }
                str = str.Trim();
                str += "]";
                Console.WriteLine($"xbar{idx}[{term.Length}] => {str}");
                idx++;
            }*/

            // Define covariance matrix
            Console.WriteLine($"La série a {delaySeries.Count} vecteurs");
            SquareMatrix covMatrix = new SquareMatrix(delaySeries.Count, 10e-2, precision);
            for (int i = 0; i < covMatrix.Dimension; i++)
            {
                for (int j = 0; j < covMatrix.Dimension; j++)
                {
                    covMatrix[i, j] = Stats.Covariance(delaySeries[i], delaySeries[j]);
                }
            }
            //Console.WriteLine(covMatrix);
            delayVector = covMatrix.ValeursPropres;
            Array.Sort(delayVector, (x, y) => y.CompareTo(x));  // Descending order

            double[] approxError = new double[covMatrix.Dimension];
            // E[l] = sqrt(delayVector[l+1])
            approxError[0] = 0.0;

            for (int i = 1; i < delayVector.Length - 1; i++)
            {
                double val = Parse(Math.Sqrt(delayVector[i]));
                approxError[i] = val;
            }

            return approxError;
        }
    }
}
