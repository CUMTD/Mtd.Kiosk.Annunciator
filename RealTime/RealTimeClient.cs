using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cumtd.Signage.Kiosk.RealTime.Models;
using Newtonsoft.Json;

namespace Cumtd.Signage.Kiosk.RealTime
{
	public class RealTimeClient : IDisposable
	{

		public bool Disposed { get; private set; }
		private HttpClient HttpClient { get; set; }

		public RealTimeClient()
		{
			HttpClient = new HttpClient();
			HttpClient.DefaultRequestHeaders.Accept.Clear();
			HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<Departure[]> GetRealtime(string id)
		{
			if (Disposed)
			{
				throw new ObjectDisposedException(nameof(HttpClient));
			}

			var json = await GetJson(id).ConfigureAwait(false);
			var dataItems = ConvertJson(json);
			return ConvertToDepartures(dataItems);
		}

		private async Task<string> GetJson(string id)
		{
			var result = await HttpClient
				.GetAsync($"https://kiosk.mtd.org/umbraco/api/realtime/IPDisplayDepartures/?id={id}")
				.ConfigureAwait(false);

			return await result
				.Content
				.ReadAsStringAsync()
				.ConfigureAwait(false);
		}

		private static IEnumerable<DataItem> ConvertJson(string json) =>
			JsonConvert.DeserializeObject<DataItem[]>(json);

		private static Departure[] ConvertToDepartures(IEnumerable<DataItem> items)
		{
			var itemsToConvert = (items ?? Enumerable.Empty<DataItem>())
				.Where(di => di.Value != "NO_DATA")
				.ToArray();

			var departures = new List<Departure>();
			for (var i = 0; i < itemsToConvert.Length; i += 2)
			{
				departures.Add(new Departure(itemsToConvert[i], itemsToConvert[i + 1]));
			}
			return departures.ToArray();
		}

		public void Dispose()
		{
			Disposed = true;
			HttpClient?.Dispose();
			HttpClient = null;
		}
	}
}
