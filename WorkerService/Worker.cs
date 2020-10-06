using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskAnnunciatorButton.Annunciator;
using KioskAnnunciatorButton.RealTime;
using KioskAnnunciatorButton.WorkerService.Readers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KioskAnnunciatorButton.WorkerService
{
	internal class Worker : BackgroundService
	{
		private readonly IReader[] _readers;
		private readonly AzureAnnunciator _annunciator;
		private readonly RealTimeClient _realTimeClient;
		private readonly IConfiguration _config;
		private readonly ILogger<Worker> _logger;
		private readonly int _waitMS;
		private DateTime _nextHeartbeat;

		private bool _reading;

		public Worker(
			IEnumerable<IReader> readers,
			AzureAnnunciator departureReader,
			RealTimeClient realTimeClient,
			IConfiguration config,
			ILogger<Worker> logger
			)
		{
			_readers = readers?.ToArray() ?? throw new ArgumentException(nameof(readers));
			_annunciator = departureReader ?? throw new ArgumentException(nameof(departureReader));
			_realTimeClient = realTimeClient ?? throw new ArgumentException(nameof(realTimeClient));
			_config = config ?? throw new ArgumentException(nameof(config));
			_logger = logger ?? throw new ArgumentException(nameof(logger));
			_waitMS = HertzToWaitMs(config.GetValue<float>("pollingHz"));
			_nextHeartbeat = CalculateNextHeartbeat();
			_reading = false;
		}

		#region Service Control

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
						await Read();
					}
				}

				await Heartbeat();

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
			foreach (var reader in _readers)
			{
				reader.Stop();
			}
			await base.StopAsync(cancellationToken);
		}

		#endregion Service Control

		private async Task Read()
		{
			_reading = true;

			try
			{
				var kioskId = _config
					.GetValue<string>("kiosk:id");

				var stopName = _config
					.GetValue<string>("kiosk:displayName");

				var departures = await _realTimeClient
					.GetRealtime(kioskId);

				await _annunciator
					.ReadDepartures(stopName, departures);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to fetch departures");
				await _annunciator.ReadError();
			}
			finally
			{
				_reading = false;
			}
		}
		private async Task Heartbeat()
		{
			if (DateTime.Now > _nextHeartbeat)
			{
				var id = _config.GetValue<string>("kiosk:id");
				try
				{
					await _realTimeClient.SendHeartbeat(id);
					_logger.LogDebug("Sent heartbeat for {kioskId}", id);
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to send heartbeat for {kioskId}", id);
				}
				finally
				{
					_nextHeartbeat = CalculateNextHeartbeat();
				}
			}
		}
		private static int HertzToWaitMs(float hertz) => (int)(1000f / hertz);
		private DateTime CalculateNextHeartbeat() => DateTime.Now.AddSeconds(_config.GetValue<int>("heartbeatSeconds"));

	}
}
