using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cumtd.Signage.Kiosk.KioskButton.Config;
using Newtonsoft.Json;
using NLog;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal sealed class ConfigurationManager
	{
		private static readonly AsyncLazy<ConfigurationManager> _config =
			new AsyncLazy<ConfigurationManager>(async () =>
			{
				var configManager = new ConfigurationManager();
				await configManager.AsyncInit().ConfigureAwait(false);
				return configManager;
			});

		public static Task<ConfigurationManager> Config => _config.Value;

		public ButtonConfig ButtonConfig { get; }
		public Logger Logger { get; }
		public string Name { get; private set; }

		private ConfigurationManager()
		{
			var settings = ReadSettings<ButtonConfig>();
			ButtonConfig = settings;
			Logger = NLogConfiguration.BuildLogFactory(ButtonConfig.Logging).GetLogger("kiosk-annunciator");
			
		}

		private async Task AsyncInit() =>
			Name = await GetName(ButtonConfig.Id).ConfigureAwait(false);

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

		private static async Task<string> GetName(string id)
		{
			HttpResponseMessage response;
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				response = await client
					.GetAsync($"https://kiosk.mtd.org/umbraco/api/settings/kiosk/?id={id}")
					.ConfigureAwait(false);
			}
			var json = await response
				.Content
				.ReadAsStringAsync()
				.ConfigureAwait(false);

			var settings = JsonConvert.DeserializeObject<SignSettings>(json);
			return settings.DisplayName;
		}

	}
}
