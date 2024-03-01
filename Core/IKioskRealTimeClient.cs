using Mtd.Kiosk.Annunciator.Core.Models;

namespace Mtd.Kiosk.Annunciator.Core;
public interface IKioskRealTimeClient
{
	Task SendHeartbeat(string kioskId, CancellationToken cancellationToken);

	Task<IReadOnlyCollection<Departure>?> GetRealtime(string kioskId, CancellationToken cancellationToken);
}
