using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColorimeterDiagnosticApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            //always do the base
            base.WndProc(ref m);

            //Once the colorimeter is connected we don't handle any more messages
            //unless we want to handle a remove it message
            //if (!colorimeterConnected && m.Msg == WM_DEVICECHANGE)
            if (m.Msg == DeviceManagement.WM_DEVICECHANGE)
            {
                if (!checkColorimeterConnectionBackgroundWorker.IsBusy)
                    checkColorimeterConnectionBackgroundWorker.RunWorkerAsync(m);

            }
        }
    }
}
