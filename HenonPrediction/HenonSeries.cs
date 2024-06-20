using HenonPrediction.Formatting;
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
        private bool startFlag = false;
        private List<HenonTerm> series;
        public HenonSeries(double a, double b)
        {
            A = a;
            B = b;
        }
        public HenonSeries(): this(0, 0)
        {
        }
        public List<HenonTerm> GenerateSeries(double initX, double initY, int term)
        {
            if (term == 0)
            {
                startFlag = false;   // End the recursive call
                return series;
            }
            if (!startFlag)
            {
                startFlag = true;
                series = new List<HenonTerm>();
            }
            HenonTerm prev = new HenonTerm(initX, initY);
            series.Add(prev);
            HenonTerm next = generateNext(prev);
            return GenerateSeries(next.X, next.Y, term - 1);
        }
        public static double[] ExtractXValue(List<HenonTerm> hs)
        {
            double[] x = (from t in hs select t.X).ToArray();
            return x;
        }
        public static double[] ExtractYValue(List<HenonTerm> hs)
        {
            double[] y = (from t in hs select t.Y).ToArray();
            return y;
        }
        protected HenonTerm generateNext(HenonTerm prev)
        {
            double nextX = prev.Y + 1 - A * Math.Pow(prev.X, 2);
            double nextY = B * prev.X;
            return new HenonTerm(nextX, nextY);
        } 
    }

    class HenonTerm: PreciseDouble
    {
        protected double x, y;
        public double X {
            get {
                return x;
            }
            protected set {
                x = Parse(value);
            }
        }
        public double Y {
            get { return y; }
            protected set {
                y = Parse(value);
            }
        }

        public HenonTerm(double x, double y, int precision): base(precision)
        {
            X = x;
            Y = y;
        }
        public HenonTerm(double x, double y): this(x, y, 8)
        {
            // 8 point precision by default
        }
        public override string ToString()
        {
            return $"[{X.ToString(precisionFormat)}, {Y.ToString(precisionFormat)}]";
        }

        protected override void PostModification()
        {
            // Adjust value
            X = X;
            Y = Y;
        }
    }
}
