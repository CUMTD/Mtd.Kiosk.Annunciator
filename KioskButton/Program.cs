using System;
using System.Threading;
using Cumtd.Signage.Kiosk.SeaLevel;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal static class Program
	{
		private static void Main()
		{
			void Test(bool pressed) => Console.WriteLine(pressed ? "pressed" : "unpressed");
			var reader = new ButtonReader(Test);


			const int hz = 20;
			const int sleep = 1000 / hz;
			const int seconds = 10;
			const int cycles = seconds * hz;

			var cycle = 1;

			reader.Start();
			do
			{

				Thread.Sleep(sleep);
				cycle++;
			} while (cycle <= cycles);

			reader.Stop();
			Thread.Sleep(1000);

			Console.ReadLine();

		}
	}
}
