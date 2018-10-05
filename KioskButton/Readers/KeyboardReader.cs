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
				Logger.Debug($"Reading {Name}");
				var state = Console.KeyAvailable && Match(Console.ReadKey());
				Logger.Debug($"{Name} {(state ? "pressed" : "not pressed")}");
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
