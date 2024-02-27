using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Core.Config;
using Mtd.Kiosk.Annunciator.Core.Models;
using Mtd.Kiosk.Annunciator.Realtime.UmbracoApi.DTO;

namespace Mtd.Kiosk.Annunciator.Realtime.UmbracoApi;

public class UmbracoApiRealtimeClient : IKioskRealTimeClient
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<UmbracoApiRealtimeClient> _logger;
	private readonly string _heartbeatUrlTemplate;
	private readonly string _departuresUrlTemplate;

	private static readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	public UmbracoApiRealtimeClient(HttpClient httpClient, IOptions<RealTimeClientConfig> options, ILogger<UmbracoApiRealtimeClient> logger)
	{
		ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
		ArgumentNullException.ThrowIfNull(options?.Value, nameof(options));
		ArgumentException.ThrowIfNullOrWhiteSpace(options.Value.HeartbeatAddressTemplate, nameof(options.Value.HeartbeatAddressTemplate));
		ArgumentException.ThrowIfNullOrWhiteSpace(options.Value.RealTimeAddressTemplate, nameof(options.Value.RealTimeAddressTemplate));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_httpClient = httpClient;
		_heartbeatUrlTemplate = options.Value.HeartbeatAddressTemplate;
		_departuresUrlTemplate = options.Value.RealTimeAddressTemplate;
		_logger = logger;
	}

	public async Task SendHeartbeat(string kioskId, CancellationToken cancellationToken)
	{
		var url = string.Format(_heartbeatUrlTemplate, kioskId);
		_logger.LogTrace("Sending heartbeat for {id} to {url}", kioskId, url);

		HttpResponseMessage responseMessage;
		try
		{
			responseMessage = await GetDeparturesFromServer(kioskId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to send heartbeat to {kioskId}", kioskId);
			return;
		}

		if (responseMessage.IsSuccessStatusCode)
		{
			_logger.LogTrace("Successfully sent heartbeat for {kioskId}", kioskId);
		}
		else
		{
			_logger.LogWarning("Failed to send heartbeat for {kioskId}. Got status code {statusCode}", kioskId, responseMessage.StatusCode);
		}
	}

	public async Task<IReadOnlyCollection<Departure>?> GetRealtime(string kioskId, CancellationToken cancellationToken)
	{
		HttpResponseMessage responseMessage;
		try
		{
			responseMessage = await GetDeparturesFromServer(kioskId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch departures");
			return null;
		}

		if (!responseMessage.IsSuccessStatusCode)
		{
			_logger.LogError("Got status code {statusCode} from departures API", responseMessage.StatusCode);
			return null;
		}

		try
		{
			var departures = await ConvertResults(responseMessage, cancellationToken);
			return departures;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch convert departures");
			return null;
		}
	}

	private async Task<HttpResponseMessage> GetDeparturesFromServer(string id, CancellationToken cancellationToken)
	{
		var url = string.Format(_departuresUrlTemplate, id);
		_logger.LogDebug("Fetching {url}", url);
		var result = await _httpClient.GetAsync(url, cancellationToken);
		return result;
	}

	private static async Task<IReadOnlyCollection<Departure>> ConvertResults(HttpResponseMessage result, CancellationToken cancellationToken)
	{
		var stream = await result.Content.ReadAsStreamAsync(cancellationToken);
		var departures = await JsonSerializer.DeserializeAsync<JsonDeparture[]>(stream, _jsonOptions, cancellationToken);

		if (departures is null)
		{
			return [];
		}

		var converted = departures
			.Select(ConvertJsonDepartureToDeparture)
			.ToImmutableArray();

		return converted;
	}

	private static Departure ConvertJsonDepartureToDeparture(JsonDeparture item)
	{
		var baseName = $"{item.Number} {item.Direction} {item.Name}";
		var name = item.HasModifier ? $"{baseName} to {item.Modifier}" : baseName;
		return new Departure(name, item.Display);
	}
}
