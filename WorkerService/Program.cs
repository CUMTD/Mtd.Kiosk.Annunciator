using KioskAnnunciatorButton.Reader;
using KioskAnnunciatorButton.RealTime;
using KioskAnnunciatorButton.WorkerService.Readers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace KioskAnnunciatorButton.WorkerService
{
	public sealed class Program
	{
		public static void Main(string[] args) => CreateHostBuilder(args)
				.Build()
				.Run();

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host
			.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration(config =>
			{
				config.AddJsonFile("appsettings.json", false, true);
			})
			.ConfigureServices((hostContext, services) =>
			{
				services
					.AddTransient(f =>
					{
						var logger = f
							.GetRequiredService<ILogger<DepartureReader>>();

						return new DepartureReader(
							hostContext.Configuration.GetValue<string>("azure:searchKey"),
							hostContext.Configuration.GetValue<string>("azure:searchRegion"),
							logger
						);
					});

				services.AddTransient<RealTimeClient>();

				services.AddTransient<IReader, SeaLevelButtonReader>();

				services.AddHostedService<Worker>();
			})
			.ConfigureLogging((hostContext, logging) =>
			{
				Log.Logger = new LoggerConfiguration()
					.ReadFrom
					.Configuration(hostContext.Configuration)
					.CreateLogger();

				logging.ClearProviders();
				logging.AddSerilog();
			});
	}

}
