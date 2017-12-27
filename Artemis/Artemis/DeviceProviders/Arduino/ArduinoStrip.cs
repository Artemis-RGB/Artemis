using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Management;//added Management for detecting Arduino
using Artemis.DAL;
using Artemis.Properties;
using Artemis.Settings;
using Artemis.Utilities;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using Ninject.Extensions.Logging;
using System.IO;

namespace Artemis.DeviceProviders.Arduino
{
	public class ArduinoStrip : DeviceProvider
	{
		private SerialPort _port;
		const int Width = 288;//change this number to number of leds in strip
		const int Height = 1;
		private byte[] RGBColors = Enumerable.Repeat((byte)0x20, Width * 3).ToArray();//initialize the array of colors

		public ArduinoStrip(ILogger logger)
		{
			Logger = logger;//Hook into the log
			Type = DeviceType.Generic;//set to Generic type
		}

		public ILogger Logger { get; set; }

		public override void UpdateDevice(Bitmap bitmap)
		{
			if (bitmap == null)
				return;

			// Resize the bitmap
			using (var b = ImageUtilities.ResizeImage(bitmap, Width, Height))//resize the bitmap to the size we want
			{
				for (var x = 0; x < Width; x++)//loop though all pixels in x direction
				{
					var c = b.GetPixel(x, 0);//get the pixel colors
					RGBColors[(x * 3) + 0] = c.R;//add color to color array
					RGBColors[(x * 3) + 1] = c.G;
					RGBColors[(x * 3) + 2] = c.B;
				}
			}
			try
			{
				_port.Write(RGBColors, 0, Width * 3);//try to write to port
			}
			catch (Exception)
			{
				CanUse = false;//if you cant write to port then disable device
			}
		}

		private string AutodetectArduinoPort() //from Brandon https://stackoverflow.com/questions/3293889/how-to-auto-detect-arduino-com-port
		{
			ManagementScope connectionScope = new ManagementScope();
			SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

			foreach (ManagementObject item in searcher.Get())
			{
				string desc = item["Description"].ToString();
				string deviceId = item["DeviceID"].ToString();

				if (desc.Contains("Arduino"))//my due comes up as Arduino Due
				{
					return deviceId;
				}
				if (desc.Contains("USB Serial Device"))//my pro micro come up as "USB Serial Device"
				{
					return deviceId;
				}
			}
			return null;//if device not found then return empty
		}

		public override bool TryEnable()
		{
			// Do a check here to see if your device is available. Make sure you set CanUse so
			// Artemis knows it's available.

			// Lets open COM connection, this could be an Arduino or something similar
			String port = AutodetectArduinoPort();
			if (port == null)
			{
				CanUse = false;//if no device found then disable
			}
			else
			{
				CanUse = true;//enable device
				_port = new SerialPort(port, 115200);//create new port with baud rate of choose like 9600 or 115200
				_port.WriteBufferSize = Width * 6;//this may not be necessary
				try
				{
					_port.Open();
				}
				catch (Exception)
				{
					CanUse = false;//if you cant open port then disable device
				}
			}

			return CanUse;
		}

		public override void Disable()
		{
			// Disable doesn't need to be implemented since this isn't a keyboard (this may change)
			throw new NotSupportedException("Can only disable a keyboard");
		}
	}
}
