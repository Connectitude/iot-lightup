using Connectitude.LightUp.Options;
using System;

namespace Connectitude.LightUp.Options.TeamCity
{
    public class Project
    {
        public string Id { get; set; }

        public bool IsRecursive { get; set; }

        public string IncludeBuildConfigIds { get; set; }

        public string ExcludeBuildConfigIds { get; set; }

        public LightOption AlertLight { get; set; }
    }
}
