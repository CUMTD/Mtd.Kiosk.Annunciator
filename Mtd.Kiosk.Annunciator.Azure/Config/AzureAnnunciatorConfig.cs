using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Annunciator.Azure.Config;

public class AzureAnnunciatorConfig
{
	public const string ConfigSectionName = "AzureAnnunciator";

	[Required]
	public required string SubscriptionKey { get; set; }
	[Required]
	public required string ServiceRegion { get; set; }
}
