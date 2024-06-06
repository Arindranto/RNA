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
        public double X { get; protected set; }
        public double Y { get; protected set; }

        public HenonTerm(double x, double y)
        {
            X = x;
            Y = y;
        }
        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }
    }
}
