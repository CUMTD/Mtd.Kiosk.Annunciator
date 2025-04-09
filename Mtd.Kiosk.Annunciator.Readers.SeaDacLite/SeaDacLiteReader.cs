using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Mtd.Kiosk.Annunciator.Core;
using Sealevel;

namespace Mtd.Kiosk.Annunciator.Readers.SeaDacLite;

public sealed class SeaDacReader(ILogger<SeaDacReader> logger) : ButtonReader(logger), IDisposable
{
	public override string Name => "SeaDAC Lite Reader";
	public const string KEY = "SEADAC";


	private readonly SeaMAX _seaMax = new();
	private BackgroundWorker? _worker;
	private bool _isDisposed = false;
	private bool _isRunning = false;
	private readonly byte[] _bytes = new byte[1]; // stores current button state (byte[] required by SeaMAX library)
	private bool _lastState = false;

	public override void Start()
	{
		if (_isDisposed)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, Name);
		}

		if (_isRunning)
		{
			_logger.LogWarning("{Name} is already running", Name);
			return;
		}

		// start
		OpenRead();
		_worker = new BackgroundWorker { WorkerSupportsCancellation = true };
		_worker.DoWork += DoWork;
		_isRunning = true;
		_worker.RunWorkerAsync();
		_logger.LogInformation("{Name} started", Name);
	}

	private void OpenRead()
	{
		// initialization code from v2
		if (!_seaMax.IsSeaDACInitialized)
		{
			var initResult = _seaMax.SDL_Initialize();
			if (initResult < 0)
			{
				throw new Exception($"SDL_Initialize failed: {initResult}");
			}

			var deviceCount = _seaMax.SDL_SearchForDevices();
			if (deviceCount < 0)
			{
				throw new Exception($"SDL_SearchForDevices failed: {deviceCount}");
			}
			if (deviceCount == 0)
			{
				throw new Exception("No SDL devices found");
			}

			if (_seaMax.SDL_FirstDevice() < 0)
			{
				throw new Exception("SDL_FirstDevice failed");
			}
		}

		if (!_seaMax.IsSeaMAXOpen)
		{
			var id = 0;
			if (_seaMax.SDL_GetDeviceID(ref id) < 0)
			{
				throw new Exception("SDL_GetDeviceID failed");
			}

			if (_seaMax.SM_Open($"SeaDAC Lite {id}") < 0)
			{
				throw new Exception("SM_Open failed");
			}
		}
	}

	private void DoWork(object? sender, DoWorkEventArgs e)
	{
		// get the backgroundWorker that raised the event
		var worker = (BackgroundWorker)sender!;

		// check if ok to continue
		while (!worker.CancellationPending)
		{
			// poll button state every 10ms
			try
			{
				CheckButtonState();
				Thread.Sleep(10);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error monitoring button state");
			}
		}
		CleanupAfterStop();
	}

	private void CheckButtonState()
	{

		if (_seaMax.SM_NotifyInputState(0) != 2)
		{
			_seaMax.SM_NotifyOnInputChange(0, 4, _bytes, 16, 0);
		}

		// 1 = pressed, 0 = not pressed
		var currentState = _bytes[0] >= 1;
		if (currentState == _lastState)
		{
			return;
		}

		_logger.LogDebug("Button state changed to {State}", currentState);
		if (currentState)
		{
			ButtonPressPending = true;
		}

		_lastState = currentState;
	}

	private void CleanupAfterStop()
	{
		_seaMax.SM_NotifyInputState(1);
		_isRunning = false;
	}

	public override void Stop()
	{
		if (_worker?.IsBusy == true)
		{
			_worker.CancelAsync();
		}

		Dispose();
		_logger.LogInformation("{Name} stopped", Name);
	}

	public override void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}

		if (_seaMax.IsSeaMAXOpen)
		{
			_seaMax.SM_NotifyInputState(1);
			_seaMax.SM_Close();
		}

		if (_seaMax.IsSeaDACInitialized)
		{
			_seaMax.SDL_Cleanup();
		}

		_worker?.Dispose();
		_isDisposed = true;
		// base.Dispose(); // Remove this line to fix CS0205 error
	}
}
