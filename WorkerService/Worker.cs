using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskAnnunciatorButton.Reader;
using KioskAnnunciatorButton.RealTime;
using KioskAnnunciatorButton.SeaLevelReader;
using KioskAnnunciatorButton.WorkerService.Readers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KioskAnnunciatorButton.WorkerService
{
	internal class Worker : BackgroundService
	{
		private readonly IReader[] _readers;
		private readonly DepartureReader _departureReader;
		private readonly RealTimeClient _realTimeClient;
		private readonly IConfiguration _config;
		private readonly ILogger<Worker> _logger;
		private readonly int _waitMS;

		private bool _reading;

		public Worker(
			IEnumerable<IReader> readers,
			DepartureReader departureReader,
			RealTimeClient realTimeClient,
			IConfiguration config,
			ILogger<Worker> logger
			)
		{
			_readers = readers?.ToArray() ?? throw new ArgumentException(nameof(readers));
			_departureReader = departureReader ?? throw new ArgumentException(nameof(departureReader));
			_realTimeClient = realTimeClient ?? throw new ArgumentException(nameof(realTimeClient));
			_config = config ?? throw new ArgumentException(nameof(config));
			_logger = logger ?? throw new ArgumentException(nameof(logger));
			_waitMS = HertzToWaitMs(config.GetValue<int>("pollingHz"));
			_reading = false;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogTrace("Tick {date}", DateTime.Now.ToString("HH:mm:ss fff"));
				if (!_reading)
				{
					var pressed = _readers
						.Where(br => br.GetPressed())
						.ToArray();

					if (pressed.Length > 0)
					{
						_logger.LogInformation($"{pressed[0].Name} pressed");

						_reading = true;

						Departure[] departures;

						try
						{
							var kioskId = _config
								.GetValue<string>("kiosk:id");
							var stopName = _config
								.GetValue<string>("kiosk:displayName");

							departures = await _realTimeClient
								.GetRealtime(kioskId);

							await _departureReader
								.ReadDepartures(stopName, departures);
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Failed to fetch departures");
							await _departureReader.ReadError();
						}
						finally
						{
							_reading = false;
						}

					}
				}
				await Task.Delay(_waitMS);
			}
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting service");
			foreach (var reader in _readers)
			{
				reader.Start();
			}
			await base.StartAsync(cancellationToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping service");
			foreach(var reader in _readers)
			{
				reader.Stop();
			}
			await base.StopAsync(cancellationToken);
		}

		private static int HertzToWaitMs(int hertz) => 1000 / hertz;

	}
}
