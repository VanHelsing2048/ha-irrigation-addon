using System.Text.Json;
using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class IrrigationStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string _path;
    private IrrigationRuntimeState? _cached;

    public IrrigationStateStore(IConfiguration configuration)
    {
        _path = configuration["IRRIGATION_STATE_PATH"]
            ?? Environment.GetEnvironmentVariable("IRRIGATION_STATE_PATH")
            ?? "/data/state.json";
    }

    public async Task<IrrigationRuntimeState> GetAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cached is not null)
            {
                return _cached;
            }

            if (!File.Exists(_path))
            {
                _cached = new IrrigationRuntimeState();
                return _cached;
            }

            await using var stream = File.OpenRead(_path);
            _cached = await JsonSerializer.DeserializeAsync<IrrigationRuntimeState>(stream, JsonOptions, cancellationToken)
                ?? new IrrigationRuntimeState();
            return _cached;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(IrrigationRuntimeState state, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(state, JsonOptions), cancellationToken);
            _cached = state;
        }
        finally
        {
            _lock.Release();
        }
    }
}
