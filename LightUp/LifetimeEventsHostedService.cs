using Connectitude.LightUp.Hue;
using Connectitude.LightUp.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Connectitude.LightUp
{
    internal class LifetimeEventsHostedService : IHostedService
    {
        private readonly ILogger m_Logger;
        private readonly IHostApplicationLifetime m_AppLifetime;
        private readonly IOptionsMonitor<ApplicationOptions> m_Options;
        private readonly Bridge m_HueBridge;
        private readonly AlertScanner m_AlertScanner;
        private Timer m_Timer;
        private DateTime? m_LastAlertAt;

        public LifetimeEventsHostedService(
            ILogger<LifetimeEventsHostedService> logger,
            IHostApplicationLifetime appLifetime,
            IOptionsMonitor<ApplicationOptions> options,
            Bridge hueBridge, AlertScanner alertScanner)
        {
            m_Logger = logger;
            m_AppLifetime = appLifetime;
            m_Options = options;
            m_HueBridge = hueBridge;
            m_AlertScanner = alertScanner;
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

            if (string.IsNullOrEmpty(Options.HueBridge.Id))
            {
                m_Logger.LogError("BridgeId is missing in configuration.");
                m_AppLifetime.StopApplication();
                return;
            }

            var bridgeIp = await m_HueBridge.FindAsync(Options.HueBridge.Id);
            if (bridgeIp == null)
            {
                m_Logger.LogError($"Bridge with id '{Options.HueBridge.Id}' not found on network.");
                m_AppLifetime.StopApplication();
                return;
            }

            if (!(await m_HueBridge.InitializeAsync(bridgeIp.IpAddress, Options.HueBridge.AppKey, Options.HueBridge.LightNames.Split(','))))
            {
                m_Logger.LogError("The application must be linked with the Hue Bridge. Press the button on the Hue Bridge before restarting application.");
                m_AppLifetime.StopApplication();
                return;
            }

            m_Timer = new Timer(OnTimer, null, 0, Timeout.Infinite);
        }

        private void OnStopping()
        {
            m_Logger.LogInformation("Application stopping...");

            m_Timer?.Change(Timeout.Infinite, Timeout.Infinite);
            m_Timer?.Dispose();
        }

        private void OnStopped()
        {
            m_Logger.LogInformation("Application stopped.");

            // Perform post-stopped activities here
        }

        private async void OnTimer(object state)
        {
            try
            {
                if (!IsScheduled(Options.Schedules))
                {
                    if (Options.IsBlackoutEnabled)
                    {
                        await m_HueBridge.TurnOffAsync();
                    }

                    return;
                }

                bool hasAnyAlert = false;

                await foreach (var alertLight in m_AlertScanner.ScanAsync(CancellationToken.None))
                {
                    hasAnyAlert = true;

                    if (m_LastAlertAt.HasValue &&
                        m_LastAlertAt.Value.AddSeconds(Options.AlertDelay) >= DateTime.UtcNow)
                    {
                        continue;
                    }

                    var alertColor = alertLight.Color ?? "FF0000";
                    await m_HueBridge.AlertAsync(alertColor, alertLight.Brightness);

                    m_LastAlertAt = DateTime.UtcNow;

                    // TODO: Priority between alert scans?
                    await Task.Delay(7 * 1000);
                }

                if (!hasAnyAlert)
                {
                    if (string.IsNullOrEmpty(Options.AmbientLight?.Color))
                    {
                        await m_HueBridge.TurnOffAsync();
                    }
                    else
                    {
                        var ambientBrightness = Options.AmbientLight?.Brightness ?? 100;
                        await m_HueBridge.ChangeColorAsync(Options.AmbientLight.Color, ambientBrightness);  
                    }
                }
            }
            catch (Exception exception)
            {
                m_Logger.LogError(exception, "Error while running alert rules.");
            }
            finally
            {
                m_Timer.Change(Options.AlertScanFrequency * 1000, Timeout.Infinite);
            }
        }

        private bool IsScheduled(Schedule[] schedules)
        {
            var todaysSchedules = schedules.Where(schedule => schedule.Days.Contains(DateTime.Now.DayOfWeek));

            var timeUtc = DateTime.UtcNow.TimeOfDay;
            foreach (var schedule in todaysSchedules)
            {
                if (schedule.IsWithin(timeUtc))
                    return true;
            }

            return false;
        }
    }
}

