using System;
using System.IO;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using static System.Environment;

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

			var loggingFile = new FileInfo(Path.Combine(Environment.GetFolderPath(SpecialFolder.Desktop), "annunciator_log.txt"));
			if (!loggingFile.Exists)
			{
				loggingFile.Create();
			}
			var fileTarget = new FileTarget
			{
				Encoding = Encoding.UTF8,

				Name = "fileTarget",
				ArchiveFileName="logging-{###}.txt",
				ArchiveNumbering= ArchiveNumberingMode.DateAndSequence,
				ArchiveEvery = FileArchivePeriod.Day,
				WriteBom = false,
				FileName = loggingFile.FullName,
				MaxArchiveDays = 7,
				Layout= "${message}${newline}${newline}${exception:format=ToString}"
			};

			var eventLogTarget = new EventLogTarget
			{
				Source = "Kiosk Annunciator Button Service",
				Layout = "${message}${newline}${newline}${exception:format=ToString}"
			};

			// and rules
			config.AddRule(loggingConfig.FileLogLevel, LogLevel.Fatal, fileTarget, "*");
			config.AddRule(loggingConfig.EventLogLevel, LogLevel.Fatal, eventLogTarget, "*");

			return config;
		}
	}
}
