using Connectitude.LightUp.Hue;
using Connectitude.LightUp.Jira;
using Connectitude.LightUp.Options;
using Connectitude.LightUp.TeamCity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Connectitude.LightUp
{
    public class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("data/configuration.json", optional: true, true);
                    configHost.AddJsonFile("data/settings.json", optional: true, true);
                    //configHost.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ApplicationOptions>(hostContext.Configuration);
                    
                    services.AddHostedService<LifetimeEventsHostedService>();
                    
                    services.AddHttpClient();

                    services.AddSingleton<Bridge>();
                    
                    services.AddTransient<JiraClient>();
                    services.AddTransient<TeamCityClient>();
                    services.AddTransient<AlertScanner>();
                });
    }
}

