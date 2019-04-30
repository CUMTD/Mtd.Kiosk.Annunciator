using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal static class Program
	{
		#region Console Hide Imports

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private const int SW_HIDE = 0;
		private const int SW_SHOW = 5;

		#endregion Console Hide Imports

		#region Hotkey Config

		private const KeyModifiers QUIT_MODIFIERS = KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift;
		private const Keys QUIT_KEY = Keys.Q;

		private const KeyModifiers TOGGLE_CONSOLE_MODIFIERS = KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift;
		private const Keys TOGGLE_CONSOLE_KEY = Keys.C;

		#endregion Hotkey Config

		private static ConfigurationManager Config;

		private static AnnunciatorService Service;

		private static bool ShowConsole;

		private static async Task Main()
		{
			// get the application config
			// this does a roundtrip to the server for the app name
			Config = await ConfigurationManager.Config;

			// register global quit and console toggle hotkeys
			HotKeyManager.RegisterHotKey(QUIT_KEY, QUIT_MODIFIERS);
			HotKeyManager.RegisterHotKey(TOGGLE_CONSOLE_KEY, TOGGLE_CONSOLE_MODIFIERS);
			HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

			// show or hide the console window based on the application config
			ToggleConsoleWindow(!Config.ButtonConfig.HideConsole);

			// Start the annunciation service
			// This will actually do all the work
			Service = new AnnunciatorService(Config);
			Service.Start();

			// keep the console from quitting
			var result = string.Empty;
			while (!string.Equals(result, "quit", StringComparison.CurrentCultureIgnoreCase))
			{
				Console.WriteLine("Type 'quit' to exit");
				result = Console.ReadLine();
			}

		}

		private static void ToggleConsoleWindow(bool? show = null)
		{
			var showConsole = show ?? !ShowConsole;
			ShowConsole = showConsole;

			Config.Logger.Debug($"Toggling console: {(showConsole ? "Show" : "Hide")}");

			var handle = GetConsoleWindow();
			ShowWindow(handle, showConsole ? SW_SHOW : SW_HIDE);
		}

		private static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
		{
			// ReSharper disable once SwitchStatementMissingSomeCases
			switch (e.Key)
			{
				// quit
				case QUIT_KEY when e.Modifiers == QUIT_MODIFIERS:
					Config.Logger.Debug("Quit key pressed, exiting");
					Environment.Exit(0);
					break;
				// toggle console
				case TOGGLE_CONSOLE_KEY when e.Modifiers == TOGGLE_CONSOLE_MODIFIERS:
					ToggleConsoleWindow();
					break;
			}
		}

	}
}
