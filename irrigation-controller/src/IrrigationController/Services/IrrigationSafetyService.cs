using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class IrrigationSafetyService
{
    private readonly HomeAssistantClient _homeAssistant;
    private readonly ILogger<IrrigationSafetyService> _logger;

    public IrrigationSafetyService(HomeAssistantClient homeAssistant, ILogger<IrrigationSafetyService> logger)
    {
        _homeAssistant = homeAssistant;
        _logger = logger;
    }

    public async Task TurnOnZoneAsync(ZoneConfig zone, SafetyConfig safety, CancellationToken cancellationToken)
    {
        await ExecuteWithRetryAsync(
            $"turn on {zone.Entity}",
            safety,
            async token =>
            {
                await _homeAssistant.TurnOnAsync(zone.Entity, token);
                if (safety.VerifyZoneStateAfterSwitch)
                {
                    await VerifyStateAsync(zone.Entity, "on", safety, token);
                }
            },
            cancellationToken);
    }

    public async Task TurnOffZoneAsync(ZoneConfig zone, SafetyConfig safety, CancellationToken cancellationToken)
    {
        await ExecuteWithRetryAsync(
            $"turn off {zone.Entity}",
            safety,
            async token =>
            {
                await _homeAssistant.TurnOffAsync(zone.Entity, token);
                if (safety.VerifyZoneStateAfterSwitch)
                {
                    await VerifyStateAsync(zone.Entity, "off", safety, token);
                }
            },
            cancellationToken);
    }

    public async Task StopAllKnownZonesAsync(IrrigationConfig config, CancellationToken cancellationToken)
    {
        foreach (var zone in config.Zones.Values)
        {
            try
            {
                await TurnOffZoneAsync(zone, config.Safety, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to turn off {EntityId}.", zone.Entity);
            }
        }
    }

    private async Task VerifyStateAsync(
        string entityId,
        string expectedState,
        SafetyConfig safety,
        CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(safety.SwitchRetryDelayMs), cancellationToken);
        var actualState = await _homeAssistant.GetStateAsync(entityId, cancellationToken);
        if (!string.Equals(actualState, expectedState, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Expected {entityId} to be {expectedState}, but it is {actualState ?? "unknown"}.");
        }
    }

    private async Task ExecuteWithRetryAsync(
        string action,
        SafetyConfig safety,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        var attempts = Math.Max(1, safety.SwitchRetryCount + 1);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                await operation(cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < attempts)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Failed to {Action}; retry {Attempt}/{Attempts}.", action, attempt, attempts);
                await Task.Delay(TimeSpan.FromMilliseconds(safety.SwitchRetryDelayMs), cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }

        throw new InvalidOperationException($"Unable to {action}.", lastException);
    }
}
