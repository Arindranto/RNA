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
        public Neuron[] inputLayer;
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

            // Initialize
            for (int i = 0; i < inputs.Length; i++)
            {
                inputLayer[i].Output = inputs[i];
                inputLayerOutputs[i] = inputLayer[i].Output;
            }

            // Forward Propagation
            for (int i = 0; i < hiddenLayer.Length; i++)
            {
                hiddenLayer[i].Output = hiddenLayer[i].Transfer(hiddenLayer[i].Evaluate(inputLayerOutputs));
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
                hiddenLayerDeltas[i] = hiddenLayer[i].DerivatedTransfer(h) * sum;
            }

            // Weights adjustment
            for (int i = 0; i < hiddenLayer.Length; i++)
            {
                for (int j = 0; j < inputLayer.Length; j++)
                {
                    h = hiddenLayer[i].Weights[j];
                    sum = LearningStep * hiddenLayerDeltas[i] * inputLayerOutputs[j];
                    hiddenLayer[i].Weights[j] = Parse(h - sum);
                }
            }

            for (int i = 0; i < outputLayer.Length; i++)
            {
                for (int j = 0; j < hiddenLayer.Length; j++)
                {
                    h = outputLayer[i].Weights[j];
                    sum = LearningStep * outputLayerDeltas[i] * hiddenLayerOutputs[j];
                    outputLayer[i].Weights[j] = Parse(h - sum);
                }
            }
        }
    }
}
