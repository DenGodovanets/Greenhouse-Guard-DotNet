using GreenhouseGuard.Domain.Entities;

namespace GreenhouseGuard.Domain.Services;

public sealed class AnomalyDetector
{
    public Anomaly? Detect(
        string sensorType,
        decimal value,
        IReadOnlyCollection<decimal> historicalValues,
        int requiredSampleSize,
        decimal zScoreThreshold)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sensorType);
        ArgumentNullException.ThrowIfNull(historicalValues);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requiredSampleSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(zScoreThreshold);

        if (historicalValues.Count < requiredSampleSize)
        {
            return null;
        }

        var mean = historicalValues.Average();
        var standardDeviation = CalculateStandardDeviation(historicalValues, mean);

        if (standardDeviation == 0m)
        {
            return null;
        }

        var zScore = Math.Abs((value - mean) / standardDeviation);
        if (zScore <= zScoreThreshold)
        {
            return null;
        }

        return Anomaly.Create(
            sensorType,
            value,
            zScore,
                $"Z-score {zScore:0.00} exceeded threshold {zScoreThreshold:0.00} for {sensorType}.");
    }

    private static decimal CalculateStandardDeviation(IReadOnlyCollection<decimal> values, decimal mean)
    {
        var variance = values
            .Select(value => Math.Pow((double)(value - mean), 2))
            .Average();

        return (decimal)Math.Sqrt(variance);
    }
}
