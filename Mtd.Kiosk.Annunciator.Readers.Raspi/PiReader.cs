using System.Collections.Immutable;
using System.Device.Gpio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Readers.Raspi.Config;

namespace Mtd.Kiosk.Annunciator.Readers.Raspi;

public sealed class PiReader : ButtonReader, IButtonReader, IDisposable
{
	public const string KEY = "RASPI";

	private readonly ImmutableArray<int> _gpioPins;

	private bool disposedValue;
	private GpioController? _controller;
	public override string Name => "Raspberry Pi Reader";

	public PiReader(IOptions<PiReaderConfig> options, ILogger<PiReader> logger) : base(logger)
	{
		ArgumentNullException.ThrowIfNull(options?.Value, nameof(options.Value));
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.Value.Pins.Length, 0, nameof(options.Value.Pins));

		for (var pin = 0; pin < options.Value.Pins.Length; pin++)
		{
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.Value.Pins[pin], 0, nameof(options.Value.Pins));
		}

		_gpioPins = [.. options.Value.Pins];
		_controller = new GpioController();
	}
	public override void Start()
	{
		if (disposedValue)
		{
			throw new Exception("Object is disposed. Cannot start.");
		}

		if (_controller is null)
		{
			throw new Exception("Controller is null. Cannot start.");
		}

		for (var pin = 0; pin < _gpioPins.Length; pin++)
		{
			_logger.LogDebug("Opening pin {pin}", _gpioPins[pin]);
			_controller.OpenPin(_gpioPins[pin], PinMode.InputPullUp);

			Thread.Sleep(500); // prevent a button press on pin open
			_controller.RegisterCallbackForPinValueChangedEvent(_gpioPins[pin], PinEventTypes.Rising | PinEventTypes.Falling, Callback);
		}

		_logger.LogInformation("{readerName} started", Name);
	}
	public override void Stop()
	{
		for (var pin = 0; pin < _gpioPins.Length; pin++)
		{
			_logger.LogDebug("Closing pin {pin}", _gpioPins[pin]);
			_controller?.UnregisterCallbackForPinValueChangedEvent(_gpioPins[pin], Callback);
			_controller?.ClosePin(_gpioPins[pin]);
		}

		_logger.LogInformation("{readerName} stopped", Name);
	}
	private void Callback(object sender, PinValueChangedEventArgs args)
	{
		_logger.LogDebug("Button {changeType} on pin {pin}", args.ChangeType, args.PinNumber);
		ButtonPressPending = true;
	}

	#region IDisposable
	private void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_controller?.Dispose();
			}

			_controller = null;
			disposedValue = true;
		}
	}

	public override void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#endregion IDisposable

}
