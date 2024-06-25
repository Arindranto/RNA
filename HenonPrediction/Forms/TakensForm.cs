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
    public partial class TakensForm : Form
    {
        public int N, T, Size;
        public TakensForm()
        {
            InitializeComponent();
        }

        private void tbN_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!"0123456789\b".Contains(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int d;

            if (!int.TryParse(tbTaille.Text, out d))
            {
                Notifications.ShowWarning($"{tbN.Text} n'est pas un nombre valide");
                return;
            }
            Size = d;
            if (!int.TryParse(tbN.Text, out d))
            {
                Notifications.ShowWarning($"{tbN.Text} n'est pas un nombre valide");
                return;
            }
            N = d;
            if (!int.TryParse(tbT.Text, out d))
            {
                Notifications.ShowWarning($"{tbT.Text} n'est pas un nombre valide");
                return;
            }
            T = d;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
