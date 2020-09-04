using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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


        private Hid MyHid = new Hid();
		private DeviceManagement MyDeviceManagement = new DeviceManagement();


		public Form1()
        {
            InitializeComponent();
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

			RegisterForDeviceNotifications("", this.Handle, hidGuid,ref deviceNotificationHandle);

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


		///  <summary>
		///  Use SetupDi API functions to retrieve the device path name of an
		///  attached device that belongs to a device interface class.
		///  </summary>
		///  
		///  <param name="myGuid"> an interface class GUID. </param>
		///  <param name="devicePathName"> a pointer to the device path name 
		///  of an attached device. </param>
		///  
		///  <returns>
		///   True if a device is found, False if not. 
		///  </returns>
		// ALREADY IN DEVICEMANAGEMENT.CS
		//private Boolean FindDeviceFromGuid(System.Guid myGuid, ref String[] devicePathName)
		//{
		//	Int32 bufferSize = 0;
		//	IntPtr detailDataBuffer = IntPtr.Zero;
		//	Boolean deviceFound;
		//	IntPtr deviceInfoSet = new System.IntPtr();
		//	Boolean lastDevice = false;
		//	Int32 memberIndex = 0;
		//	SP_DEVICE_INTERFACE_DATA MyDeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
		//	Boolean success;

		//	try
		//	{
		//		// ***
		//		//  API function

		//		//  summary 
		//		//  Retrieves a device information set for a specified group of devices.
		//		//  SetupDiEnumDeviceInterfaces uses the device information set.

		//		//  parameters 
		//		//  Interface class GUID.
		//		//  Null to retrieve information for all device instances.
		//		//  Optional handle to a top-level window (unused here).
		//		//  Flags to limit the returned information to currently present devices 
		//		//  and devices that expose interfaces in the class specified by the GUID.

		//		//  Returns
		//		//  Handle to a device information set for the devices.
		//		// ***

		//		deviceInfoSet = SetupDiGetClassDevs(ref myGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

		//		deviceFound = false;
		//		memberIndex = 0;

		//		// The cbSize element of the MyDeviceInterfaceData structure must be set to
		//		// the structure's size in bytes. 
		//		// The size is 28 bytes for 32-bit code and 32 bits for 64-bit code.

		//		MyDeviceInterfaceData.cbSize = Marshal.SizeOf(MyDeviceInterfaceData);

		//		do
		//		{
		//			// Begin with 0 and increment through the device information set until
		//			// no more devices are available.

		//			// ***
		//			//  API function

		//			//  summary
		//			//  Retrieves a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.
		//			//  On return, MyDeviceInterfaceData contains the handle to a
		//			//  SP_DEVICE_INTERFACE_DATA structure for a detected device.

		//			//  parameters
		//			//  DeviceInfoSet returned by SetupDiGetClassDevs.
		//			//  Optional SP_DEVINFO_DATA structure that defines a device instance 
		//			//  that is a member of a device information set.
		//			//  Device interface GUID.
		//			//  Index to specify a device in a device information set.
		//			//  Pointer to a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.

		//			//  Returns
		//			//  True on success.
		//			// ***

		//			success = SetupDiEnumDeviceInterfaces
		//				(deviceInfoSet,
		//				IntPtr.Zero,
		//				ref myGuid,
		//				memberIndex,
		//				ref MyDeviceInterfaceData);

		//			// Find out if a device information set was retrieved.

		//			if (!success)
		//			{
		//				lastDevice = true;

		//			}
		//			else
		//			{
		//				// A device is present.

		//				// ***
		//				//  API function: 

		//				//  summary:
		//				//  Retrieves an SP_DEVICE_INTERFACE_DETAIL_DATA structure
		//				//  containing information about a device.
		//				//  To retrieve the information, call this function twice.
		//				//  The first time returns the size of the structure.
		//				//  The second time returns a pointer to the data.

		//				//  parameters
		//				//  DeviceInfoSet returned by SetupDiGetClassDevs
		//				//  SP_DEVICE_INTERFACE_DATA structure returned by SetupDiEnumDeviceInterfaces
		//				//  A returned pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA 
		//				//  Structure to receive information about the specified interface.
		//				//  The size of the SP_DEVICE_INTERFACE_DETAIL_DATA structure.
		//				//  Pointer to a variable that will receive the returned required size of the 
		//				//  SP_DEVICE_INTERFACE_DETAIL_DATA structure.
		//				//  Returned pointer to an SP_DEVINFO_DATA structure to receive information about the device.

		//				//  Returns
		//				//  True on success.
		//				// ***                     

		//				success = SetupDiGetDeviceInterfaceDetail
		//					(deviceInfoSet,
		//					ref MyDeviceInterfaceData,
		//					IntPtr.Zero,
		//					0,
		//					ref bufferSize,
		//					IntPtr.Zero);

		//				// Allocate memory for the SP_DEVICE_INTERFACE_DETAIL_DATA structure using the returned buffer size.

		//				detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

		//				// Store cbSize in the first bytes of the array. The number of bytes varies with 32- and 64-bit systems.

		//				Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

		//				// Call SetupDiGetDeviceInterfaceDetail again.
		//				// This time, pass a pointer to DetailDataBuffer
		//				// and the returned required buffer size.

		//				success = SetupDiGetDeviceInterfaceDetail
		//					(deviceInfoSet,
		//					ref MyDeviceInterfaceData,
		//					detailDataBuffer,
		//					bufferSize,
		//					ref bufferSize,
		//					IntPtr.Zero);

		//				// Skip over cbsize (4 bytes) to get the address of the devicePathName.

		//				IntPtr pDevicePathName = new IntPtr(detailDataBuffer.ToInt32() + 4);

		//				// Get the String containing the devicePathName.

		//				devicePathName[memberIndex] = Marshal.PtrToStringAuto(pDevicePathName);


		//				deviceFound = true;
		//			}
		//			memberIndex = memberIndex + 1;
		//		}
		//		while (!((lastDevice == true)));



		//		return deviceFound;
		//	}
		//	catch (Exception ex)
		//	{
		//		throw;
		//	}
		//	finally
		//	{
		//		if (detailDataBuffer != IntPtr.Zero)
		//		{
		//			// Free the memory allocated previously by AllocHGlobal.

		//			Marshal.FreeHGlobal(detailDataBuffer);
		//		}
		//		// ***
		//		//  API function

		//		//  summary
		//		//  Frees the memory reserved for the DeviceInfoSet returned by SetupDiGetClassDevs.

		//		//  parameters
		//		//  DeviceInfoSet returned by SetupDiGetClassDevs.

		//		//  returns
		//		//  True on success.
		//		// ***

		//		if (deviceInfoSet != IntPtr.Zero)
		//		{
		//			SetupDiDestroyDeviceInfoList(deviceInfoSet);
		//		}
		//	}
		//}




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
	}
}
