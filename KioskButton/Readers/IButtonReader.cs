using System;

namespace Cumtd.Signage.Kiosk.KioskButton.Readers
{
	public interface IButtonReader : IDisposable
	{
		string Name { get; }
		bool Pressed { get; }
	}
}
