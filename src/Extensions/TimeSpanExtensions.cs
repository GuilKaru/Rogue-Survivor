using System;

namespace RogueSurvivor.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToStringShort(this TimeSpan rt)
        {
            string timeDays = rt.Days == 0 ? "" : string.Format("{0} d ", rt.Days);
            string timeHours = rt.Hours == 0 ? "" : string.Format("{0:D2} h ", rt.Hours);
            string timeMinutes = rt.Minutes == 0 ? "" : string.Format("{0:D2} m ", rt.Minutes);
            string timeSeconds = rt.Seconds == 0 ? "" : string.Format("{0:D2} s", rt.Seconds);
            return string.Format("{0}{1}{2}{3}", timeDays, timeHours, timeMinutes, timeSeconds);
        }
    }
}
