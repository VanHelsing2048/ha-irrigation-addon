using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class DiagnosticsService
{
    private readonly IrrigationStateStore _stateStore;

    public DiagnosticsService(IrrigationStateStore stateStore)
    {
        _stateStore = stateStore;
    }

    public async Task RecordWeatherAsync(WeatherAdjustment weather, CancellationToken cancellationToken)
    {
        var state = await _stateStore.GetAsync(cancellationToken);
        state.Diagnostics.LastWeather = new WeatherDiagnostic
        {
            Et0Mm = weather.Et0Mm,
            ExpectedRainMm = weather.ExpectedRainMm,
            EffectiveRainMm = weather.EffectiveRainMm,
            MaxRainProbability = weather.MaxRainProbability,
            ShouldSkip = weather.ShouldSkip
        };
        await _stateStore.SaveAsync(state, cancellationToken);
    }

    public async Task RecordDecisionAsync(
        string type,
        string message,
        string? cycleId,
        string? zoneId,
        CancellationToken cancellationToken)
    {
        var state = await _stateStore.GetAsync(cancellationToken);
        state.Diagnostics.LastDecision = new DecisionDiagnostic
        {
            Type = type,
            Message = message,
            CycleId = cycleId,
            ZoneId = zoneId
        };
        await _stateStore.SaveAsync(state, cancellationToken);
    }

    public async Task RecordErrorAsync(string source, string message, CancellationToken cancellationToken)
    {
        var state = await _stateStore.GetAsync(cancellationToken);
        state.Diagnostics.LastError = new ErrorDiagnostic
        {
            Source = source,
            Message = message
        };
        await _stateStore.SaveAsync(state, cancellationToken);
    }
}
