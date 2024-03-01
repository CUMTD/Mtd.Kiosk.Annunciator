namespace Mtd.Kiosk.Annunciator.Realtime.UmbracoApi.DTO;
internal sealed class JsonDeparture
{
	public required string Name { get; set; }
	public required string Number { get; set; }
	public required string Direction { get; set; }
	public required string Display { get; set; }
	public required string Modifier { get; set; }
	public bool HasModifier { get; set; }
}
