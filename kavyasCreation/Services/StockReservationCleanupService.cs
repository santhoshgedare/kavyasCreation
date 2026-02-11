using Core.Interfaces;

namespace kavyasCreation.Services
{
    public class StockReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockReservationCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public StockReservationCleanupService(IServiceProvider serviceProvider, ILogger<StockReservationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stock Reservation Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
                    
                    await inventoryService.ReleaseExpiredReservationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error releasing expired stock reservations");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Stock Reservation Cleanup Service stopped");
        }
    }
}
