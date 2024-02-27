using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mtd.Kiosk.Annunciator.Core;

namespace Mtd.Kiosk.Annunciator.Service;
internal class AnnunciatorService : BackgroundService, IDisposable
{
	private readonly IReadOnlyCollection<IButtonReader> _readers;
	private readonly ILogger<AnnunciatorService> _logger;
	public AnnunciatorService(IEnumerable<IButtonReader> readers, ILogger<AnnunciatorService> logger)
	{
		ArgumentNullException.ThrowIfNull(nameof(readers));
		_readers = readers.ToImmutableArray();
		ArgumentOutOfRangeException.ThrowIfZero(_readers.Count, nameof(readers));

		ArgumentNullException.ThrowIfNull(nameof(logger));
		_logger = logger;
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
			ReaderButtonPressed(reader);
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

	private void ReaderButtonPressed(IButtonReader reader)
	{
		// Do the actual work of reading reading.
		_logger.LogInformation("{readerName} button pressed", reader.Name);
	}

}
