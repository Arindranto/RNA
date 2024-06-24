using HenonPrediction.Formatting;
using HenonPrediction.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HenonPrediction.ANN
{
    public delegate double TransferFunction(double input);
    public class Neuron
    {
        private double output;
        public TransferFunction Transfer;
        public TransferFunction DerivatedTransfer;
        public double[] Weights;
        public double Output;
        public Neuron(double[] weights, TransferFunction t, TransferFunction d)
        {
            this.Weights = new double[weights.Length];
            this.Transfer = t;
            this.DerivatedTransfer = d;
            int i = 0;
            foreach (double w in weights)
            {
                this.Weights[i++] = w;
            }
        }
        public Neuron(double[] weights) : this(weights, GeneralFunctions.Sigmoid, GeneralFunctions.SigmoidPrime)
        {
        }
        public double Evaluate(double[] inputs)
        {
            if (inputs.Length != Weights.Length)
            {
                throw new Exception("Nombres de poids et d'entrées différents");
            }
            double val = 0.0;
            for (int i = 0; i < inputs.Length; i++)
            {
                val += Weights[i] * inputs[i];
            }
            return val;
        }
        public override string ToString()
        {
            return string.Join(" ", Weights.Select(w => w.ToString("0.00000000")));
        }
    }
}
