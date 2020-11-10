namespace Connectitude.LightUp.Options.Jira
{
    public class JiraOptions
    {
        public string BaseUrl { get; set; }

        public string Username { get; set; }

        public string Token { get; set; }       

        public JiraQuery[] Queries { get; set; }
    }
}
