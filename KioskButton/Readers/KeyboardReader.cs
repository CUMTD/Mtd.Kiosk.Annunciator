using System;
using System.Windows.Forms;
using NLog;

namespace Cumtd.Signage.Kiosk.KioskButton.Readers
{
	/// <summary>
	/// Responds to keyboard input
	/// </summary>
	public abstract class KeyboardReader : IButtonReader
	{
		public abstract string Name { get; }

		private ILogger Logger { get; }

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

		private Keys Key { get; }
		private KeyModifiers Modifiers { get; }

		protected KeyboardReader(Keys key, KeyModifiers modifiers, ILogger logger)
		{
			Key = key;
			Modifiers = modifiers;
			Logger = logger ?? throw new ArgumentException(nameof(logger));
			HotKeyManager.RegisterHotKey(Key, Modifiers);
			HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
		}

		public void Dispose() { }

		private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
		{
			if (e.Key == Key && e.Modifiers == Modifiers)
			{
				Logger.Debug($"{Name} pressed");
				_pressed = true;
			}
		}

	}
}
