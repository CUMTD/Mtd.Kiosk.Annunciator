using System;
using NLog;
using Topshelf;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal static class Program
	{
		private static void Main()
		{
			var host = HostFactory.New(config =>
			{
				// display
				config.SetServiceName("mtd-annunciator-service");
				config.SetDisplayName("Annunciator Service");
				config.SetDescription("Annunciator Service");

				// behavior
				config.EnableShutdown();
				config.StartAutomatically();
				config.Service<AnnunciatorService>(serviceConfigurator =>
				{
					serviceConfigurator.ConstructUsing(_ => new AnnunciatorService());
					serviceConfigurator.WhenStarted(aService => aService.Start());
					serviceConfigurator.WhenStopped(aService => aService.Stop());
					serviceConfigurator.WhenShutdown(aService => aService.Dispose());
				});

				// permissions
				config.RunAsNetworkService();

				// recovery
				config.EnableServiceRecovery(serviceRecoveryConfigurator =>
				{
					serviceRecoveryConfigurator.RestartComputer(1, "Annunciator Service Stopped. Restarting...");
					serviceRecoveryConfigurator.SetResetPeriod(1);
				});

				// logging
				config.DependsOnEventLog();
				config.UseNLog(NLogLogManager.Instance);
			});

			var exitCode = host.Run();

			Environment.ExitCode = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
		}
	}
}
