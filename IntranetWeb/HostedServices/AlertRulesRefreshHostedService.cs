using Interfaces.Magazyn;
using Microsoft.Extensions.DependencyInjection;

namespace IntranetWeb.HostedServices
{
    public class AlertRulesRefreshHostedService : BackgroundService
    {
        private static readonly TimeSpan DefaultStartupDelay = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan DefaultRunInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AlertRulesRefreshHostedService> _logger;
        private readonly TimeSpan _startupDelay;
        private readonly TimeSpan _runInterval;

        public AlertRulesRefreshHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<AlertRulesRefreshHostedService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var startupSeconds = configuration.GetValue<int?>("Jobs:AlertRulesRefresh:StartupDelaySeconds");
            var intervalSeconds = configuration.GetValue<int?>("Jobs:AlertRulesRefresh:RunIntervalSeconds");

            _startupDelay = startupSeconds.HasValue && startupSeconds.Value >= 0
                ? TimeSpan.FromSeconds(startupSeconds.Value)
                : DefaultStartupDelay;

            _runInterval = intervalSeconds.HasValue && intervalSeconds.Value > 0
                ? TimeSpan.FromSeconds(intervalSeconds.Value)
                : DefaultRunInterval;

            if (_runInterval < MinInterval)
            {
                _runInterval = MinInterval;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(_startupDelay, stoppingToken);

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

                await Task.Delay(_runInterval, stoppingToken);
            }
        }
    }
}
