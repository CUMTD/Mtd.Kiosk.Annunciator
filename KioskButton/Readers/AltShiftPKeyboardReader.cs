using System;
using Topshelf.Logging;

namespace Cumtd.Signage.Kiosk.KioskButton.Readers
{
	/// <inheritdoc />
	/// <summary>
	/// Responds to the keyboard combination Alt + Shift + P
	/// </summary>
	public sealed class AltShiftPKeyboardReader : KeyboardReader
	{
		public override string Name => "Alt + Shift + P Keyboard Reader";


		public AltShiftPKeyboardReader(LogWriter logger) : base(logger)
		{
		}

		protected override bool Match(ConsoleKeyInfo press) =>
			press.Key == ConsoleKey.P && press.Modifiers == (ConsoleModifiers.Alt | ConsoleModifiers.Shift);
	}
}
