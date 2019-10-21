using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Connectitude.LightUp.TeamCity
{
    public class TeamCityClient
    {
        private readonly ILogger<TeamCityClient> m_Logger;
        private readonly IHttpClientFactory m_HttpClientFactory;

        public TeamCityClient(ILogger<TeamCityClient> logger, IHttpClientFactory httpClientFactory)
        {
            m_Logger = logger;
            m_HttpClientFactory = httpClientFactory;
        }

        public async Task<ICollection<Build>> GetBuildsAsync(string baseUrl, string token, string projectId, string[] include, string[] exclude, bool recursive, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(baseUrl) ||
                string.IsNullOrEmpty(token) ||
                string.IsNullOrEmpty(projectId))
                return Enumerable.Empty<Build>().ToArray();

            string locator = recursive ? "affectedProject" : "project";

            string url = $"{baseUrl}/app/rest/buildTypes?locator={locator}:(id:{projectId})&fields=buildType(id,builds($locator(running:false,canceled:false,count:1),build(number,status)))";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");
            requestMessage.Headers.Add("Accept", "application/json");

            using var httpClient = m_HttpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                m_Logger.LogError($"Failure getting builds from TeamCity project '{projectId}'. Error: {message}");                
                
            }

            var json = await response.Content.ReadAsStringAsync();

            var jsonOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var buildTypeResponse = JsonSerializer.Deserialize<BuildTypeResponse>(json, jsonOptions);
            var buildTypes = buildTypeResponse.BuildTypes;
            
            if (include?.Any() ?? false)
            {                
                return buildTypes
                    .Where(buildType => include.Contains(buildType.Id))
                    .SelectMany(buildType => buildType.Builds.Build)
                    .ToArray();
            }
            
            if (exclude?.Any() ?? false)
            {
                return buildTypes
                    .Where(buildType => !exclude.Contains(buildType.Id))
                    .SelectMany(buildType => buildType.Builds.Build)
                    .ToArray();
            }

            return buildTypes
                .SelectMany(buildType => buildType.Builds.Build)
                .ToArray();
        }
    }
}
