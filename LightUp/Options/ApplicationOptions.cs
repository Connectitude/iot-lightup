using System;

namespace Connectitude.LightUp.Options
{
    public class ApplicationOptions
    {
        public HueBridge HueBridge { get; set; }

        public AtlassianCloud AtlassianCloud { get; set; }

        public Jira.JiraOptions Jira { get; set; }

        public int AlertScanFrequency { get; set; }        
    }
}
