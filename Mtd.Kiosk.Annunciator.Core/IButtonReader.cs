namespace Mtd.Kiosk.Annunciator.Core;

public interface IButtonReader
{
	string Name { get; }

	event EventHandler? ButtonPressed;
	void Start();
	void Stop();
}
