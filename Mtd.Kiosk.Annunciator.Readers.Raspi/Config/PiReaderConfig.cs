using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Annunciator.Readers.Raspi.Config;
public class PiReaderConfig
{
    public const string ConfigSectionName = "PiReader";

    [Required]
    public required int[] Pins { get; set; }

    public required string[] Foo { get; set; }
}