using System;
using System.IO;
using Cumtd.Signage.Kiosk.KioskButton.Config;
using Newtonsoft.Json;
using NLog;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal sealed class ConfigurationManager
	{
		public ButtonConfig ButtonConfig { get; }
		public Logger Logger { get; }

		public static readonly Lazy<ConfigurationManager> Config = new Lazy<ConfigurationManager>(() => new ConfigurationManager());

		private ConfigurationManager()
		{
			ButtonConfig = ReadSettings<ButtonConfig>();
			using (var logFactory = NLogConfiguration.BuildLogFactory(ButtonConfig.Logging))
			{
				Logger = logFactory.GetLogger("kiosk-annunciator");
			}
		}

		private static T ReadSettings<T>()
		{
			var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
			var fileNameBase = typeof(T).Name;
			var file = new FileInfo(Path.Combine(basePath, $"{fileNameBase}.json"));

#if DEBUG
			var altFile = new FileInfo(Path.Combine(basePath, $"{fileNameBase}.debug.json"));
			if (altFile.Exists)
			{
				file = altFile;
			}
#endif

			if (file.Exists)
			{
				var contents = File.ReadAllText(file.FullName);
				return JsonConvert.DeserializeObject<T>(contents);
			}
			throw new FileNotFoundException($"Can't find config file {file.FullName}");
		}
	}
}
