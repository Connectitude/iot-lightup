namespace Connectitude.LightUp.Jira
{
    public class Issue
    {
        public string Expand { get; set; }
        public string Id { get; set; }
        
        public string Key { get; set; }
        public Fields Fields { get; set; }
    }
}
