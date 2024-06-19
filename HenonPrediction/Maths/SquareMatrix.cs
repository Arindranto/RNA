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
                    if (i != j && A[i, j] > max)
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
        public void JacobiEigen()
        {
            // Calcul des valeurs propres par l'algorithme de Jacobi
            SquareMatrix A = (SquareMatrix)Clone();
            SquareMatrix P = new SquareMatrix(Dimension);

            double b, c;

            while (A > A.Threshold)
            {
                for (int i = 0; i < Dimension; i++)
                {
                    for (int j = 0; j < Dimension; j++)
                    {
                        if (A[i, i] != A[j, j])
                        {
                            b = Math.Abs(A[i, i] - A[j, j]);
                            c = 2 * A[i, j] * Math.Sign(A[i, i] - A[j, j]);
                            P[i, i] = Math.Sqrt((1 + b / (c*c + b*b)));
                            P[j, j] = P[i, i];
                            P[i, j] = -c / (2 * P[i, i] * Math.Sqrt(c * c + b * b));
                            P[j, i] = -P[i, j];
                        }
                        else
                        {
                            P[i, i] = P[j, j] = Math.Sqrt(2) / 2;
                            P[i, j] = -Math.Sqrt(2) / 2;
                            P[j, i] = -P[i, j];
                        }
                    }
                }
                A = P * A * P.Transposed();
            }
            eigen = new double[Dimension];
            for (int i = 0; i < Dimension; i++)
            {
                eigen[i] = A[i, i];
            }
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
