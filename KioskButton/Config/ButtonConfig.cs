using NLog;

namespace Cumtd.Signage.Kiosk.KioskButton.Config
{
	internal class ButtonConfig
	{
		public string Id { get; set; }
		public bool HideConsole { get; set; }
		public string DisplayName { get; set; }
		public Readers Readers { get; set; }
		public Logging Logging { get; set; }

	}

	internal class Readers
	{
		public bool UseSeaDac { get; set; }
		public bool UsePanicButton { get; set; }
	}

	internal class Logging
	{
		public string File { get; set; }
		public LogLevel FileLogLevel => GetLevel(File);
		public string Event { get; set; }
		public LogLevel EventLogLevel => GetLevel(Event);

		private static LogLevel GetLevel(string level)
		{
			switch (level)
			{
				case "trace": return LogLevel.Trace;
				case "debug": return LogLevel.Debug;
				case "info": return LogLevel.Info;
				case "warn": return LogLevel.Warn;
				case "error": return LogLevel.Error;
				case "fatal": return LogLevel.Fatal;
				case "off": return LogLevel.Off;
				default: return LogLevel.Info;
			}
		}
	}


}
