namespace IrrigationController.Models;

public sealed class DecisionPlan
{
    public DecisionDay Today { get; set; } = new();
    public DecisionDay Tomorrow { get; set; } = new();
}

public sealed class DecisionDay
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "NA";
    public string WeatherLabel { get; set; } = "Meteo non disponibile";
    public string Decision { get; set; } = "In attesa";
    public string DecisionClass { get; set; } = "warn";
    public double ExpectedRainMm { get; set; }
    public double EffectiveRainMm { get; set; }
    public int RainProbability { get; set; }
    public double Et0Mm { get; set; }
    public List<DecisionCycle> Cycles { get; set; } = [];
    public List<DecisionEvent> Events { get; set; } = [];
}

public sealed class DecisionCycle
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Time { get; set; } = "";
    public string Icon { get; set; } = "DROP";
    public string Decision { get; set; } = "";
    public string DecisionClass { get; set; } = "";
    public List<DecisionZone> Zones { get; set; } = [];
}

public sealed class DecisionZone
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "DROP";
    public string Text { get; set; } = "";
}

public sealed class DecisionEvent
{
    public string Time { get; set; } = "";
    public string Icon { get; set; } = "INFO";
    public string Text { get; set; } = "";
}
