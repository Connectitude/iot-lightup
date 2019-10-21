using System;

namespace Connectitude.LightUp.Options.TeamCity
{
    public class TeamCityOptions
    {
        public string BaseUrl { get; set; }

        public string Token { get; set; }   
        
        public Project[] Projects { get; set; }
    }
}
