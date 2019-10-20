namespace Connectitude.LightUp.Options
{
    public class ApplicationOptions
    {
        public ApplicationOptions()
        {
            AlertScanFrequency = 60000;
        }

        public HueBridge HueBridge { get; set; }

        public AtlassianCloud AtlassianCloud { get; set; }

        public Jira.JiraOptions Jira { get; set; }

        public Schedule[] Schedules { get; set; }

        public bool IsBlackoutEnabled { get; set; }

        public int AlertScanFrequency { get; set; }        
    }
}
