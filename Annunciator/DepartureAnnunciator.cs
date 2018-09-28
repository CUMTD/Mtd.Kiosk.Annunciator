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
		public static void ReadDepartures(IReadOnlyCollection<Departure> departures, Action<string> logger = null)
		{
			logger = logger ?? (_ => { });

			var synth = GetSynth();

			foreach (var departure in departures)
			{
				var read = $"{departure.Name} {departure.JoinWord} {departure.Time}";
				logger(read);
				synth.Speak(read);
			}

			logger(string.Empty);

		}

		private static SpeechSynthesizer GetSynth()
		{
			var synth = new SpeechSynthesizer();

			synth.SetOutputToDefaultAudioDevice();

			var assembly = Assembly.GetExecutingAssembly();
			var binPath = Path.GetDirectoryName(assembly.Location);
			var path = Path.Combine(binPath ?? throw new InvalidOperationException(), "Directions.xml");
			synth.AddLexicon(new Uri(path), "application/pls+xml");

			return synth;
		}

	}
}
