using System;
using System.Threading;
using Cumtd.Signage.Kiosk.SeaLevel;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal static class Program
	{
		private static void Main()
		{
			using (var reader = ButtonReader.Instance)
			{


				const int hz = 20;
				const int sleep = 1000 / hz;
				const int seconds = 20;
				const int cycles = seconds * hz;

				var lastState = false;

				var cycle = 1;

				do
				{
					var pressed = reader.ReadButtonState();
					if (pressed != lastState)
					{
						Console.WriteLine(pressed ? "Pressed" : "Released");
					}

					lastState = pressed;
					Thread.Sleep(sleep);
					cycle++;
				} while (cycle <= cycles);

			}

			using (var instance = ButtonReader.Instance)
			{
				Console.WriteLine($"Final State {instance.ReadButtonState()}");
			}
				Console.ReadLine();
		}
	}
}
