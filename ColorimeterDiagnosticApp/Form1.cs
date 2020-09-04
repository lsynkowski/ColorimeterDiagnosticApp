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


		public Form1()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_DEVICECHANGE)
            {
				listBox1.Items.Add($"WParam : { m.WParam }   |LParam : { m.LParam }  |Result : { m.Result }  |MHWnd : { m.HWnd }");

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
