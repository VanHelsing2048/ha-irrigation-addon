using IrrigationController.Models;
using IrrigationController.Services;
using System.Text.Encodings.Web;
using System.Text.Json;
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
builder.Services.AddSingleton<WaterBalanceService>();
builder.Services.AddSingleton<IrrigationSafetyService>();
builder.Services.AddSingleton<CalibrationService>();
builder.Services.AddSingleton<CycleRunner>();
builder.Services.AddSingleton<IrrigationOverviewService>();
builder.Services.AddHostedService<StartupSafetyService>();
builder.Services.AddHostedService<MqttDiscoveryPublisher>();
builder.Services.AddHostedService<WaterBalanceUpdater>();
builder.Services.AddHostedService<CycleScheduler>();

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/ui"));

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/ui", async (IrrigationOverviewService overviewService, CancellationToken cancellationToken) =>
{
    var overview = await overviewService.GetOverviewAsync(cancellationToken);
    var cycles = string.Join("", overview.Cycles.Select(cycle => $"""
        <tr>
          <td><strong>{HtmlEncoder.Default.Encode(cycle.Name)}</strong><span>{HtmlEncoder.Default.Encode(cycle.Id)}</span></td>
          <td>{cycle.Mode}</td>
          <td>{HtmlEncoder.Default.Encode(cycle.NextRunText)}</td>
          <td><button onclick="startCycle('{HtmlEncoder.Default.Encode(cycle.Id)}')">Start</button></td>
        </tr>
        """));
    var zones = string.Join("", overview.Zones.Select(zone => $"""
        <tr>
          <td><strong>{HtmlEncoder.Default.Encode(zone.Name)}</strong><span>{HtmlEncoder.Default.Encode(zone.Entity)}</span></td>
          <td><span class="pill {HtmlEncoder.Default.Encode(zone.StateClass)}">{HtmlEncoder.Default.Encode(zone.State)}</span></td>
          <td>{zone.WaterBalanceMm:0.0} mm</td>
          <td>{HtmlEncoder.Default.Encode(zone.CalibrationText)}</td>
          <td>
            <button onclick="startZone('{HtmlEncoder.Default.Encode(zone.Id)}')">Start</button>
            <button class="secondary" onclick="stopZone('{HtmlEncoder.Default.Encode(zone.Id)}')">Stop</button>
            <button class="secondary" onclick="calibrateZone('{HtmlEncoder.Default.Encode(zone.Id)}')">Calibra</button>
            <button class="secondary" onclick="applyCalibration('{HtmlEncoder.Default.Encode(zone.Id)}')">Applica</button>
          </td>
        </tr>
        """));
    var validationRows = string.Join("", overview.Validation.Errors.Concat(overview.Validation.Warnings).Select(issue => $"""
        <li><strong>{HtmlEncoder.Default.Encode(issue.Path)}</strong> {HtmlEncoder.Default.Encode(issue.Message)}</li>
        """));
    var validationBlock = string.IsNullOrWhiteSpace(validationRows)
        ? """<div class="status ok">Configurazione valida</div>"""
        : $$"""<div class="status warn"><strong>Configurazione da verificare</strong><ul>{{validationRows}}</ul></div>""";
    var eventRows = string.Join("", overview.RecentEvents.Select(item => $"""
        <tr>
          <td>{HtmlEncoder.Default.Encode(item.Timestamp.ToLocalTime().ToString("dd/MM HH:mm"))}</td>
          <td>{HtmlEncoder.Default.Encode(item.Type)}</td>
          <td>{HtmlEncoder.Default.Encode(item.ZoneId ?? "-")}</td>
          <td>{HtmlEncoder.Default.Encode(item.Message)}</td>
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
            .status.ok { border-color: #6aa66a; }
            .status.warn { border-color: #d6a542; }
            .grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 10px; margin-bottom: 18px; }
            .metric { padding: 12px; border: 1px solid #d8ded0; border-radius: 8px; background: #ffffff; }
            .metric strong { display: block; font-size: 20px; }
            table { width: 100%; border-collapse: collapse; background: #ffffff; border: 1px solid #d8ded0; }
            th, td { text-align: left; padding: 12px; border-bottom: 1px solid #e6eadf; vertical-align: middle; }
            th { font-size: 12px; text-transform: uppercase; color: #586150; }
            span { display: block; color: #657061; font-size: 12px; margin-top: 3px; }
            .pill { display: inline-block; min-width: 54px; padding: 4px 8px; border-radius: 999px; text-align: center; font-size: 12px; }
            .pill.on { background: #d7f1df; color: #195735; }
            .pill.off { background: #ecefe7; color: #46513e; }
            .pill.unknown { background: #fff0cf; color: #6d4b00; }
            button { min-width: 72px; border: 0; border-radius: 6px; padding: 8px 10px; background: #1f7a4d; color: white; cursor: pointer; }
            button.secondary { background: #5f6959; }
            button.danger { background: #b23a32; }
            @media (prefers-color-scheme: dark) {
              body { background: #171a16; color: #eef2e9; }
              .status, .metric, table { background: #20241e; border-color: #384030; }
              th, td { border-color: #30362b; }
              th, span { color: #aeb8a7; }
              .pill.on { background: #214d33; color: #d7f1df; }
              .pill.off { background: #31372e; color: #d7ddd2; }
              .pill.unknown { background: #5f4b1a; color: #fff3cf; }
            }
            @media (max-width: 760px) {
              body { padding: 14px; }
              .grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
              table { font-size: 14px; }
            }
          </style>
        </head>
        <body>
          <main>
            <header>
              <h1>Irrigazione</h1>
              <div>
                <button class="secondary" onclick="location.href='/config'">Config</button>
                <button class="danger" onclick="globalStop()">Stop</button>
              </div>
            </header>
            <div class="grid">
              <div class="metric"><span>Runner</span><strong id="status">{{HtmlEncoder.Default.Encode(overview.Runner.Status ?? "idle")}}</strong></div>
              <div class="metric"><span>Ciclo</span><strong>{{HtmlEncoder.Default.Encode(overview.Runner.CycleName ?? "-")}}</strong></div>
              <div class="metric"><span>Zona</span><strong>{{HtmlEncoder.Default.Encode(overview.Runner.ZoneName ?? "-")}}</strong></div>
              <div class="metric"><span>Bilancio</span><strong>{{HtmlEncoder.Default.Encode(overview.LastWaterBalanceUpdateDate)}}</strong></div>
            </div>
            {{validationBlock}}
            <h2>Cicli</h2>
            <table>
              <thead><tr><th>Ciclo</th><th>Modalita</th><th>Prossima</th><th>Comando</th></tr></thead>
              <tbody>{{cycles}}</tbody>
            </table>
            <h2>Zone</h2>
            <table>
              <thead><tr><th>Zona</th><th>Stato HA</th><th>Deficit</th><th>Calibrazione</th><th>Comando</th></tr></thead>
              <tbody>{{zones}}</tbody>
            </table>
            <h2>Eventi</h2>
            <table>
              <thead><tr><th>Quando</th><th>Tipo</th><th>Zona</th><th>Dettaglio</th></tr></thead>
              <tbody>{{eventRows}}</tbody>
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
            function applyCalibration(id) { post('/api/calibration/zones/' + id + '/apply'); }
            async function calibrateZone(id) {
              const minutes = prompt('Minuti test calibrazione', '10');
              if (!minutes) return;
              await post('/api/calibration/zones/' + id + '/start?minutes=' + encodeURIComponent(minutes));
              const values = prompt('Misure mm separate da virgola, es. 1.8,2.1,1.6');
              if (!values) return;
              const measurements = values.split(',').map(x => Number(x.trim())).filter(x => !Number.isNaN(x));
              const res = await fetch('/api/calibration/zones/' + id + '/complete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ minutes: Number(minutes), measurements_mm: measurements })
              });
              const body = await res.json().catch(() => ({}));
              document.getElementById('status').textContent = body.recommendation || body.message || JSON.stringify(body);
            }
          </script>
        </body>
        </html>
        """;

    return Results.Content(html, "text/html");
});

app.MapGet("/config", async (IrrigationConfigStore store, CancellationToken cancellationToken) =>
{
    var config = await store.GetAsync(cancellationToken);
    var json = JsonSerializer.Serialize(config, new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    });

    var html = $$"""
        <!doctype html>
        <html lang="it">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>Config Irrigazione</title>
          <style>
            :root { color-scheme: light dark; font-family: Segoe UI, sans-serif; }
            body { margin: 0; padding: 24px; background: #f7f7f2; color: #20231f; }
            main { max-width: 1100px; margin: 0 auto; }
            header { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-bottom: 18px; }
            textarea { box-sizing: border-box; width: 100%; min-height: 70vh; padding: 14px; border: 1px solid #cbd4c2; border-radius: 8px; font: 14px Consolas, monospace; }
            button { min-width: 86px; border: 0; border-radius: 6px; padding: 8px 10px; background: #1f7a4d; color: white; cursor: pointer; }
            button.secondary { background: #5f6959; }
            .status { margin: 12px 0; padding: 12px 14px; border: 1px solid #ccd8c2; border-radius: 8px; background: #ffffff; }
            @media (prefers-color-scheme: dark) {
              body { background: #171a16; color: #eef2e9; }
              textarea, .status { background: #20241e; color: #eef2e9; border-color: #384030; }
            }
          </style>
        </head>
        <body>
          <main>
            <header>
              <h1>Configurazione</h1>
              <div>
                <button class="secondary" onclick="location.href='/ui'">Indietro</button>
                <button onclick="saveConfig()">Salva</button>
              </div>
            </header>
            <div class="status" id="status">Modifica JSON, poi salva. Viene creato un backup automatico.</div>
            <textarea id="config">{{HtmlEncoder.Default.Encode(json)}}</textarea>
          </main>
          <script>
            async function saveConfig() {
              const status = document.getElementById('status');
              let parsed;
              try {
                parsed = JSON.parse(document.getElementById('config').value);
              } catch (error) {
                status.textContent = 'JSON non valido: ' + error.message;
                return;
              }
              const res = await fetch('/api/config', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(parsed)
              });
              const body = await res.json().catch(() => ({}));
              status.textContent = body.message || (res.ok ? 'Configurazione salvata' : JSON.stringify(body));
            }
          </script>
        </body>
        </html>
        """;

    return Results.Content(html, "text/html");
});

app.MapGet("/api/config", async (IrrigationConfigStore store, CancellationToken cancellationToken) =>
    Results.Ok(await store.GetAsync(cancellationToken)));

app.MapPut("/api/config", async (
    IrrigationConfig config,
    IrrigationConfigStore store,
    IrrigationConfigValidator validator,
    CancellationToken cancellationToken) =>
{
    var validation = validator.Validate(config);
    if (!validation.IsValid)
    {
        return Results.BadRequest(validation);
    }

    await store.SaveAsync(config, cancellationToken);
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
