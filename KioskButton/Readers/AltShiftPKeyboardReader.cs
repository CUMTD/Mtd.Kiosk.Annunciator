using System.Windows.Forms;
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

		public AltShiftPKeyboardReader(LogWriter logger) : base(Keys.P, KeyModifiers.Alt | KeyModifiers.Shift, logger)
		{
		}
	}
}
