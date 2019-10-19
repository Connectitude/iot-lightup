using System.Collections.Generic;
using System.Text;

namespace Connectitude.LightUp.Jira
{

    public class IssueResponse
    {
        public int Total { get; set; }
        public Issue[] Issues { get; set; }
    }
}
