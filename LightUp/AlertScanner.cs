using Connectitude.LightUp.Jira;
using Connectitude.LightUp.Options;
using Connectitude.LightUp.TeamCity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connectitude.LightUp
{
    public class AlertScanner
    {
        private readonly ILogger m_Logger;
        private readonly IOptionsMonitor<ApplicationOptions> m_Options;
        private readonly JiraClient m_JiraClient;
        private readonly TeamCityClient m_TeamCityClient;

        public AlertScanner(
            ILogger<AlertScanner> logger,
            IOptionsMonitor<ApplicationOptions> options,
            JiraClient jiraClient, TeamCityClient teamCityClient)
        {
            m_Logger = logger;
            m_Options = options;
            m_JiraClient = jiraClient;
            m_TeamCityClient = teamCityClient;
        }

        public ApplicationOptions Options => m_Options.CurrentValue;

        public async IAsyncEnumerable<LightOption> ScanAsync(CancellationToken cancellationToken)
        {
            foreach (var board in Options.Jira.Boards)
            {
                foreach (var query in board.Queries)
                {
                    var issues = await m_JiraClient.GetIssuesAsync(
                        Options.AtlassianCloud.BaseUrl,
                        Options.AtlassianCloud.Username,
                        Options.AtlassianCloud.Token,
                        board.Id, query.Query,
                        cancellationToken);

                    if (!issues.Any())
                        continue;

                    if (query.AlertLight != null)
                        yield return query.AlertLight;
                }
            }

            foreach (var project in Options.TeamCity.Projects)
            {
                var include = project.IncludeBuildConfigIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .ToArray();

                var exclude = project.ExcludeBuildConfigIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .ToArray();

                var builds = await m_TeamCityClient.GetBuildsAsync(
                    Options.TeamCity.BaseUrl,
                    Options.TeamCity.Token,
                    project.Id, include, exclude, project.IsRecursive,
                    cancellationToken);

                var failingBuilds = builds.Where(build => !"SUCCESS".Equals(build.Status, StringComparison.InvariantCultureIgnoreCase));
                if (!failingBuilds.Any())
                    continue;

                if (project.AlertLight != null)
                    yield return project.AlertLight;
            }
        }
    }
}
