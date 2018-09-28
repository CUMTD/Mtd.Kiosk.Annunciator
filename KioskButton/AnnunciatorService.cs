using System;
using System.Timers;
using Cumtd.Signage.Kiosk.Annunciator;
using Cumtd.Signage.Kiosk.SeaLevel;
using Cumtd.Signage.Kiosk.RealTime;
using Cumtd.Signage.Kiosk.RealTime.Models;
using Topshelf.Logging;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal class AnnunciatorService : IDisposable
	{
		public bool Disposed { get; private set; }

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

		private ConfigurationManager Config { get; }
		
		private Timer Timer { get; }

		private ButtonReader Reader { get; }

		private LogWriter Logger { get; }
		
		public AnnunciatorService()
		{
			Config = ConfigurationManager.Config;
			Logger = HostLogger.Current.Get("kiosk-annunciator");
			Timer = new Timer(33)
			{
				AutoReset = true
			};
			Timer.Elapsed += Timer_Elapsed;
			Reader = new ButtonReader(value => Pressed = value, Logger.Debug);
			Reader.Start();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (NewState)
			{
				var pressed = Pressed;
				Logger.DebugFormat("New State: {0}", pressed ? "Pressed" : "Unpressed");
				if (pressed)
				{
					if (!Reading)
					{

						Logger.Debug("Reading");
						Reading = true;
						Departure[] departures;
						using (var client = new RealTimeClient())
						{
							try
							{
								var getTask = client.GetRealtime(Config.Id);
								getTask.Wait();
								departures = getTask.Result;
							}
							catch (Exception ex)
							{
								Logger.Error("Error getting departures", ex);
								DepartureAnnunciator.ReadError(Logger.Info);
								return;
							}

						}

						Logger.DebugFormat("Fetched {0} departures", departures.Length);
						DepartureAnnunciator.ReadDepartures(Config.Name, departures, Logger.Info);
						Logger.Debug("Done reading");
						Reading = false;
					}
					else
					{
						Logger.Debug("Already reading");
					}
				}
			}
		}

		public void Start()
		{
			if (Disposed)
			{
				Logger.Error("Already Disposed");
				throw new Exception("Already disposed");
			}

			Logger.Info("Starting Service");
			Timer.Start();
		}

		public void Stop()
		{
			Logger.Info("Stopping Service");
			Timer.Stop();
		}

		public void Dispose()
		{
			Logger.Debug("Disposing Service");
			Disposed = true;
			Timer.Elapsed -= Timer_Elapsed;
			Reader.Stop();
			Timer.Stop();
		}
	}
}
