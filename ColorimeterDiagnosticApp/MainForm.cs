using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ColorimeterDiagnosticApp
{
    public partial class MainForm : Form
    {
        private const int WM_DEVICECHANGE = 0x219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        private const Int32 DBT_DEVTYP_DEVICEINTERFACE = 5;
        private const Int32 DEVICE_NOTIFY_WINDOW_HANDLE = 0;

        private const int NUMBER_OF_DEVICE_PATHNAMES = 128;

        private DeviceManagement MyDeviceManagement = new DeviceManagement();

        private BackgroundWorker colorimeterConnectedBackgroundWorker;

        private Colorimeter colorimeter = null;
        //private bool colorimeterDetected = false;

        public MainForm()
        {
            InitializeComponent();

            colorimeterConnectedBackgroundWorker = new BackgroundWorker();
            colorimeterConnectedBackgroundWorker.DoWork += new DoWorkEventHandler(colorimeterFunction_DoWork);
            colorimeterConnectedBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(colorimeterFunction_RunWorkerCompleted);

        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_DEVICECHANGE)
            {
                if ((m.WParam.ToInt32() == DBT_DEVICEARRIVAL))
                {
                    listBox1.Items.Add("A device has been attached.");

                    //// Look for a colorimeter if one isn't already attached
                    if (colorimeter == null)
                    {

                        Guid colorimeterGuid = Guid.Empty;
                        Hid.HidD_GetHidGuid(ref colorimeterGuid);

                        String[] devicePathNames = MyDeviceManagement.FindDevicePathsFromGuid(colorimeterGuid, NUMBER_OF_DEVICE_PATHNAMES);

                        var devicePath = Hid.FindDevicePath(Colorimeter.COLORIMETER_PRODUCT_ID, Colorimeter.COLORIMETER_VENDOR_ID, devicePathNames);
                        var readHandle = FileIO.CreateFile(devicePath, FileIO.GENERIC_READ, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, FileIO.FILE_FLAG_OVERLAPPED, 0);

                        //we have found a viable colorimeter with a working path and handle
                        if (!devicePath.Equals(null) && !readHandle.IsInvalid)
                        {
                            //colorimeterDetected = true;
                            colorimeter = new Colorimeter(readHandle, devicePath);

                            // Start checkColorimeterDeviceStateBackgroundWorker
                            // When the Background Worker completes, a different backgroundworker
                            // is called to get the firmware and test file versions
                            var request = new ColorimeterRequest()
                            {
                                ColorimeterRequestType = ColorimeterRequestType.FirmwareVersion | ColorimeterRequestType.TestFileVersion | ColorimeterRequestType.DeviceState
                            };
                            if (!colorimeterConnectedBackgroundWorker.IsBusy)
                            {
                                colorimeterConnectedBackgroundWorker.RunWorkerAsync(request);
                            }                   

                        }
                    }

                    ////  Find out if it's the device we're communicating with.
                    if (MyDeviceManagement.DeviceNameMatch(m, colorimeter.colorimeterPathName))
                    {
                        listBox1.Items.Add("Colorimeter attached.");
                    }
                }

                //  If WParam contains DBT_DEVICEREMOVAL, a device has been removed.  We only care if the removed device is a colorimeter we are already talking to
                else if ((m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)  && colorimeter != null)
                {

                    if (MyDeviceManagement.DeviceNameMatch(m, colorimeter.colorimeterPathName))
                    {
                        listBox1.Items.Add("Colorimeter removed.");

                        //colorimeterDetected = false;
                        colorimeter = null;
                    }

                }

                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Guid hidGuid = Guid.Empty;
            Hid.HidD_GetHidGuid(ref hidGuid);

            IntPtr deviceNotificationHandle;
            deviceNotificationHandle = IntPtr.Zero;

            RegisterForDeviceNotifications("", this.Handle, hidGuid, ref deviceNotificationHandle);

        }


        [StructLayout(LayoutKind.Sequential)]
        private class DEV_BROADCAST_DEVICEINTERFACE
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            internal Guid dbcc_classguid;
            internal Int16 dbcc_name;
        }

        ///  <summary>
        ///  Requests to receive a notification when a device is attached or removed.
        ///  </summary>
        ///  
        ///  <param name="devicePathName"> handle to a device. </param>
        ///  <param name="formHandle"> handle to the window that will receive device events. </param>
        ///  <param name="classGuid"> device interface GUID. </param>
        ///  <param name="deviceNotificationHandle"> returned device notification handle. </param>
        ///  
        ///  <returns>
        ///  True on success.
        ///  </returns>
        ///  
        private Boolean RegisterForDeviceNotifications(String devicePathName, IntPtr formHandle, Guid classGuid, ref IntPtr deviceNotificationHandle)
        {
            // A DEV_BROADCAST_DEVICEINTERFACE header holds information about the request.

            DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
            IntPtr devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;
            Int32 size = 0;

            try
            {
                // Set the parameters in the DEV_BROADCAST_DEVICEINTERFACE structure.

                // Set the size.

                size = Marshal.SizeOf(devBroadcastDeviceInterface);
                devBroadcastDeviceInterface.dbcc_size = size;

                // Request to receive notifications about a class of devices.

                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;

                devBroadcastDeviceInterface.dbcc_reserved = 0;

                // Specify the interface class to receive notifications about.

                devBroadcastDeviceInterface.dbcc_classguid = classGuid;

                // Allocate memory for the buffer that holds the DEV_BROADCAST_DEVICEINTERFACE structure.

                devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(size);

                // Copy the DEV_BROADCAST_DEVICEINTERFACE structure to the buffer.
                // Set fDeleteOld True to prevent memory leaks.

                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

                // ***
                //  API function

                //  summary
                //  Request to receive notification messages when a device in an interface class
                //  is attached or removed.

                //  parameters 
                //  Handle to the window that will receive device events.
                //  Pointer to a DEV_BROADCAST_DEVICEINTERFACE to specify the type of 
                //  device to send notifications for.
                //  DEVICE_NOTIFY_WINDOW_HANDLE indicates the handle is a window handle.

                //  Returns
                //  Device notification handle or NULL on failure.
                // ***

                deviceNotificationHandle = RegisterDeviceNotification(formHandle, devBroadcastDeviceInterfaceBuffer, DEVICE_NOTIFY_WINDOW_HANDLE);

                // Marshal data from the unmanaged block devBroadcastDeviceInterfaceBuffer to
                // the managed object devBroadcastDeviceInterface

                Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);

                if ((deviceNotificationHandle.ToInt32() == IntPtr.Zero.ToInt32()))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (devBroadcastDeviceInterfaceBuffer != IntPtr.Zero)
                {
                    // Free the memory allocated previously by AllocHGlobal.

                    Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBuffer);
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);


        private void buttonClickHandler(object sender, EventArgs e)
        {
            if (colorimeter != null)
            {
                var request = new ColorimeterRequest();

                switch (((Button)sender).Name)
                {
                    case "SaveUserTestsFileButton":
                        request.ColorimeterRequestType = ColorimeterRequestType.GetUserTestsFile;
                        break;
                    case "SaveTestResultsButton":
                        request.ColorimeterRequestType = ColorimeterRequestType.GetTestResults;
                        break;
                    case "UpdateUserTestsFileButton":
                        request.ColorimeterRequestType = ColorimeterRequestType.UpdateUserTestsFile;
                        break;
                    case "UpdateTaylorTestsFileButton":
                        request.ColorimeterRequestType = ColorimeterRequestType.UpdateTaylorTestsFile;
                        break;
                    case "UpdateFirmwareButton":
                        request.ColorimeterRequestType = ColorimeterRequestType.UpdateFirmware;
                        break;
                    default:
                        break;
                }

                if (!colorimeterConnectedBackgroundWorker.IsBusy)
                {
                    colorimeterConnectedBackgroundWorker.RunWorkerAsync(request);
                }

            }
        }

        private void browseSaveFile_Click(object sender, EventArgs e)
        {
            // Create an Open File dialog instance
            SaveFileDialog saveFile = new SaveFileDialog();

            String textBoxName = "";
            // Configure Open File dialog box
            switch (((Button)sender).Name)
            {
                case "SaveTestResultsBrowseButton":
                    // Configure Open File dialog box
                    saveFile.Title = "Test results file...";
                    saveFile.Filter = "Binary Files (*.bin)|*.bin|All Files|*.*";
                    textBoxName = "SaveTestResultsPathTextBox";
                    break;
                case "SaveUserTestsFileBrowseButton":
                    // Configure Open File dialog box
                    saveFile.Title = "Save Output File...";
                    saveFile.Filter = "Test Configuration Files (*.tcf)|*.tcf|All Files|*.*";
                    textBoxName = "SaveUserTestsPathTextBox";
                    break;
                default:
                    break;
            }
            // If user click OK, populate filename text box with the selection
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                this.Controls[textBoxName].Text = saveFile.FileName;
            }
        }

        private void browseOpenFile_Click(object sender, EventArgs e)
        {
            // Create an Open File dialog instance
            OpenFileDialog openFile = new OpenFileDialog();

            String textBoxName = "";
            // Configure Open File dialog box
            switch (((Button)sender).Name)
            {
                case "UpdateUserTestsFileBrowseButton":
                    // Configure Open File dialog box
                    openFile.Title = "Browse for User Test Configuration File...";
                    openFile.Filter = "Test Configuration Files (*.tcf)|*.tcf|All Files|*.*";
                    textBoxName = "UpdateUserTestsPathTextBox";
                    break;
                case "UpdateTaylorTestsFileBrowseButton":
                    textBoxName = "UpdatTaylorTestsPathTextBox";
                    break;
                case "UpdateFirmwareBrowseButton":
                    textBoxName = "UpdateFirmwarePathTextBox";
                    break;
                default:
                    break;
            }
            // If user click OK, populate filename text box with the selection
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                this.Controls[textBoxName].Text = openFile.FileName;
            }
        }

        private void colorimeterFunction_DoWork(object sender, DoWorkEventArgs e)
        {

            var incomingRequest = (ColorimeterRequest)e.Argument;

            ColorimeterResponse outgoingResponse = new ColorimeterResponse();

            // refactor ColorimeterRequest to use ColorimeterRequestType instead of multiple 
            if (incomingRequest.ColorimeterRequestType.HasFlag(ColorimeterRequestType.FirmwareVersion))
            {
                outgoingResponse.FirmwareVersion = colorimeter.GetColorimeterVersion();
            }

            if (incomingRequest.ColorimeterRequestType.HasFlag(ColorimeterRequestType.TestFileVersion))
            {

                outgoingResponse.TestFileVersion = colorimeter.GetTaylorTestFileVersion();
                
            }

            if (incomingRequest.ColorimeterRequestType.HasFlag(ColorimeterRequestType.DeviceState))
            {
                //don't need to return a value Colorimeter keeps its own reference to its device state
                colorimeter.GetDeviceState();
            }

            //if (incomingRequest.ColorimeterRequestType.HasFlag(ColorimeterRequestType.GetUserTestsFile))
            //{
            //    //outgoingResponse.responseInfo.Add("you requested the user tests file");

            //    ReceiveFile(SaveUserTestsPathTextBox.Text, OutCmd.SendUserTests, outgoingResponse);
            //}
            //if (incomingRequest.ColorimeterRequestType.HasFlag(ColorimeterRequestType.GetTestResults))
            //{
            //    outgoingResponse.responseInfo.Add("you requested test results");
            //    ReceiveFile(SaveTestResultsPathTextBox.Text, OutCmd.SendTestResults, outgoingResponse);
            //}
            //if (incomingRequest.ColorimeterRequestType.HasFlag(ColorimeterRequestType.UpdateUserTestsFile))
            //{
            //    if(File.Exists(UpdateUserTestsPathTextBox.Text))
            //    {
            //        outgoingResponse.responseInfo.Add("you requested update user tests file");
            //        SendTestFile(UpdateUserTestsPathTextBox.Text, OutCmd.UserTestStart, outgoingResponse);
            //    }
            //}


            //assign that variable to e.Result
            e.Result = outgoingResponse;

        }
        private void colorimeterFunction_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var ds = colorimeter.DeviceState;
            var response = (ColorimeterResponse)e.Result;

            if (!response.FirmwareVersion.Equals(""))
            {
                firmwareVersionTextBox.Text = response.FirmwareVersion;
            }

            if (!response.TestFileVersion.Equals(""))
            {
                testFileVersionTextBox.Text = response.TestFileVersion;
            }

            foreach (string item in response.responseInfo)
            {
                listBox1.Items.Add($"{item}");
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }

        }

    }
}
