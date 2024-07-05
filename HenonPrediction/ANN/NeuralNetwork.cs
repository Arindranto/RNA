using HenonPrediction.Formatting;
using HenonPrediction.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HenonPrediction.ANN
{
    public class NeuralNetwork: PreciseDouble
    {
        public double LearningStep;
        private Neuron[] inputLayer;    // Only to receive the input
        public Neuron[] hiddenLayer;
        public Neuron[] outputLayer;
        public NeuralNetwork(int input, int hidden, int output, double learningStep, int precision): base(precision)
        {
            Random rand = new Random();
            inputLayer = new Neuron[input];
            hiddenLayer = new Neuron[hidden];
            outputLayer = new Neuron[output];
            LearningStep = learningStep;
            // Input weights
            for (int i = 0; i < input; i++)
            {
                inputLayer[i] = new Neuron(new double[] { 1.0 });
            }

            // Hidden Weights
            double[] weights = new double[input];
            for (int i = 0; i < hidden; i++)
            {
                // Random weigths each time
                for (int j = 0; j < input; j++)
                {
                    weights[j] = rand.NextDouble();
                }
                hiddenLayer[i] = new Neuron(weights);
            }

            // Output layer
            weights = new double[hidden];
            for (int i = 0; i < output; i++)
            {
                // Random weigths each time
                for (int j = 0; j < hidden; j++)
                {
                    weights[j] = rand.NextDouble();
                }
                outputLayer[i] = new Neuron(weights);
            }
        }

        public NeuralNetwork(int input, int hidden, double learningStep, int precision): this(input, hidden, 1, learningStep, precision)
        {
        }
        public NeuralNetwork(int input, int hidden, double learningStep): this(input, hidden, 1, learningStep, 8)
        {
        }
        public NeuralNetwork(int input, int hidden) : this(input, hidden, 0.1)
        {
        }
        public NeuralNetwork(NeuralNetwork network): base(network.Precision)
        {
            // Clone
            int input = network.inputLayer.Length;
            int hidden = network.hiddenLayer.Length;
            int output = network.outputLayer.Length;

            inputLayer = new Neuron[network.inputLayer.Length];
            hiddenLayer = new Neuron[network.hiddenLayer.Length];
            outputLayer = new Neuron[network.outputLayer.Length];
            LearningStep = network.LearningStep;

            for (int i = 0; i < input; i++)
            {
                inputLayer[i] = new Neuron(new double[] { 1.0 });
            }
            for (int i = 0; i < hidden; i++)
            {
                hiddenLayer[i] = new Neuron(new double[input]);
            }
            for (int i = 0; i < output; i++)
            {
                outputLayer[i] = new Neuron(new double[hidden]);
            }

            this.CopyWeight(network);
        }
        public void ShowWeights()
        {
            string str = "";
            foreach (Neuron n in hiddenLayer)
            {
                str += $"-- {n} -";
            }
            str += Environment.NewLine;
            foreach (Neuron n in outputLayer)
            {
                str += $"-- {n} -";
            }
            Console.WriteLine(str);
        }
        public void Train(double[] inputs, double[] outputs)
        {
            if (inputs.Length != inputLayer.Length)
            {
                throw new Exception("Le nombre de données en entrée est différent du nombre d'unités d'entrée");
            }
            // Initialize the output of the input layer to the inputs
            double[] inputLayerOutputs = new double[inputLayer.Length];
            double[] hiddenLayerOutputs = new double[hiddenLayer.Length];
            double[] outputLayerOutputs = new double[outputLayer.Length];
            double[] inputLayerDeltas = new double[inputLayer.Length];
            double[] hiddenLayerDeltas = new double[hiddenLayer.Length];
            double[] outputLayerDeltas = new double[outputLayer.Length];
            double h, sum;

            foreach (Neuron n in hiddenLayer)
            {
                n.Transfer = GeneralFunctions.Sigmoid;
                n.DerivatedTransfer = GeneralFunctions.SigmoidPrime;
            }
            foreach (Neuron n in outputLayer)
            {
                n.Transfer = GeneralFunctions.Sigmoid;
                n.DerivatedTransfer = GeneralFunctions.SigmoidPrime;
            }

            // Initialize
            for (int i = 0; i < inputs.Length; i++)
            {
                inputLayer[i].Output = inputs[i];
                inputLayerOutputs[i] = inputLayer[i].Output;
            }

            // Forward Propagation
            for (int i = 0; i < hiddenLayer.Length; i++)
            {
                h = hiddenLayer[i].Evaluate(inputLayerOutputs);
                double val = hiddenLayer[i].Transfer(h);
                hiddenLayer[i].Output = val;
                hiddenLayerOutputs[i] = hiddenLayer[i].Output;
            }

            for (int i = 0; i < outputLayer.Length; i++)
            {
                h = outputLayer[i].Evaluate(hiddenLayerOutputs);
                outputLayer[i].Output = outputLayer[i].Transfer(h);
                outputLayerOutputs[i] = outputLayer[i].Output;
                // Delta calculation
                outputLayerDeltas[i] = outputLayer[i].DerivatedTransfer(h) * (outputs[i] - outputLayer[i].Output);
            }

            // Back propagation
            for (int i = 0; i < hiddenLayer.Length; i++)
            {
                h = hiddenLayer[i].Evaluate(inputLayerOutputs);
                sum = 0.0;
                for (int j = 0; j < outputLayerDeltas.Length; j++)
                {
                    sum += outputLayer[j].Weights[i] * outputLayerDeltas[j];
                }
                double delta = hiddenLayer[i].DerivatedTransfer(h) * sum;
                hiddenLayerDeltas[i] = delta;
            }

            // Weights adjustment
            for (int i = 0; i < hiddenLayer.Length; i++)
            {
                for (int j = 0; j < inputLayer.Length; j++)
                {
                    h = hiddenLayer[i].Weights[j];
                    sum = LearningStep * hiddenLayerDeltas[i] * inputLayerOutputs[j];
                    hiddenLayer[i].Weights[j] = Parse(h + sum);
                }
            }

            for (int i = 0; i < outputLayer.Length; i++)
            {
                for (int j = 0; j < hiddenLayer.Length; j++)
                {
                    h = outputLayer[i].Weights[j];
                    sum = LearningStep * outputLayerDeltas[i] * hiddenLayerOutputs[j];
                    outputLayer[i].Weights[j] = Parse(h + sum);
                }
            }
            resetOutput();
        }
        private void CopyWeight(NeuralNetwork network)
        {
            int input = inputLayer.Length;
            int hidden = hiddenLayer.Length;
            int output = outputLayer.Length;

            for (int i = 0; i < hidden; i++)
            {
                for (int j = 0; j < input; j++) hiddenLayer[i].Weights[j] = network.hiddenLayer[i].Weights[j];
            }
            for (int i = 0; i < output; i++)
            {
                for (int j = 0; j < hidden; j++) outputLayer[i].Weights[j] = network.outputLayer[i].Weights[j];
            }
        }
        public double EnterEpoch(double[] timeSeries, int prototypeNumber, double prev)
        {
            // Return the NMSE
            if (outputLayer.Length != 1)
            {
                throw new Exception("Impossible de faire l'apprentissage avec plus de 1 unité de sortie");
            }
            double nmse = 0.0;
            NeuralNetwork tmp = new NeuralNetwork(this);
            for (int i = 0; i < prototypeNumber; i++)
            {
                tmp.CopyWeight(this);   // Clone to store the weights values
                double[] inputData = timeSeries.Skip(i).Take(inputLayer.Length).ToArray();
                double[] outputData = timeSeries.Skip(i + inputLayer.Length).Take(1).ToArray();
                Train(inputData, outputData);
                double networkOutput = Predict(inputData)[0];
                nmse += Math.Pow(outputData[0] - networkOutput, 2.0);
            }
            // 1/(N * var) * (sum((predicted - expected)²))
            //int totalUsed = inputLayer.Length + prototypeNumber;
            //nmse = nmse/(prototypeNumber * Stats.Variance(timeSeries.Take(totalUsed).ToArray()));
            nmse /= prototypeNumber;
            if (nmse > prev)
            {
                // Surapprentissage
                this.CopyWeight(tmp);
            }
            resetOutput();
            return nmse;
        }

        public double[] Predict(double[] inputs)
        {
            if (inputs.Length != inputLayer.Length)
            {
                throw new Exception("Nombre d'entrées différents du nombre d'unité d'entrée");
            }

            foreach (Neuron n in hiddenLayer)
            {
                n.Transfer = GeneralFunctions.Sigmoid;
                //n.DerivatedTransfer = GeneralFunctions.SigmoidPrime;
            }
            foreach (Neuron n in outputLayer)
            {
                n.Transfer = GeneralFunctions.Sigmoid;
                //n.DerivatedTransfer = GeneralFunctions.SigmoidPrime;
            }

            double sum;
            for (int i = 0; i < inputs.Length; i++)
            {
                inputLayer[i].Output = inputs[i];
            }
            for (int i = 0; i < hiddenLayer.Length; i++)
            {
                sum = hiddenLayer[i].Evaluate(inputLayer.Select(n => n.Output).ToArray());
                /*for (int j = 0; j < inputs.Length; j++)
                {
                    sum += inputLayer[j].Output * hiddenLayer[i].Weights[j];
                }*/
                hiddenLayer[i].Output = hiddenLayer[i].Transfer(sum);
            }
            for (int i = 0; i < outputLayer.Length; i++)
            {
                sum = outputLayer[i].Evaluate(hiddenLayer.Select(n => n.Output).ToArray());
                /*for (int j = 0; j < hiddenLayer.Length; j++)
                {
                    sum += hiddenLayer[j].Output * outputLayer[i].Weights[j];
                }*/
                outputLayer[i].Output = outputLayer[i].Transfer(sum);
            }
            double[] output = outputLayer.Select(neuron => Parse(neuron.Output)).ToArray();
            resetOutput();
            return output;
        }
        private void resetOutput() {
            foreach (Neuron n in inputLayer)
            {
                n.Output = 0.0;
            }
            foreach (Neuron n in hiddenLayer)
            {
                n.Output = 0.0;
            }
            foreach (Neuron n in outputLayer)
            {
                n.Output = 0.0;
            }
        }

        public override string ToString()
        {
            string str = "{";
            foreach (Neuron n in inputLayer)
            {
                str += $"[{n}]";
            }
            str += "-->";
            foreach (Neuron n in hiddenLayer)
            {
                str += $"[{n}]";
            }
            str += "==>";
            foreach (Neuron n in outputLayer)
            {
                str += $"[{n}]";
            }
            str += "}";
            return str;
        }
    }
}
