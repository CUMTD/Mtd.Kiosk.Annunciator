using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Speech.Synthesis;
using Cumtd.Signage.Kiosk.RealTime.Models;

namespace Cumtd.Signage.Kiosk.Annunciator
{
	public static class DepartureAnnunciator
	{
		private static readonly Action<string> _defaultLogger = _ => { };

		public static void ReadDepartures(string stopName, IReadOnlyCollection<Departure> departures, Action<string> logger = null)
		{
			logger = logger ?? _defaultLogger;

			var synth = GetSynth();

			// no departures
			if (departures == null || departures.Count == 0)
			{
				ReadLine("There are no upcoming departures at this time.", logger);
			}
			else
			{
				ReadLine($"Departures for {stopName} as of {DateTime.Now:h:mm tt}", logger);
				// read each line
				foreach (var departure in departures)
				{
					var read = $"{departure.Name} {departure.JoinWord} {departure.Time}";
					logger(read);
					synth.Speak(read);
				}
			}
		}

		public static void ReadError(Action<string> logger = null) =>
			ReadLine("There was an error loading departures. Please try again later or call 384-8188.", logger ?? _defaultLogger);

		private static void ReadLine(string line, Action<string> logger)
		{
			var synth = GetSynth();
			logger(line);
			synth.Speak(line);
		}

		private static SpeechSynthesizer GetSynth()
		{
			var synth = new SpeechSynthesizer
			{
				Rate = -2
			};

			synth.SetOutputToDefaultAudioDevice();

			var assembly = Assembly.GetExecutingAssembly();
			var binPath = Path.GetDirectoryName(assembly.Location);
			var path = Path.Combine(binPath ?? throw new InvalidOperationException(), "Directions.xml");
			synth.AddLexicon(new Uri(path), "application/pls+xml");

			return synth;
		}

	}
}
