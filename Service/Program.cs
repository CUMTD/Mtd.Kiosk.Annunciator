using System;
using System.Linq;
using System.Threading.Tasks;
using KioskAnnunciatorButton.Reader;
using KioskAnnunciatorButton.RealTime;
using KioskAnnunciatorButton.Service.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace KioskAnnunciatorButton.Service
{
	internal sealed class Program
	{
		private static readonly ProgramConfig _config = GetConfig();
		private static readonly IServiceProvider _serviceProvider = GetServiceProvider();
		public async static Task Main()
		{
			var reader = _serviceProvider
				.GetRequiredService<DepartureReader>();

			var client = _serviceProvider
				.GetRequiredService<RealTimeClient>();

			var departures = await client
				.GetRealtime(_config.Kiosk.Id);

			await reader
				.ReadDepartures(_config.Kiosk.DisplayName, departures.ToArray());

			var logger = _serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
			logger.LogInformation("Done");

			DisposeServices();
		}

		#region Bootstrap

		private static ProgramConfig GetConfig()
		{
			var appConfig = new ConfigurationBuilder()
				.AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
				.Build();
			var config = new ProgramConfig();
			appConfig.Bind(config);
			return config;
		}

		private static IServiceProvider GetServiceProvider()
		{
			ConfigureLogger();

			var services = new ServiceCollection();

			services
				.AddLogging(config =>
				{
					config.ClearProviders();
					config.AddSerilog();
				});

			services.AddTransient<Program>();

			services
				.AddTransient(f =>
				{
					var logger = f
						.GetRequiredService<ILogger<DepartureReader>>();

					return new DepartureReader(
						_config.Azure.SearchKey,
						_config.Azure.SearchRegion,
						logger
					);
				});

			services.AddTransient<RealTimeClient>();

			return services
				.BuildServiceProvider(true);
		}

		private static void ConfigureLogger()
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appSettings.json", false, true)
				.Build();

			Log.Logger = new LoggerConfiguration()
				.ReadFrom
				.Configuration(configuration)
				.CreateLogger();
		}

		#endregion Bootstrap

		private static void DisposeServices()
		{
			if (_serviceProvider == null)
			{
				return;
			}
			if (_serviceProvider is IDisposable)
			{
				((IDisposable)_serviceProvider).Dispose();
			}
		}

	}
}
