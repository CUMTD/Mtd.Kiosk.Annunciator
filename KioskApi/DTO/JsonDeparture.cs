namespace Mtd.Kiosk.Annunciator.Realtime.UmbracoApi.DTO;
internal sealed class JsonDeparture
{

	public required string Number { get; set; }

	public required string Direction { get; set; }

	public required string Name { get; set; }

	public required string Modifier { get; set; }

	public required string DepartsIn { get; set; }

	public required bool IsRealtime { get; set; }
}
