using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HenonPrediction
{
    class HenonSeries
    {
        public double A { get; protected set; }
        public double B { get; protected set; }
        public HenonSeries(double a, double b)
        {
            A = a;
            B = b;
        }
        public HenonSeries(): this(0, 0)
        {
        }
        public void GenerateSeries(double initX, double initY, int term)
        {
            HenonTerm prev = new HenonTerm(initX, initY);
            Console.WriteLine(prev);
            if (term < 0)
            {
                return;
            }
            HenonTerm next = generateNext(prev);
            GenerateSeries(next.X, next.Y, term - 1);
        }
        protected HenonTerm generateNext(HenonTerm prev)
        {
            double nextX = prev.Y + 1 - A * Math.Pow(prev.X, 2);
            double nextY = B * prev.X;
            return new HenonTerm(nextX, nextY);
        } 
    }

    class HenonTerm
    {
        protected double x, y;
        protected int precision = 8;
        protected string precisionFormat;
        public int Precision
        {
            get { return precision; }
            set
            {
                precision = value;
                precisionFormat = string.Concat("0.", new string('0', precision));
                // Reformat the number
                X = X;
                Y = Y;
            }
        }
        public double X {
            get {
                return x;
            }
            protected set {
                x = double.Parse(value.ToString(precisionFormat));
            }
        }
        public double Y {
            get { return y; }
            protected set {
                y = double.Parse(value.ToString(precisionFormat));
            }
        }

        public HenonTerm(double x, double y, int precision)
        {
            Precision = precision;
            X = x;
            Y = y;
        }
        public HenonTerm(double x, double y): this(x, y, 8)
        {
            // 8 point precision by default
        }
        public override string ToString()
        {
            return $"[{X.ToString("0.00000000")}, {Y.ToString("0.00000000")}]";
        }
    }
}
