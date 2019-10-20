using System;

namespace Connectitude.LightUp.Options
{
    public class Schedule
    {
        public TimeSpan StartTimeUtc { get; set; }

        public TimeSpan EndTimeUtc { get; set; }

        public DayOfWeek[] Days { get; set; }

        public bool IsWithin(TimeSpan timeUtc)
        {
            return (timeUtc >= StartTimeUtc) && (timeUtc <= EndTimeUtc);
        }
    }
}
