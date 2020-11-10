using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Connectitude.LightUp.Jira
{
    public class JiraClient
    {
        private readonly ILogger<JiraClient> m_Logger;
        private readonly IHttpClientFactory m_HttpClientFactory;

        public JiraClient(ILogger<JiraClient> logger, IHttpClientFactory httpClientFactory)
        {
            m_Logger = logger;
            m_HttpClientFactory = httpClientFactory;
        }

        public async Task<ICollection<Issue>> GetIssuesAsync(string baseUrl, string username, string token, string boardId, string query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(baseUrl) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(token) ||
                string.IsNullOrEmpty(query))
                return Enumerable.Empty<Issue>().ToArray();

            string auth = $"{username}:{token}";
            string bin64Auth = Convert.ToBase64String(Encoding.Default.GetBytes(auth));

            var httpMethod = HttpMethod.Post;
            string url = $"{baseUrl}/rest/api/3/search";

            bool isBoardQuery = !string.IsNullOrEmpty(boardId);
            if (isBoardQuery)
            {
                httpMethod = HttpMethod.Get;
                string encodedQuery = HttpUtility.UrlEncode(query);
                url = $"{baseUrl}/rest/agile/1.0/board/{boardId}/issue?jql={encodedQuery}&fields=status&maxResults=1";
            }            
            
            using var requestMessage = new HttpRequestMessage(httpMethod, url);
            requestMessage.Headers.Add("Authorization", $"Basic {bin64Auth}");
            requestMessage.Headers.Add("Accept", "application/json");

            var jsonOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            if (!isBoardQuery) {
                string jsonBody = JsonSerializer.Serialize(new 
                {
                    Jql = query,
                    MaxResults = 1,
                    Fields = new [] { "summary" }
                }, jsonOptions);
                requestMessage.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }

            using var httpClient = m_HttpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                m_Logger.LogError($"Failure getting issues from Jira query '{query}'. Error: {message}");
            }

            var json = await response.Content.ReadAsStringAsync();
            
            var issueResponse = JsonSerializer.Deserialize<IssueResponse>(json, jsonOptions);
            return issueResponse.Issues;
        }
    }
}
