using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Core.Config;
using Mtd.Kiosk.Annunciator.Core.Models;

namespace Mtd.Kiosk.Annunciator.Service;
internal class AnnunciatorService : BackgroundService, IDisposable
{
	private bool currentlyAnnouncing = false;

	private readonly KioskConfig _config;
	private readonly IReadOnlyCollection<IButtonReader> _readers;
	private readonly IKioskRealTimeClient _kioskRealTimeClient;
	private readonly IAnnunciator _annunciator;
	private readonly ILogger<AnnunciatorService> _logger;
	public AnnunciatorService(
		IOptions<KioskConfig> config,
		IEnumerable<IButtonReader> readers,
		IKioskRealTimeClient kioskRealTimeClient,
		IAnnunciator annunciator,
		ILogger<AnnunciatorService> logger
	)
	{
		ArgumentNullException.ThrowIfNull(config.Value, nameof(config));
		ArgumentException.ThrowIfNullOrWhiteSpace(config.Value.Name, nameof(config.Value.Name));
		ArgumentException.ThrowIfNullOrWhiteSpace(config.Value.Id, nameof(config.Value.Id));
		ArgumentNullException.ThrowIfNull(readers, nameof(readers));
		ArgumentNullException.ThrowIfNull(kioskRealTimeClient, nameof(kioskRealTimeClient));
		ArgumentNullException.ThrowIfNull(annunciator, nameof(annunciator));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_config = config.Value;
		_readers = readers.ToImmutableArray();
		_kioskRealTimeClient = kioskRealTimeClient;
		_annunciator = annunciator;
		_logger = logger;

		ArgumentOutOfRangeException.ThrowIfZero(_readers.Count, nameof(readers));
	}

	#region BackgroundService

	protected async override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// This doesn't actually need to run
		// Since the workers get started in the StartAsync method and run in a differnt thread
		// Still useful as a heartbeat.
		while (!stoppingToken.IsCancellationRequested)
		{
			_logger.LogInformation("{serviceName} is running", nameof(AnnunciatorService));
			await Task.Delay(60_000, stoppingToken);
		}
		await StopAsync(CancellationToken.None);
	}

	public async override Task StartAsync(CancellationToken cancellationToken)
	{
		foreach (var reader in _readers)
		{
			_logger.LogDebug("{readerName} event handler registered", reader.Name);
			// This will be called regardless of which reader is pressed
			reader.ButtonPressed += ReaderButtonPressed;

			// Will start the BG worker.
			reader.Start();
		}

		_logger.LogInformation("{serviceName} started", nameof(AnnunciatorService));
		await base.StartAsync(cancellationToken);
	}


	public async override Task StopAsync(CancellationToken cancellationToken)
	{
		foreach (var reader in _readers)
		{
			_logger.LogTrace("{readerName} event handler unregistered", reader.Name);
			reader.ButtonPressed -= ReaderButtonPressed;

			// Stop the BG worker
			reader.Stop();
		}

		_logger.LogInformation("{serviceName} stopped", nameof(AnnunciatorService));

		await base.StopAsync(cancellationToken);
	}

	#endregion BackgroundService

	private void ReaderButtonPressed(object? sender, EventArgs e)
	{
		if (sender is IButtonReader reader)
		{
			ReaderButtonPressed(reader, CancellationToken.None);
		}
		else if (sender is null)
		{
			// This should never happen.
			_logger.LogError("Sender is null.");
		}
		else
		{
			// This should never happen.
			_logger.LogError("Unknown sender type: {type}", sender.GetType().Name);
		}
	}

	private async void ReaderButtonPressed(IButtonReader reader, CancellationToken cancellationToken)
	{
		if (currentlyAnnouncing)
		{
			_logger.LogDebug("Already announcing. Ignoring button press.");
			return;
		}

		_logger.LogInformation("{readerName} button pressed", reader.Name);

		currentlyAnnouncing = true;

		IReadOnlyCollection<Departure>? departures;
		try
		{
			departures = await _kioskRealTimeClient.GetRealtime(_config.Id, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch departures");
			currentlyAnnouncing = false;
			return;
		}

		_logger.LogDebug("Fetched {count} departures for {kioskId}", departures?.Count ?? -1, _config.Id);

		try
		{
			await _annunciator.ReadDepartures(_config.Name, departures, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to announce departures");
		}

		currentlyAnnouncing = false;
	}

}
