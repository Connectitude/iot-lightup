using System;
using System.Text.Json.Serialization;

namespace Connectitude.LightUp.TeamCity
{
    public class BuildTypeResponse
    {
        [JsonPropertyName("buildType")]
        public BuildType[] BuildTypes { get; set; }
    }
}
