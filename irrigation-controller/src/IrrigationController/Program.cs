using IrrigationController.Models;
using IrrigationController.Services;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<IrrigationConfigStore>();
builder.Services.AddSingleton<AddonOptionsStore>();
builder.Services.AddSingleton<IrrigationConfigValidator>();
builder.Services.AddSingleton<IrrigationStateStore>();
builder.Services.AddHttpClient<HomeAssistantClient>();
builder.Services.AddSingleton<WeatherAdjustmentService>();
builder.Services.AddSingleton<WeatherForecastDiagnosticsService>();
builder.Services.AddSingleton<WaterBalanceService>();
builder.Services.AddSingleton<IrrigationSafetyService>();
builder.Services.AddSingleton<CalibrationService>();
builder.Services.AddSingleton<DiagnosticsService>();
builder.Services.AddSingleton<CycleRunner>();
builder.Services.AddSingleton<IrrigationOverviewService>();
builder.Services.AddSingleton<DecisionPlanService>();
builder.Services.AddSingleton<CycleSimulationService>();
builder.Services.AddSingleton<UiRenderer>();
builder.Services.AddHostedService<StartupSafetyService>();
builder.Services.AddHostedService<MqttDiscoveryPublisher>();
builder.Services.AddHostedService<WaterBalanceUpdater>();
builder.Services.AddHostedService<CycleScheduler>();

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("ui"));
app.MapGet("/config", () => Results.Redirect("ui#settings"));
app.MapGet("/ui", (UiRenderer ui) => Results.Content(ui.Render(), "text/html"));

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/config", async (IrrigationConfigStore store, CancellationToken cancellationToken) =>
    Results.Ok(await store.GetAsync(cancellationToken)));

app.MapPut("/api/config", async (
    IrrigationConfig config,
    HttpRequest request,
    IrrigationConfigStore store,
    IrrigationConfigValidator validator,
    DiagnosticsService diagnostics,
    CancellationToken cancellationToken) =>
{
    var validation = validator.Validate(config);
    if (!validation.IsValid)
    {
        return Results.BadRequest(validation);
    }

    await store.SaveAsync(config, cancellationToken);
    await diagnostics.RecordEventAsync(
        request.Headers["X-Irrigation-Action"].FirstOrDefault() ?? "config_saved",
        request.Headers["X-Irrigation-Message"].FirstOrDefault() ?? "Configuration saved.",
        request.Headers["X-Irrigation-Cycle"].FirstOrDefault(),
        request.Headers["X-Irrigation-Zone"].FirstOrDefault(),
        cancellationToken);

    return Results.Ok(new { message = "Configuration saved.", validation.Warnings });
});

