namespace Connectitude.LightUp.Jira
{
    public class Epic
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }        
        public bool Done { get; set; }
    }
}
