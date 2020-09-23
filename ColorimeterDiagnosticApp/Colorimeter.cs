using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
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

        public string colorimeterPathName;
        public SafeFileHandle hidHandle;
        public SafeFileHandle readHandle;
        public SafeFileHandle writeHandle;
        public byte[] inputBuffer;
        public bool newInputData;


        private string firmwareVersion;
        private string testFileVersion;
        private Hid MyHid = new Hid();

        public string DeviceState { get; set; }


        //Use this constructor to initialize a device
        public Colorimeter(SafeFileHandle handle, string pathName)
        {

            //  Get handles to use in requesting Input and Output reports.
            readHandle = handle;
            colorimeterPathName = pathName;

            //get the Hid Handle from the pathName
            hidHandle = FileIO.CreateFile(colorimeterPathName, 0, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);


            //  Learn the capabilities of the device.
            MyHid.Capabilities = MyHid.GetDeviceCapabilities(hidHandle);
            MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);

            Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);


            // Initialize input report buffer
            inputBuffer = new byte[MyHid.Capabilities.InputReportByteLength];
            newInputData = false;

            writeHandle = FileIO.CreateFile(colorimeterPathName, FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

            //  Flush any waiting reports in the input buffer. (optional)
            MyHid.FlushQueue(readHandle);
        }

        //This class should operate at the level of the device itself and expose the public methods that correspond directly
        //to the Colorimeter's functionality described in the COLORIMETER COMMUNICATION INTERFACE AND FILE FORMAT SPECIFICATION
        //documentation.
        //This is directly from section 5.0 - Communication Interface, 5.1 - Overview

        //View the firmware version loaded on the Colorimeter
        public string GetColorimeterVersion()
        {

            firmwareVersion = GetSinglePacketResponse(OutCmd.SendFwVers);
            return firmwareVersion;

        }

        //Get the Taylor test file version loaded on the Colorimeter
        public string GetTaylorTestFileVersion()
        {
            testFileVersion = GetSinglePacketResponse(OutCmd.SendTestFileVers);
            return testFileVersion;
        }

        //Get the User test file loaded on the Colorimeter
        public void GetUserTestFile(string saveFilePath)
        {
            // If the output file exists, delete it
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath); 
            }

            ReceiveFile(saveFilePath, OutCmd.SendUserTests);

        }

        //Receive test results from the Colorimeter
        public void GetTestResults(string saveFilePath)
        {
            // If the output file exists, delete it
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            ReceiveFile(saveFilePath, OutCmd.SendTestResults);
        }

        //Start the Factory Acceptance Test(FAT)
        public void StartFactoryAcceptanceTest()
        {

        }

        //Upgrade the device firmware(supported by a USB-enabled bootloader)
        public void UpgradeDeviceFirmware()
        {

        }


        //Load Taylor Test Files 
        public void LoadTaylorTestFiles(string taylorTestFilePath)
        {

        }

        //Load User test files
        public void LoadUserTestFiles()
        {

        }


        //Delete test results from the Colorimeter
        public void DeleteTestResultsFromColorimeter()
        {

        }

        //This method is not mentioned as a public function of the Colorimeter, but it is something that users of the form would want to know about it
        public string GetDeviceState()
        {
            DeviceState = GetSinglePacketResponse(OutCmd.QueryFirmwareState);
            return DeviceState;
        }

        //This method is used for the cases where a single response packet will be enough to contain the results, and we will not have to iterate through a large amount of data
        private string GetSinglePacketResponse(OutCmd outCommand)
        {
            try
            {

                ////build command to send buffer
                byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];
                outputBuffer[0] = 0;
                outputBuffer[1] = Convert.ToByte(outCommand);//the value of the query firmware state
                outputBuffer[2] = 0;


                //Write command to buffer
                Hid.OutputReportViaInterruptTransfer outputReport = new Hid.OutputReportViaInterruptTransfer();
                if (!outputReport.Write(outputBuffer, writeHandle))
                {
                    //Failure writing to the report
                    throw new Exception();
                }


                //Read from the input report
                bool deviceConnected = true;
                bool success = false;

                Hid.InputReportViaInterruptTransfer myInputReport = new Hid.InputReportViaInterruptTransfer();
                myInputReport.Read(hidHandle, readHandle, writeHandle, ref deviceConnected, ref inputBuffer, ref success);

                if (!success)
                {
                    //failure reading from the report
                    throw new Exception();
                }


                InputReportReceived(ref inputBuffer, success);
                newInputData = true;


                if (outCommand.Equals(OutCmd.SendFwVers) || outCommand.Equals(OutCmd.SendTestFileVers))
                {
                    //Both of these types of queries return the same type of packet, an encoded string
                    return Encoding.GetEncoding("iso-8859-1").GetString(inputBuffer, 3, inputBuffer[2]);
                }

                if (outCommand.Equals(OutCmd.QueryFirmwareState))
                {
                    switch ((InCmd)inputBuffer[1])
                    {
                        case InCmd.BootloaderRunning:
                            return "BootloaderRunning";

                        case InCmd.MainFwRunning:
                            return "MainFwRunning";

                        default:
                            return "Unknown";
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
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

        private void InputReportReceived(ref Byte[] inputReportBuffer, bool success)
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



        public void SendTaylorTestFile(string testFilename)
        {
            Boolean done;
            int i, j, readLen;
            byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];
            byte[] testData = new byte[COLORIMETER_MAX_REPORT_OUTPUT_LENGTH];
            BinaryReader testFileReader = new BinaryReader(File.OpenRead((string)testFilename));

            try
            {
                // Alert colorimeter that we are going to start sending test data
                SendCommand(OutCmd.TaylorTestStart);

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
                        //colorimeterResponse.responseInfo.Add("SendTestFile(): read " + readLen + " bytes");
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
                            //colorimeterResponse.responseInfo.Add("SendTestFile(): data byte " + j + " is " + String.Format(@"\x{0:x2}", Convert.ToByte(testData[j])));
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
                //colorimeterResponse.responseInfo.Add("Exception thrown in colorimeter.SendTestFile():");
                testFileReader.Close();
            }
        }

        public void ReceiveFile(string fileName, OutCmd outCmd)
        {
            BinaryWriter outputFileWriter = null;
            try
            {

                // Open the output file and create an BinaryWriter object so we can write to it
                outputFileWriter = new BinaryWriter(File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write));

                // Request the file from the colorimeter
                WriteCommandToOutputBuffer(outCmd);

                // Wait for ACK
                newInputData = ReadInputBuffer();

                if ((InCmd)inputBuffer[1] == InCmd.ACK)
                {
                    byte checksum = 0;
                    bool done = false;
                    while (!done)
                    {
                        // ACK received, now wait for test data

                        newInputData = false;
                        newInputData = ReadInputBuffer();

                        if (newInputData)
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

                                WriteCommandToOutputBuffer(OutCmd.ACK);
                            }
                            else if (inputBuffer[1] == (byte)InCmd.DataSendComplete)
                            {
                                done = true;
                                if (checksum == inputBuffer[3])
                                {
                                    WriteCommandToOutputBuffer(OutCmd.ACK);
                                }
                                else
                                {
                                    WriteCommandToOutputBuffer(OutCmd.NAK);
                                }
                            }
                            else
                            {
                                WriteCommandToOutputBuffer(OutCmd.NAK);
                                done = true;

                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {

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

        private bool WriteCommandToOutputBuffer(OutCmd command)
        {
            byte[] outputBuffer = new byte[MyHid.Capabilities.OutputReportByteLength];

            outputBuffer[0] = 0;
            outputBuffer[1] = Convert.ToByte(command);
            outputBuffer[2] = 0;
            Write(ref outputBuffer);

            Hid.OutputReportViaInterruptTransfer outputReport = new Hid.OutputReportViaInterruptTransfer();
            return outputReport.Write(outputBuffer, writeHandle);

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

        private bool Write(ref byte[] outputBuffer)
        {
            bool success = false;

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

        // replaces setupread
        public bool ReadInputBuffer()
        {
            var ret = false;

            Hid.InputReportViaInterruptTransfer myInputReport = new Hid.InputReportViaInterruptTransfer();

            //i don't want this sub function to be able to disconnect the colorimeter
            var myColorimeterDetected = true;


            myInputReport.Read(hidHandle, readHandle, writeHandle, ref myColorimeterDetected, ref inputBuffer, ref ret);

            for (int count = 0; count < inputBuffer.Length; count++)
            {
                //  Copy input data to buffer
                inputBuffer[count] = inputBuffer[count];
            }

            return ret;
        }


        //replaces InputReportReceived
        public bool UpdateInputBuffer(ref Byte[] inputReportBuffer)
        {

            for (int count = 0; count < inputReportBuffer.Length; count++)
            {
                //  Copy input data to buffer
                inputBuffer[count] = inputReportBuffer[count];
            }

            return true;

        }

    }
}
