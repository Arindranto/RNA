using HenonPrediction.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HenonPrediction.Forms
{
    public partial class ABForm : Form
    {
        public double A, B;
        public ABForm()
        {
            InitializeComponent();
        }

        private void tbB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!"0123456789.".Contains(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            double d;
            if (!double.TryParse(tbA.Text, NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture("en-GB"), out d))
            {
                Notifications.ShowWarning($"{tbA.Text} n'est pas un nombre valide");
                return;
            }
            A = d;
            if (!double.TryParse(tbB.Text, NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture("en-GB"), out d))
            {
                Notifications.ShowWarning($"{tbB.Text} n'est pas un nombre valide");
                return;
            }
            B = d;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void tbB_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            double d;
            /*if (!double.TryParse(tb.Text, out d))
            {
                Notifications.ShowWarning($"{tb.Text} n'est pas un nombre valide");
            }*/
        }
    }
}
