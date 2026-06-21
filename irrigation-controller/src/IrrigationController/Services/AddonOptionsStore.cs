using System.Text.Json;
using System.Text.Json.Serialization;
using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class AddonOptionsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _path;

    public AddonOptionsStore(IConfiguration configuration)
    {
        _path = configuration["ADDON_OPTIONS_PATH"]
            ?? Environment.GetEnvironmentVariable("ADDON_OPTIONS_PATH")
            ?? "/data/options.json";
    }

    public AddonOptions? Read()
    {
        if (!File.Exists(_path))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AddonOptions>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
