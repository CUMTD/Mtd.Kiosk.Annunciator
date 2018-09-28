using System.Text.RegularExpressions;

namespace Cumtd.Signage.Kiosk.RealTime.Models
{
	public class Departure
	{
		public string Name { get; set; }
		public string Time { get; set; }
		public bool Due => string.Equals(Time, "DUE");
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

		private static readonly Regex DirectionRegex = new Regex("([0-9]+)([a-zA-Z])");

		internal Departure(DataItem name, DataItem time)
		{
			Name = DirectionRegex.Replace(name.Value, "$1 $2");
			Time = time.Value.Replace("min", "minute");
		}
	}
}
