using HenonPrediction.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HenonPrediction.Maths
{
    class SquareMatrix: PreciseDouble, ICloneable
    {
        private int dim;
        private double[,] matrix;
        private double[] eigen;
        private double threshold;
        private int maxNoChange = 20;

        public int Dimension
        {
            get
            {
                return dim;
            }
        }
        public double[] ValeursPropres
        {
            get
            {
                if (eigen == null)
                {
                    JacobiEigen();
                }
                return eigen;
            }
        }
        public double[] Diagonal
        {
            get
            {
                double[] diag = new double[Dimension];
                for (int i = 0; i < Dimension; i++)
                {
                    diag[i] = this[i, i];
                }
                return diag;
            }
        }
        public double Threshold
        {
            get
            {
                return threshold;
            }
            set
            {
                if (value != threshold)
                {
                    threshold = value;
                    if (eigen != null)
                    {
                        JacobiEigen();  // Recalculer lees valeurs propres en fonction du seuil
                    }
                }
            }
        }
        public SquareMatrix(int dim, double threshold, int precision): base(precision)
        {
            this.dim = dim;
            this.Threshold = threshold;
            matrix = new double[dim, dim];
        }
        public SquareMatrix(double[,] matr, double threshold, int precision) : base(precision)
        {
            this.dim = matr.GetLength(0);
            this.Threshold = threshold;
            this.matrix = new double[dim, dim];
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    this[i, j] = matr[i, j];
                }
            }
        }

        public static SquareMatrix Identity(int dim, double threshold, int precision)
        {
            SquareMatrix I = new SquareMatrix(dim, threshold, precision);
            for (int i = 0; i < dim; i++)
            {
                I[i, i] = 1;    // Diag
                for (int j = i + 1; j < dim; j++)
                {
                    // Non Diag
                    I[i, j] = 0;
                    I[j, i] = 0;
                }
            }
            return I;
        }
        public static SquareMatrix Identity(int dim)
        {
            SquareMatrix I = new SquareMatrix(dim);
            for (int i = 0; i < dim; i++)
            {
                I[i, i] = 1;    // Diag
                for (int j = i + 1; j < dim; j++)
                {
                    // Non Diag
                    I[i, j] = 0;
                    I[j, i] = 0;
                }
            }
            return I;
        }
        public SquareMatrix(int dim, int precision): this(dim, 10e-5, precision) { }
        public SquareMatrix(int dim): this(dim, 10e-5, 8) { }
        public double this[int l, int c]
        {
            get
            {
                return matrix[l, c];
            }
            set
            {
                if (matrix[l, c] != value)
                {
                    eigen = null;
                    matrix[l, c] = Parse(value);   // Pour formater avec precision
                }
            }
        }
        public static SquareMatrix operator *(SquareMatrix a, SquareMatrix b)
        {
            if (a.Dimension != b.Dimension)
            {
                throw new Exception("La dimension des 2 matrices carrées ne sont pas identique");
            }
            SquareMatrix x = new SquareMatrix(a.Dimension);
            for (int i = 0; i < a.Dimension; i++)
            {
                for (int j = 0; j < a.Dimension; j++)
                {
                    x[i, j] = 0.0;
                    for (int k = 0; k < a.Dimension; k++)
                    {
                        x[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return x;
        }
        public static bool operator > (SquareMatrix A, double d)
        {
            double max = A.Parse(d);
            for (int i = 0; i < A.Dimension; i++) {
                for (int j = 0; j < A.Dimension; j++)
                {
                    if (i != j && Math.Abs(A[i, j]) > max)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool operator < (SquareMatrix A, double d)
        {
            return !(A > d);
        }
        public double Min()
        {
            double min = double.PositiveInfinity;
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    if (i != j && Math.Abs(this[i, j]) < min)
                    {
                        min = Math.Abs(this[i, j]);
                    }
                }
            }
            return min;
        }
        public double Max()
        {
            double max = double.NegativeInfinity;
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    if (i != j && Math.Abs(this[i, j]) > max)
                    {
                        max = Math.Abs(this[i, j]);
                    }
                }
            }
            return max;
        }
        private int[] MaxIndex()
        {
            int[] point = new int[2];
            double max = double.NegativeInfinity;
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    if (i != j && Math.Abs(this[i, j]) > max)
                    {
                        max = Math.Abs(this[i, j]);
                        point[0] = i;
                        point[1] = j;
                    }
                }
            }
            return point;
        }
        public void JacobiEigen()
        {
            // Calcul des valeurs propres par l'algorithme de Jacobi
            SquareMatrix A = (SquareMatrix)Clone();
            SquareMatrix P;
            int[] maxIndex;
            int noChange = maxNoChange;
            double min = A.Min();

            // Initialization
            /*bool[] changed = new bool[Dimension];
            double[] tmpEigen = new double[Dimension];
            int state = Dimension;

            for (int i = 0; i < Dimension; i++)
            {
                changed[i] = true;
                tmpEigen[i] = Diagonal[i];
            }*/

            double b, c;

            while (A > A.Threshold && noChange != 0)
            {
                P = Identity(Dimension);

                maxIndex = A.MaxIndex();
                int i = maxIndex[0];
                int j = maxIndex[1];

                if (i > j)
                {
                    int tmp = i;
                    i = j;
                    j = tmp;
                }
                //Console.WriteLine(A[i, j]);
                // Get the index max
                /*for (int i = 0; i < Dimension; i++)
                {
                    for (int j = i + 1; j < Dimension; j++)
                    {*/
                    if (A[i, i] != A[j, j])
                    {
                        b = Math.Abs(A[i, i] - A[j, j]);
                        c = 2 * A[i, j] * Math.Sign(A[i, i] - A[j, j]);
                        P[i, i] = Math.Sqrt((1 + b / Math.Sqrt(c*c + b*b))/2);
                        P[j, j] = P[i, i];
                        P[i, j] = -c / (2 * P[i, i] * Math.Sqrt(c*c + b*b));
                        P[j, i] = -P[i, j];
                    }
                    else
                    {
                        P[i, i] = P[j, j] = Math.Sqrt(2) / 2;
                        P[i, j] = -Math.Sqrt(2) / 2;
                        P[j, i] = -P[i, j];
                    }
                /*    }
                }*/
                A = P * A * P.Transposed();
                if (A.Min() == min)
                {
                    noChange--; // Pas de changement pendant les derniers
                }
                else if (noChange < maxNoChange)
                {
                    noChange = maxNoChange;
                }
                min = A.Min();
                //Console.WriteLine(string.Join("\t", A.Diagonal));
            }
            eigen = A.Diagonal;
        }

        public SquareMatrix Transposed()
        {
            SquareMatrix clone = (SquareMatrix)Clone();
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    clone[i, j] = this[j, i];
                }
            }
            return clone;
        }

        public object Clone()
        {
            SquareMatrix clone = new SquareMatrix(Dimension);
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    clone[i, j] = this[i, j];
                }
            }
            return clone;
        }

        public override string ToString()
        {
            string str = "";
            string tmp = "";
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    tmp += $"{this[i, j]}  ";
                }
                str = $"{str}\n|{tmp.Trim()}|";
                tmp = "";
            }
            return str.Trim();
        }
    }
}
