using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HenonPrediction
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            HenonSeries hs = new HenonSeries(1.4, 0.3);
            hs.GenerateSeries(0, 0, 500);
        }
    }
}
