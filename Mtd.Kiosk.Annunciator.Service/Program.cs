using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mtd.Kiosk.Annunciator.Azure;
using Mtd.Kiosk.Annunciator.Azure.Config;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Core.Config;
using Mtd.Kiosk.Annunciator.Readers.Raspi;
using Mtd.Kiosk.Annunciator.Readers.Raspi.Config;
using Mtd.Kiosk.Annunciator.Realtime.UmbracoApi;
using Mtd.Kiosk.Annunciator.Service;
using Mtd.Kiosk.Annunciator.Service.Extensions;
using Serilog;

try
{
	var host = Host
		.CreateDefaultBuilder(args)
		.ConfigureAppConfiguration((context, config) =>
		{
			var basePath = context.HostingEnvironment.ContentRootPath;
			config
				.SetBasePath(basePath)
				.AddJsonFile("appsettings.json", true, true)
				.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
				.AddEnvironmentVariables("Mtd_Kiosk_Annunciator_Service_");

			if (context.HostingEnvironment.IsDevelopment())
			{
				var assembly = Assembly.GetExecutingAssembly();
				config.AddUserSecrets(assembly, true);
			}
		})
		.ConfigureServices((context, services) =>
		{
			_ = services
				.Configure<HostOptions>(hostOptions =>
				{
					hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
				});

			// Options
			_ = services
				.Configure<PiReaderConfig>(context.Configuration.GetSection(PiReaderConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<PiReaderConfig>(PiReaderConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(PiReaderConfig.ConfigSectionName));

			_ = services
				.Configure<RealTimeClientConfig>(context.Configuration.GetSection(RealTimeClientConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<RealTimeClientConfig>(RealTimeClientConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(RealTimeClientConfig.ConfigSectionName));

			_ = services
				.Configure<KioskConfig>(context.Configuration.GetSection(KioskConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<KioskConfig>(KioskConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(KioskConfig.ConfigSectionName));

			_ = services
				.Configure<AzureAnnunciatorConfig>(context.Configuration.GetSection(AzureAnnunciatorConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<AzureAnnunciatorConfig>(AzureAnnunciatorConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(AzureAnnunciatorConfig.ConfigSectionName));

			// Readers
			_ = services.AddSingleton<IButtonReader, PiReader>();

			// Clients
			_ = services.AddHttpClient<IKioskRealTimeClient, UmbracoApiRealtimeClient>(client =>
			{
				client.DefaultRequestHeaders.Add("Accept", "application/json");
			});

			// Annunciators
			_ = services.AddSingleton<IAnnunciator, AzureAnnunciator>();

			// Services
			_ = services.AddHostedService<AnnunciatorService>();

		})
		.UseSerilog((context, loggingConfig) =>
		{
			loggingConfig
				.ReadFrom
				.Configuration(context.Configuration);
		}, true)
		.AddOSSpecificService()
		.Build();

	await host.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Host terminated unexpectedly");
	Environment.ExitCode = 1;
}
finally
{
	Log.CloseAndFlush();
}
