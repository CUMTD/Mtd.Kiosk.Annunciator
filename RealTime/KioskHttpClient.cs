using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace KioskAnnunciatorButton.RealTime
{
	internal sealed class KioskHttpClient
	{
		private static readonly object syncLock = new object();

		private static HttpClient _instance;

		private KioskHttpClient() { }

		public static HttpClient Instance()
		{
			if (_instance == null)
			{
				lock (syncLock)
				{
					_instance = CreateClient();
				}
			}
			return _instance;
		}

		private static HttpClient CreateClient()
		{
			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.BaseAddress = new Uri("https://kiosk.mtd.org");
			return httpClient;
		}

	}
}
