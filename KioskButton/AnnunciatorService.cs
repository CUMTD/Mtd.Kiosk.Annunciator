using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Timers;
using Cumtd.Signage.Kiosk.Annunciator;
using Cumtd.Signage.Kiosk.KioskButton.Readers;
using Cumtd.Signage.Kiosk.RealTime;
using Cumtd.Signage.Kiosk.RealTime.Models;
using Topshelf.Logging;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal class AnnunciatorService : IDisposable
	{
		public bool Disposed { get; private set; }

		public bool Reading { get; private set; }

		private ConfigurationManager Config { get; }

		private Timer Timer { get; }

		private IButtonReader[] ButtonReaders { get; }


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


			var readers = new List<IButtonReader>();
			if (Config.ButtonConfig.Readers.UseSeaDac)
			{
				readers.Add(new SeaLevelButtonReader(Logger));
			}

			if (Config.ButtonConfig.Readers.UsePanicButton)
			{
				readers.Add(new AltShiftPKeyboardReader(Logger));
			}

			if (readers.Count == 0)
			{
				throw new ConfigurationErrorsException("Must use at least one button reader");
			}

			ButtonReaders = readers.ToArray();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!Reading)
			{
				var pressed = ButtonReaders.Where(br => br.Pressed).ToArray();
				if (pressed.Length > 0)
				{
					Logger.Info($"{pressed.First()} pressed");
					Reading = true;
					Departure[] departures;
					using (var client = new RealTimeClient())
					{
						try
						{
							var getTask = client.GetRealtime(Config.ButtonConfig.Id);
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
			foreach (var reader in ButtonReaders)
			{
				reader.Dispose();
			}
			Timer.Stop();
		}
	}
}
