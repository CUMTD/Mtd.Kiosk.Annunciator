using System;
using Sealevel;

namespace Cumtd.Signage.Kiosk.SeaLevel
{
	internal class SeaDacButton : IDisposable
	{
		private Action<string> Logger { get; }
		private SeaMAX SeaMax { get; set; }
		
		public SeaDacButton(Action<string> logger)
		{
			Logger = logger ?? Console.WriteLine;
			SeaMax = new SeaMAX();
		}

		public void OpenRead()
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

			if (SeaMax?.IsSeaMAXOpen == false)
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

		public void ListenChange(ref byte[] bytes, int pollingRate)
		{
			OpenRead();
			if (SeaMax.SM_NotifyInputState(0) != 2)
			{
				// update `bytes` when state changes
				// update every 16 milliseconds (60 hz)
				// don't block so that it can be canceled
				SeaMax.SM_NotifyOnInputChange(0, 4, bytes, pollingRate, 0);
			}
		}

		#region Private Methods

		#endregion Private Methods

		#region IDisposable

		public void Dispose()
		{
			if (SeaMax == null)
			{
				return;
			}

			if (SeaMax.IsSeaDACInitialized && SeaMax.IsSeaMAXOpen)
			{
				// cancel listen
				SeaMax.SM_NotifyInputState(1);
			}

			//clean up the SDL interface to release the internally allocated memory
			if (SeaMax.IsSeaDACInitialized)
			{
				Logger("Cleaning up SDL Interface.");
				SeaMax.SDL_Cleanup();
			}

			//we need to remember to close the connection when we are done with it
			if (SeaMax.IsSeaMAXOpen)
			{
				Logger("Closing SeaMAX Connection.");
				SeaMax.SM_Close();
			}

			SeaMax = null;
		}

		#endregion IDisposable

	}
}
