using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Cumtd.Signage.Kiosk.KioskButton.Config
{
	public static class NLogConfiguration
	{
		private static readonly Lazy<LogFactory> _instance = new Lazy<LogFactory>(BuildLogFactory);
		public static LogFactory Instance => _instance.Value;

		private static LogFactory BuildLogFactory() => new LogFactory
		{
			Configuration = GetConfig()
		};

		private static LoggingConfiguration GetConfig()
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
			config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, consoleTarget));
			config.LoggingRules.Add(new LoggingRule("*", LogLevel.Warn, eventLogTarget));

			return config;
		}
	}
}
