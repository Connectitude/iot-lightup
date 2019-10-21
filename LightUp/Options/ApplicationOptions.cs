namespace Connectitude.LightUp.Options
{
    public class ApplicationOptions
    {
        public ApplicationOptions()
        {
            AlertScanFrequency = 60;
            AlertDelay = 300;
        }

        public HueBridge HueBridge { get; set; }

        public AtlassianCloud AtlassianCloud { get; set; }

        public Jira.JiraOptions Jira { get; set; }

        public Schedule[] Schedules { get; set; }

        public bool IsBlackoutEnabled { get; set; }

        /// <summary>
        /// Frequency, in seconds, to scan for alerts.
        /// </summary>
        public int AlertScanFrequency { get; set; }

        /// <summary>
        /// Delay, in seconds, between alerts.
        /// </summary>
        public int AlertDelay { get; set; }

        public LightOption AmbientLight { get; set; }
    }
}
