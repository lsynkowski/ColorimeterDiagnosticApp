using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    class Colorimeter
    {
        public const short COLORIMETER_PRODUCT_ID = 0x0003;
        public const short COLORIMETER_VENDOR_ID = 0x2229;

        private const byte COLORIMETER_MAX_REPORT_OUTPUT_LENGTH = 60;

        public String colorimeterPathName;
        public SafeFileHandle hidHandle;
        public SafeFileHandle readHandle;
        public SafeFileHandle writeHandle;
        public byte[] inputBuffer;
        public Boolean newInputData;


        private string firmwareVersion;
        private String testFileVersion;
        private DeviceStates deviceState;


        private Hid MyHid = new Hid();


        //Use this constructor to initialize a device
        public Colorimeter(SafeFileHandle handle, string pathName)
        {

            //  Get handles to use in requesting Input and Output reports.
            readHandle = handle;
            colorimeterPathName = pathName;


            //  Learn the capabilities of the device.
            MyHid.Capabilities = MyHid.GetDeviceCapabilities(hidHandle);
            MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);

            Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);
            //  Find out if the device is a system mouse or keyboard.
            //hidUsage = MyHid.GetHidUsage(MyHid.Capabilities);


            // Initialize input report buffer
            inputBuffer = new byte[MyHid.Capabilities.InputReportByteLength];
            newInputData = false;

            writeHandle = FileIO.CreateFile(colorimeterPathName, FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

            //  Flush any waiting reports in the input buffer. (optional)
            MyHid.FlushQueue(readHandle);
        }


        //private String hidUsage;

        //This class should operate at the level of the device itself and expose the public methods that correspond directly
        //to the Colorimeter's functionality described in the COLORIMETER COMMUNICATION INTERFACE AND FILE FORMAT SPECIFICATION
        //documentation.
        //This is directly from section 5.0 - Communication Interface, 5.1 - Overview

        //View the firmware version loaded on the Colorimeter
        public ColorimeterResponse GetColorimeterVersion()
        {
            Query(QueryType.FirmwareVersion);
            return null;
        }

        //View the Taylor test file version loaded on the Colorimeter
        public ColorimeterResponse GetTaylorTestFileVersion()
        {
            return null;
        }
        

        //Receive the User test file loaded on the Colorimeter
        public ColorimeterResponse GetUserTestFile()
        {
            return null;
        }

        //Receive test results from the Colorimeter
        public ColorimeterResponse GetTestResults()
        {
            return null;
        }

        //Start the Factory Acceptance Test(FAT)
        public ColorimeterResponse StartFactoryAcceptanceTest()
        {
            return null;
        }

        //Upgrade the device firmware(supported by a USB-enabled bootloader)
        public ColorimeterResponse UpgradeDeviceFirmware()
        {
            return null;
        }


        //Load Taylor Test Files 
        public ColorimeterResponse LoadTaylorTestFiles()
        {
            return null;
        }

        //Load User test files
        public ColorimeterResponse LoadUserTestFiles()
        {
            return null;
        }


        //Delete test results from the Colorimeter
        public ColorimeterResponse DeleteTestResultsFromColorimeter()
        {
            return null;
        }



        private Boolean Query(QueryType queryType)
        {
            byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];
            Boolean retval = false;

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
            Write(ref outputBuffer);
            // Setup to read response
            if (SetupRead() == true)
            {
                switch (queryType)
                {
                    case QueryType.FirmwareVersion:
                        retval = QueryFirmwareVersion();
                        break;
                    case QueryType.TestFileVersion:
                        retval = QueryTestFileVersion();
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

            return retval;
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

        public Boolean QueryFirmwareVersion()
        {
            //(expecting ACK) from response
            if (WaitForResponse() == InCmd.FirmwareVersion)//FirmwareVersion = 10,
            {
                firmwareVersion = Encoding.GetEncoding("iso-8859-1").GetString(inputBuffer, 3, inputBuffer[2]);
                //listBox1.Items.Add($"Firmware version: { firmwareVersion }");
                //response.responseInfo.Add($"Firmware version: { firmwareVersion }");
               // response.firmwareVersion = firmwareVersion;
                return true;
            }
            return false;
        }

        public Boolean QueryTestFileVersion()
        {
            //(expecting ACK) from response
            if (WaitForResponse() == InCmd.TestFileVersion)
            {
                testFileVersion = Encoding.GetEncoding("iso-8859-1").GetString(inputBuffer, 3, inputBuffer[2]);
                //response.responseInfo.Add($"Test File version: { testFileVersion }");
                //response.testFileVersion = testFileVersion;
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
            //response.responseInfo.Add($"Device State: { deviceState }");
            return true;
        }


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

        private void InputReportReceived(ref Byte[] inputReportBuffer, Boolean success)
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
                //response.responseInfo.Add("The attempt to read an Input report has failed");
            }
        }

        public void SendTestFile(string testFilename, OutCmd startCmd, ColorimeterResponse colorimeterResponse)
        {
            Boolean done;
            int i, j, readLen;
            byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];
            byte[] testData = new byte[COLORIMETER_MAX_REPORT_OUTPUT_LENGTH];
            BinaryReader testFileReader = new BinaryReader(File.OpenRead((string)testFilename));

            try
            {
                // Alert colorimeter that we are going to start sending test data
                SendCommand(startCmd);

                // Wait for ACK
                //this.SetupRead();
                if (WaitForResponse() == InCmd.ACK)
                {
                    done = false;

                    // Send each line of firmware file to device
                    while (!done)
                    {
                        readLen = testFileReader.Read(testData, 0, COLORIMETER_MAX_REPORT_OUTPUT_LENGTH);

                        // make sure device is still connected
                        //
                        // WARNING: This might not work correctly.  ColorimeterDetected is updated in frmMain.OnDeviceChange, but
                        // will that interrupt this loop?
                        //
                        // See note in function header!
                        // if (colorimeterDetected)
                        //{
                        Array.Clear(outputBuffer, 0, MyHid.Capabilities.OutputReportByteLength);

                        outputBuffer[0] = 0;        // Report number

                        // If readLen is greater than 0, data was read from file
                        colorimeterResponse.responseInfo.Add("SendTestFile(): read " + readLen + " bytes");
                        if (readLen > 0)
                        {
                            outputBuffer[1] = Convert.ToByte(OutCmd.TestData);
                            outputBuffer[2] = Convert.ToByte(readLen);
                        }

                        // If readLen is 0, then we've reached EOF
                        else
                        {
                            outputBuffer[1] = Convert.ToByte(OutCmd.TestComplete);
                            outputBuffer[2] = 0;

                            done = true;
                        }

                        // Copy test data to output buffer 
                        for (i = 3, j = 0; j < readLen; i++, j++)
                        {
                            colorimeterResponse.responseInfo.Add("SendTestFile(): data byte " + j + " is " + String.Format(@"\x{0:x2}", Convert.ToByte(testData[j])));
                            outputBuffer[i] = Convert.ToByte(testData[j]);
                        }

                        // send data
                        Write(ref outputBuffer);

                        // Wait for ACK before proceeding
                        SetupRead();
                        if (WaitForResponse() != InCmd.ACK)
                        {
                            // Error occurred
                            // todo: actually throw a valid exception
                            throw new Exception();
                        }
                        //} // if (ColorimeterDetected)
                    } // while ((fwLine = fwFileReader.ReadLine()) != null)
                }

                // Close test file
                testFileReader.Close();
            }
            catch
            {
                colorimeterResponse.responseInfo.Add("Exception thrown in colorimeter.SendTestFile():");
                testFileReader.Close();
            }
        }

        public void ReceiveFile(string fileName, OutCmd outCmd)
        {
            bool done;
            byte checksum = 0;
            BinaryWriter outputFileWriter = null;

            try
            {
                // If the output file exists, delete it
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                // Open the output file and create an BinaryWriter object so we can write to it
                outputFileWriter = new BinaryWriter(File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write));

                // Request the file from the colorimeter
                SendCommand(outCmd);
                // Wait for ACK
                SetupRead();

                if (WaitForResponse() == InCmd.ACK)
                {
                    // Loop, receiving test data until colorimeter tells us we're done
                    done = false;
                    while (!done)
                    {
                        // ACK received, now wait for test data
                        SetupRead();
                        if (WaitForInputReport())
                        {
                            // Verify that the data we received is correct given outCmd
                            if ((outCmd == OutCmd.SendUserTests && inputBuffer[1] == (byte)InCmd.TestData) ||
                                 (outCmd == OutCmd.SendTestResults && inputBuffer[1] == (byte)InCmd.ResultsData))
                            {
                                // Update our checksum
                                for (int i = 0; i < inputBuffer[2]; i++)
                                {
                                    checksum ^= inputBuffer[3 + i];
                                }

                                // If input report received successfully, write data
                                outputFileWriter.Write(inputBuffer, 3, inputBuffer[2]);
                                SendCommand(OutCmd.ACK);
                            }
                            else if (inputBuffer[1] == (byte)InCmd.DataSendComplete)
                            {
                                done = true;
                                if (checksum == inputBuffer[3])
                                {
                                    SendCommand(OutCmd.ACK);
                                }
                                else
                                {
                                    SendCommand(OutCmd.NAK);
                                    //colorimeterResponse.responseInfo.Add("Checksums do not match! Checksum Error");
                                    //MessageBox.Show("Checksums do not match!", "Checksum Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                SendCommand(OutCmd.NAK);
                                done = true;
                                //colorimeterResponse.responseInfo.Add("File transfer failed Error");
                                //MessageBox.Show("Test file transfer failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            done = true;
                            //colorimeterResponse.responseInfo.Add("File transfer failed. Error");
                            //MessageBox.Show("Test file transfer failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               //colorimeterResponse.responseInfo.Add($"{ex.Message} Exception occurred!");
                //MessageBox.Show(ex.Message, "Exception occurred!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            finally
            {
                if (outputFileWriter != null)
                    outputFileWriter.Close();
            }
        }

        private void SendCommand(OutCmd command)
        {
            byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];

            outputBuffer[0] = 0;
            outputBuffer[1] = Convert.ToByte(command);
            outputBuffer[2] = 0;
            Write(ref outputBuffer);

            if (command == OutCmd.FirmwareStart || command == OutCmd.TaylorTestStart || command == OutCmd.UserTestStart)
            {
                SetupRead();
            }
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

        private Boolean Write(ref byte[] outputBuffer)
        {
            Boolean success = false;

            try
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
                            //response.responseInfo.Add("Write to colorimeter failed.");
                        }
                    }
                    else
                    {
                        //response.responseInfo.Add("This HID device doesn't have an Output report.");
                    }
                }
                else
                {
                    //response.responseInfo.Add("Invalid write handle.");
                }

                return success;
            }
            catch (Exception ex)
            {
                //response.responseInfo.Add($"Colorimeter.Write() {ex}");
                throw;
            }
        }

        ///  <summary>
        ///  Initiates exchanging reports. 
        ///  The application sends a report and requests to read a report.
        ///  </summary>
        ///  
        ///  <returns>True if read successful, false otherwise</returns>
        public Boolean SetupRead()
        {
            Boolean success = false;

            try
            {

                // Check read handle
                if (!readHandle.IsInvalid)
                {
                    if (MyHid.Capabilities.InputReportByteLength > 0)
                    {
                        Hid.InputReportViaInterruptTransfer myInputReport = new Hid.InputReportViaInterruptTransfer();

                        //i don't want this sub function to be able to disconnect the colorimeter
                        var myColorimeterDetected = true;


                        myInputReport.Read(hidHandle, readHandle, writeHandle, ref myColorimeterDetected, ref inputBuffer, ref success);
                        InputReportReceived(ref inputBuffer, success);

                    }
                    else
                    {
                        //response.responseInfo.Add("This HID device doesn't have an Input report.");
                    }
                }
                else
                {
                    //response.responseInfo.Add("Invalid read handle.");
                }

                return success;
            }
            catch (Exception ex)
            {
                //response.responseInfo.Add($"Colorimeter.SetupRead() {ex}");
                throw;
            }
        }

    }
}
