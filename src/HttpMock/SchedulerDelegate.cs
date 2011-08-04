using System;
using System.Diagnostics;
using Kayak;

namespace HttpMock
{
	internal class SchedulerDelegate : ISchedulerDelegate
	{
		public void OnException(IScheduler scheduler, Exception e)
		{
			Debug.WriteLine("Error on scheduler.");
			Debug.WriteLine(e.ToString());
			e.DebugStackTrace();
		}

		public void OnStop(IScheduler scheduler)
		{
			Debug.WriteLine("STOPPED!");
		}
	}
}