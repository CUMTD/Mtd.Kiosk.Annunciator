using System;
using Topshelf.Logging;

namespace Cumtd.Signage.Kiosk.KioskButton.Readers
{
	/// <summary>
	/// Responds to keyboard input
	/// </summary>
	public abstract class KeyboardReader : IButtonReader
	{
		public abstract string Name { get; }

		private LogWriter Logger { get; }

		public bool Pressed
		{
			get
			{
				var state = Console.KeyAvailable && Match(Console.ReadKey());
				if (state)
				{
					Logger.Debug($"{Name} pressed");
				}
				return state;
			}
		}

		protected KeyboardReader(LogWriter logger)
		{
			Logger = logger ?? throw new ArgumentException(nameof(logger));
		}

		public void Dispose() { }

		protected abstract bool Match(ConsoleKeyInfo press);
	}
}
