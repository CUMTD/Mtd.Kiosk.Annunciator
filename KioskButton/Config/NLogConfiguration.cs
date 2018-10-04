using NLog;
using NLog.Config;
using NLog.Targets;

namespace Cumtd.Signage.Kiosk.KioskButton.Config
{
	internal static class NLogConfiguration
	{
		public static LogFactory BuildLogFactory(Logging loggingConfig) => new LogFactory
		{
			Configuration = GetConfig(loggingConfig)
		};

		private static LoggingConfiguration GetConfig(Logging loggingConfig)
		{
			var config = new LoggingConfiguration();

			var nullTarget = new NullTarget();
			config.AddTarget("null", nullTarget);

			var consoleTarget = new ColoredConsoleTarget();
			config.AddTarget("console", consoleTarget);

			var eventLogTarget = new EventLogTarget
			{
				Source = "Kiosk Annunciator Button Service",
				Layout = "${message}${newline}${newline}${exception:format=ToString}"
			};
			config.AddTarget("event", eventLogTarget);

			// and rules
			config.LoggingRules.Add(new LoggingRule("*", loggingConfig.ConsoleLogLevel, consoleTarget));
			config.LoggingRules.Add(new LoggingRule("*", loggingConfig.EventLogLevel, eventLogTarget));

			return config;
		}
	}
}
