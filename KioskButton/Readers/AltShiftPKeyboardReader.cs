using System.Windows.Forms;
using NLog;

namespace Cumtd.Signage.Kiosk.KioskButton.Readers
{
	/// <inheritdoc />
	/// <summary>
	/// Responds to the keyboard combination Alt + Shift + P
	/// </summary>
	public sealed class AltShiftPKeyboardReader : KeyboardReader
	{
		public override string Name => "Alt + Shift + P Keyboard Reader";

		public AltShiftPKeyboardReader(ILogger logger) : base(Keys.P, KeyModifiers.Alt | KeyModifiers.Shift, logger)
		{
		}
	}
}
