using IrrigationController.Models;

namespace IrrigationController.Services;

public static class CalibrationCalculator
{
    public static CalibrationResult? Calculate(string zoneId, CalibrationCompleteRequest request)
    {
        if (string.IsNullOrWhiteSpace(zoneId) || request.Minutes <= 0 || request.MeasurementsMm.Count == 0)
        {
            return null;
        }

        if (request.MeasurementsMm.Any(value => value < 0))
        {
            return null;
        }

        var average = request.MeasurementsMm.Average();
        if (average <= 0)
        {
            return null;
        }

        var min = request.MeasurementsMm.Min();
        var max = request.MeasurementsMm.Max();
        var precipitationRate = average / request.Minutes * 60;
        var uniformity = min / average * 100;

        return new CalibrationResult
        {
            ZoneId = zoneId,
            Minutes = request.Minutes,
            MeasurementsMm = request.MeasurementsMm,
            AverageMm = Math.Round(average, 2),
            MinMm = Math.Round(min, 2),
            MaxMm = Math.Round(max, 2),
            PrecipitationRateMmH = Math.Round(precipitationRate, 2),
            DistributionUniformityPercent = Math.Round(uniformity, 1),
            Recommendation = BuildRecommendation(precipitationRate, uniformity)
        };
    }

    private static string BuildRecommendation(double precipitationRate, double uniformity)
    {
        var uniformityText = uniformity switch
        {
            >= 80 => "uniformity is good",
            >= 65 => "uniformity is acceptable",
            _ => "uniformity is low; check sprinkler coverage before trusting automatic timing"
        };

        return $"Set precipitation_rate_mm_h to {precipitationRate:0.##}; {uniformityText}.";
    }
}
