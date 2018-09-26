using System;
using Sealevel;

namespace Cumtd.Signage.Kiosk.SeaLevel
{
	public sealed class ButtonReader : IDisposable
	{
		private static ButtonReader _instance { get; set; }
		public static ButtonReader Instance => _instance ?? (_instance = new ButtonReader());

		private SeaMAX SeaMax { get; }

		private ButtonReader()
		{
			SeaMax = new SeaMAX();
		}

		public bool ReadButtonState()
		{
			Init();

			var data = new byte[1];
			SeaMax.SM_ReadDigitalInputs(0, 1, data);

			return data[0] >= 1;
		}

		private void Init()
		{
			InitSeaDac();
			OpenSeaMax();
		}

		private void InitSeaDac()
		{
			if (!SeaMax.IsSeaDACInitialized)
			{
				SeaMax.SDL_Initialize();
				var count = SeaMax.SDL_SearchForDevices();

				if (count < 0)
				{
					throw new Exception($"Error {count} while searching for SDL Devices.");
				}

				if (count == 0)
				{
					throw new Exception("No devices found");
				}

				if (SeaMax.SDL_FirstDevice() < 0)
				{
					throw new Exception("Error selecting first device");
				}
			}
		}

		private void OpenSeaMax()
		{
			if (!SeaMax.IsSeaMAXOpen)
			{
				var id = 0;
				if (SeaMax.SDL_GetDeviceID(ref id) < 0)
				{
					throw new Exception("Can't get id");
				}

				if (SeaMax.SM_Open($"SeaDAC Lite {id}") < 0)
				{
					throw new Exception("Couldn't open read");
				}
			}
		}

		public void Dispose()
		{
			if (SeaMax == null)
			{
				return;
			}

			//clean up the SDL interface to release the internally allocated memory
			if (SeaMax.IsSeaDACInitialized)
			{
				Console.WriteLine("Cleaning up SDL Interface.");
				SeaMax.SDL_Cleanup();
			}

			//we need to remember to close the connection when we are done with it
			if (SeaMax.IsSeaMAXOpen)
			{
				Console.WriteLine("Closing SeaMAX Connection.");
				SeaMax.SM_Close();
			}
		}
	}
}
