using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mtd.Kiosk.Annunciator.Readers.Simple.Config;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Readers.Simple;
using Serilog;
using Mtd.Kiosk.Annunciator.Service.Extensions;
using Mtd.Kiosk.Annunciator.Service;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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
				.AddEnvironmentVariables("Mtd.Kiosk.Annunciator.Service_");

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
				.Configure<PressEveryNSecondsReaderConfig>(context.Configuration.GetSection(PressEveryNSecondsReaderConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<PressEveryNSecondsReaderConfig>(PressEveryNSecondsReaderConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(PressEveryNSecondsReaderConfig.ConfigSectionName));

			// Readers
			_ = services
				.AddSingleton<IButtonReader, PressEveryNSecondsReader>();

			// Services
			_ = services
				.AddHostedService<AnnunciatorService>();

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
