using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Cumtd.Signage.Kiosk.Annunciator;
using Cumtd.Signage.Kiosk.KioskButton.Readers;
using Cumtd.Signage.Kiosk.RealTime;
using Cumtd.Signage.Kiosk.RealTime.Models;
using NLog;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal class AnnunciatorService : IDisposable
	{
		public bool Disposed { get; private set; }

		public bool Reading { get; private set; }

		private ConfigurationManager Config { get; }

		private Timer Timer { get; }
		private Timer HeartBeatTimer { get; }

		private IButtonReader[] ButtonReaders { get; }

		private ILogger Logger { get; }

		public AnnunciatorService(ConfigurationManager config)
		{
			Config = config ?? throw new ArgumentException(nameof(config));
			Logger = Config.Logger;

			Timer = new Timer(33)
			{
				AutoReset = true
			};
			Timer.Elapsed += Timer_Elapsed;

			var readers = new List<IButtonReader>();
			if (Config.ButtonConfig.Readers.UseSeaDac)
			{
				Logger.Debug($"Adding {nameof(SeaLevelButtonReader)} reader");
				readers.Add(new SeaLevelButtonReader(Logger));
			}

			Logger.Debug($"Adding {nameof(AltShiftPKeyboardReader)} reader");
			readers.Add(new AltShiftPKeyboardReader(Logger));

			if (readers.Count == 0)
			{
				throw new ConfigurationErrorsException("Must use at least one button reader");
			}

			ButtonReaders = readers.ToArray();

			HeartBeatTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds)
			{
				AutoReset = true
			};
			HeartBeatTimer.Elapsed += HeartBeatTimer_Elapsed;

		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			// check this every loop so we don't get double reads
			var pressed = ButtonReaders.Where(br => br.Pressed).ToArray();

			if (!Reading)
			{
				if (pressed.Length > 0)
				{
					Logger.Info($"{pressed[0].Name} pressed");
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
							Logger.Error(ex, "Error getting departures");
							DepartureAnnunciator.ReadError(Logger.Info);
							return;
						}

					}

					Logger.Debug($"Fetched {departures.Length} departures");
					DepartureAnnunciator.ReadDepartures(Config.ButtonConfig.DisplayName, departures, Logger.Info);
					Logger.Debug("Done reading");
					Reading = false;
				}
			}
		}

		private void HeartBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				Logger.Trace("Begin Sending Heartbeat");
				var heartBeatTask = SendHeartBeat();
				heartBeatTask.Wait();
				Logger.Trace("Done Sending Heartbeat");
			}
			catch (Exception ex)
			{
				Logger.Warn(ex, "Failed to send heartbeat.");
			}
		}

		private async Task SendHeartBeat()
		{
			HttpResponseMessage response;
			using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) })
			{
				response = await client
					.GetAsync($"https://kiosk.mtd.org/umbraco/api/health/buttonheartbeat?id={Config.ButtonConfig.Id}")
					.ConfigureAwait(false);
			}

			if (response.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new Exception($"Failed with status code: {response.StatusCode}");
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
			HeartBeatTimer.Start();
		}

		// ReSharper disable once UnusedMember.Global
		public void Stop()
		{
			Logger.Info("Stopping Service");
			Timer.Stop();
			HeartBeatTimer.Stop();
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
			HeartBeatTimer.Stop();
		}
	}
}
