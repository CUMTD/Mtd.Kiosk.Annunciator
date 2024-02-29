using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Readers.Simple.Config;

namespace Mtd.Kiosk.Annunciator.Readers.Simple;

public sealed class PressEveryNSecondsReader : BackgroundButtonReader, IButtonReader
{
	public override string Name => "Press Every N Seconds Reader";

	private readonly TimeSpan _interval;

	public PressEveryNSecondsReader(IOptions<PressEveryNSecondsReaderConfig> options, ILogger<PressEveryNSecondsReader> logger) : base(logger)
	{
		ArgumentNullException.ThrowIfNull(options?.Value, nameof(options));
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.Value.Seconds, 1, nameof(options.Value.Seconds));

		_interval = TimeSpan.FromSeconds(options.Value.Seconds);
	}

	public async override Task<bool> DetectButtonPress(CancellationToken cancellationToken)
	{
		await Task.Delay(_interval);
		return true;
	}
}
