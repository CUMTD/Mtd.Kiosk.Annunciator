using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Annunciator.Readers.Simple.Config;
public class PressEveryNSecondsReaderConfig
{
	[Required, Range(1, int.MaxValue)]
	public int Seconds { get; protected set; }
}
