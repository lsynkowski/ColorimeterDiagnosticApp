using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ColorimeterDiagnosticApp
{
    public partial class Form1 : Form
    {
        private const int WM_DEVICECHANGE = 0x219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        private const Int32 DBT_DEVTYP_DEVICEINTERFACE = 5;
        private const Int32 DEVICE_NOTIFY_WINDOW_HANDLE = 0;

        //variables from Colorimeter Class
        private Boolean colorimeterDetected = false;
        private String colorimeterPathName;
        private String hidUsage;
        private SafeFileHandle hidHandle;
        private SafeFileHandle readHandle;
        private SafeFileHandle writeHandle;
        private byte[] inputBuffer;
        private Boolean newInputData;

        private string firmwareVersion;
        private String testFileVersion;
        private DeviceStates deviceState;


        //
        private Hid MyHid = new Hid();
        private DeviceManagement MyDeviceManagement = new DeviceManagement();

        private BackgroundWorker checkColorimeterConnectionBackgroundWorker;

        public Form1()
        {
            InitializeComponent();

            checkColorimeterConnectionBackgroundWorker = new BackgroundWorker();
            checkColorimeterConnectionBackgroundWorker.DoWork += new DoWorkEventHandler(checkColorimeterConnectionBackgroundWorker_DoWork);
            checkColorimeterConnectionBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(checkColorimeterConnectionBackgroundWorker_RunWorkerCompleted);
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
                    if (colorimeterDetected == false)
                    {
                        if (FindDevice() == true)
                        {
                            listBox1.Items.Add("Found colorimeter");
                        }
                    }

                    ////  Find out if it's the device we're communicating with.
                    if (MyDeviceManagement.DeviceNameMatch(m, colorimeterPathName))
                    {
                        listBox1.Items.Add("My device attached.");
                    }
                }

                //  If WParam contains DBT_DEVICEREMOVAL, a device has been removed.
                else if ((m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE))
                {
                    listBox1.Items.Add("A device has been removed.");

                    //  Find out if it's the device we're communicating with.
                    if (MyDeviceManagement.DeviceNameMatch(m, colorimeterPathName))
                    {
                        listBox1.Items.Add("My device removed.");

                        //  Set MyDeviceDetected False so on the next data-transfer attempt,
                        //  FindTheHid() will be called to look for the device 
                        //  and get a new handle.
                        colorimeterDetected = false;
                    }



                }


                listBox1.Items.Add($"WParam : { m.WParam }   |LParam : { m.LParam }  |Result : { m.Result }  |MHWnd : { m.HWnd }");
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Guid hidGuid = Guid.Empty;
            HidD_GetHidGuid(ref hidGuid);

            IntPtr deviceNotificationHandle;
            deviceNotificationHandle = IntPtr.Zero;

            RegisterForDeviceNotifications("", this.Handle, hidGuid, ref deviceNotificationHandle);

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

        /// <summary>
        /// Locates a colorimeter using HID device drivers
        /// </summary>
        /// 
        /// <returns>True if device was found, False if device was not found</returns>
        public Boolean FindDevice()
        {
            Boolean deviceFound = false;
            String functionName = "";
            String[] devicePathName = new String[128];
            Guid colorimeterGuid = Guid.Empty;
            Int32 memberIndex = 0;
            Int16 myProductID = 0x0003;
            Int16 myVendorID = 0x2229;
            Boolean success = false;

            try
            {
                HidD_GetHidGuid(ref colorimeterGuid);
                listBox1.Items.Add("   *** GUID for system HIDs: " + colorimeterGuid.ToString());

                deviceFound = MyDeviceManagement.FindDeviceFromGuid(colorimeterGuid, ref devicePathName);

                if (deviceFound)
                {
                    memberIndex = 0;

                    do
                    {
                        //  ***
                        //  API function:
                        //  CreateFile

                        //  Purpose:
                        //  Retrieves a handle to a device.

                        //  Accepts:
                        //  A device path name returned by SetupDiGetDeviceInterfaceDetail
                        //  The type of access requested (read/write).
                        //  FILE_SHARE attributes to allow other processes to access the device while this handle is open.
                        //  A Security structure or Nothing. 
                        //  A creation disposition value. Use OPEN_EXISTING for devices.
                        //  Flags and attributes for files. Not used for devices.
                        //  Handle to a template file. Not used.

                        //  Returns: a handle without read or write access.
                        //  This enables obtaining information about all HIDs, even system
                        //  keyboards and mice. 
                        //  Separate handles are used for reading and writing.
                        //  ***

                        hidHandle = FileIO.CreateFile(devicePathName[memberIndex], 0, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                        if (!hidHandle.IsInvalid)
                        {
                            //  The returned handle is valid, 
                            //  so find out if this is the device we're looking for.

                            //  Set the Size property of DeviceAttributes to the number of bytes in the structure.
                            MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);

                            //  ***
                            //  API function:
                            //  HidD_GetAttributes

                            //  Purpose:
                            //  Retrieves a HIDD_ATTRIBUTES structure containing the Vendor ID, 
                            //  Product ID, and Product Version Number for a device.

                            //  Accepts:
                            //  A handle returned by CreateFile.
                            //  A pointer to receive a HIDD_ATTRIBUTES structure.

                            //  Returns:
                            //  True on success, False on failure.
                            //  ***                            
                            success = Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);

                            if (success)
                            {
                                //  Find out if the device matches the one we're looking for.
                                if ((MyHid.DeviceAttributes.VendorID == myVendorID) && (MyHid.DeviceAttributes.ProductID == myProductID))
                                {
                                    listBox1.Items.Add("  My device detected");

                                    //  Display the information in form's list box.
                                    listBox1.Items.Add("Device detected:");
                                    listBox1.Items.Add("  Vendor ID = " + Convert.ToString(MyHid.DeviceAttributes.VendorID, 16));
                                    listBox1.Items.Add("  Product ID = " + Convert.ToString(MyHid.DeviceAttributes.ProductID, 16));

                                    colorimeterDetected = true;

                                    //  Save the DevicePathName for OnDeviceChange().
                                    colorimeterPathName = devicePathName[memberIndex];
                                }
                                else
                                {
                                    //  It's not a match, so close the handle.
                                    colorimeterDetected = false;
                                    hidHandle.Close();
                                }
                            }
                            else
                            {
                                //  There was a problem in retrieving the information.
                                listBox1.Items.Add("  Error in filling HIDD_ATTRIBUTES structure.");
                                colorimeterDetected = false;
                                hidHandle.Close();
                            }
                        }

                        //  Keep looking until we find the device or there are no devices left to examine.
                        memberIndex = memberIndex + 1;
                    }
                    while (!((colorimeterDetected | (memberIndex == devicePathName.Length))));
                }

                if (colorimeterDetected)
                {
                    //  The device was detected.
                    //  Register main form to receive notifications if the device is removed or attached.
                    // success = this.MyDeviceManagement.RegisterForDeviceNotifications(this.colorimeterPathName, this.mainFormHandle, colorimeterGuid, ref this.deviceNotificationHandle);

                    // Debug.WriteLine("RegisterForDeviceNotifications = " + success);

                    //  Learn the capabilities of the device.
                    MyHid.Capabilities = MyHid.GetDeviceCapabilities(hidHandle);

                    if (success)
                    {
                        //  Find out if the device is a system mouse or keyboard.
                        hidUsage = MyHid.GetHidUsage(MyHid.Capabilities);

                        //  Get handles to use in requesting Input and Output reports.
                        readHandle = FileIO.CreateFile(colorimeterPathName, FileIO.GENERIC_READ, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, FileIO.FILE_FLAG_OVERLAPPED, 0);

                        functionName = "CreateFile, ReadHandle";
                        listBox1.Items.Add(functionName);
                        listBox1.Items.Add("  Returned handle: " + readHandle.ToString());

                        // Initialize input report buffer
                        inputBuffer = new byte[MyHid.Capabilities.InputReportByteLength];
                        newInputData = false;

                        if (readHandle.IsInvalid)
                        {
                            listBox1.Items.Add("The device is a system " + hidUsage + ".");
                            listBox1.Items.Add("Windows 2000 and Windows XP obtain exclusive access to Input and Output reports for this devices.");
                            listBox1.Items.Add("Applications can access Feature reports only.");
                        }
                        else
                        {
                            writeHandle = FileIO.CreateFile(colorimeterPathName, FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                            functionName = "CreateFile, WriteHandle";
                            listBox1.Items.Add(functionName);
                            listBox1.Items.Add("  Returned handle: " + writeHandle.ToString());

                            //  Flush any waiting reports in the input buffer. (optional)
                            MyHid.FlushQueue(readHandle);

                            // Colorimeter has been found, query it to determine if it's running
                            // bootloader or main firmware
                            //if (this.QueryDeviceState() == true)
                            //{
                            //    // This could be an error, if we get here the device wasn't found
                            //    // todo: might want to check for DeviceState.Unknown also, that would be a problem
                            //}

                            //if (deviceState == DeviceStates.MainFwRunning)
                            //{
                            //    if (QueryFirmwareVersion())
                            //    {
                            //        firmwareVersionTextBox.Text = firmwareVersion;
                            //    }
                            //    else
                            //    {
                            //        firmwareVersionTextBox.Text = "Failed";
                            //    }

                            //    if (QueryTestFileVersion())
                            //    {
                            //        testFileVersionTextBox.Text = testFileVersion;
                            //    }
                            //    else
                            //    {
                            //        testFileVersionTextBox.Text = "Failed";
                            //    }
                            //}
                        }
                    }
                }
                else
                {
                    //  The device wasn't detected.
                    listBox1.Items.Add("Device not found.");
                    listBox1.Items.Add(" Device not found.");
                }
                return colorimeterDetected;
            }
            catch (Exception ex)
            {
                //frmMain.DisplayException("Colorimeter.FindDevice()", ex);
                throw;
            }
        }
        public enum QueryType
        {
            FirmwareVersion,
            TestFileVersion,
            DeviceState

        }

        private enum DeviceStates : byte
        {
            Unknown,
            BootloaderRunning,
            MainFwRunning,
        };

        public Boolean Query(QueryType queryType, ColorimeterResponse response)
        {
            byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];
            Boolean retval = false;

            if (colorimeterDetected)
            {
                // Build output packet
                outputBuffer[0] = 0;
                // outputBuffer[1] different for each QueryType
                switch (queryType)
                {
                    case QueryType.FirmwareVersion:
                        outputBuffer[1] = Convert.ToByte(OutCmd.SendFwVers);//the value of the query firmware vers
                        break;
                    case QueryType.TestFileVersion:
                        outputBuffer[1] = Convert.ToByte(OutCmd.SendTestFileVers);//the value of the query test file vers
                        break;
                    case QueryType.DeviceState:
                        outputBuffer[1] = Convert.ToByte(OutCmd.QueryFirmwareState);//the value of the query firmware state
                        break;
                }
                outputBuffer[2] = 0;
                Write(ref outputBuffer, response);
                // Setup to read response
                if (SetupRead(response) == true)
                {
                    switch (queryType)
                    {
                        case QueryType.FirmwareVersion:
                            retval = QueryFirmwareVersion(response);
                            break;
                        case QueryType.TestFileVersion:
                            retval = QueryTestFileVersion(response);
                            break;
                        case QueryType.DeviceState:
                            retval = QueryDeviceState();
                            break;
                    }
                }
                else
                {
                    retval = false;
                }
            }
            return retval;
        }

        public Boolean QueryFirmwareVersion(ColorimeterResponse response)
        {
            //(expecting ACK) from response
            if (WaitForResponse() == InCmd.FirmwareVersion)//FirmwareVersion = 10,
            {
                firmwareVersion = Encoding.GetEncoding("iso-8859-1").GetString(inputBuffer, 3, inputBuffer[2]);
                //listBox1.Items.Add($"Firmware version: { firmwareVersion }");
                response.responseInfo.Add($"Firmware version: { firmwareVersion }");
                response.firmwareVersion = firmwareVersion;
                return true;
            }
            return false;
        }

        public Boolean QueryTestFileVersion(ColorimeterResponse response)
        {
            //(expecting ACK) from response
            if (WaitForResponse() == InCmd.TestFileVersion)
            {
                testFileVersion = Encoding.GetEncoding("iso-8859-1").GetString(inputBuffer, 3, inputBuffer[2]);
                response.responseInfo.Add($"Test File version: { testFileVersion }");
                response.testFileVersion = testFileVersion;
                return true;
            }
            return false;
        }

        public Boolean QueryDeviceState()
        {
            // Retrieve response and set device state flag appropriately
            switch (WaitForResponse())
            {
                case InCmd.BootloaderRunning:
                    deviceState = DeviceStates.BootloaderRunning;
                    break;

                case InCmd.MainFwRunning:
                    deviceState = DeviceStates.MainFwRunning;
                    break;

                default:
                    deviceState = DeviceStates.Unknown;
                    break;
            }
            return true;
        }


        /// <summary>
        /// Write an output report to the colorimeter
        /// </summary> 
        /// 
        /// <param name="outputBuffer">Reference to byte array containing output data</param>\
        /// <returns>True if write is successful, false otherwise</returns>
        private Boolean Write(ref byte[] outputBuffer, ColorimeterResponse response)
        {
            Boolean success = false;

            try
            {
                // If colorimeter is connected, write to device
                if ((colorimeterDetected == true))
                {
                    // Check write handle
                    if (!writeHandle.IsInvalid)
                    {
                        if (MyHid.Capabilities.OutputReportByteLength > 0)
                        {
                            Hid.OutputReportViaInterruptTransfer outputReport = new Hid.OutputReportViaInterruptTransfer();
                            success = outputReport.Write(outputBuffer, writeHandle);

                            if (!success)
                            {
                                response.responseInfo.Add("Write to colorimeter failed.");
                            }
                        }
                        else
                        {
                            response.responseInfo.Add("This HID device doesn't have an Output report.");
                        }
                    }
                    else
                    {
                        response.responseInfo.Add("Invalid write handle.");
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                response.responseInfo.Add($"Colorimeter.Write() {ex}");
                throw;
            }
        }

        ///  <summary>
        ///  Initiates exchanging reports. 
        ///  The application sends a report and requests to read a report.
        ///  </summary>
        ///  
        ///  <returns>True if read successful, false otherwise</returns>
        public Boolean SetupRead(ColorimeterResponse response)
        {
            Boolean success = false;

            try
            {
                if (colorimeterDetected == true)
                {
                    // Check read handle
                    if (!readHandle.IsInvalid)
                    {
                        if (MyHid.Capabilities.InputReportByteLength > 0)
                        {
                            Hid.InputReportViaInterruptTransfer myInputReport = new Hid.InputReportViaInterruptTransfer();
                            myInputReport.Read(hidHandle, readHandle, writeHandle, ref colorimeterDetected, ref inputBuffer, ref success);
                            InputReportReceived(ref inputBuffer, success, response);

                            // IAsyncResult ar = null;
                            // Hid.InputReportViaInterruptTransfer myInputReport = new Hid.InputReportViaInterruptTransfer();
                            // ReadInputReportDelegate MyReadInputReportDelegate = new ReadInputReportDelegate(myInputReport.Read);

                            // ar = MyReadInputReportDelegate.BeginInvoke(hidHandle, readHandle, writeHandle, ref ColorimeterDetected, ref inputBuffer, ref success, new AsyncCallback(InputReportReceived), MyReadInputReportDelegate);
                        }
                        else
                        {
                            response.responseInfo.Add("This HID device doesn't have an Input report.");
                        }
                    }
                    else
                    {
                        response.responseInfo.Add("Invalid read handle.");
                    }
                }
                else
                {
                    response.responseInfo.Add("Colorimeter not detected");
                }

                return success;
            }
            catch (Exception ex)
            {
                response.responseInfo.Add($"Colorimeter.SetupRead() {ex}");
                throw;
            }
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

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern void HidD_GetHidGuid(ref System.Guid HidGuid);

        /// <summary>
        /// Wait for input report from colorimeter.
        /// </summary>
        /// 
        /// <returns>The response from colorimeter (ACK or NAK)</returns>
        private InCmd WaitForResponse()
        {
            // TODO: we need a timeout here
            while (!newInputData) ;

            // Read response from colorimeter.  Figure out how to pend on input report reception
            return (InCmd)inputBuffer[1];
        }

        private void InputReportReceived(ref Byte[] inputReportBuffer, Boolean success, ColorimeterResponse response)
        {
            Int32 count = 0;

            if (success)
            {
                for (count = 0; count < inputReportBuffer.Length; count++)
                {
                    //  Copy input data to buffer
                    inputBuffer[count] = inputReportBuffer[count];
                }

                newInputData = true;
            }
            else
            {
                response.responseInfo.Add("The attempt to read an Input report has failed");
            }
        }


        public void ReceiveTestFile(string receivedTestFileName, ColorimeterResponse colorimeterResponse)
        {
            bool done;
            byte checksum = 0;
            BinaryWriter outputFileWriter = null;

            try
            {
                // If the output file exists, delete it
                if (File.Exists(receivedTestFileName))
                {
                    File.Delete(receivedTestFileName);
                }

                // Open the output file and create an BinaryWriter object so we can write to it
                outputFileWriter = new BinaryWriter(File.Open(receivedTestFileName, FileMode.OpenOrCreate, FileAccess.Write));

                // Request the user test file from the colorimeter
                StartSendingUserTestFile(colorimeterResponse);

                // Wait for ACK
                SetupRead(colorimeterResponse);
                if (WaitForResponse() == InCmd.ACK)
                {
                    // Loop, receiving test data until colorimeter tells us we're done
                    done = false;
                    while (!done)
                    {
                        // ACK received, now wait for test data
                        SetupRead(colorimeterResponse);
                        if (WaitForInputReport())
                        {
                            // Verify that the data we received is user test data
                            if (inputBuffer[1] == (byte)InCmd.TestData)
                            {
                                // Update our checksum
                                for (int i = 0; i < inputBuffer[2]; i++)
                                {
                                    checksum ^= inputBuffer[3 + i];
                                }

                                // If input report received successfully, write data
                                outputFileWriter.Write(inputBuffer, 3, inputBuffer[2]);
                                SendACK(colorimeterResponse);
                            }
                            else if (inputBuffer[1] == (byte)InCmd.DataSendComplete)
                            {
                                done = true;
                                if (checksum == inputBuffer[3])
                                {
                                    SendACK(colorimeterResponse);
                                }
                                else
                                {
                                    SendNAK(colorimeterResponse);
                                    colorimeterResponse.responseInfo.Add("Checksums do not match! Checksum Error");
                                }
                            }
                            else
                            {
                                SendNAK(colorimeterResponse);
                                done = true;
                                colorimeterResponse.responseInfo.Add("Test file transfer failed Error");
                            }
                        }
                        else
                        {
                            done = true;
                            colorimeterResponse.responseInfo.Add("Test file transfer failed. Error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                colorimeterResponse.responseInfo.Add($"{ex.Message} Exception occurred!");
            }

            finally
            {
                if (outputFileWriter != null)
                    outputFileWriter.Close();
            }
        }

        private void StartSendingUserTestFile(ColorimeterResponse colorimeterResponse)
        {
            byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];

            outputBuffer[0] = 0;
            outputBuffer[1] = Convert.ToByte(OutCmd.SendUserTests);
            outputBuffer[2] = 0;
            Write(ref outputBuffer, colorimeterResponse);

        }

        public void SendACK(ColorimeterResponse colorimeterResponse)
        {
            byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];

                outputBuffer[0] = 0;
                outputBuffer[1] = Convert.ToByte(OutCmd.ACK);
                outputBuffer[2] = 0;
                Write(ref outputBuffer, colorimeterResponse);
        }

        public void SendNAK(ColorimeterResponse colorimeterResponse)
        {
            byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];

            outputBuffer[0] = 0;
            outputBuffer[1] = Convert.ToByte(OutCmd.NAK);
            outputBuffer[2] = 0;
            Write(ref outputBuffer, colorimeterResponse);
        }

        private bool WaitForInputReport()
        {
            while (!newInputData) ;

            return true;
        }

        private enum InCmd : byte
        {
            none,

            ACK,
            NAK,

            BootloaderRunning = 5,
            MainFwRunning,

            FirmwareVersion = 10,
            TestFileVersion = 11,

            TestData = 15,
            ResultsData,
            DataSendComplete,
            DataSendFailed,
        };


        // Data packet command bytes
        public enum OutCmd : byte
        {
            none,

            ACK = 1,
            NAK,

            SendFwVers = 5,
            SendTestFileVers,
            SendUserTests,
            SendTestResults,

            StartFAT = 10,

            QueryFirmwareState = 15,
            VectorBootLoader,

            FirmwareStart = 20,
            FirmwareData,
            FirmwareComplete,

            TaylorTestStart = 30,
            UserTestStart,
            TestData,
            TestComplete,

            DeleteTestResults = 40,
        };


        private void buttonClickHandler(object sender, EventArgs e)
        {
            if (colorimeterDetected)
            {
                var request = new ColorimeterRequest();

                switch (((Button)sender).Name)
                {
                    case "GetUserTestsFileButton":
                        request.colorimeterRequestType = ColorimeterRequestType.GetUserTestsFile;
                        break;
                    default:
                        break;
                }


                if (!checkColorimeterConnectionBackgroundWorker.IsBusy)
                {
                    checkColorimeterConnectionBackgroundWorker.RunWorkerAsync(request);
                }

            }
        }

        private void browseUserTestSaveButton_Click(object sender, EventArgs e)
        {
            // Create an Open File dialog instance
            SaveFileDialog saveFile = new SaveFileDialog();

            // Configure Open File dialog box
            saveFile.Title = "Save Output File...";
            saveFile.Filter = "Test Configuration Files (*.tcf)|*.tcf|All Files|*.*";

            // If user click OK, populate filename text box with the selection
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                saveUserTestPathTextBox.Text = saveFile.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorimeterDetected)
            {
                var request = new ColorimeterRequest()
                {
                    colorimeterRequestType = ColorimeterRequestType.FirmwareVersion
                };

                if (!checkColorimeterConnectionBackgroundWorker.IsBusy)
                {
                    checkColorimeterConnectionBackgroundWorker.RunWorkerAsync(request);
                }

            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (colorimeterDetected)
            {
                var request = new ColorimeterRequest()
                {
                    colorimeterRequestType = ColorimeterRequestType.TestFileVersion
                };
                if (!checkColorimeterConnectionBackgroundWorker.IsBusy)
                {
                    checkColorimeterConnectionBackgroundWorker.RunWorkerAsync(request);
                }
            }
        }


        private void checkColorimeterConnectionBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            //here is how you get the arguments (incoming object)
            var incomingRequest = (ColorimeterRequest)e.Argument;

            var outgoingResponse = new ColorimeterResponse();

            switch (incomingRequest.colorimeterRequestType)
            {
                case ColorimeterRequestType.Transfer:
                    outgoingResponse.responseInfo.Add("you requested a transfer");
                    break;
                case ColorimeterRequestType.FirmwareVersion:
                    outgoingResponse.responseInfo.Add("you requested the firmware version");
                    Query(QueryType.FirmwareVersion, outgoingResponse);
                    break;
                case ColorimeterRequestType.TestFileVersion:
                    outgoingResponse.responseInfo.Add("you requested the test file version");
                    Query(QueryType.TestFileVersion, outgoingResponse);
                    break;
                case ColorimeterRequestType.GetUserTestsFile:
                    outgoingResponse.responseInfo.Add("you requested the user tests file");
                    ReceiveTestFile(saveUserTestPathTextBox.Text, outgoingResponse);
                    break;
                default:
                    outgoingResponse.responseInfo.Add("you requested something else");
                    break;
            }

            //assign that variable to e.Result
            e.Result = outgoingResponse;

        }

        private void checkColorimeterConnectionBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            var response = (ColorimeterResponse)e.Result;

            if (!response.firmwareVersion.Equals(""))
            {
                textBox1.Text = response.firmwareVersion;
            }

            if (!response.testFileVersion.Equals(""))
            {
                textBox2.Text = response.testFileVersion;
            }

            foreach (string item in response.responseInfo)
            {
                listBox1.Items.Add($"{item}");
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }

        }
    }
}
