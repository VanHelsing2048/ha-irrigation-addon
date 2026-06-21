namespace IrrigationController.Services;

public sealed class StartupSafetyService : IHostedService
{
    private readonly IrrigationConfigStore _configStore;
    private readonly IrrigationConfigValidator _validator;
    private readonly IrrigationSafetyService _safety;
    private readonly ILogger<StartupSafetyService> _logger;

    public StartupSafetyService(
        IrrigationConfigStore configStore,
        IrrigationConfigValidator validator,
        IrrigationSafetyService safety,
        ILogger<StartupSafetyService> logger)
    {
        _configStore = configStore;
        _validator = validator;
        _safety = safety;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        var validation = _validator.Validate(config);
        if (!validation.IsValid || !config.Safety.TurnOffAllZonesOnStartup)
        {
            return;
        }

        _logger.LogInformation("Turning off all known irrigation zones on startup.");
        await _safety.StopAllKnownZonesAsync(config, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
