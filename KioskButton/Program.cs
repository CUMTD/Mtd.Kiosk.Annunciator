using System;
using Topshelf;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	internal static class Program
	{
		private static void Main()
		{
			var rc = HostFactory.Run(x => { x.Service<AnnunciatorService>(s =>
				{
					s.ConstructUsing(_ => new AnnunciatorService());
					s.WhenStarted(aService => aService.Start());
					s.WhenStopped(aService => aService.Stop());
				});
				x.RunAsNetworkService();
				x.StartAutomatically();

				x.EnableServiceRecovery(r =>
				{
					r.RestartComputer(1, "Annunciator Service Stopped. Restarting...");
					r.SetResetPeriod(1);
				});

				x.SetDescription("Annunciator Service");
				x.SetDisplayName("Annunciator Service");
				x.SetServiceName("mtd-annunciator-service");
			});

			Environment.ExitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
		}
	}
}
