using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Readers.Simple.Config;

namespace Mtd.Kiosk.Annunciator.Readers.Simple;

public class PressEveryNSecondsReader : IButtonReader
{
	private bool isRunning = false;
	private readonly TimeSpan _interval;
	private readonly ILogger<PressEveryNSecondsReader> _logger;

	public event EventHandler? ButtonPressed;

	public PressEveryNSecondsReader(IOptions<PressEveryNSecondsReaderConfig> options, ILogger<PressEveryNSecondsReader> logger)
	{
		ArgumentNullException.ThrowIfNull(options?.Value, nameof(options));
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.Value.Seconds, 1, nameof(options.Value.Seconds));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_interval = TimeSpan.FromSeconds(options.Value.Seconds);
		_logger = logger;
	}

	public string Name => "Press Every N Seconds Reader";

	public async Task Start(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Starting {name} reader", Name);

		isRunning = true;
		while (!cancellationToken.IsCancellationRequested && isRunning)
		{
			await Task.Delay(_interval, cancellationToken);

			_logger.LogInformation("Button pressed by {name}", Name);
			ButtonPressed?.Invoke(this, EventArgs.Empty);
		}
		isRunning = false;
	}

	public Task Stop(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopping {name} reader", Name);
		isRunning = false;
		return Task.CompletedTask;
	}
}
