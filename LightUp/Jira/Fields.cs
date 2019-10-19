using System;

namespace Connectitude.LightUp.Jira
{
    public class Fields
    {
        public DateTime StatusCategoryChangeDate { get; set; }
        public IssueType Issuetype { get; set; }
        
        public Sprint Sprint { get; set; }
        public Project Project { get; set; }
        
        
        public Resolution Resolution { get; set; }
        
        public DateTime Created { get; set; }
        public Epic Epic { get; set; }
        public Priority Priority { get; set; }
        
        
        
        public Assignee Assignee { get; set; }
        public DateTime Updated { get; set; }
        public Status Status { get; set; }
        
        public string Description { get; set; }
        
        public string Summary { get; set; }
        public Creator Creator { get; set; }
        
        public Reporter Reporter { get; set; }

        
        public string Environment { get; set; }        
    }
}
