using System;
using Topshelf;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal static class Program
	{
		private static void Main()
		{
			var config = ConfigurationManager.Config;

			var host = HostFactory.New(hostConfigurator =>
			{
				// display
				hostConfigurator.SetServiceName("mtd-annunciator-service");
				hostConfigurator.SetDisplayName("Annunciator Service");
				hostConfigurator.SetDescription("Annunciator Service");

				// behavior
				hostConfigurator.EnableShutdown();
				hostConfigurator.StartAutomatically();
				hostConfigurator.Service<AnnunciatorService>(serviceConfigurator =>
				{
					serviceConfigurator.ConstructUsing(_ => new AnnunciatorService());
					serviceConfigurator.WhenStarted(aService => aService.Start());
					serviceConfigurator.WhenStopped(aService => aService.Stop());
					serviceConfigurator.WhenShutdown(aService => aService.Dispose());
				});

				// permissions
				hostConfigurator.RunAsNetworkService();

				// recovery
				hostConfigurator.EnableServiceRecovery(serviceRecoveryConfigurator =>
				{
					serviceRecoveryConfigurator.RestartComputer(1, "Annunciator Service Stopped. Restarting...");
					serviceRecoveryConfigurator.SetResetPeriod(1);
				});

				// logging
				hostConfigurator.DependsOnEventLog();
				hostConfigurator.UseNLog(config.NLogFactory);
			});

			var exitCode = host.Run();

			Environment.ExitCode = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
		}
	}
}
