using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Q42.HueApi.ColorConverters.Original;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.IO;

namespace LightUp
{
    internal class LifetimeEventsHostedService : IHostedService
    {
        private const string DeviceName = "67b66b4f-c8d2-41b1-";
        private const string SettingsFileName = "appsettings.json";

        private readonly ILogger m_Logger;
        private readonly IHostApplicationLifetime m_AppLifetime;
        private readonly IOptionsMonitor<ApplicationOptions> m_Options;

        public LifetimeEventsHostedService(
            ILogger<LifetimeEventsHostedService> logger,
            IHostApplicationLifetime appLifetime,
            IOptionsMonitor<ApplicationOptions> options)
        {
            m_Logger = logger;
            m_AppLifetime = appLifetime;
            m_Options = options;
        }

        public ApplicationOptions Options => m_Options.CurrentValue;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            m_AppLifetime.ApplicationStarted.Register(OnStarted);
            m_AppLifetime.ApplicationStopping.Register(OnStopping);
            m_AppLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async void OnStarted()
        {
            m_Logger.LogInformation("Application started.");

            if (string.IsNullOrEmpty(Options.BridgeId))
            {
                m_Logger.LogError("BridgeId is missing in configuration.");
                m_AppLifetime.StopApplication();
                return;
            }

            var bridgeLocator = new HttpBridgeLocator();
            var bridgeIps = await bridgeLocator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
            var bridgeIp = bridgeIps.FirstOrDefault(bridge => bridge.BridgeId.Equals(Options.BridgeId, StringComparison.InvariantCultureIgnoreCase));
            if (bridgeIp == null)
            {
                m_Logger.LogError($"Bridge with id '{Options.BridgeId}' not found on network.");
                m_AppLifetime.StopApplication();
                return;
            }
            
            var hueClient = new LocalHueClient(bridgeIp.IpAddress);

            if (string.IsNullOrEmpty(Options.AppKey))
            {
                try
                {
                    string appKey = await hueClient.RegisterAsync("LightUp", DeviceName);

                    await SaveAppKeyAsync(appKey);

                    Options.AppKey = appKey;
                }
                catch (LinkButtonNotPressedException)
                {
                    m_Logger.LogError("The application must be linked with the Hue Bridge. Press the button on the Hue Bridge before restarting application.");
                    m_AppLifetime.StopApplication();
                    return;
                }               
            }

            hueClient.Initialize(Options.AppKey);            
            
            var command = new LightCommand();            
            command.TurnOn().SetColor(new RGBColor("FF0000"));

            // Blink once
            command.Alert = Alert.Once;

            // Or start a colorloop
            //command.Effect = Effect.ColorLoop;

            if (string.IsNullOrEmpty(Options.LightNames))
            {
                await hueClient.SendCommandAsync(command);
            }
            else
            {
                var lights = await hueClient.GetLightsAsync();
                string[] lightNames = Options.LightNames.Split(',');
                string[] lightIds = 
                    lights
                        .Where(light =>
                            lightNames.Any(lightName =>
                                light.Name.Equals(lightName.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                        .Select(light => light.Id)
                        .ToArray();

                await hueClient.SendCommandAsync(command, lightIds);
            }
        }

        private void OnStopping()
        {
            m_Logger.LogInformation("Application stopping...");

            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            m_Logger.LogInformation("Application stopped.");

            // Perform post-stopped activities here
        }

        private async Task SaveAppKeyAsync(string appKey)
        {
            ApplicationOptions settings;

            try
            {
                using (var fileStream = File.Open(SettingsFileName, FileMode.OpenOrCreate))
                {
                    settings = await JsonSerializer.DeserializeAsync<ApplicationOptions>(fileStream);
                }
            }
            catch (JsonException)
            {
                settings = new ApplicationOptions();
            }

            settings.AppKey = appKey;

            using (var fileStream = File.Open(SettingsFileName, FileMode.Create))
            {
                var options = new JsonSerializerOptions() { WriteIndented = true };
                await JsonSerializer.SerializeAsync(fileStream, settings, options);
            }
        }
    }
}

