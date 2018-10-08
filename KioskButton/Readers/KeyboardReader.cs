using System;
using System.Windows.Forms;
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

		private bool _pressed;
		public bool Pressed
		{
			get
			{
				var pressed = _pressed;
				if (pressed)
				{
					_pressed = false;
				}
				return pressed;
			}
		}

		protected KeyboardReader(Keys key, KeyModifiers modifiers, LogWriter logger)
		{
			Logger = logger ?? throw new ArgumentException(nameof(logger));
			HotKeyManager.RegisterHotKey(key, modifiers);
			HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
		}

		public void Dispose() { }

		private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
		{
			Logger.Debug("Key pressed");
			_pressed = true;
		}

	}
}
