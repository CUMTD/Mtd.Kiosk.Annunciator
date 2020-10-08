using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KioskAnnunciatorButton.RealTime
{
	public sealed class RealTimeClient
	{
		private readonly HttpClient _client = KioskHttpClient.Instance();

		private readonly ILogger<RealTimeClient> _logger;

		public RealTimeClient(ILogger<RealTimeClient> logger)
		{
			_client = KioskHttpClient.Instance();
			_logger = logger ?? throw new ArgumentException(nameof(logger));
		}

		public async Task<Departure[]> GetRealtime(string id)
		{
			try
			{
				var result = await GetDeparturesFromServer(id);
				var departures = await ConvertResults(result);
				return departures.ToArray();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to fetch departures");
				return Enumerable.Empty<Departure>().ToArray();
			}
		}

		public async Task SendHeartbeat(string id)
		{
			var url = $"/umbraco/api/health/buttonheartbeat?id={id}";
			_logger.LogTrace("Sending heartbeat for {id} to {url}", id, url);
			try
			{
				var result = await _client.GetAsync(url);
				if (result.IsSuccessStatusCode)
				{
					_logger.LogTrace("Sent heartbeat successfully");
				}
				else
				{
					_logger.LogWarning("Failed to send heartbeat with status code {code}", result.StatusCode);
				}
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex, "Failed to send heartbeat to {url}", url);
			}
		}

		private async Task<HttpResponseMessage> GetDeparturesFromServer(string id)
		{
			var url = $"/umbraco/api/realtime/getdepartures?id={id}&log=false";
			_logger.LogDebug("Fetching {url}", url);
			var start = DateTime.Now;
			var result = await _client
				.GetAsync($"/umbraco/api/realtime/getdepartures?id={id}&log=false");
			var end = DateTime.Now;
			_logger.LogTrace("Finished in {ms} ms", (end - start).TotalMilliseconds);
			_logger.LogDebug("Fetched with code {code}", result.StatusCode);
			return result;
		}

		private static async Task<IEnumerable<Departure>> ConvertResults(HttpResponseMessage result)
		{
			var stream = await result.Content.ReadAsStreamAsync();
			var departures = await JsonSerializer.DeserializeAsync<JsonDeparture[]>(stream, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			});
			var converted = departures
				.Select(ConvertJsonDepartureToDeparture);
			return converted;
		}

		private static Departure ConvertJsonDepartureToDeparture(JsonDeparture item)
		{
			var baseName = $"{item.Number} {item.Direction} {item.Name}";
			var name = item.HasModifier ? $"{baseName} to {item.Modifier}" : baseName;
			return new Departure(name, item.Display);
		}
	}
}
