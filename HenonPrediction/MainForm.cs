using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.SQLite;
using System.IO;
using HenonPrediction.Maths;
using HenonPrediction.ANN;

namespace HenonPrediction
{
    public partial class MainForm : Form
    {
        HashSet<Color> availableColors = new HashSet<Color>()
        {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Teal,
            Color.Purple,
            Color.Gray,
            Color.Magenta,
            Color.Orange
        };
        SQLiteConnection connection;
        private double min, max, zoomSize = 0.0;
        private bool mode;
        public bool MainMode
        {
            get { return mode; }
            set
            {
                mode = value;
                if (value)
                {
                    // Main Chart
                    graph.ChartAreas["SecondaryArea"].Visible = false;
                    graph.Legends["SecondaryLegend"].Enabled = false;
                    graph.ChartAreas["MainArea"].Visible = true;
                    graph.Legends["MainLegend"].Enabled = true;
                }
                else
                {
                    // Secondary Chart
                    graph.ChartAreas["SecondaryArea"].Visible = true;
                    graph.Legends["SecondaryLegend"].Enabled = true;
                    graph.ChartAreas["MainArea"].Visible = false;
                    graph.Legends["MainLegend"].Enabled = false;
                }
                refresh();
            }
        }
        public ChartArea ActiveArea
        {
            get
            {
                string areaName = "MainArea";
                if (!MainMode)
                {
                    areaName = "SecondaryArea";
                }
                return graph.ChartAreas[areaName];
            }
        }
        private Random random = new Random();
        public MainForm()
        {
            InitializeComponent();
            // Areas
            // Main Chart Area
            ChartArea area = new ChartArea();
            area.Name = "MainArea";
            area.AxisX.IntervalType = DateTimeIntervalType.Number;
            area.CursorX.AutoScroll = true;
            area.AxisX.ScaleView.Zoomable = true;
            area.AxisX.ScaleView.SizeType = DateTimeIntervalType.Number;
            area.AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            this.graph.ChartAreas.Add(area);
            // Other Chart
            area = new ChartArea();
            area.Name = "SecondaryArea";
            area.AxisX.IntervalType = DateTimeIntervalType.Number;
            area.CursorX.AutoScroll = true;
            area.AxisX.ScaleView.Zoomable = true;
            area.AxisX.ScaleView.SizeType = DateTimeIntervalType.Number;
            area.AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            this.graph.ChartAreas.Add(area);

            // Legends
            // Main
            Legend legend = new Legend();
            legend.Name = "MainLegend";
            this.graph.Legends.Add(legend);
            // Secondary
            legend = new Legend();
            legend.Name = "SecondaryLegend";
            this.graph.Legends.Add(legend);

            yEnFonctionDeXToolStripMenuItem_Click(null, null);  // Select the first menu

            HenonSeries hs = new HenonSeries(1.4, 0.3);
            List<HenonTerm> original500 = hs.GenerateSeries(0, 0, 500);
            Series originalSeries = addSeries("Original500", "500 premières valeurs de la série de Hénon", "SecondaryArea");
            plotXYSeries(originalSeries, original500);

            /*
            Takens takens = new Takens();
            double[] approximation = takens.ApproximationError(HenonSeries.ExtractXValue(original500), 50, 10);
            Series takensSeries = addSeries("TakensApprox", "Erreur d'approximation moyenne après l'algorithme de Takens");
            plotXYSeries(takensSeries, approximation);
            */
            //graph.MouseWheel += HandleMouseWheel;
            refresh();
        }
        void refresh()
        {
            IEnumerable<Series> series = graph.Series.Where(s => s.ChartArea == ActiveArea.Name);
            if (series.Count() > 0)
            {
                min = double.PositiveInfinity;
                max = double.NegativeInfinity;
                foreach (Series s in series)
                {
                    min = Math.Min(min, s.Points.Min(pt => pt.XValue));
                    max = Math.Max(max, s.Points.Max(pt => pt.XValue));
                }
                ActiveArea.AxisX.Minimum = min;
                ActiveArea.AxisX.Maximum = max;
                zoomSize = max;
                ActiveArea.AxisX.ScaleView.SmallScrollSize = zoomSize;
                //ActiveArea.AxisX.ScaleView.Zoom(min, max);
            }
        }
        void HandleMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                ZoomIn();
            }
            else
            {
                ZoomOut();
            }
        }
        void ZoomIn()
        {
            //area.AxisX.ScaleView.Zoom(min, zoomSize);
            zoomSize = (zoomSize - min);
            ActiveArea.AxisX.ScaleView.Zoom(min, zoomSize);
            ActiveArea.AxisX.ScaleView.SmallScrollSize = zoomSize;
        }
        void ZoomOut()
        {
            zoomSize = (zoomSize - min);
            ActiveArea.AxisX.ScaleView.Zoom(min, zoomSize);
            ActiveArea.AxisX.ScaleView.SmallScrollSize = zoomSize;
        }
        void plotXYSeries(Series series, double[] points)
        {
            int idx = 0;
            foreach (double val in points)
            {
                series.Points.AddXY(idx, val);
                idx++;
            }
        }
        void plotXYSeries(Series series, List<HenonTerm> points)
        {
            foreach (HenonTerm ht in points)
            {
                series.Points.AddXY(ht.X, ht.Y);
            }
        }
        void plotSeries(Series series, double[] points)
        {
            foreach (double d in points)
            {
                series.Points.Add(d);
            }
        }
        void plotSeries(Series series, List<HenonTerm> points, bool x)
        {
            if (x)
            {
                plotSeries(series, HenonSeries.ExtractXValue(points));
            }
            else
            {
                plotSeries(series, HenonSeries.ExtractYValue(points));
            }
            /*double pt;
            foreach (HenonTerm ht in points)
            {
                pt = x ? ht.X : ht.Y;
                series.Points.Add(pt);
            }*/
        }

        void plotSeries(Series series, List<HenonTerm> points)
        {
            plotSeries(series, points, true);
        }
        
        Color randomColor()
        {
            int index = random.Next() % availableColors.Count;
            return availableColors.ToArray()[index];
        }
        Series addSeries(string name, string legendText, string legendName, string chartAreaName)
        {
            Series series = new Series();

            series.BorderWidth = 1;
            series.ChartArea = chartAreaName;
            series.ChartType = SeriesChartType.Spline;

            // Color
            Color seriesColor = randomColor();
            series.Color = seriesColor;
            availableColors.Remove(series.Color);
            //series.IsValueShownAsLabel = true;
            series.Legend = legendName;
            series.LegendText = legendText;
            series.MarkerSize = 5;
            series.MarkerStyle = MarkerStyle.Circle;
            series.Name = name;
            series.XValueType = ChartValueType.Double;
            series.YValueType = ChartValueType.Double;
            this.graph.Series.Add(series);

            return series;
        }
        Series addSeries(string name, string legendText, string areaName)
        {
            string legendName = areaName.Replace("Area", "Legend");
            return addSeries(name, legendText, legendName, areaName);
        }
        Series addSeries(string name, string legendText)
        {
            return addSeries(name, legendText, "MainArea");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string path = Path.Combine(Application.StartupPath, "henon.db");
            string connString = $"Data Source={path}";
            connection = new SQLiteConnection(connString);
            TestNeuralNetwork();
        }
        private void TestNeuralNetwork()
        {
            NeuralNetwork network = new NeuralNetwork(3, 2, 1, 0.1, 8);
            
            network.hiddenLayer[0].Weights[0] = 0.2;
            network.hiddenLayer[0].Weights[1] = 0.1;
            network.hiddenLayer[0].Weights[2] = 0.1;
            
            network.hiddenLayer[1].Weights[0] = 0.3;
            network.hiddenLayer[1].Weights[1] = 0.2;
            network.hiddenLayer[1].Weights[2] = 0.3;
            
            network.outputLayer[0].Weights[0] = 0.2;
            network.outputLayer[0].Weights[1] = 0.3;

            network.Train(new double[] { 1.0, 0.0, 1.0 }, new double[] { 1.0 });

            network.ShowWeights();
        }
        private void yEnFonctionDeXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainMode = false;
        }

        private void prédictionDesValeursDeXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainMode = true;
        }
    }
}
