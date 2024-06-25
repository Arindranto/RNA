using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HenonPrediction.Utils
{
    static class Notifications
    {
        public static DialogResult ShowNotification(string message, MessageBoxButtons btn, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            return MessageBox.Show(message, "Henon Prediction", btn, icon);
        }
        public static DialogResult ShowError(string message)
        {
            return ShowNotification(message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static DialogResult ShowWarning(string message)
        {
            return ShowNotification(message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
