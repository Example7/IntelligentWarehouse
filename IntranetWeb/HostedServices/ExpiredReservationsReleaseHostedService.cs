using Interfaces.Magazyn;
using Microsoft.Extensions.DependencyInjection;

namespace IntranetWeb.HostedServices
{
    public class ExpiredReservationsReleaseHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiredReservationsReleaseHostedService> _logger;

        public ExpiredReservationsReleaseHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<ExpiredReservationsReleaseHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Small startup delay so app boots quickly before first pass.
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var rezerwacjaService = scope.ServiceProvider.GetRequiredService<IRezerwacjaService>();
                    var released = await rezerwacjaService.ReleaseExpiredAsync(DateTime.UtcNow, stoppingToken);

                    if (released > 0)
                    {
                        _logger.LogInformation("Auto-zwolniono {Count} przeterminowanych rezerwacji.", released);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas automatycznego zwalniania przeterminowanych rezerwacji.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
