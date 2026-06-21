using IrrigationController.Models;
using IrrigationController.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<IrrigationConfigStore>();
builder.Services.AddSingleton<IrrigationConfigValidator>();
builder.Services.AddSingleton<IrrigationStateStore>();
builder.Services.AddHttpClient<HomeAssistantClient>();
builder.Services.AddSingleton<WeatherAdjustmentService>();
builder.Services.AddSingleton<CycleRunner>();
builder.Services.AddHostedService<CycleScheduler>();

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/ui"));

app.MapGet("/ui", async (IrrigationConfigStore store, CycleRunner runner, IrrigationStateStore stateStore, CancellationToken cancellationToken) =>
{
    var config = await store.GetAsync(cancellationToken);
    var state = await stateStore.GetAsync(cancellationToken);
    var cycles = string.Join("", config.Cycles.Select(cycle => $"""
        <tr>
          <td><strong>{cycle.Value.Name}</strong><span>{cycle.Key}</span></td>
          <td>{cycle.Value.Mode}</td>
          <td><button onclick="startCycle('{cycle.Key}')">Start</button></td>
        </tr>
        """));
    var zones = string.Join("", config.Zones.Select(zone => $"""
        <tr>
          <td><strong>{zone.Value.Name}</strong><span>{zone.Value.Entity}</span></td>
          <td>{(state.WaterBalance.TryGetValue(zone.Key, out var balance) ? balance.ToString("0.0") : "0.0")} mm</td>
          <td>
            <button onclick="startZone('{zone.Key}')">Start</button>
            <button class="secondary" onclick="stopZone('{zone.Key}')">Stop</button>
          </td>
        </tr>
        """));

    var html = $$"""
        <!doctype html>
        <html lang="it">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>Irrigazione</title>
          <style>
            :root { color-scheme: light dark; font-family: Segoe UI, sans-serif; }
            body { margin: 0; padding: 24px; background: #f7f7f2; color: #20231f; }
            main { max-width: 980px; margin: 0 auto; }
            header { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-bottom: 24px; }
            h1, h2 { margin: 0; letter-spacing: 0; }
            h1 { font-size: 28px; }
            h2 { font-size: 18px; margin: 24px 0 10px; }
            .status { padding: 12px 14px; border: 1px solid #ccd8c2; border-radius: 8px; background: #ffffff; }
            table { width: 100%; border-collapse: collapse; background: #ffffff; border: 1px solid #d8ded0; }
            th, td { text-align: left; padding: 12px; border-bottom: 1px solid #e6eadf; vertical-align: middle; }
            th { font-size: 12px; text-transform: uppercase; color: #586150; }
            span { display: block; color: #657061; font-size: 12px; margin-top: 3px; }
            button { min-width: 72px; border: 0; border-radius: 6px; padding: 8px 10px; background: #1f7a4d; color: white; cursor: pointer; }
            button.secondary { background: #5f6959; }
            button.danger { background: #b23a32; }
            @media (prefers-color-scheme: dark) {
              body { background: #171a16; color: #eef2e9; }
              .status, table { background: #20241e; border-color: #384030; }
              th, td { border-color: #30362b; }
              th, span { color: #aeb8a7; }
            }
          </style>
        </head>
        <body>
          <main>
            <header>
              <h1>Irrigazione</h1>
              <button class="danger" onclick="globalStop()">Stop</button>
            </header>
            <div class="status" id="status">{{runner.Current.Status ?? "idle"}}</div>
            <h2>Cicli</h2>
            <table>
              <thead><tr><th>Ciclo</th><th>Modalita</th><th>Comando</th></tr></thead>
              <tbody>{{cycles}}</tbody>
            </table>
            <h2>Zone</h2>
            <table>
              <thead><tr><th>Zona</th><th>Deficit</th><th>Comando</th></tr></thead>
              <tbody>{{zones}}</tbody>
            </table>
          </main>
          <script>
            async function post(url) {
              const res = await fetch(url, { method: 'POST' });
              const body = await res.json().catch(() => ({}));
              document.getElementById('status').textContent = body.message || JSON.stringify(body);
            }
            function startCycle(id) { post('/api/cycles/' + id + '/start'); }
            function startZone(id) { post('/api/zones/' + id + '/start?minutes=5'); }
            function stopZone(id) { post('/api/zones/' + id + '/stop'); }
            function globalStop() { post('/api/stop'); }
          </script>
        </body>
        </html>
        """;

    return Results.Content(html, "text/html");
});

app.MapGet("/api/config", async (IrrigationConfigStore store, CancellationToken cancellationToken) =>
    Results.Ok(await store.GetAsync(cancellationToken)));

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

app.MapPost("/api/cycles/{cycleId}/start", async (
    string cycleId,
    CycleRunner runner,
    CancellationToken cancellationToken) =>
{
    var result = await runner.StartCycleAsync(cycleId, TriggerSource.Manual, cancellationToken);
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

await app.RunAsync();
