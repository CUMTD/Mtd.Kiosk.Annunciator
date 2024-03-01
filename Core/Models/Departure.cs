namespace Mtd.Kiosk.Annunciator.Core.Models;
public class Departure(string name, string time)
{
	public string Name { get; set; } = name;
	public string Time { get; set; } = time.Replace("min", "minute");
	public bool Due => string.Equals(Time, "DUE", StringComparison.CurrentCultureIgnoreCase);
	public bool Realtime => Time.Contains("min") || Due;
	public string JoinWord
	{
		get
		{

			if (Due)
			{
				return "is";
			}

			return Realtime ?
				"in" :
				"at";
		}
	}
}
