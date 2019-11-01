namespace Cumtd.Signage.Kiosk.RealTime.Models
{
	internal sealed class JsonDeparture
	{
		public string Name { get; set; }
		public string Number { get; set; }
		public string Direction { get; set; }
		public string Display { get; set; }
		public string Modifier { get; set; }
		public bool HasModifier { get; set; }
	}
}
