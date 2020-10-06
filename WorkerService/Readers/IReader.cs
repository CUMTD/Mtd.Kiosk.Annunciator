namespace KioskAnnunciatorButton.WorkerService.Readers
{
	internal interface IReader
	{
		string Name { get; }
		bool GetPressed();
		void Start();
		void Stop();
	}
}
