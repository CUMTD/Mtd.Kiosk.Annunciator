using System;
using System.Threading.Tasks;

namespace Cumtd.Signage.Kiosk.KioskButton
{
	public class AsyncLazy<T> : Lazy<Task<T>>
	{
		/// <inheritdoc />
		// ReSharper disable once UnusedMember.Global
		public AsyncLazy(Func<T> valueFactory) :
			base(() => Task.Factory.StartNew(valueFactory))
		{ }

		/// <inheritdoc />
		public AsyncLazy(Func<Task<T>> taskFactory) :
			base(() => Task.Factory.StartNew(taskFactory).Unwrap())
		{ }
	}
}
