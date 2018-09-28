using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Cumtd.Signage.Kiosk.Annunciator;
using Cumtd.Signage.Kiosk.SeaLevel;
using Cumtd.Signage.Kiosk.RealTime;
using Cumtd.Signage.Kiosk.RealTime.Models;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal class AnnunciatorService
	{
		private bool _newState;
		public bool NewState
		{
			get
			{
				var returnVal = _newState;
				_newState = false;
				return returnVal;
			}
		}

		private bool _pressed;
		public bool Pressed
		{
			get
			{
				_newState = false;
				return _pressed;
			}
			set
			{
				_newState = true;
				_pressed = value;
			}
		}

		public bool Reading { get; private set; }

		private Timer Timer { get; }

		private ButtonReader Reader { get; }

		public AnnunciatorService()
		{
			Timer = new Timer(33)
			{
				AutoReset = true
			};
			Timer.Elapsed += Timer_Elapsed;
			Reader = new ButtonReader(value => Pressed = value);
			Reader.Start();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (NewState)
			{
				if (Pressed && ! Reading)
				{
					Reading = true;
					Departure[] departures;
					using (var client = new RealTimeClient())
					{
						try
						{
							var getTask = client.GetRealtime("b20297e2-4c13-49b9-be4f-b1842f6108c9");
							getTask.Wait();
							departures = getTask.Result;
						}
						catch (Exception)
						{
							DepartureAnnunciator.ReadError();
							return;
						}
						
					}

					DepartureAnnunciator.ReadDepartures(departures, Console.WriteLine);
					Reading = false;
				}
			}
		}

		public void Start() => Timer.Start();

		public void Stop() => Timer.Stop();

	}
}
