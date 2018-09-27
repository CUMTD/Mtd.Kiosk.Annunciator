using System;
using System.ComponentModel;

namespace Cumtd.Signage.Kiosk.SeaLevel
{
	public sealed class ButtonReader
	{
		private BackgroundWorker Worker { get; set; }
		private Action<bool> Callback { get; }
		public bool IsRunning { get; private set; }
		public bool IsFinished { get; private set; }

		public ButtonReader(Action<bool> callback)
		{
			Callback = callback ?? throw new ArgumentException(nameof(callback));
			IsFinished = false;
		}

		public void Start()
		{
			if (IsRunning)
			{
				Console.WriteLine("Already running");
			}
			else if (IsFinished)
			{
				Console.WriteLine("Finished. Can't run again");
			}
			else
			{
				Console.WriteLine("Starting Worker");
				Worker = GetWorker();
				IsRunning = true;
				Worker.RunWorkerAsync();
			}
		}

		public void Stop()
		{
			Worker?.CancelAsync();
			IsFinished = true;
		}

		private BackgroundWorker GetWorker()
		{
			var worker = new BackgroundWorker
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};

			worker.DoWork += BackgroundWorker_DoWork;
			worker.ProgressChanged += BackgroundWorker_ProgressChanged;
			worker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

			return worker;
		}

		#region BackgroundWorker Event Handlers

		private static void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var bgWorker = (BackgroundWorker)sender;

			using (var seaDacButton = new SeaDacButton())
			{
				// the last button state
				bool? lastState = null;
				var bytes = new byte[1];

				while (!bgWorker.CancellationPending)
				{
					// listen if not already listening
					seaDacButton.ListenChange(ref bytes, 16);

					// 1 == true
					var current = bytes[0] >= 1;

					// last state is diferent from curent
					if (!lastState.HasValue || current != lastState.Value)
					{
						bgWorker.ReportProgress(current ? 1 : 0);
					}

					// update last state
					lastState = current;
				}

			}

			e.Cancel = true;
		}

		private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			var pushed = e.ProgressPercentage == 1;
			Callback(pushed);
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			IsRunning = false;
			Console.WriteLine("RunWorkerCompleted");
		}

		#endregion BackgroundWorker Event Handlers
	}
}
