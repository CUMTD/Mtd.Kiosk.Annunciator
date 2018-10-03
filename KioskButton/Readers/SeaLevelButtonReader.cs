using Cumtd.Signage.Kiosk.SeaLevel;
using Topshelf.Logging;

namespace Cumtd.Signage.Kiosk.KioskButton.Readers
{
    public class SeaLevelButtonReader : IButtonReader
    {
	    public string Name => "Sea Level Button Reader";

		private ButtonReader Reader { get; set; }

	    public bool Pressed { get; private set; }

	    public SeaLevelButtonReader(LogWriter logger)
	    {
		    Reader = new ButtonReader(value => Pressed = value, logger.Debug);
		    Reader.Start();
	    }

	    public void Dispose()
	    {
		    Reader.Stop();
			Reader = null;
	    }
    }
}
