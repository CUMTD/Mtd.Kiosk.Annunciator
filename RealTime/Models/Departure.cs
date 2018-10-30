using System;

namespace Cumtd.Signage.Kiosk.RealTime.Models
{
	public class Departure
	{
		public string Name { get; set; }
		public string Time { get; set; }
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

		internal Departure(string name, string time)
		{
			Name = name;
			Time = time.Replace("min", "minute");
		}
	}
}
