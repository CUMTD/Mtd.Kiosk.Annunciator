using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Readers.Raspi.Config;
using System.Device.Gpio;

namespace Mtd.Kiosk.Annunciator.Readers.Raspi;

public sealed class PiReader : IButtonReader, IDisposable
{
    private readonly ILogger<PiReader> _logger;

    private readonly int[] _gpioPins;
    private bool disposedValue;

    private GpioController? _controller;

    public event EventHandler? ButtonPressed;

    public string Name => "Raspberry Pi Reader";

    public PiReader(IOptions<PiReaderConfig> options, ILogger<PiReader> logger)
    {
        ArgumentNullException.ThrowIfNull(options?.Value, nameof(options.Value));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.Value.Pins.Length, 0, nameof(options.Value.Pins));

        for (var pin = 0; pin < options.Value.Pins.Length; pin++)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.Value.Pins[pin], 0, nameof(options.Value.Pins));
        }

        _logger = logger;

        _gpioPins = options.Value.Pins;

        _controller = new GpioController();
    }

    private void Callback(object sender, PinValueChangedEventArgs args)
    {
        ButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    public void Start()
    {
        for (var pin = 0; pin < _gpioPins.Length; pin++)
        {
            _logger.LogDebug("Opening pin {pin}", _gpioPins[pin]);
            _controller?.OpenPin(_gpioPins[pin], PinMode.InputPullUp);
            Thread.Sleep(500); // prevent a button press on pin open
            _controller?.RegisterCallbackForPinValueChangedEvent(_gpioPins[pin], PinEventTypes.Rising | PinEventTypes.Falling, Callback);
        }
    }
    public void Stop()
    {
        for (var pin = 0; pin < _gpioPins.Length; pin++)
        {
            _controller?.UnregisterCallbackForPinValueChangedEvent(_gpioPins[pin], Callback);
            _controller?.ClosePin(_gpioPins[pin]);
        }

    }

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

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
