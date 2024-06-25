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
using HenonPrediction.Utils;
using HenonPrediction.Forms;

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
        private NeuralNetwork network;
        private TextBox tbHenon;
        private Series originalHenon;
        private double a, b;
        private double A
        {
            get
            {
                return a;
            }
            set
            {
                if (value != a)
                {
                    a = value;
                    truncateDatabase();
                }
            }
        }
        private double B
        {
            get
            {
                return b;
            }
            set
            {
                if (value != b)
                {
                    b = value;
                    truncateDatabase();
                }
            }
        }
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
                    showMainControls();
                }
                else
                {
                    // Secondary Chart
                    graph.ChartAreas["SecondaryArea"].Visible = true;
                    graph.Legends["SecondaryLegend"].Enabled = true;
                    graph.ChartAreas["MainArea"].Visible = false;
                    graph.Legends["MainLegend"].Enabled = false;
                    showSecondaryControls();
                }
                refresh();
            }
        }
        void showMainControls()
        {
            mainSplit.Panel2.Controls.Clear();

            TabControl tb = new TabControl();
            tb.Name = "mainTabControl";
            tb.Dock = DockStyle.Fill;

            // 2 tabpages
            TabPage tp = new TabPage();
            tp.Text = "Apprentissage";
            tp.Name = "training";
            tb.TabPages.Add(tp);

            tp = new TabPage();
            tp.Text = "Prédiction";
            tp.Name = "prediction";
            tb.TabPages.Add(tp);

            mainSplit.Panel2.Controls.Add(tb);
        }
        void showSecondaryControls()
        {
            string value = "500";
            if (tbHenon != null)
            {
                value = tbHenon.Text;
            }
            mainSplit.Panel2.Controls.Clear();

            Font font = new Font("Verdana", 10.0F, FontStyle.Bold);

            Label lbl = new Label();
            lbl.Location = new Point(20, 30);
            lbl.Font = font;
            lbl.Text = "Nombre de point à afficher";
            lbl.AutoSize = true;

            TextBox tb = new TextBox();
            tb.Location = new Point(20, 35 + lbl.Height);
            tb.Font = font;
            tb.AutoSize = true;
            tb.Text = value;
            tb.KeyPress += NumberKeyPress;

            Button btn = new Button();
            btn.AutoSize = true;
            btn.Text = "Afficher";
            btn.Location = new Point(20, 40 + lbl.Height + tb.Height);
            btn.Click += PlotHenonNumber;

            tbHenon = tb;
            mainSplit.Panel2.Controls.Add(lbl);
            mainSplit.Panel2.Controls.Add(tb);
            mainSplit.Panel2.Controls.Add(btn);
        }
        private void truncateDatabase()
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Henon";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        private void PlotHenonNumber(object sender, EventArgs args)
        {
            int termNumber;
            if (!int.TryParse(tbHenon.Text, out termNumber))
            {
                Notifications.ShowError($"{tbHenon.Text} n'est pas un nombre");
            }
            else
            {
                if (originalHenon == null)
                {
                    originalHenon = addSeries("OriginalHenon","", "SecondaryArea");
                }
                originalHenon.Points.Clear();
                originalHenon.LegendText = $"{termNumber} premières valeurs de la série de Hénon";
                
                List<HenonTerm> series = getTerm(termNumber);
                
                if (termNumber <= 20)
                {
                    originalHenon.IsValueShownAsLabel = true;
                }
                else
                {
                    originalHenon.IsValueShownAsLabel = false;
                }

                plotXYSeries(originalHenon, series);
            }
        }
        private void NumberKeyPress(object sender, KeyPressEventArgs args)
        {
            if (args.KeyChar == 13)
            {
                args.Handled = true;
                PlotHenonNumber(null, null);
            }
            else if (!"0123456789\b".Contains(args.KeyChar.ToString()))
            {
                args.Handled = true;
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
            List<HenonTerm> original500 = null;
            string connString = $"Data Source={path}";
            connection = new SQLiteConnection(connString);
            if (!File.Exists(path))
            {
                InitDb(path);
                /*
                Takens takens = new Takens();
                double[] approximation = takens.ApproximationError(HenonSeries.ExtractXValue(original500), 50, 10);
                Series takensSeries = addSeries("TakensApprox", "Erreur d'approximation moyenne après l'algorithme de Takens");
                plotXYSeries(takensSeries, approximation);
                */
            }
            /*
            Hide();
            using (ABForm form = new ABForm())
            {
                form.ShowDialog();
                if (form.DialogResult == DialogResult.OK)
                {
                    A = form.A;
                    B = form.B;
                    Show();
                    TestNeuralNetwork();
                }
                else
                {
                    Application.Exit();
                }
            }*/
            A = 1.2;
            B = 0.4;
        }
        void generateSeries(int limit)
        {
            HenonSeries hs = new HenonSeries(A, B);
            connection.Open();
            int max = 0;
            double start = 0.0;
            double end = 0.0;

            using (SQLiteCommand existing = connection.CreateCommand())
            {
                existing.CommandText = "SELECT n, x, y FROM Henon WHERE n = (SELECT MAX(n) FROM Henon LIMIT 1) LIMIT 1";

                using (var reader = existing.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        max = reader.GetInt32(0);
                        start = reader.GetDouble(1);
                        end = reader.GetDouble(2);
                    }
                }
            }

            if (limit > max)
            {
                // Insert
                SQLiteCommand generateCommand = connection.CreateCommand();
                string generateQuery = "INSERT INTO Henon(n ,x, y) VALUES ";
                limit = limit - max;
                if (max > 0)
                {
                    limit++;
                }
                List<HenonTerm> list = hs.GenerateSeries(start, end, limit);
                if (max > 0)
                {
                    // Ignore first
                    list.RemoveAt(0);
                }
                List<string> queryValues = list.Select((c, idx) => $"({max + idx + 1}, {c.X.ToString().Replace(",", ".")}, {c.Y.ToString().Replace(",", ".")})")
                                               //.Where(arr => int.Parse(arr[0].ToString()) > max)
                                               //.Select(arr => $"({arr[0]}, {arr[1].ToString().Replace(",",".")}, {arr[2].ToString().Replace(",", ".")})")
                                               .ToList();
                generateQuery += string.Join(",", queryValues);
                generateCommand.CommandText = generateQuery;
                generateCommand.ExecuteNonQuery();
                generateCommand.Dispose();
            }

            connection.Close();
        }
        List<HenonTerm> getTerm(int limit)
        {
            generateSeries(limit);
            List<HenonTerm> l = new List<HenonTerm>();
            connection.Open();
            SQLiteCommand getHenon = connection.CreateCommand();
            getHenon.CommandText = $"SELECT x, y FROM Henon ORDER BY n ASC LIMIT {limit}";
            using (var reader = getHenon.ExecuteReader())
            {
                while (reader.Read())
                {
                    double x = reader.GetDouble(0);
                    double y = reader.GetDouble(1);
                    HenonTerm ht = new HenonTerm(x, y);
                    l.Add(ht);
                }
            }
            connection.Close();
            return l;
        }
        void InitDb(string path)
        {
            string createQuery = @"
                CREATE TABLE Henon(
                    id_henon INTEGER PRIMARY KEY,
                    n INTEGER NOT NULL,
                    x REAL NOT NULL,
                    y REAL NOT NULL
                )
            ";
            /*string connString = $"Data Source={path}";
            connection = new SQLiteConnection(connString);*/
            connection.Open();
            SQLiteCommand createCommand = connection.CreateCommand();
            createCommand.CommandText = createQuery;
            createCommand.ExecuteNonQuery();
            createCommand.Dispose();
            connection.Close();
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
