using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Cumtd.Signage.Kiosk.KioskButton.Config;
using Newtonsoft.Json;
using NLog;

namespace Cumtd.Signage.Kiosk.KioskButton
{
    public class ConfigurationManager
    {
	    private static readonly Lazy<ConfigurationManager> _config =
		    new Lazy<ConfigurationManager>(() => new ConfigurationManager());

	    public static ConfigurationManager Config => _config.Value;

	    public LogFactory NLogFactory => NLogConfiguration.Instance;
		public string Id { get; }
		public string Name { get; }

	    private ConfigurationManager()
	    {
		    var settings = ReadSettings<ButtonConfig>();
		    Id = settings.Id;
		    var getTask = GetName(Id);
		    getTask.Wait();
		    Name = getTask.Result;
	    }

	    private static T ReadSettings<T>()
	    {
		    var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
			var fileNameBase = $"{typeof(T).Name}";
		    var file = new FileInfo(Path.Combine(basePath, $"{fileNameBase}.json"));

		    if (file.Exists)
		    {
			    var contents = File.ReadAllText(file.FullName);
			    return JsonConvert.DeserializeObject<T>(contents);
		    }
		    throw new FileNotFoundException($"Can't find config file {file.FullName}");
		}

	    private static async Task<string> GetName(string id)
	    {
			HttpResponseMessage response;
		    using (var client = new HttpClient())
		    {
			    client.DefaultRequestHeaders.Accept.Clear();
			    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			    response = await client
				    .GetAsync($"https://kiosk.mtd.org/umbraco/api/settings/kiosk/?id={id}")
				    .ConfigureAwait(false);
		    }
			var json = await response
				.Content
				.ReadAsStringAsync()
			    .ConfigureAwait(false);

		    var settings = JsonConvert.DeserializeObject<SignSettings>(json);
		    return settings.DisplayName;
	    }

    }
}
