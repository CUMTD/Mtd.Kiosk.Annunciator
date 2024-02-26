namespace Mtd.Kiosk.Annunciator.Core;

public interface IButtonReader
{
	string Name { get; }

	event EventHandler? ButtonPressed;
	Task Start(CancellationToken cancellationToken);
	Task Stop(CancellationToken cancellationToken);
}
