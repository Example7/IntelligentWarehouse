using Interfaces.Magazyn;
using Microsoft.Extensions.DependencyInjection;

namespace IntranetWeb.HostedServices
{
    public class AlertRulesRefreshHostedService : BackgroundService
    {
        private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AlertRulesRefreshHostedService> _logger;

        public AlertRulesRefreshHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<AlertRulesRefreshHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(StartupDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
                    var result = await alertService.GenerujAlertyZRegulAsync();

                    if (result.LiczbaNowychAlertow > 0 || result.LiczbaAutoPotwierdzonychAlertow > 0)
                    {
                        _logger.LogInformation(
                            "Alert rules refresh: new={NewAlerts}, auto-acked={AutoAcked}, skipped-duplicates={SkippedDuplicates}, processed-rules={ProcessedRules}.",
                            result.LiczbaNowychAlertow,
                            result.LiczbaAutoPotwierdzonychAlertow,
                            result.LiczbaPominietychDuplikatow,
                            result.LiczbaPrzetworzonychRegul);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while refreshing alerts from rules in background job.");
                }

                await Task.Delay(RunInterval, stoppingToken);
            }
        }
    }
}
