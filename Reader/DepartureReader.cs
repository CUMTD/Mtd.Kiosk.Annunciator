using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KioskAnnunciatorButton.RealTime;
using Microsoft.Extensions.Logging;
using Microsoft.CognitiveServices.Speech;
using System.IO;

namespace KioskAnnunciatorButton.Reader
{
	public class DepartureReader
	{

		private readonly ILogger<DepartureReader> _logger;
		private readonly SpeechSynthesizer _synth;
		public DepartureReader(string subscriptionKey, string serviceRegion, ILogger<DepartureReader> logger)
		{
			var config = SpeechConfig.FromSubscription(subscriptionKey, serviceRegion);
			_synth = new SpeechSynthesizer(config);
			_logger = logger ?? throw new ArgumentException(nameof(logger));
		}

		public async Task ReadDepartures(string stopName, IReadOnlyCollection<Departure> departures)
		{
			// no departures
			if (departures == null || departures.Count == 0)
			{
				await ReadLine("There are no upcoming departures at this time.");
			}
			else
			{
				await ReadLine($"Departures for {stopName} as of {DateTime.Now:h:mm tt}");
				// read each line
				foreach (var departure in departures)
				{
					await ReadLine($"{departure.Name} {departure.JoinWord} {departure.Time}");
				}
			}
		}

		public Task ReadError() =>
			ReadLine("There was an error loading departures. Please try again later or call 384-8188.");

		private async Task ReadLine(string text)
		{
			using var result = await _synth.SpeakSsmlAsync(GenerateSsml(text));

			if (result.Reason == ResultReason.SynthesizingAudioCompleted)
			{
				_logger.LogDebug("Speech synthesized to speaker for text [{text}]", text);
			}
			else if (result.Reason == ResultReason.Canceled)
			{
				var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
				_logger.LogWarning("CANCELED: Reason={reason}", cancellation.Reason);

				if (cancellation.Reason == CancellationReason.Error)
				{
					_logger.LogError("CANCELED: ErrorCode={error}", cancellation.ErrorCode);
					_logger.LogError("CANCELED: ErrorDetails=[{details}]", cancellation.ErrorDetails);
				}
			}
			else
			{
				_logger.LogWarning("Unknown error Reason={reason}", result.Reason.ToString());
			}
			_logger.LogTrace("Result id={id}", result.ResultId);

		}

		/// <summary>
		/// Create SSML XML for reading text.
		/// </summary>
		/// <param name="text">The text to read.</param>
		/// <remarks>https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/speech-synthesis-markup?tabs=csharp</remarks>
		/// <returns>XML</returns>
		private string GenerateSsml(string text)
		{
			var sb = new StringBuilder();

			sb.AppendLine("<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"https://www.w3.org/2001/mstts\" xml:lang=\"en-US\">");
			sb.AppendLine("\t<voice name=\"en-US-AriaNeural\">");
			sb.AppendLine("\t\t<mstts:express-as style=\"customerservice\">");
			sb.AppendLine($"\t\t\t{text}");
			sb.AppendLine("\t\t</mstts:express-as>");
			sb.AppendLine("\t</voice>");
			sb.AppendLine("</speak>");

			var xml = sb.ToString();

			_logger.LogTrace("Generated SSML '{text}'", xml);

			return xml;
		}

	}
}
