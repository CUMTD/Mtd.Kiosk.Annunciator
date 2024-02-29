using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Mtd.Kiosk.Annunciator.Core;
public abstract class BackgroundButtonReader : IButtonReader, IDisposable
{
	// Use a BackgroundWorker to run the DetectButtonPress method in its own thread
	protected BackgroundWorker? _worker;

	// Make cancelation token available to the derived class
	protected bool CancelPending => _worker?.CancellationPending ?? false;

	protected readonly ILogger<BackgroundButtonReader> _logger;


	protected bool _isBackgroundWorkerCurrentlyRunning;
	protected bool _isDisposed;

	public event EventHandler? ButtonPressed;

	public BackgroundButtonReader(ILogger<BackgroundButtonReader> logger)
	{
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));
		_logger = logger;
	}

	// This is where the button detecting logic should go.
	// The derived class should implement this method and return true if the button is pressed.
	// The method will be awaited in a loop in the BackgroundWorker_DoWork method each time it returns.
	public abstract Task<bool> DetectButtonPress(CancellationToken cancellationToken);

	#region IButtonReader
	public abstract string Name { get; }

	public void Start()
	{
		if (_isBackgroundWorkerCurrentlyRunning)
		{
			_logger.LogWarning("{workerName} is already running", Name);
		}
		else if (_isDisposed)
		{
			_logger.LogError("{workerName} has already been disposed", Name);
		}
		else
		{
			// create a new BackgroundWorker and start it
			_logger.LogInformation("Starting worker");
			_worker = CreateWorker();
			_isBackgroundWorkerCurrentlyRunning = true;
			_worker.RunWorkerAsync();
		}
	}
	public void Stop()
	{
		_worker?.CancelAsync();
		Dispose();
		_logger.LogInformation("Stopped worker {workerName}", Name);
	}

	#endregion IButtonReader

	#region BackgroundWorker Event Handlers

	private async void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
	{
		if (sender is not BackgroundWorker bgWorker)
		{
			// this should never happen
			_logger.LogError("BackgroundWorker is null");
			return;
		}
		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Token.Register(() => bgWorker.CancelAsync());

		// Run the DetectButtonPress method in a loop until the BackgroundWorker is canceled
		while (!bgWorker.CancellationPending)
		{
			var pressed = await DetectButtonPress(cancellationTokenSource.Token);
			if (pressed)
			{
				ButtonPressed?.Invoke(this, EventArgs.Empty);
			}
		}

		// The BackgroundWorker has been canceled
		_isBackgroundWorkerCurrentlyRunning = false;
		Dispose();
	}


	#endregion BackgroundWorker Event Handlers

	private BackgroundWorker CreateWorker()
	{
		// create a new worker and register do work event
		var worker = new BackgroundWorker
		{
			WorkerReportsProgress = true,
			WorkerSupportsCancellation = true
		};

		worker.DoWork += BackgroundWorker_DoWork;

		return worker;
	}

	#region IDisposable

	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_worker?.Dispose();
				_worker = null;
			}

			_isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#endregion IDisposable

}