app.MapGet("/api/config/validate", async (
    IrrigationConfigStore store,
    IrrigationConfigValidator validator,
    CancellationToken cancellationToken) =>
{
    var config = await store.GetAsync(cancellationToken);
    var result = validator.Validate(config);
    return result.IsValid ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/config/reload", async (IrrigationConfigStore store, CancellationToken cancellationToken) =>
{
    await store.ReloadAsync(cancellationToken);
    return Results.Ok(new { reloaded = true });
});

app.MapGet("/api/status", async (CycleRunner runner, IrrigationStateStore stateStore, CancellationToken cancellationToken) =>
{
    var state = await stateStore.GetAsync(cancellationToken);
    return Results.Ok(new
    {
        runner.Current,
        state.WaterBalance,
        state.LastScheduledRuns
    });
});

app.MapGet("/api/overview", async (IrrigationOverviewService overviewService, CancellationToken cancellationToken) =>
    Results.Ok(await overviewService.GetOverviewAsync(cancellationToken)));

app.MapGet("/api/decision-plan", async (DecisionPlanService decisionPlan, CancellationToken cancellationToken) =>
    Results.Ok(await decisionPlan.GetAsync(cancellationToken)));

app.MapGet("/api/entities/irrigation", async (HomeAssistantClient homeAssistant, CancellationToken cancellationToken) =>
{
    var entities = await homeAssistant.GetEntitiesAsync(cancellationToken);
    return Results.Ok(entities
        .Where(entity => entity.EntityId.StartsWith("switch.", StringComparison.OrdinalIgnoreCase)
            || entity.EntityId.StartsWith("valve.", StringComparison.OrdinalIgnoreCase))
        .OrderBy(entity => entity.FriendlyName.Length == 0 ? entity.EntityId : entity.FriendlyName, StringComparer.OrdinalIgnoreCase));
});

app.MapGet("/api/entities/weather", async (HomeAssistantClient homeAssistant, CancellationToken cancellationToken) =>
{
    var entities = await homeAssistant.GetEntitiesAsync(cancellationToken);
    return Results.Ok(entities
        .Where(entity => entity.EntityId.StartsWith("weather.", StringComparison.OrdinalIgnoreCase))
        .OrderBy(entity => entity.FriendlyName.Length == 0 ? entity.EntityId : entity.FriendlyName, StringComparer.OrdinalIgnoreCase));
});

app.MapGet("/api/diagnostics", async (IrrigationStateStore stateStore, CancellationToken cancellationToken) =>
{
    var state = await stateStore.GetAsync(cancellationToken);
    return Results.Ok(state.Diagnostics);
});

app.MapGet("/api/weather/forecast-check", async (
    IrrigationConfigStore store,
    WeatherForecastDiagnosticsService weatherDiagnostics,
    CancellationToken cancellationToken) =>
{
    var config = await store.GetAsync(cancellationToken);
    return Results.Ok(await weatherDiagnostics.CheckAsync(config, cancellationToken));
});

app.MapPost("/api/cycles/{cycleId}/start", async (
    string cycleId,
    CycleRunner runner,
    CancellationToken cancellationToken) =>
{
    var result = await runner.StartCycleAsync(cycleId, TriggerSource.Manual, cancellationToken);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/cycles/{cycleId}/dry-run", async (
    string cycleId,
    CycleRunner runner,
    CancellationToken cancellationToken) =>
{
    var result = await runner.DryRunCycleAsync(cycleId, cancellationToken);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/simulation/{cycleId}", async (
    string cycleId,
    CycleSimulationService simulation,
    CancellationToken cancellationToken) =>
{
    var result = await simulation.SimulateAsync(cycleId, cancellationToken);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/cycles/{cycleId}/stop", async (string cycleId, CycleRunner runner) =>
{
    await runner.StopAsync($"Stop requested for {cycleId}");
    return Results.Ok(new { stopped = true });
});

app.MapPost("/api/stop", async (CycleRunner runner) =>
{
    await runner.StopAsync("Global stop requested");
    return Results.Ok(new { stopped = true });
});

app.MapPost("/api/zones/{zoneId}/start", async (
    string zoneId,
    int? minutes,
    CycleRunner runner,
    CancellationToken cancellationToken) =>
{
    var result = await runner.StartZoneAsync(zoneId, TimeSpan.FromMinutes(minutes ?? 5), cancellationToken);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/zones/{zoneId}/stop", async (
    string zoneId,
    CycleRunner runner,
    CancellationToken cancellationToken) =>
{
    await runner.StopZoneAsync(zoneId, cancellationToken);
    return Results.Ok(new { stopped = true });
});

app.MapPost("/api/calibration/zones/{zoneId}/start", async (
    string zoneId,
    int? minutes,
    CalibrationService calibration,
    CancellationToken cancellationToken) =>
{
    var result = await calibration.StartAsync(zoneId, minutes ?? 10, cancellationToken);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapPost("/api/calibration/zones/{zoneId}/complete", async (
    string zoneId,
    CalibrationCompleteRequest request,
    CalibrationService calibration,
    CancellationToken cancellationToken) =>
{
    var result = await calibration.CompleteAsync(zoneId, request, cancellationToken);
    return result is null ? Results.BadRequest(new { message = "Invalid calibration data." }) : Results.Ok(result);
});

app.MapPost("/api/calibration/zones/{zoneId}/apply", async (
    string zoneId,
    CalibrationService calibration,
    CancellationToken cancellationToken) =>
{
    var result = await calibration.ApplyAsync(zoneId, cancellationToken);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

await app.RunAsync();
