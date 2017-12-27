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
		const int Width = 288;
		const int Height = 1;
		private byte[] RGBColors = Enumerable.Repeat((byte)0x20, Width * 3).ToArray();

		public ArduinoStrip(ILogger logger)
		{
			Logger = logger;
			Type = DeviceType.Generic;
		}

		public ILogger Logger { get; set; }

		public override void UpdateDevice(Bitmap bitmap)
		{
			if (bitmap == null)
				return;

			// Resize the bitmap
			using (var b = ImageUtilities.ResizeImage(bitmap, Width, Height))
			{
				for (var x = 0; x < Width; x++)
				{
					var c = b.GetPixel(x, 0);
					RGBColors[(x * 3) + 0] = c.R;
					RGBColors[(x * 3) + 1] = c.G;
					RGBColors[(x * 3) + 2] = c.B;
				}
			}
			_port.Write(RGBColors, 0, Width * 3);
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
				if (desc.Contains("USB Serial Device"))//my pro micro come up as this
				{
					return deviceId;
				}
			}
			return null;
		}

		public override bool TryEnable()
		{
			// Do a check here to see if your device is available. Make sure you set CanUse so
			// Artemis knows it's available.
			CanUse = true;

			// Lets open a COM1 connection, this could be an Arduino or something similar
			if (CanUse)
			{
				_port = new SerialPort(AutodetectArduinoPort(), 9600);
				_port.WriteBufferSize = Width * 6;
				_port.Open();
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
