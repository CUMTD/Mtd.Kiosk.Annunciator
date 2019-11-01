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
	public sealed class RealTimeClient : IDisposable
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
				.GetAsync($"https://kiosk.mtd.org/umbraco/api/realtime/getdepartures?id={id}&log=false")
				.ConfigureAwait(false);

			return await result
				.Content
				.ReadAsStringAsync()
				.ConfigureAwait(false);
		}

		private static IEnumerable<JsonDeparture> ConvertJson(string json) =>
			JsonConvert.DeserializeObject<IEnumerable<JsonDeparture>>(json);

		private static Departure[] ConvertToDepartures(IEnumerable<JsonDeparture> items)
		{
			var itemsToConvert = (items ?? Enumerable.Empty<JsonDeparture>())
				.ToArray();
			
			var departures = new List<Departure>();
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var item in itemsToConvert)
			{
				var baseName = $"{item.Number} {item.Direction} {item.Name}";
				var name = item.HasModifier ? $"{baseName} to {item.Modifier}" : baseName;
				departures.Add(new Departure(name, item.Display));
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
