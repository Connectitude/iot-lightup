using Connectitude.LightUp.Options;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
using Q42.HueApi.Models.Bridge;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Connectitude.LightUp.Hue
{
    public class Bridge
    {
        private const string DeviceName = "67b66b4f-c8d2-41b1-";
        private const string SettingsFileName = "settings.json";
        private readonly string SettingsFilePath = Path.Combine("data", SettingsFileName);

        private LocalHueClient m_HueClient;
        private string[] m_LightIds;

        public async Task<LocatedBridge> FindAsync(string bridgeId)
        {
            var bridgeLocator = new HttpBridgeLocator();
            var bridgeIps = await bridgeLocator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
            return bridgeIps.FirstOrDefault(bridge => bridge.BridgeId.Equals(bridgeId, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<bool> InitializeAsync(string bridgeIpAddress, string appKey, params string[] lightNames)
        {
            m_HueClient = new LocalHueClient(bridgeIpAddress);

            if (string.IsNullOrEmpty(appKey))
            {
                try
                {
                    appKey = await m_HueClient.RegisterAsync("LightUp", DeviceName);

                    await SaveAppKeyAsync(appKey);
                }
                catch (LinkButtonNotPressedException)
                {
                    return false;
                }
            }

            m_HueClient.Initialize(appKey);

            if (lightNames.Any())
            {
                var lights = await m_HueClient.GetLightsAsync();
                m_LightIds = lights
                    .Where(light =>
                        lightNames.Any(lightName =>
                            light.Name.Equals(lightName.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                    .Select(light => light.Id)
                    .ToArray();
            }

            return true;
        }

        public Task AlertAsync(string color, double brightness = 100)
        {
            var command = new LightCommand();
            command.Brightness = (byte)(brightness / 100F * 255);
            command.TurnOn().SetColor(new RGBColor(color));
            command.Alert = Alert.Multiple;

            // Or start a colorloop
            //command.Effect = Effect.ColorLoop;

            return SendCommandAsync(command);
        }

        public Task ChangeColorAsync(string color, byte brightness = 100)
        {
            var command = new LightCommand();
            command.Brightness = (byte)(brightness / 100F * 255);
            command.TurnOn().SetColor(new RGBColor(color));

            return SendCommandAsync(command);
        }

        public Task TurnOffAsync()
        {
            var command = new LightCommand();
            command.TurnOff();

            return SendCommandAsync(command);
        }

        private Task SendCommandAsync(LightCommand command)
        {
            if (m_LightIds == null)
                return m_HueClient.SendCommandAsync(command);

            return m_HueClient.SendCommandAsync(command, m_LightIds);
        }

        private async Task SaveAppKeyAsync(string appKey)
        {
            using var fileStream = File.Open(SettingsFilePath, FileMode.Create);

            var settings = new { HueBridge = new HueBridge { AppKey = appKey } };

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
                        
            await JsonSerializer.SerializeAsync(fileStream, settings, options);
        }
    }
}
