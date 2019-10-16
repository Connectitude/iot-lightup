using Q42.HueApi.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LightUp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ApplicationOptions>(hostContext.Configuration);
                    services.AddHostedService<LifetimeEventsHostedService>();
                });
    }

    public class ApplicationOptions
    {
        public string BridgeId { get; set; }
        public string AppKey { get; set; }
        public string LightNames { get; set; }
    }
}

