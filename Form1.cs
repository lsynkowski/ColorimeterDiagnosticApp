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
            label2.Text = "No message received";
        }

        ///  <summary>
        ///   Overrides WndProc to enable checking for and handling WM_DEVICECHANGE messages.
        ///  </summary>
        ///  
        ///  <param name="m"> a Windows Message </param>
        protected override void WndProc(ref Message m)
        {
            //always do the base
            base.WndProc(ref m);
            var inComingMessage = m;
            //Once the colorimeter is connected we don't handle any more messages
            //unless we want to handle a remove it message
            //if (!colorimeterConnected && m.Msg == WM_DEVICECHANGE)
            if (m.Msg == DeviceManagement.WM_DEVICECHANGE)
            {
                if ((inComingMessage.WParam.ToInt32() == DeviceManagement.DBT_DEVICEARRIVAL))
                {
                    label2.Text = "Device attached.";
                }
            }
        }





        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

    }
}