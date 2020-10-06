using KioskAnnunciatorButton.SeaLevelReader;
using Microsoft.Extensions.Logging;

namespace KioskAnnunciatorButton.WorkerService.Readers
{
	internal class SeaLevelButtonReader : IReader
	{
		public string Name => "Sea Level Button Reader";

		private bool _pressed;

		private readonly ButtonReader _reader;

		public bool Pressed { get; private set; }

		public SeaLevelButtonReader(ILogger<ButtonReader> logger)
		{
			_pressed = false;
			_reader = new ButtonReader(PressedChangeCallback, logger);
		}

		public void Start() => _reader.Start();

		public void Stop() => _reader.Stop();

		private void PressedChangeCallback(bool pressed)
		{
			if (pressed)
			{
				_pressed = true;
			}
		}

		public bool GetPressed()
		{
			var toResturn = _pressed;
			_pressed = false;
			return toResturn;
		}
	}
}
