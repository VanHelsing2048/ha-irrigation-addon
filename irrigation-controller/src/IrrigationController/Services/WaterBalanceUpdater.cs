namespace IrrigationController.Services;

public sealed class WaterBalanceUpdater : BackgroundService
{
    private readonly IrrigationConfigStore _configStore;
    private readonly IrrigationConfigValidator _validator;
    private readonly WeatherAdjustmentService _weather;
    private readonly WaterBalanceService _waterBalance;
    private readonly ILogger<WaterBalanceUpdater> _logger;

    public WaterBalanceUpdater(
        IrrigationConfigStore configStore,
        IrrigationConfigValidator validator,
        WeatherAdjustmentService weather,
        WaterBalanceService waterBalance,
        ILogger<WaterBalanceUpdater> logger)
    {
        _configStore = configStore;
        _validator = validator;
        _weather = weather;
        _waterBalance = waterBalance;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var config = await _configStore.GetAsync(stoppingToken);
                var validation = _validator.Validate(config);
                if (validation.IsValid)
                {
                    var weather = await _weather.CalculateAsync(config, stoppingToken);
                    await _waterBalance.EnsureDailyBalanceAsync(config, weather, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to update daily water balance.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
