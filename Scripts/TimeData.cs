using System;
namespace DayNight
{
	public struct TimeData
	{
		public int Hours { get; private set; }
		public int Minutes { get; private set; }

		public TimeData (float time)
		{
			Hours = (int) time;
			Minutes = (int) (time % 1 * 60);
		}

		public override string ToString ()
		{
			return string.Format ("{0:00}:{1:00}", Hours, Minutes);
		}
	}
}

