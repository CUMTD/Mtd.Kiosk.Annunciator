using System;
using System.IO;
using System.Reflection;
using KioskAnnunciatorButton.Annunciator;
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
		public static void Main(string[] args)
		{
			try
			{
				CreateHostBuilder(args)
					.Build()
					.Run();
			}
			catch (Exception ex)
			{
				Log.Logger.Fatal(ex, "Application unable to start");
				throw ex;
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host
			.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration(config =>
			{
				config
					.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
					.AddJsonFile("appsettings.json", false, true);
			})
			.ConfigureServices((hostContext, services) =>
			{
				services
					.AddTransient(f => new AzureAnnunciator(
						hostContext.Configuration.GetValue<string>("azure:searchKey"),
						hostContext.Configuration.GetValue<string>("azure:searchRegion"),
						f.GetRequiredService<ILogger<AzureAnnunciator>>()
					));

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
